using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public UIDocument uiDocument;
    public string gameSceneName = "Game";

    // UI Elements - Settings
    private VisualElement _settingsOverlay;

    // UI Elements - Leaderboard
    private VisualElement _leaderboardOverlay;
    private ListView _leaderboardList;
    private Button _tabDaily, _tabWeekly, _tabAllTime;

    // Data Structure for Leaderboard
    public struct LeaderboardEntry
    {
        public string name;
        public int score;
        public int rank;
        public bool isPlayer;
    }

    private List<LeaderboardEntry> _currentData = new List<LeaderboardEntry>();

    [Header("Audio Settings")] [SerializeField]
    AudioSource audioSource;

    [SerializeField] private AudioClip proceedSound;
    [SerializeField] private AudioClip cancelSound;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("MainMenuManager: uiDocument is not assigned in the Inspector!");
            return;
        }

        var root = uiDocument.rootVisualElement;

        // Find the main "Root" container (excludes the Overlays which are siblings)
        var rootMenu = root.Q<VisualElement>("Root");

        // Register a click event on the background container
        rootMenu.RegisterCallback<ClickEvent>(OnBackgroundClicked);


        // --- 1. SETTINGS LOGIC ---
        _settingsOverlay = root.Q<VisualElement>("SettingsOverlay");
        var settingsBtn = root.Q<Button>("SettingsBtn");
        var closeSettingsBtn = root.Q<Button>("CloseSettingsBtn");

        // proceed sound for opening
        settingsBtn.clicked += () =>
        {
            PlaySound(proceedSound);
            _settingsOverlay.style.display = DisplayStyle.Flex;
        };

        // cancel sound for closing
        closeSettingsBtn.clicked += () =>
        {
            PlaySound(cancelSound);
            _settingsOverlay.style.display = DisplayStyle.None;
        };

        // --- 2. LEADERBOARD LOGIC ---
        _leaderboardOverlay = root.Q<VisualElement>("LeaderboardOverlay");
        _leaderboardList = root.Q<ListView>("LeaderboardList");

        var leaderboardBtn = root.Q<Button>("LeaderboardBtn");
        var closeLbBtn = root.Q<Button>("CloseLeaderboardBtn");

        // proceed sound for opening
        leaderboardBtn.clicked += () =>
        {
            PlaySound(proceedSound);
            OpenLeaderboard("Weekly");
        };

        // cancel sound for closing
        closeLbBtn.clicked += () =>
        {
            PlaySound(cancelSound);
            _leaderboardOverlay.style.display = DisplayStyle.None;
        };

        // Tabs (Proceed sounds)
        _tabDaily = root.Q<Button>("TabDaily");
        _tabWeekly = root.Q<Button>("TabWeekly");
        _tabAllTime = root.Q<Button>("TabAllTime");

        if (_tabDaily != null)
            _tabDaily.clicked += () =>
            {
                PlaySound(proceedSound);
                SwitchTab("Daily");
            };

        if (_tabWeekly != null)
            _tabWeekly.clicked += () =>
            {
                PlaySound(proceedSound);
                SwitchTab("Weekly");
            };

        if (_tabAllTime != null)
            _tabAllTime.clicked += () =>
            {
                PlaySound(proceedSound);
                SwitchTab("AllTime");
            };

        // Initialize the ListView
        if (_leaderboardList != null) ConfigureListView();
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnBackgroundClicked(ClickEvent evt)
    {
        // We need to check if the user clicked a Button (or an icon/label inside a Button)
        // evt.target is the specific element clicked (e.g., the Label inside the button)
        VisualElement clickedElement = evt.target as VisualElement;

        // Walk up the hierarchy from the clicked element to see if it belongs to a Button
        while (clickedElement != null && clickedElement != evt.currentTarget)
        {
            if (clickedElement is Button)
            {
                // We clicked a button! Do NOT start the game.
                return;
            }

            clickedElement = clickedElement.parent;
        }

        // Double check the element itself (in case it was the button directly)
        if (evt.target is Button) return;

        // If we passed the checks, it was an empty space click
        Debug.Log("Empty space tapped! Loading Game...");

        // Play the sound on a persistent object so it doesn't cut off
        PlaySoundCrossScene(proceedSound);

        SceneManager.LoadScene(gameSceneName);
    }

    // New helper method: Creates a temporary object that survives the scene load
    private void PlaySoundCrossScene(AudioClip clip)
    {
        GameObject tempAudioObj = new GameObject("TempAudio_Transition");
        DontDestroyOnLoad(tempAudioObj);

        AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();
        tempSource.clip = clip;

        // Copy volume/pitch from the main source to maintain consistency
        tempSource.volume = audioSource.volume;
        tempSource.pitch = audioSource.pitch;


        tempSource.Play();

        // Destroy the temporary object after the clip has finished playing
        Destroy(tempAudioObj, clip.length);
    }

    // --- LEADERBOARD HELPER METHODS ---

    private void OpenLeaderboard(string tabName)
    {
        _leaderboardOverlay.style.display = DisplayStyle.Flex;
        SwitchTab(tabName);
    }

    private void SwitchTab(string tabName)
    {
        // 1. Update Tab Visuals
        _tabDaily.RemoveFromClassList("tab-active");
        _tabWeekly.RemoveFromClassList("tab-active");
        _tabAllTime.RemoveFromClassList("tab-active");

        if (tabName == "Daily") _tabDaily.AddToClassList("tab-active");
        else if (tabName == "Weekly") _tabWeekly.AddToClassList("tab-active");
        else _tabAllTime.AddToClassList("tab-active");

        // 2. Generate Data and Refresh
        GenerateFakeData(tabName);

        _leaderboardList.itemsSource = _currentData;
        _leaderboardList.Rebuild();

        _leaderboardList.ScrollToItem(0);
    }

    private void ConfigureListView()
    {
        // Create the Visual Tree for a single row
        _leaderboardList.makeItem = () =>
        {
            var row = new VisualElement();
            row.AddToClassList("rank-row");

            var rankLabel = new Label();
            rankLabel.name = "Rank";
            rankLabel.AddToClassList("row-text");
            rankLabel.style.width = Length.Percent(15);
            rankLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

            var nameLabel = new Label();
            nameLabel.name = "Name";
            nameLabel.AddToClassList("row-text");
            nameLabel.style.width = Length.Percent(55);
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            var scoreLabel = new Label();
            scoreLabel.name = "Score";
            scoreLabel.AddToClassList("row-text");
            scoreLabel.style.width = Length.Percent(30);
            scoreLabel.style.unityTextAlign = TextAnchor.MiddleRight;

            row.Add(rankLabel);
            row.Add(nameLabel);
            row.Add(scoreLabel);

            return row;
        };

        // Bind data to the row
        _leaderboardList.bindItem = (element, index) =>
        {
            var entry = _currentData[index];

            var rankLbl = element.Q<Label>("Rank");
            var nameLbl = element.Q<Label>("Name");
            var scoreLbl = element.Q<Label>("Score");

            rankLbl.text = entry.rank.ToString();
            nameLbl.text = entry.name;
            scoreLbl.text = entry.score.ToString("N0");

            // Highlight Player
            if (entry.isPlayer)
            {
                element.AddToClassList("player-row");
                nameLbl.text = "YOU";
            }
            else
            {
                element.RemoveFromClassList("player-row");
            }
        };

        _leaderboardList.fixedItemHeight = 80;
    }

    private void GenerateFakeData(string seed)
    {
        _currentData.Clear();
        int playerScore = PlayerPrefs.GetInt("HighScore", 0);

        // Generate 5000 random players
        var rng = new System.Random(seed.GetHashCode());

        for (int i = 0; i < 5000; i++)
        {
            _currentData.Add(new LeaderboardEntry
            {
                name = "Player " + rng.Next(1000, 99999),
                score = rng.Next(5, 1000),
                isPlayer = false
            });
        }

        // Add the real player
        _currentData.Add(new LeaderboardEntry
        {
            name = "Player",
            score = playerScore,
            isPlayer = true
        });

        // Sort descending
        _currentData.Sort((a, b) => b.score.CompareTo(a.score));

        // Assign ranks
        for (int i = 0; i < _currentData.Count; i++)
        {
            var entry = _currentData[i];
            entry.rank = i + 1;
            _currentData[i] = entry;
        }
    }
}
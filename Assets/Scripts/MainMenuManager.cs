using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    public UIDocument uiDocument;
    public string gameSceneName = "Game";

    // UI Elements
    private VisualElement _settingsOverlay;
    private VisualElement _leaderboardOverlay;
    private ListView _leaderboardList;
    private Button _tabDaily, _tabWeekly, _tabAllTime;
    private VisualElement _profileOverlay;
    private Label _profileBestScoreLabel;
    private Label _profileTotalGamesLabel;
    private VisualElement _shopOverlay;
    private VisualElement _stickyPlayerRow;
    private Label _stickyRankLabel;
    private Label _stickyScoreLabel;
    private int _playerIndexInList = -1; // To track where we are
    

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

        // --- BACKGROUND CLICK ---
        var inputArea = root.Q<VisualElement>("InputArea");
        if (inputArea != null)
        {
            inputArea.RegisterCallback<ClickEvent>(OnBackgroundClicked);
        }

        // --- 1. SETTINGS LOGIC ---
        _settingsOverlay = root.Q<VisualElement>("SettingsOverlay");
        var settingsBtn = root.Q<Button>("SettingsBtn");
        var closeSettingsBtn = root.Q<Button>("CloseSettingsBtn");

        if (settingsBtn != null)
        {
            settingsBtn.clicked += () =>
            {
                PlaySound(proceedSound);
                _settingsOverlay.style.display = DisplayStyle.Flex;
            };
        }

        if (closeSettingsBtn != null)
        {
            closeSettingsBtn.clicked += () =>
            {
                PlaySound(cancelSound);
                _settingsOverlay.style.display = DisplayStyle.None;
            };
        }

        // --- 2. LEADERBOARD LOGIC ---
        _leaderboardOverlay = root.Q<VisualElement>("LeaderboardOverlay");
        _leaderboardList = root.Q<ListView>("LeaderboardList");
        var leaderboardBtn = root.Q<Button>("LeaderboardBtn");
        var closeLbBtn = root.Q<Button>("CloseLeaderboardBtn");
        _stickyPlayerRow = root.Q<VisualElement>("StickyPlayerRow");
        _stickyRankLabel = root.Q<Label>("StickyRank");
        _stickyScoreLabel = root.Q<Label>("StickyScore");
        _leaderboardList.RegisterCallback<GeometryChangedEvent>(OnLeaderboardGeometryChange);

        if (leaderboardBtn != null)
        {
            leaderboardBtn.clicked += () =>
            {
                PlaySound(proceedSound);
                OpenLeaderboard("Weekly");
            };
        }

        if (closeLbBtn != null)
        {
            closeLbBtn.clicked += () =>
            {
                PlaySound(cancelSound);
                _leaderboardOverlay.style.display = DisplayStyle.None;
            };
        }

        // --- 3. PROFILE LOGIC (NEW) ---
        _profileOverlay = root.Q<VisualElement>("ProfileOverlay");
        _profileBestScoreLabel = root.Q<Label>("ProfileBestScore");
        _profileTotalGamesLabel = root.Q<Label>("ProfileTotalGames");

        // "ProfileContainer" is the button name in your original UXML (top-right)
        var profileBtn = root.Q<Button>("ProfileContainer");
        var closeProfileBtn = root.Q<Button>("CloseProfileBtn");
        var exportProfileBtn = root.Q<Button>("ExportProfileBtn");

        if (profileBtn != null)
        {
            profileBtn.clicked += () =>
            {
                PlaySound(proceedSound);
                OpenProfile();
            };
        }

        if (closeProfileBtn != null)
        {
            closeProfileBtn.clicked += () =>
            {
                PlaySound(cancelSound);
                _profileOverlay.style.display = DisplayStyle.None;
            };
        }

        if (exportProfileBtn != null)
        {
            exportProfileBtn.clicked += () =>
            {
                PlaySound(proceedSound);
                ExportProfileJSON();
                Debug.Log("Exporting Profile Data...");
            };
        }

        // --- TABS LOGIC ---
        _tabDaily = root.Q<Button>("TabDaily");
        _tabWeekly = root.Q<Button>("TabWeekly");
        _tabAllTime = root.Q<Button>("TabAllTime");

        if (_tabDaily != null)
        {
            _tabDaily.clicked += () =>
            {
                PlaySound(proceedSound);
                SwitchTab("Daily");
            };
        }

        if (_tabWeekly != null)
        {
            _tabWeekly.clicked += () =>
            {
                PlaySound(proceedSound);
                SwitchTab("Weekly");
            };
        }

        if (_tabAllTime != null)
        {
            _tabAllTime.clicked += () =>
            {
                PlaySound(proceedSound);
                SwitchTab("AllTime");
            };
        }

        // Initialize the ListView
        if (_leaderboardList != null) ConfigureListView();
        
        // --- 4. SHOP LOGIC ---
        _shopOverlay = root.Q<VisualElement>("ShopOverlay");
        var shopBtn = root.Q<Button>("ShopBtn");
        var closeShopBtn = root.Q<Button>("CloseShopBtn");

        if (shopBtn != null)
        {
            shopBtn.clicked += () =>
            {
                PlaySound(proceedSound);
                _shopOverlay.style.display = DisplayStyle.Flex;
            };
        }

        if (closeShopBtn != null)
        {
            closeShopBtn.clicked += () =>
            {
                PlaySound(cancelSound);
                _shopOverlay.style.display = DisplayStyle.None;
            };
        }
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
        tempSource.mute = audioSource.mute;
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
        // NEW: Update Sticky Row Data
        if (_playerIndexInList != -1)
        {
            var pEntry = _currentData[_playerIndexInList];
            if (_stickyRankLabel != null) _stickyRankLabel.text = pEntry.rank.ToString();
            if (_stickyScoreLabel != null) _stickyScoreLabel.text = pEntry.score.ToString("N0");
        }
        
        // Force a check (reset to top)
        CheckStickyRowVisibility(0);
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

        // Assign ranks AND FIND PLAYER
        _playerIndexInList = -1;

        for (int i = 0; i < _currentData.Count; i++)
        {
            var entry = _currentData[i];
            entry.rank = i + 1;
            _currentData[i] = entry;

            if (entry.isPlayer)
            {
                _playerIndexInList = i;
            }
        }
    }

    private void OpenProfile()
    {
        // Fetch data from PlayerPrefs
        int bestScore = PlayerPrefs.GetInt("HighScore", 0);
        int totalGames = PlayerPrefs.GetInt("TotalGames", 0);

        // Update UI
        if (_profileBestScoreLabel != null)
            _profileBestScoreLabel.text = $"BEST SCORE: {bestScore}";

        if (_profileTotalGamesLabel != null)
            _profileTotalGamesLabel.text = $"GAMES PLAYED: {totalGames}";

        // Show Overlay
        _profileOverlay.style.display = DisplayStyle.Flex;
    }

    private void ExportProfileJSON()
    {
        // 1. Create a new data object
        UserProfileData data = new UserProfileData();

        // 2. Gather Data
        // Getting name from the UI Label (ensure you have a reference to it, or use a default)
        var nameLabel = uiDocument.rootVisualElement.Q<Label>("PlayerName");
        data.playerName = nameLabel != null ? nameLabel.text : "Unknown Player";

        // Getting Stats from PlayerPrefs
        data.highScore = PlayerPrefs.GetInt("HighScore", 0);
        data.totalGames = PlayerPrefs.GetInt("TotalGames", 0);

        // Getting Settings
        data.soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        data.vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;

        // Getting Meta Data
        data.appVersion = Application.version;
        data.timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 3. Convert to JSON string
        // 'true' makes the JSON pretty-printed (readable with indentation)
        string jsonString = JsonUtility.ToJson(data, true);

        // 4. Write to file
        string fileName = "UserProfile_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(path, jsonString);

        Debug.Log($"Profile Exported successfully to: {path}");

        // Optional: Open the file location (Works on Windows/Mac Editor)
#if UNITY_EDITOR
        UnityEditor.EditorUtility.RevealInFinder(path);
#endif
    }
    
    private void OnLeaderboardGeometryChange(GeometryChangedEvent evt)
    {
        // Only need to do this once to find the scrollview
        var scrollView = _leaderboardList.Q<ScrollView>();
        if (scrollView != null)
        {
            // Listen to scroll changes
            scrollView.verticalScroller.valueChanged += (value) =>
            {
                CheckStickyRowVisibility(value);
            };
        }
    }

    private void CheckStickyRowVisibility(float scrollValue)
    {
        if (_playerIndexInList == -1) return;

        // Calculate visible range
        float listHeight = _leaderboardList.resolvedStyle.height;
        float itemHeight = _leaderboardList.fixedItemHeight; // 80
        
        // How many items can we see at once?
        float visibleItemsCount = listHeight / itemHeight;

        // What is the index of the first item at the top?
        // ScrollValue is usually 0 to HighNumber. 
        // We can use the ListView's scrollOffset directly if available, 
        // or calculate from Scroller. 
        // The safest way in UI Toolkit ListView:
        var scrollView = _leaderboardList.Q<ScrollView>();
        float currentScrollY = scrollView.scrollOffset.y;
        
        int firstVisibleIndex = (int)(currentScrollY / itemHeight);
        int lastVisibleIndex = firstVisibleIndex + (int)visibleItemsCount;

        // Logic: If Player is BELOW the view, show sticky row.
        // We add a small buffer (+1) to make the transition smoother
        if (_playerIndexInList > lastVisibleIndex)
        {
            _stickyPlayerRow.style.display = DisplayStyle.Flex;
        }
        else
        {
            _stickyPlayerRow.style.display = DisplayStyle.None;
        }
    }
}
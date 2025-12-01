using System;

[System.Serializable]
public class UserProfileData
{
    public string playerName;
    public int highScore;
    public int totalGames;
    
    // Settings
    public bool soundEnabled;
    public bool vibrationEnabled;

    // Meta Data
    public string appVersion;
    public string timestamp;
}
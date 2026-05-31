[System.Serializable] 
public class PlayerData
{
    public string playerName;
    public string email;
    public string password;
    
    public int totalWins;
    public int totalLosses;
    public float bgmVolume;
    public float sfxVolume;

    public PlayerData()
    {
        playerName = "New Challenger";
        email = "";
        password = "";
        totalWins = 0;
        totalLosses = 0;
        bgmVolume = 1f; 
        sfxVolume = 1f; 
    }
}
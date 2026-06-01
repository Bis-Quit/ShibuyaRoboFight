using System.Collections.Generic;

[System.Serializable] 
public class PlayerData
{
    public string playerName;
    public string email;
    public string password;
    
    public int totalWins;
    public int totalLosses;

    public PlayerData()
    {
        playerName = "New Challenger";
        email = "";
        password = "";
        totalWins = 0;
        totalLosses = 0;
    }
}

[System.Serializable]
public class PlayerDatabase
{
    public List<PlayerData> accountList = new List<PlayerData>(); 
}
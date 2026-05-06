using UnityEngine;
using UnityEngine.SceneManagement;

public class ChallengeModeManager : MonoBehaviour
{
    [Header("Scene Setting")]
    public string gameplaySceneName = "";

    public void SelectEasyMode()
    {
        Debug.Log("EASY Mode Selected!");

        PlayerPrefs.SetInt("EnemyDifficulty", 0);
        PlayerPrefs.Save();

        LoadGameplayScene();
    }

    public void SelectedHardMode()
    {
        Debug.Log("Hard Mode Selected! Prepare to die!");

        PlayerPrefs.SetInt("EnemyDifficulty", 1);
        PlayerPrefs.Save();

        LoadGameplayScene();
    }

    private void LoadGameplayScene()
    {
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogError("Scene gameplay belum di assign!");
        }
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Database Character")]
    public CharacterData[] characterDatabase;

    [Header("UI Reference - Right Panel")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI bgnameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI hpText;

    [Header("3D Model Setup")]
    public Transform modelSpawnPoint;
    private GameObject currentDisplayedModel;

    [Header("UI Setting")]
    public List<CharacterButton> allButtons;

    [Header("Background Setup")]
    public List<GameObject> backgroundList;

    [Header("Background Name Setup")]
    public List<GameObject> backgroundNameList;

    [Header("Audio BGM")]
    public AudioClip sceneBGM;

    private int selectedIndex = 0;

    private void Start()
    {
        if (sceneBGM != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(sceneBGM);
        }
        PreviewCharacter(0);
    }

    public void PreviewCharacter(int index)
    {   
        if (index == selectedIndex && currentDisplayedModel != null) return;

        selectedIndex = index;
        CharacterData data = characterDatabase[index];

        if (data.characterVO != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(data.characterVO, true);
        }

        nameText.text = data.characterName;
        bgnameText.text = data.characterName;
        descriptionText.text = data.specialSkillDescription;
        hpText.text = data.maxHealth.ToString();

        for (int i = 0; i < backgroundList.Count; i++)
        {
            if (backgroundList[i] != null)
            {
                backgroundList[i].SetActive(i == index);
            }
        }
        for (int i = 0; i < backgroundNameList.Count; i++)
        {
            if (backgroundNameList[i] != null)
                backgroundNameList[i].SetActive(i == index);
        }
        if (currentDisplayedModel != null)
        {
            Destroy(currentDisplayedModel);
        }
        if (data.visualPrefab != null)
        {
            currentDisplayedModel = Instantiate(data.visualPrefab, modelSpawnPoint);
            currentDisplayedModel.transform.localPosition = Vector3.zero;
            currentDisplayedModel.transform.localRotation = Quaternion.identity;

            Animator anim = currentDisplayedModel.GetComponent<Animator>();
            if (anim != null) anim.Play("idle");
        }

        for (int i = 0; i < allButtons.Count; i++)
        {
            if (i == index)
                allButtons[i].SetSelected(true);
            else
                allButtons[i].SetSelected(false);
        }
    }

    public void ConfirmSelection()
    {
        Debug.Log("Karakter Terpilih: " + characterDatabase[selectedIndex].characterName);

        PlayerPrefs.DeleteKey("CurrentEnemyID");

        PlayerPrefs.SetInt("SelectedPlayerID", selectedIndex);
        PlayerPrefs.Save();

        SceneManager.LoadScene("ChallengeMode");
    }
}

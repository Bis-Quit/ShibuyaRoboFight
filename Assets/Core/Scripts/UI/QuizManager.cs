using UnityEngine;
using TMPro;

public class QuizManager : MonoBehaviour
{
    public GameObject[] panels;
    public GameObject scorePanels;

    public TextMeshProUGUI benarText;
    public TextMeshProUGUI salahText;

    private int currentPanelIndex = 0;
    private int correctAnswers = 0;

    void Start()
    {
        ShowPanel(0);
        scorePanels.SetActive(false);
    }

    public void AnswerButton(bool isCorrect)
    {
        if (isCorrect) correctAnswers++;
        currentPanelIndex++;

        if (currentPanelIndex < panels.Length)
        {
            ShowPanel (currentPanelIndex);
        }
        else
        {
            ShowScore();
        }
    }

    void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }
    }

    void ShowScore()
    {
        foreach (GameObject p in panels) p.SetActive(false);
        scorePanels.SetActive(true);

        int wrongAnswer = panels.Length - correctAnswers;
        benarText.text = "Benar : " + correctAnswers;
        salahText.text = "Salah : " + wrongAnswer;
    }
}

using UnityEngine;

public class MateriManager : MonoBehaviour
{
    [Header ("Panel Materi")]
    public GameObject materi1;
    public GameObject materi2;
    public GameObject materi3;

    public void NextToMateri2()
    {
        materi1.SetActive(false);
        materi2.SetActive(true);
    }

    public void NextToMateri3()
    {
        materi2.SetActive(false);
        materi3.SetActive(true);
    }

    public void BackToMateri1()
    {
        materi2.SetActive(false);
        materi1.SetActive(true);
    }

    public void BackToMateri2()
    {
        materi3.SetActive(false);
        materi2.SetActive(true);
    }
} 

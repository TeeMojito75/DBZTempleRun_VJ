using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Events : MonoBehaviour
{
    public GameObject pausePanel;
    public void ReplayGame()
    {
        SceneManager.LoadScene("DBTemple");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoBack()
    {
        SceneManager.LoadScene("Menu");
    }

    public void ResumeGame()
    {
        PlayerManager.paused = false;
        Time.timeScale = 1;
        pausePanel.SetActive(false); // Oculta el PausePanel
    }
}

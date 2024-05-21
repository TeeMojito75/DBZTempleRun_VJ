using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Events : MonoBehaviour
{
    
    public void ReplayGame()
    {
        SceneManager.LoadScene("DBTemple");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

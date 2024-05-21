using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static bool gameOver;
    public GameObject gameOverPanel;
    
    public static bool isGameStarted;
    public GameObject startingText;
    public static int numberOfCoins;
    public Text coinsText;
    void Start()
    {
        gameOver = false;
        isGameStarted = false;
        Time.timeScale = 1;
        numberOfCoins = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOver)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0;
        }
        coinsText.text = "Coins: " + numberOfCoins;
        if (Input.GetKeyUp(KeyCode.Space) && !isGameStarted)
        {
            isGameStarted = true;
            Destroy (startingText);
        }

    }
}

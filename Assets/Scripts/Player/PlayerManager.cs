using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TempleRun.Player;


public class PlayerManager : MonoBehaviour
{
    public static bool gameOver;
    public GameObject gameOverPanel;

    public static bool paused;
    public GameObject pausePanel;
    
    public static bool isGameStarted;
    public GameObject startingText;
    public Text scoreText;
    public Text godModeText;
    public static int score;
    private float elapsedTime = 0.0f;
    private float timeForNextPoint = 1.0f; // Tiempo inicial requerido para ganar un punto
    private float accelerationFactor = 1.01f; // Factor de aceleración para la disminución del tiempo requerido
    private float minTimeForPoint = 0.1f; // Tiempo mínimo requerido para un punto    
    void Start()
    {
        gameOver = false;
        paused = false;
        isGameStarted = false;
        Time.timeScale = 1;
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOver)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0;
        }
        if (paused)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0;
        }
        if(isGameStarted)
        {
            // Acumula el tiempo transcurrido
            elapsedTime += Time.deltaTime;

            // Comprueba si ha pasado el tiempo requerido para el próximo punto
            while (elapsedTime >= timeForNextPoint)
            {
                // Incrementa el puntaje y resta el tiempo correspondiente del tiempo acumulado
                score += 1;
                elapsedTime -= timeForNextPoint;

                // Disminuye el tiempo requerido para el próximo punto, con un límite mínimo
                timeForNextPoint = Mathf.Max(minTimeForPoint, timeForNextPoint / accelerationFactor);
            }
        }
        scoreText.text = "Score: " + score;
        if (PC2.godMode)
        {
            godModeText.text = "God Mode";
        }
        else
        {
            godModeText.text = "";
        }
        if (Input.GetKeyUp(KeyCode.Space) && !isGameStarted)
        {
            isGameStarted = true;
            Destroy (startingText);
        }

    }
}

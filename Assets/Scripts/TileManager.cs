using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public float tileLength = 15;
    public int numberOfTiles = 30;
    public float turnOffset = 7; 
    public float turnLength = 4;
    private int lastTurnTileIndex = 12;
    private List<GameObject> activeTiles = new List<GameObject>();

    public Transform playerTransform;

    private Vector3 spawnDirection = Vector3.forward; // Dirección de generación inicial
    private Vector3 lastSpawnPoint = Vector3.zero; // Última posición de generación
    private bool isTurning = false;
    private int tilesGenerated = 0;
    private bool isSpawning = false;
    void Start()
{
    spawnDirection = Vector3.forward; // Inicializa la dirección de generación como el eje z
    lastSpawnPoint = playerTransform.position - new Vector3(0, 0, 15); // Inicializa la última posición de generación 15 unidades detrás de la posición inicial del personaje

    for (int i = 0; i < numberOfTiles; i++)
    {
        if (i == 0 || i == 1)
        {
            SpawnTile(0);
        }
        else
        {
            SpawnTile(Random.Range(0, tilePrefabs.Length));
        }
    }
}

   // Update is called once per frame
void Update()
{
    if ((playerTransform.position.z > activeTiles[0].transform.position.z + tileLength || isTurning) && !isSpawning) // Ajusta la condición
    {
        isSpawning = true;
        SpawnTile(Random.Range(0, tilePrefabs.Length));
        tilesGenerated++;

        if (tilesGenerated > numberOfTiles)
        {
            DeleteTile();
        }

        isTurning = false;
        isSpawning = false;
    }
}

public void SpawnTile(int tileIndex)
{
    while ((lastTurnTileIndex == 11 && tileIndex == 11) || (lastTurnTileIndex == 12 && tileIndex == 12))
    {
        tileIndex = Random.Range(0, tilePrefabs.Length);
    }

    // Actualiza el índice de la última baldosa de giro generada si la baldosa generada es de giro
    if (tileIndex == 11 || tileIndex == 12)
    {
        lastTurnTileIndex = tileIndex;
    }

    lastSpawnPoint += spawnDirection * tileLength;
    GameObject go = Instantiate(tilePrefabs[tileIndex], lastSpawnPoint, Quaternion.LookRotation(spawnDirection));
    activeTiles.Add(go);

    Vector3 oldDirection = spawnDirection;

    // Cambia la dirección de generación cuando se genera una baldosa con índice 11 o 12
    if (tileIndex == 11)
    {
        spawnDirection = Quaternion.Euler(0, 90, 0) * spawnDirection;
    }
    else if (tileIndex == 12)
    {
        spawnDirection = Quaternion.Euler(0, -90, 0) * spawnDirection;
    }

    // Ajusta la última posición de generación por el offset en el eje X y Z
    if (tileIndex == 11 || tileIndex == 12)
    {
        if (oldDirection == Vector3.forward)
        {
            lastSpawnPoint.x += (tileIndex == 11 ? turnOffset : turnOffset);
            lastSpawnPoint.z += (tileIndex == 11 ? turnLength : -turnLength);
        }
        else if (oldDirection == Vector3.right)
        {
            lastSpawnPoint.x -= (tileIndex == 11 ? turnLength : -turnLength);
            lastSpawnPoint.z += (tileIndex == 11 ? turnOffset : turnOffset);
        }
        else if (oldDirection == Vector3.back)
        {
            lastSpawnPoint.x -= (tileIndex == 11 ? turnOffset : turnOffset);
            lastSpawnPoint.z -= (tileIndex == 11 ? turnLength : -turnLength);
        }
        else if (oldDirection == Vector3.left)
        {
            lastSpawnPoint.x += (tileIndex == 11 ? turnLength : -turnLength);
            lastSpawnPoint.z -= (tileIndex == 11 ? turnOffset : turnOffset);
        }
        isTurning = true;
    }
}

private void DeleteTile()
{
    new WaitForSeconds(3);
    Destroy(activeTiles[0]);
    activeTiles.RemoveAt(0);
}
}

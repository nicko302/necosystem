using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimalSpawn : MonoBehaviour
{
    public static int chunkSizeIndex = 8;

    [Header("Fox")]
    public GameObject[] foxPrefabs;
    public int foxSpawnSparsity;

    [Header("Rabbit")]
    public GameObject[] rabbitPrefabs;
    public int rabbitSpawnSparsity;

    public int mapChunkSize
    {
        get
        {
            return meshGenerator.supportedChunkSizes[chunkSizeIndex] - 1;
        }
    }


    void SpawnFox()
    {
        for (int y = -mapChunkSize; y < mapChunkSize; y++)
            for (int x = -mapChunkSize; x < mapChunkSize; x++) //for each x and y position
            {
                if (Random.Range(1, foxSpawnSparsity) == 1)
                {
                    GameObject foxesParent = GameObject.Find("Foxes");
                    GameObject foxPrefab = foxPrefabs[Random.Range(0, foxPrefabs.Length)];
                    GameObject fox = Instantiate(foxPrefab, foxesParent.transform);
                    fox.transform.position = new Vector3(x, 60, y);
                    fox.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                }
            }
    }
    void SpawnRabbit()
    {
        for (int y = -mapChunkSize; y < mapChunkSize; y++)
            for (int x = -mapChunkSize; x < mapChunkSize; x++) //for each x and y position
            {
                if (Random.Range(1, rabbitSpawnSparsity) == 1)
                {
                    GameObject rabbitsParent = GameObject.Find("Rabbits");
                    GameObject rabbitPrefab = rabbitPrefabs[Random.Range(0, rabbitPrefabs.Length)];
                    GameObject rabbit = Instantiate(rabbitPrefab, rabbitsParent.transform);
                    rabbit.transform.position = new Vector3(x, 60, y);
                    rabbit.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                }
            }
    }


    void Start()
    {
        Debug.Log("start");
        int scene = (SceneManager.GetActiveScene()).buildIndex;
        if (scene == 2)
        {
            StartCoroutine(WaitSeconds());
        }
    }

    IEnumerator WaitSeconds()
    {
        yield return new WaitForSeconds(2);
        SpawnFox();
        SpawnRabbit();
    }
}

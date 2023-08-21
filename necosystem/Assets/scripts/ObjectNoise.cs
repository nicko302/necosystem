using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectNoise : MonoBehaviour
{
    public GameObject[] treePrefabs;
    public static int chunkSizeIndex = 8;
    public noiseData noiseData;
    int x =1;
    public float treeNoiseScale = 0.5f;
    public float treeDensity = 0.5f;
    public int mapChunkSize
    {
        get
        {
            return meshGenerator.supportedChunkSizes[chunkSizeIndex] - 1;
        }
    }

    public float[,] GenerateNoiseMap()
    {
        System.Random random = new System.Random();
        x = random.Next(1, 1000000000);
        float[,] noiseMap = noise.generateNoiseMap(mapChunkSize, mapChunkSize, x, treeNoiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, Vector2.zero, noiseData.normalizeMode); //pass variables into noise generation function

        return noiseMap; //return noisemap
    }

    void GenerateTrees()
    {
        for (int y = 0; y < mapChunkSize; y++)
            for (int x = 0; x < mapChunkSize; x++) //for each x and y position
            {
                float[,] noiseMap = GenerateNoiseMap();
                //float v = Random.Range(0f, treeDensity);
                if (Random.Range(1,5) == 1) //if (noiseMap[x, y] < v)
                {
                    GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                    GameObject tree = Instantiate(prefab, transform);
                    tree.transform.position = new Vector3(x, 0, y);
                    tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    tree.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f);
                }
            }
    }

    private void Awake()
    {
        int scene = (SceneManager.GetActiveScene()).buildIndex;
        if (scene == 2)
        {
            GenerateTrees();
        }
    }
}

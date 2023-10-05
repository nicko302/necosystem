using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectNoise : MonoBehaviour
{
    public static int chunkSizeIndex = 8;

    [Header("Trees")]
    public GameObject[] treePrefabs;
    public int treeDensity;
    public float minTreeScale = .7f;
    public float maxTreeScale = 1.3f;

    [Header("Grass")]
    public GameObject[] grassPrefabs;
    public int grassDensity;
    public float minGrassScale = .7f;
    public float maxGrassScale = 1.3f;
    public List<Transform> grassTransforms = new List<Transform>();

    [Header("Objects")]
    public GameObject[] objectPrefabs;
    public int objectDensity;
    public float minObjectScale = .7f;
    public float maxObjectScale = 1.3f;


    public int mapChunkSize
    {
        get
        {
            return meshGenerator.supportedChunkSizes[chunkSizeIndex] - 1;
        }
    }

    void GenerateTrees()
    {
        for (int y = -mapChunkSize; y < mapChunkSize; y++)
            for (int x = -mapChunkSize; x < mapChunkSize; x++) //for each x and y position
            {
                if (Random.Range(1, treeDensity) == 1)
                {
                    GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                    GameObject tree = Instantiate(treePrefab, this.transform);
                    tree.transform.position = new Vector3(x, 60, y);
                    tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    tree.transform.localScale = Vector3.one * Random.Range(minTreeScale, maxTreeScale);
                }
            }
    }
    void GenerateGrass()
    {
        for (int y = -mapChunkSize; y < mapChunkSize; y++)
            for (int x = -mapChunkSize; x < mapChunkSize; x++) //for each x and y position
            {
                if (Random.Range(1, grassDensity) == 1)
                {
                    GameObject grassPrefab = grassPrefabs[Random.Range(0, grassPrefabs.Length)];
                    GameObject grass = Instantiate(grassPrefab, this.transform);
                    grassTransforms.Add(grass.transform);
                    grass.transform.position = new Vector3(x, 60, y);
                    grass.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    grass.transform.localScale = Vector3.one * Random.Range(minGrassScale, maxGrassScale);
                }
            }
    }
    void GenerateObjects()
    {
        for (int y = -mapChunkSize; y < mapChunkSize; y++)
            for (int x = -mapChunkSize; x < mapChunkSize; x++) //for each x and y position
            {
                if (Random.Range(1,objectDensity) == 1)
                {
                    GameObject objPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
                    GameObject obj = Instantiate(objPrefab, this.transform);
                    obj.transform.position = new Vector3(x, 60, y);
                    obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    obj.transform.localScale = Vector3.one * Random.Range(minObjectScale, maxObjectScale);
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
        yield return new WaitForSeconds(1);
        GenerateObjects();
        GenerateTrees();
        GenerateGrass();
    }
}

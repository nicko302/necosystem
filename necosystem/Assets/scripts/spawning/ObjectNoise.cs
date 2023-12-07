using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

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
    public List<GameObject> allGrass = new List<GameObject>();
    public GrassRegrowth grassRegrowth;


    [Header("Objects")]
    public GameObject[] objectPrefabs;
    public int objectDensity;
    public float minObjectScale = .7f;
    public float maxObjectScale = 1.3f;

    private float timer;


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
                if (UnityEngine.Random.Range(1, treeDensity) == 1)
                {
                    GameObject treePrefab = treePrefabs[UnityEngine.Random.Range(0, treePrefabs.Length)];
                    GameObject tree = Instantiate(treePrefab, this.transform);
                    tree.transform.position = new Vector3(x, 60, y);
                    tree.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);
                    tree.transform.localScale = Vector3.one * UnityEngine.Random.Range(minTreeScale, maxTreeScale);
                    tree.transform.parent = GameObject.Find("Trees").transform;
                }
            }
    }
    void GenerateGrass()
    {
        for (int y = -mapChunkSize; y < mapChunkSize; y++)
            for (int x = -mapChunkSize; x < mapChunkSize; x++) //for each x and y position
            {
                if (UnityEngine.Random.Range(1, grassDensity) == 1)
                {
                    GameObject grassPrefab = grassPrefabs[UnityEngine.Random.Range(0, grassPrefabs.Length)];
                    GameObject grass = Instantiate(grassPrefab, this.transform) as GameObject;
                    grassTransforms.Add(grass.transform);
                    allGrass.Add(grass);
                    grass.transform.position = new Vector3(x, 60, y);
                    grass.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);
                    grass.transform.localScale = Vector3.one * UnityEngine.Random.Range(minGrassScale, maxGrassScale);
                    grass.transform.parent = GameObject.Find("Grass").transform;
                }
            }
    }
    void GenerateObjects()
    {
        for (int y = -mapChunkSize; y < mapChunkSize; y++)
            for (int x = -mapChunkSize; x < mapChunkSize; x++) //for each x and y position
            {
                if (UnityEngine.Random.Range(1,objectDensity) == 1)
                {
                    GameObject objPrefab = objectPrefabs[UnityEngine.Random.Range(0, objectPrefabs.Length)];
                    GameObject obj = Instantiate(objPrefab, this.transform);
                    obj.transform.position = new Vector3(x, 60, y);
                    obj.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);
                    obj.transform.localScale = Vector3.one * UnityEngine.Random.Range(minObjectScale, maxObjectScale);
                    obj.transform.parent = GameObject.Find("Objects").transform;
                }
            }
    }

    void UpdateGrassList()
    {
        for (int i = 0; i < allGrass.Count; i++)
        {
            var grass = allGrass[i];
            if (grass == null)
            {
                allGrass.RemoveAt(i); // remove the null at this index
                i--;
                continue; // move to the next iteration
            }
        }
        grassRegrowth.canRegrow = true;

    }


    void Start()
    {
        Debug.Log("start");
        int scene = (SceneManager.GetActiveScene()).buildIndex;
        if (scene == 1)
        {
            StartCoroutine(WaitSeconds());
        }
    }

    void Update()
    {
        /*
        if (timer <= 0)
        {
            UpdateGrassList();
            timer = 3;
        }
        else
        {
            timer -= Time.deltaTime;
        }
        */
    }

    IEnumerator WaitSeconds() // delay object generation to allow terrain to generate beforehand
    {
        yield return new WaitForSeconds(2);
        GenerateObjects();
        GenerateTrees();
        GenerateGrass();
        yield return new WaitForSeconds(5);
        UpdateGrassList();
    }
}
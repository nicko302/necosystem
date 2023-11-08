using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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

    [Header("Grass regrowth")]
    [SerializeField]
    private float timer = 0;
    [SerializeField]
    private float minInterval;
    [SerializeField]
    private float maxInterval;
    private bool canRegrow = false;


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
                    GameObject grass = Instantiate(grassPrefab, this.transform);
                    grassTransforms.Add(grass.transform);
                    grass.transform.position = new Vector3(x, 60, y);
                    grass.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);
                    grass.transform.localScale = Vector3.one * UnityEngine.Random.Range(minGrassScale, maxGrassScale);
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

    private void Update()
    {
        if (canRegrow)
        {
            if (timer <= 0)
            {
                // if timer reaches zero, a single grass will regrow
                GrassRegrowth();
                timer = UnityEngine.Random.Range(minInterval, maxInterval); // reset the interval timer
            }
            else
            {
                // otherwise, if the timer is greater than zero, reduce the timer by Time.deltaTime (the time in seconds since the last frame)
                timer -= Time.deltaTime; // timer counts down
            }
        }
    }

    IEnumerator WaitSeconds() // delay object generation to allow terrain to generate beforehand
    {
        yield return new WaitForSeconds(2);
        GenerateObjects();
        GenerateTrees();
        GenerateGrass();
        yield return new WaitForSeconds(10);
        canRegrow = true;
    }

    void GrassRegrowth()
    {
        // instantiate grass object & assign scale and rotation
        GameObject grassPrefab = grassPrefabs[UnityEngine.Random.Range(0, grassPrefabs.Length)];
        GameObject grass = Instantiate(grassPrefab, this.transform);
        grassTransforms.Add(grass.transform);
        grass.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);
        grass.transform.localScale = Vector3.one * 0.3f;

        // select random location near to a current grass object
        int maxIndex = grassTransforms.IndexOf(grassTransforms.Max());
        int minIndex = grassTransforms.IndexOf(grassTransforms.Min());
        int posOffset = UnityEngine.Random.Range(2, 5);
        grass.transform.position = grassTransforms[UnityEngine.Random.Range(minIndex, maxIndex)].transform.position + new Vector3 (posOffset, 20, posOffset);

        Debug.Log("grass regrown at " + grass.transform.position);
    }
}
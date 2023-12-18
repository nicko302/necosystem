using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

public class GrassRegrowth : MonoBehaviour
{
    public GameObject newGrassPrefab;
    public List<GameObject> allGrass = new List<GameObject>();
    public ObjectNoise objectNoise;

    [Header("Grass regrowth")]
    [SerializeField]
    private float timer = 0;
    [SerializeField]
    private float minInterval = 5;
    [SerializeField]
    private float maxInterval = 5;
    public bool canRegrow = false;
    GameObject grassPrefab;

    private void Update()
    {
        if (canRegrow)
        {
            if (timer <= 0)
            {
                // if timer reaches zero, a single grass will regrow
                GrassRegrow();
                timer = UnityEngine.Random.Range(minInterval, maxInterval); // reset the interval timer
            }
            else
            {
                // otherwise, if the timer is greater than zero, reduce the timer by Time.deltaTime (the time in seconds since the last frame)
                timer -= Time.deltaTime; // timer counts down
            }
        }
    }

    void GrassRegrow()
    {
        allGrass = objectNoise.allGrass;

        // instantiate grass object & assign scale and rotation
        grassPrefab = newGrassPrefab;
        var grass = (GameObject) Instantiate(grassPrefab, GameObject.Find("New grass").transform);

        allGrass.Add(grass);

        grass.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0);
        grass.transform.localScale = Vector3.one * 0.3f;

        // select random location near to a random current grass object
        int maxIndex = allGrass.IndexOf(allGrass.Last());
        int minIndex = allGrass.IndexOf(allGrass.First());
        int posOffset = UnityEngine.Random.Range(2, 5);
        GameObject grassObject = allGrass[UnityEngine.Random.Range(minIndex, maxIndex)];

        if (grassObject != null)
        {
            Vector3 grassTransform = grassObject.transform.position;

            Vector3 newPos = grassTransform + (Vector3.one * posOffset);
            grass.transform.position = newPos;
            Debug.Log("grass regrown at " + grass.transform.position);
        }
        else
        {
            Debug.Log("No grass transform");
        }
    }
}

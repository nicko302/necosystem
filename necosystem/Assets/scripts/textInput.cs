using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class textInput : MonoBehaviour
{
    public string inputText;
    public GameObject inputField;
    public GameObject islandMesh;
    public string[] fields = new string[2];
    public noiseData noiseData;
    public terrainData terrainData;
    public endlessTerrain endlessTerrain;
    public mapGenerator mapGenerator;
    public int x = -1;

    public void ReadInputSeed(string s)
    {
        inputText = s;
        
        
        if (Int32.TryParse(s, out x))
        {
            noiseData.seed = x;
            Debug.Log("seed: " + noiseData.seed);

            mapGenerator.DrawMapInEditor();
        }
        else
        {
            Debug.Log("bad");
        }
        

    }

    public void ReadInputSize(string s)
    {
        inputText = s;

        if (Int32.TryParse(s, out x))
        {
            terrainData.uniformScale = x;
            Debug.Log("size: " + terrainData.uniformScale);

            mapGenerator.DrawMapInEditor();
        }
        else
        {
            Debug.Log("bad");
        }
        
    }

    public void OnClick()
    {
        //textureData.ApplyToMaterial(terrainMaterial);
    }

    private void Start()
    {
        islandMesh.SetActive(true);
    }
}
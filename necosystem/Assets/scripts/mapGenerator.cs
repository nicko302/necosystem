using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using static UnityEngine.EventSystems.EventTrigger;

public class mapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public textureData textureData;

    public Material terrainMaterial;

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;
    public bool autoUpdate;

    float[,] falloffMap;

    void Start()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }



    public void DrawMapInEditor()
    {
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        HeightMap heightMap = heightMapGenerator.generateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);

        mapDisplay display = FindObjectOfType<mapDisplay>(); //sets display to object with mapDisplay script
        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(textureGenerator.TextureFromHeightMap(heightMap.values));
        else if (drawMode == DrawMode.Mesh)
            display.DrawMesh(meshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
        else if (drawMode == DrawMode.FalloffMap)
            display.DrawTexture(textureGenerator.TextureFromHeightMap(falloffGenerator.generateFalloffMap(meshSettings.numVertsPerLine)));
    }

    

    private void OnValidate() //called automatically when a variable is changed in inspector
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    
}
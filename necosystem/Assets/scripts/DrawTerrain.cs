using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.TerrainTools;

public class DrawTerrain : MonoBehaviour
{
    public static terrainData _terrainData;
    public static terrainData terrainData
    {
        get
        {
            if (_terrainData == null)
            {
                _terrainData = GameObject.FindObjectOfType<terrainData>();
            }
            return _terrainData;
        }
    }

    public static textureData _textureData;
    public static textureData textureData
    {
        get
        {
            if (_textureData == null)
            {
                _textureData = GameObject.FindObjectOfType<textureData>();
            }
            return _textureData;
        }
    }

    public static mapGenerator _mapGenerator;
    public static mapGenerator mapGenerator
    {
        get
        {
            if (_mapGenerator == null)
            {
                _mapGenerator = GameObject.FindObjectOfType<mapGenerator>();
            }
            return _mapGenerator;
        }
    }

    public static Material terrainMaterial;

    public static void DrawMap()
    {
        int mapChunkSize = mapGenerator.mapChunkSize;
        terrainMaterial = mapGenerator.terrainMaterial;

        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        MapData mapData = mapGenerator.GenerateMapData(Vector2.zero);
        mapDisplay display = FindObjectOfType<mapDisplay>(); //sets display to object with mapDisplay script
        display.DrawMesh(meshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, 0, terrainData.useFlatShading));
    }
}

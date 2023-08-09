using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public bool autoUpdate;

    public void generateMap()
    {
        float[,] noiseMap = noise.generateNoiseMap(mapWidth, mapHeight, noiseScale); //pass variables into noise generation function

        mapDisplay display = FindObjectOfType<mapDisplay>(); //sets display to object with mapDisplay script
        display.drawNoiseMap(noiseMap); //draws the noise map on the display object
    }
}

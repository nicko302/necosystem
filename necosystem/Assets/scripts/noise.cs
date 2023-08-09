using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class noise
{
    public static float[,] generateNoiseMap(int mapWidth, int mapHeight, float scale)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight]; //parameters required for noisemap generation

        if (scale <= 0) //catch any division errors if scale is <= 0
        {
            scale = 0.0001f;
        }

        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++) //for each x and y position
            {
                float sampleX = x / scale; //make the sample values differ from the int x and y values
                float sampleY = y / scale;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY); //generate perlinnoise from these values
                noiseMap[x, y] = perlinValue;
            }

        return noiseMap;
    }
}

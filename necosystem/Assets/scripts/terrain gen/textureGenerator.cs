using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class textureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1); //retrieve x and y values from heightMap array


        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++) //for every x and y position
            {
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]); //every row of pixels = a colour between white and black
            }
        
        return TextureFromColourMap(colourMap, width, height);
    }
}

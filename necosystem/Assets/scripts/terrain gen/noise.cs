using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public enum NormalizeMode { Local, Global };

    /// <summary>
    /// Calculates perlin noise values, taking different parameters for customisation and fine-tuning
    /// </summary>
    /// <param name="seed"> A number determining a specific point on the noisemap, used to initialise a pseudorandom number generator. </param>
    /// <param name="octaves"> How many times a function gets called to add finer detail. </param>
    /// <param name="persistance"> A multiplier to determine the rate of decrease of an amplitude (i.e. harshness of gradient). </param>
    /// <param name="lacunarity"> The change in frequency between octaves (i.e. the amount of detail). </param>
    /// <param name="offset"> The offset/position of the current section out of the entire noisemap. </param>
    /// <param name="normalizeMode"> Used to display the noise in different formats for debugging/visualisation. </param>
    /// <returns> An array of floats representing values of perlin noise. </returns>
    public static float[,] generateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight]; // parameters required for noisemap generation

        System.Random prng = new System.Random(seed); // pseudo-random number generator based off an input seed
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0) // catch any division errors if scale is <= 0
        {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f; // calculates half values to scale into centre
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++) // iterates through each x and y position
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency; // makes the sample values differ from the int x and y values, with offsets
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // generates perlin noise from these values
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }

        // normalise noisemap's x and y values between 0 and 1 for easier use
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++) // iterates through each x and y position to determine its appearance based on the normalise mode
            {
                if (normalizeMode == NormalizeMode.Local)
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                    noiseMap[x,y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
                

        return noiseMap;
    }
}

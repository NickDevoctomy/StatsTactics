using UnityEngine;

/// <summary>
/// Perlin noise map generator
/// Modified slightly to be instance based with parameters for setting
/// generator values.
/// 
/// Modified original implementation by Code2DTutorials
/// https://github.com/Venom0us/Code2DTutorials
/// </summary>
public class PerlinNoiseMapGenerator
{
    public int Octaves = 5;
    public float Scale = 25f;
    public Vector2 Offset = Vector2.zero;
    public float Persistance = 0.286f;
    public float Lacunarity = 2.9f;

    public float[,] Generate(
        int seed,
        int mapWidth,
        int mapHeight)
    {
        var noiseMap = new float[mapWidth, mapHeight];
        var random = new System.Random(seed);

        Vector2[] octaveOffsets = new Vector2[Octaves];
        for (int i = 0; i < Octaves; i++)
        {
            float offsetX = random.Next(-100000, 100000) + Offset.x;
            float offsetY = random.Next(-100000, 100000) + Offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int x = 0, y; x < mapWidth; x++)
        {
            for (y = 0; y < mapHeight; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < Octaves; i++)
                {
                    float sampleX = (x - halfWidth) / Scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / Scale * frequency + octaveOffsets[i].y;
                    float perlinValue = PerlinNoiseGenerator.Generate(sampleX, sampleY) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;
                    amplitude *= Persistance;
                    frequency *= Lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int x = 0, y; x < mapWidth; x++)
        {
            for (y = 0; y < mapHeight; y++)
            {
                noiseMap[x, y] = InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }

    private static float Clamp01(float value)
    {
        if (value < 0F)
            return 0F;
        else if (value > 1F)
            return 1F;
        else
            return value;
    }

    private static float InverseLerp(float a, float b, float value)
    {
        if (a != b)
            return Clamp01((value - a) / (b - a));
        else
            return 0.0f;
    }
}
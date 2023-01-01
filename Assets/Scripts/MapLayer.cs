using System;

[Serializable]
public struct MapLayer
{
    public string Name;
    public int Octaves;
    public float Scale;
    public float Persistance;
    public float Lacunarity;
    public float MaxHeight;
}

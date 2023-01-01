using System;
using UnityEngine;

[Serializable]
public struct MapLayer
{
    public string Name;
    public int Octaves;
    public float Scale;
    public float Persistance;
    public float Lacunarity;
    public float MaxHeight;
    public Material Material;
}

using System;
using UnityEngine;

[Serializable]
public struct TerrainLayer
{
    public bool Enabled;
    public string Name;
    public int Octaves;
    public float Scale;
    public float Persistance;
    public float Lacunarity;
    public float LowerLayerThreshold;
    public float MaxHeight;
    public Material Material;
    public Color InfoColor;
}

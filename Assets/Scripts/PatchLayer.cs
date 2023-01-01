using System;
using UnityEngine;

[Serializable]
public class PatchLayer
{
    public string Name;
    public string TerrainLayerName;
    public GameObject PatchPrefab;
    public int MinPatchCount = 1;
    public int MaxPatchCount = 1;
}
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public int Seed;
    public int Width;
    public int Height;

    public MapLayer[] Layers;

    void Start()
    {
        Generate();
    }

    void Update()
    {
        
    }

    public void Generate()
    {
        var curHeight = 0f;
        for(int i = 0; i < Layers.Length; i++)
        {
            var layer = Layers[i];
            var perlinNoiseMapGenerator = new PerlinNoiseMapGenerator();
            perlinNoiseMapGenerator.Octaves = layer.Octaves;
            perlinNoiseMapGenerator.Scale = layer.Scale;
            perlinNoiseMapGenerator.Offset = new Vector2(Width / 2, Height / 2);
            perlinNoiseMapGenerator.Persistance = layer.Persistance;
            perlinNoiseMapGenerator.Lacunarity = layer.Lacunarity;
            var mapLayer = perlinNoiseMapGenerator.Generate(
                Seed,
                Width,
                Height);
            var layerTiles = CreateMapTiles(
                layer.Name,
                mapLayer,
                curHeight,
                layer.MaxHeight);
            MergeCells(
                layer.Name,
                layerTiles,
                null,
                null);
            curHeight += layer.MaxHeight;
        }
    }

    private List<GameObject> CreateMapTiles(
        string layerName,
        float[,] layer,
        float minHeight,
        float maxHeight)
    {
        var layerTiles = new List<GameObject>();
        var tilesParentTransform = transform.Find(layerName);
        if (tilesParentTransform != null)
        {
            GameObject.DestroyImmediate(tilesParentTransform.gameObject);
        }

        for (int x = 0; x < Width; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                var height = layer[x, y];
                if(height >= minHeight)
                {
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = $"Tile-{x}-{y}";
                    height = Math.Clamp(height, minHeight, maxHeight);
                    tile.transform.SetParent(transform, false);
                    tile.transform.localScale = new Vector3(1, height, 1);
                    tile.transform.position = new Vector3(x, height / 2 + minHeight, y);
                    layerTiles.Add(tile);
                }
            }
        }

        return layerTiles;
    }

    private void MergeCells(
        string Name,
        List<GameObject> cells,
        Material material,
        string tag)
    {
        var combineInstances = cells.Select(x => CreateCombineInstanceFromGameObject(x)).ToArray();
        cells.ForEach(x => DestroyImmediate(x));

        var combined = new GameObject(Name);
        combined.transform.parent = transform;
        var meshRenderer = combined.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        var meshFilter = combined.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        meshFilter.sharedMesh.CombineMeshes(combineInstances, true);
        var meshCollider = combined.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.sharedMesh;
        combined.tag = tag;

        combined.SetActive(true);
    }

    private CombineInstance CreateCombineInstanceFromGameObject(GameObject gameObject)
    {
        var meshFilter = gameObject.GetComponent<MeshFilter>();
        return new CombineInstance
        {
            mesh = meshFilter.sharedMesh,
            transform = gameObject.transform.localToWorldMatrix
        };
    }
}

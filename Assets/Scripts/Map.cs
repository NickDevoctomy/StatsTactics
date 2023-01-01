using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public int Seed;
    public int Width = 32;
    public int Depth = 24;
    public float HeightScale = 2.0f;

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
        var layersParentTransform = transform.Find("Layers");
        if(layersParentTransform != null)
        {
            GameObject.DestroyImmediate(layersParentTransform.gameObject);
        }
        var layersParent = new GameObject("Layers");
        layersParent.transform.SetParent(transform, false);

        var layers = new Stack<float[,]>();
        var curHeight = 0f;
        for(int i = 0; i < Layers.Length; i++)
        {
            var layer = Layers[i];
            if(!layer.Enabled)
            {
                continue;
            }

            var perlinNoiseMapGenerator = new PerlinNoiseMapGenerator();
            perlinNoiseMapGenerator.Octaves = layer.Octaves;
            perlinNoiseMapGenerator.Scale = layer.Scale;
            perlinNoiseMapGenerator.Persistance = layer.Persistance;
            perlinNoiseMapGenerator.Lacunarity = layer.Lacunarity;
            var mapLayer = perlinNoiseMapGenerator.Generate(
                Seed + layers.Count,
                Width,
                Depth);

            if (layers.Count > 0)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Depth; y++)
                    {
                        var lowerHeight = layers.Peek()[x, y];
                        if (lowerHeight < layer.LowerLayerThreshold)
                        {
                            mapLayer[x, y] = 0f;
                        }
                        else
                        {
                            mapLayer[x, y] -= layer.LowerLayerThreshold;
                            if(mapLayer[x, y] < 0)
                            {
                                mapLayer[x, y] = 0f;
                            }
                        }
                    }
                }
            }
            layers.Push(mapLayer);

            var layerTiles = CreateMapTiles(
                layer.Name,
                mapLayer,
                curHeight,
                layer.MaxHeight);
            MergeCells(
                layer.Name,
                layerTiles,
                layer.Material,
                null,
                layersParent.transform);
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
            for(int y = 0; y < Depth; y++)
            {
                var height = layer[x, y];
                if(height > 0)
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
        string tag,
        Transform parent)
    {
        var combineInstances = cells.Select(x => CreateCombineInstanceFromGameObject(x)).ToArray();
        cells.ForEach(x => DestroyImmediate(x));

        var combined = new GameObject(Name);
        combined.transform.parent = parent;
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

        if(!string.IsNullOrEmpty(tag))
        {
            combined.tag = tag;
        }

        combined.transform.localScale = new Vector3(1f, HeightScale, 1f);

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

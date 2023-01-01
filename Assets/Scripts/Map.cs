using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Map : MonoBehaviour
{
    public string Seed = "Hello";
    public int Width = 32;
    public int Depth = 24;
    public MapLayer[] Layers;

    public bool ShowFinalHeightMapGizmos = true;

    private float[,] _finalHeightMap;

    void Start()
    {
        Generate();
    }

    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if(ShowFinalHeightMapGizmos && _finalHeightMap != null && _finalHeightMap.Length > 0)
        {
            Gizmos.color = Color.red;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Depth; y++)
                {
                    Gizmos.DrawSphere(new Vector3(x, _finalHeightMap[x, y], y), 0.05f);
                }
            }
        }
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

        _finalHeightMap = new float[Width, Depth];
        var layers = new Stack<float[,]>();
        for(int i = 0; i < Layers.Length; i++)
        {
            var layer = Layers[i];
            if(!layer.Enabled)
            {
                continue;
            }

            // create our current noise layer
            var perlinNoiseMapGenerator = new PerlinNoiseMapGenerator();
            perlinNoiseMapGenerator.Octaves = layer.Octaves;
            perlinNoiseMapGenerator.Scale = layer.Scale;
            perlinNoiseMapGenerator.Persistance = layer.Persistance;
            perlinNoiseMapGenerator.Lacunarity = layer.Lacunarity;
            var mapLayer = perlinNoiseMapGenerator.Generate(
                Seed + layers.Count,
                Width,
                Depth);

            var lowerLayer = layers.Count > 0 ? layers.Peek() : null;
            if (lowerLayer != null)
            {
                // compare with layer below and only keep anything above lower layer threshold
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Depth; y++)
                    {
                        var lowerHeight = lowerLayer[x, y];
                        if (lowerHeight < layer.LowerLayerThreshold)
                        {
                            mapLayer[x, y] = 0f;
                        }
                    }
                }
            }
            layers.Push(mapLayer);

            var layerTiles = CreateMapTiles(
                layer.Name,
                mapLayer,
                _finalHeightMap,
                layer.MaxHeight);
            MergeCells(
                layer.Name,
                layerTiles,
                layer.Material,
                null,
                layersParent.transform);

            // combine lower layer with previous lower layers so we know how high
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Depth; y++)
                {
                    var height = mapLayer[x, y];
                    _finalHeightMap[x, y] += height > layer.MaxHeight ? layer.MaxHeight : height;
                }
            }
        }
    }

    private List<GameObject> CreateMapTiles(
        string layerName,
        float[,] layer,
        float[,] lowerLayer,
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
                    tile.name = Guid.NewGuid().ToString();
                    height = Math.Clamp(height, 0f, maxHeight);
                    tile.transform.SetParent(transform, false);
                    tile.transform.localScale = new Vector3(1, height, 1);
                    tile.transform.position = new Vector3(x, height / 2 + lowerLayer[x, y], y);
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

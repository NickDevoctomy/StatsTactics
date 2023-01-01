using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject MapCellPrefab;

    public int Width = 32;
    public int Depth = 24;
    public TerrainLayer[] TerrainLayers;
    public PatchLayer[] PatchLayers;

    private MapCellInfo[,] _mapInfo;
    private List<GameObject> _mapCells;
    private Dictionary<string, List<GameObject>> _foliagePatches;

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

        _mapInfo = new MapCellInfo[Width, Depth];
        var layers = new Stack<float[,]>();
        for(int i = 0; i < TerrainLayers.Length; i++)
        {
            var layer = TerrainLayers[i];
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
                Randominator.Instance.Next(),
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
                _mapInfo,
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
                    _mapInfo[x, y].Height += height > layer.MaxHeight ? layer.MaxHeight : height;
                    if(height > 0)
                    {
                        _mapInfo[x, y].LayerName = layer.Name;
                        _mapInfo[x, y].Color = layer.InfoColor;
                    }
                }
            }
        }

        GenerateMapCells();
        GeneratePatches();
    }
    private void GenerateMapCells()
    {
        var callsParentTransform = transform.Find("Cells");
        if (callsParentTransform != null)
        {
            GameObject.DestroyImmediate(callsParentTransform.gameObject);
        }
        var cellsParent = new GameObject("Cells");
        cellsParent.transform.SetParent(transform, false);

        _mapCells = new List<GameObject>();
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Depth; y++)
            {
                var mapCell = GameObject.Instantiate(
                    MapCellPrefab,
                    new Vector3(x, _mapInfo[x, y].Height, y),
                    Quaternion.identity,
                    cellsParent.transform);
                mapCell.name = $"Cell-{x}_{y}";
                var cell = mapCell.GetComponent<MapCell>();
                cell.X = x;
                cell.Y = y;
                cell.Color = _mapInfo[x, y].Color;
                cell.LayerName = _mapInfo[x, y].LayerName;
                _mapCells.Add(mapCell);
            }
        }
    }

    private void GeneratePatches()
    {
        var patchesParentTransform = transform.Find("Patches");
        if (patchesParentTransform != null)
        {
            GameObject.DestroyImmediate(patchesParentTransform.gameObject);
        }
        var patchesParent = new GameObject("Patches");
        patchesParent.transform.SetParent(transform, false);

        _foliagePatches = new Dictionary<string, List<GameObject>>();

        foreach (var curPatchLayer in PatchLayers)
        {
            var layerParent = new GameObject(curPatchLayer.Name);
            layerParent.transform.SetParent(patchesParent.transform, false);

            var patchCount = Randominator.Instance.Next(
                curPatchLayer.MinPatchCount,
                curPatchLayer.MaxPatchCount);
            var patches = new List<GameObject>();
            while(patches.Count < patchCount)
            {
                var allSuitableCells = _mapCells
                    .Where(x => x.GetComponent<MapCell>().LayerName == curPatchLayer.TerrainLayerName)
                    .ToArray();
                var randomCell = allSuitableCells[Randominator.Instance.Next(0, allSuitableCells.Length)];
                var patchesWithinMinDistance = patches
                    .Where(x => Vector3.Distance(randomCell.transform.position, x.transform.position) < curPatchLayer.MinDistanceBetweenPatches)
                    .ToArray();
                while(patchesWithinMinDistance.Count() > 0)
                {
                    randomCell = allSuitableCells[Randominator.Instance.Next(0, allSuitableCells.Length)];
                    patchesWithinMinDistance = patches
                        .Where(x => Vector3.Distance(randomCell.transform.position, x.transform.position) < curPatchLayer.MinDistanceBetweenPatches)
                        .ToArray();
                }

                var patch = GameObject.Instantiate(
                    curPatchLayer.PatchPrefab,
                    randomCell.transform.position,
                    Quaternion.identity,
                    layerParent.transform);
                patch.name = $"Patch-{patches.Count}";
                patches.Add(patch);
            }

            _foliagePatches.Add(
                curPatchLayer.Name,
                patches);
        }
    }

    private List<GameObject> CreateMapTiles(
        string layerName,
        float[,] layer,
        MapCellInfo[,] mapInfo,
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
                    tile.transform.position = new Vector3(x, height / 2 + mapInfo[x, y].Height, y);
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

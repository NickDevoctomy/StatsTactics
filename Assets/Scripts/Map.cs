using UnityEngine;

public class Map : MonoBehaviour
{
    public int Seed;
    public int Width;
    public int Height;
    public int Octaves = 5;
    public float Scale = 25f;
    public float Persistance = 0.286f;
    public float Lacunarity = 2.9f;

    private PerlinNoiseMapGenerator _perlinNoiseMapGenerator = new PerlinNoiseMapGenerator();
    private float[,] _map;
    
    void Start()
    {
        Generate();
    }

    void Update()
    {
        
    }

    public void Generate()
    {
        _perlinNoiseMapGenerator.Octaves = Octaves;
        _perlinNoiseMapGenerator.Scale = Scale;
        _perlinNoiseMapGenerator.Offset = new Vector2(Width / 2, Height / 2);
        _perlinNoiseMapGenerator.Persistance = Persistance;
        _perlinNoiseMapGenerator.Lacunarity = Lacunarity;
        _map = _perlinNoiseMapGenerator.Generate(
            Seed,
            Width,
            Height);
        CreateMapTiles();
    }

    private void CreateMapTiles()
    {
        var tilesParentTransform = transform.Find("Tiles");
        if (tilesParentTransform != null)
        {
            GameObject.DestroyImmediate(tilesParentTransform.gameObject);
        }

        var tilesParent = new GameObject("Tiles");
        tilesParent.transform.parent = transform;

        for (int x = 0; x < Width; x++)
        {
            for(int y = 0; y < Height; y++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Tile-{x}-{y}";
                tile.transform.SetParent(tilesParent.transform, false);
                var height = 1 + (1 * _map[x, y]);
                tile.transform.localScale = new Vector3(1, height, 1);
                tile.transform.position = new Vector3(x, height / 2, y);
            }
        }
    }
}

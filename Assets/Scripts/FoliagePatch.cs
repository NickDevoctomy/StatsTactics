using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoliagePatch : MonoBehaviour
{
    public GameObject[] FoliagePrefabs;
    public int MinInstanceCount = 1;
    public int MaxInstanceCount = 1;
    public float MinScale = 1;
    public float MaxScale = 1;

    public float Radius;
    public string TerrainLayerName;

    private List<GameObject> _instances = new List<GameObject>();
    private int _prefabInstanceCount;
    private Vector3 _lastLocation = Vector3.zero;

   
    void Start()
    {
        _prefabInstanceCount = Randominator.Instance.Next(MinInstanceCount, MaxInstanceCount);
        CreateInstances();
    }

    void Update()
    {
        if(_lastLocation != transform.position)
        {
            DistributeInstances();
            _lastLocation = transform.position;
        }
    }

    private void CreateInstances()
    {
        while (_instances.Count < _prefabInstanceCount)
        {
            var instance = GameObject.Instantiate(
                FoliagePrefabs[Randominator.Instance.Next(0, FoliagePrefabs.Length)],
                transform);
            instance.name = $"FoliageInstance";
            instance.SetActive(false);
            _instances.Add(instance);
        }
    }

    private void DistributeInstances()
    {
        var affectedCells = Physics.OverlapSphere(transform.position, Radius);
        var cells = affectedCells
            .Where(x => x.gameObject.tag == "MapCell")
            .Select(y => y.gameObject)
            .Where(x => TerrainLayerName.Contains(x.GetComponent<MapCell>().LayerName))
            .ToArray();
        if(cells.Length > 0)
        {
            foreach (var curInstance in _instances)
            {
                var cell = cells[Randominator.Instance.Next(0, cells.Length)];
                var rotation = Randominator.Instance.Next(0, 360);
                curInstance.transform.localRotation = Quaternion.Euler(0, rotation, 0);
                var scale = Randominator.Instance.Next(MinScale, MaxScale);
                curInstance.transform.localScale = new Vector3(
                    scale,
                    scale,
                    scale);
                curInstance.transform.position = new Vector3(
                    cell.transform.position.x + Randominator.Instance.Next(-0.5f, 0.5f),
                    cell.transform.position.y,
                    cell.transform.position.z + Randominator.Instance.Next(-0.5f, 0.5f));

                curInstance.SetActive(true);
            }
        }
        else
        {
            _instances.ForEach(x => x.SetActive(false));
        }    
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, Radius);

        var affectedCells = Physics.OverlapSphere(transform.position, Radius);
        var cells = affectedCells
            .Where(x => x.gameObject.tag == "MapCell")
            .Select(y => y.gameObject)
            .Where(x => x.GetComponent<MapCell>().LayerName == TerrainLayerName)
            .ToArray();
        foreach (var curCell in cells)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(curCell.transform.position, 0.05f);
        }
    }
}

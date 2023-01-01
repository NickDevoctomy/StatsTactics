using System.Linq;
using UnityEngine;

public class FoliagePatch : MonoBehaviour
{
    public float Radius;
    public string TerrainLayerName;

   
    void Start()
    {
        
    }

    void Update()
    {
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, Radius);

        var affectedCells = Physics.OverlapSphere(transform.position, Radius);
        var cells = affectedCells.Where(x => x.gameObject.tag == "MapCell").Select(y => y.gameObject).ToList();
        foreach(var curCell in cells)
        {
            var cell = curCell.GetComponent<MapCell>();
            if(TerrainLayerName == cell.LayerName)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(curCell.transform.position, 0.05f);
            }
        }
    }
}

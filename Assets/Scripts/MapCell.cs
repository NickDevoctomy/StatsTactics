using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCell : MonoBehaviour
{
    public int X;
    public int Y;
    public string LayerName;
    public Color Color;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color;
        Gizmos.DrawSphere(
            transform.position,
            0.05f);
    }
}

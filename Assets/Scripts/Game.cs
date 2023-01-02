using UnityEngine;

public class Game : MonoBehaviour
{
    public string Seed;

    private Map _map;

    void Start()
    {
        _map = GetComponent<Map>();
        Initialise();
    }

    void Update()
    {
        
    }

    public void Initialise()
    {
        Randominator.Initialise(Seed);

        if(_map == null)
        {
            // Need this so initialise button works in editor
            _map = GetComponent<Map>();
        }
        _map.Generate();
    }
}

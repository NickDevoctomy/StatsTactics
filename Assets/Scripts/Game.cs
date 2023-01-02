using UnityEngine;

public class Game : MonoBehaviour
{
    public string Seed;

    void Start()
    {
        Initialise();
    }

    void Update()
    {
        
    }

    public void Initialise()
    {
        Randominator.Initialise(Seed);

        var map = GetComponent<Map>();
        map.Generate();
    }
}

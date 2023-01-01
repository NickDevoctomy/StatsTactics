using UnityEngine;

public class Game : MonoBehaviour
{
    public string Seed;

    void Start()
    {
        Randominator.Initialise(Seed);
    }

    void Update()
    {
        
    }
}

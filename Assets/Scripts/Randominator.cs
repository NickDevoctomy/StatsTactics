using System.Threading;
using UnityEngine;

public class Randominator
{
    public static Randominator Instance
    {
        get
        {
            return _instance;
        }
    }

    private static Randominator _instance;

    private System.Random _random;
    private int _index;

    private Randominator(string seed)
    {
        _random = new System.Random(seed.GetHashCode());
    }

    public int Next()
    {
        var value = _random.Next();
        //Debug.Log($"{Interlocked.Increment(ref _index)} :: {value}");
        return value;
    }

    public int Next(
        int min,
        int max)
    {
        var value = _random.Next(min, max);
        //Debug.Log($"{Interlocked.Increment(ref _index)} :: {value}");
        return value;
    }

    public float Next(
        float min,
        float max)
    {
        var ratio = (float)_random.NextDouble();
        var total = max - min;
        var value = min + (total * ratio);
        //Debug.Log($"{Interlocked.Increment(ref _index)} :: {value}");
        return value;
    }

    public static void Initialise(string seed)
    {
        _instance = new Randominator(seed);
    }
}

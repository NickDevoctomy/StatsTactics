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

    private Randominator(string seed)
    {
        _random = new System.Random(seed.GetHashCode());
    }

    public int Next()
    {
        return _random.Next();
    }

    public static void Initialise(string seed)
    {
        _instance = new Randominator(seed);
    }
}

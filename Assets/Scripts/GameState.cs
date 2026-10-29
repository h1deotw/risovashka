using System.Collections.Generic;

public static class GameState
{
    private static Dictionary<string, bool> состояния = new Dictionary<string, bool>();

    public static void Set(string ключ, bool значение)
    {
        if (состояния.ContainsKey(ключ))
            состояния[ключ] = значение;
        else
            состояния.Add(ключ, значение);
    }

    public static bool Get(string ключ)
    {
        if (состояния.ContainsKey(ключ))
            return состояния[ключ];
        return false;
    }

    public static bool Has(string ключ)
    {
        return состояния.ContainsKey(ключ);
    }

    public static void Clear()
    {
        состояния.Clear();
    }
}
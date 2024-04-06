namespace RuntimePlugin;

public static class StrtingExtendesion
{
    public static string GetIdent(this int identLevel)
    {
        if (identLevel == 0)
            return string.Empty;

        return new string(' ', identLevel * 4);
    }
}
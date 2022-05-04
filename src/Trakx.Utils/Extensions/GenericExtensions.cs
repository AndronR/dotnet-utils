namespace Trakx.Utils.Extensions;

public static class GenericExtensions
{
    public static List<T> AsSingletonList<T>(this T item)
    {
        return new List<T> {item};
    }

    public static T[] AsSingletonArray<T>(this T item)
    {
        return new [] { item };
    }

    public static IEnumerable<T> AsSingletonIEnumerable<T>(this T item)
    {
        return Enumerable.Repeat(item, 1);
    }
}
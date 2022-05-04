#pragma warning disable IDE0046 // Convert to conditional expression

namespace Trakx.Utils.Comparers;

public class SelectorComparer<T, TKey> : IEqualityComparer<T?>
{
    private readonly Func<T?, TKey?> _selector;

    public SelectorComparer(Func<T?, TKey?> selector)
    {
        _selector = selector;
    }

    public bool Equals(T? x, T? y)
    {
        var leftProp = _selector(x);
        var rightProp = _selector(y);
        if (leftProp == null && rightProp == null)
            return true;
        if (leftProp == null ^ rightProp == null)
            return false;
        return leftProp!.Equals(rightProp!);
    }

    public int GetHashCode(T? obj)
    {
        var prop = _selector.Invoke(obj);
        return prop == null ? 0 : prop.GetHashCode();
    }
}
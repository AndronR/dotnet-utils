using System;
using System.Collections.Generic;

namespace Trakx.Utils.Extensions;

public static class SymbolMappingExtensions
{
    private static readonly Dictionary<string, string> NativeToWrapped = 
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            {"btc", "wbtc"},
            {"eth", "weth"}
        };
    private static readonly Dictionary<string, string> WrappedToNative 
        = new(StringComparer.InvariantCultureIgnoreCase)
        {
            {"wbtc", "btc"},
            {"weth", "eth"}
        };

    public static string ToNativeSymbol(this string symbol)
    {
        return WrappedToNative.TryGetValue(symbol, out var native) ? native : symbol;
    }
    public static string ToWrappedSymbol(this string symbol)
    {
        return NativeToWrapped.TryGetValue(symbol, out var wrapped) ? wrapped : symbol;
    }
}
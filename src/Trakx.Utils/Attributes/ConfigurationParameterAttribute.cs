using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Trakx.Utils.Attributes;

public abstract class ConfigurationParameterAttribute : Attribute
{
    protected abstract string GetConfigPath(string className, string propertyName);

    public static List<string?> GetConfigPathsFromConfigTypes<T>(List<Type> configTypes, string prefix, string postfix)
        where T : ConfigurationParameterAttribute
    {
        var secrets = configTypes
            .SelectMany(t => t.GetProperties()
                .Select(p => p.GetCustomAttribute(typeof(T)) is T
                        attribute
                        ? prefix + attribute.GetConfigPath(t.Name, p.Name) + postfix
                        : null))
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
        return secrets;
    }
}

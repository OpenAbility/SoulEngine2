using System.Reflection;
using OpenAbility.Logging;
using SoulEngine.Core;
using SoulEngine.Util;
using BindingFlags = System.Reflection.BindingFlags;

namespace SoulEngine.Props;

/// <summary>
/// Handles loading and creation of props
/// </summary>
public static class DirectorLoader
{
    private static readonly Logger Logger = Logger.Get("DirectorLoader");

    private static readonly Dictionary<string, DirectorFactory> Factories = new Dictionary<string, DirectorFactory>();
    
    static DirectorLoader()
    {
        foreach (var type in EngineUtility.GetAllTypeAttributes<DirectorAttribute>())
        {
            if (!type.Type.IsAssignableTo(typeof(Director)))
            {
                Logger.Error("Registered director attribute at non-director type '{}'", type.Type);
                continue;
            }

            ConstructorInfo? ctor = type.Type.GetConstructor(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                [typeof(Scene), typeof(string)]);

            if (ctor == null)
            {
                Logger.Error("Director type '{}' does not expose valid constructor!", type.Type);
                continue;
            }

            Factories[type.Attribute.ID] = (scene, typeId) => (Director)ctor.Invoke([scene, typeId]);
        }
    }

    public static IEnumerable<string> Types => Factories.Keys;

    /// <summary>
    /// Creates a prop
    /// </summary>
    /// <param name="scene">The scene that owns the prop</param>
    /// <param name="type">The prop type to use</param>
    /// <param name="name">The name of the prop</param>
    /// <returns>The created prop</returns>
    public static Director Create(Scene scene, string type)
    {
        if (!Factories.TryGetValue(type, out var factory))
            throw new Exception("Invalid prop type " + type);
        return factory.Invoke(scene, type);
    }

    /// <summary>
    /// Register a prop factory
    /// </summary>
    /// <param name="id">The prop tyoe</param>
    /// <param name="factory">The prop factory</param>
    public static void RegisterFactory(string id, DirectorFactory factory)
    {
        Factories[id] = factory;
    }
}

public delegate Director DirectorFactory(Scene scene, string type);
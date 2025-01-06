using System.Reflection;
using OpenAbility.Logging;
using SoulEngine.Core;
using SoulEngine.Util;
using BindingFlags = System.Reflection.BindingFlags;

namespace SoulEngine.Props;

/// <summary>
/// Handles loading and creation of props
/// </summary>
public static class PropLoader
{
    private static readonly Logger Logger = Logger.Get("PropLoader");

    private static readonly Dictionary<string, PropFactory> Factories = new Dictionary<string, PropFactory>();
    
    static PropLoader()
    {
        foreach (var type in EngineUtility.GetAllTypeAttributes<PropAttribute>())
        {
            if (!type.Type.IsAssignableTo(typeof(Prop)))
            {
                Logger.Error("Registered prop attribute at non-prop type '{}'", type.Type);
                continue;
            }

            ConstructorInfo? ctor = type.Type.GetConstructor(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                [typeof(Scene), typeof(string), typeof(string)]);

            if (ctor == null)
            {
                Logger.Error("Prop type '{}' does not expose constructor!", type.Type);
                continue;
            }

            Factories[type.Attribute.ID] = (scene, typeId, name) => (Prop)ctor.Invoke([scene, typeId, name]);
        }
    }

    /// <summary>
    /// Creates a prop
    /// </summary>
    /// <param name="scene">The scene that owns the prop</param>
    /// <param name="type">The prop type to use</param>
    /// <param name="name">The name of the prop</param>
    /// <returns>The created prop</returns>
    public static Prop Create(Scene scene, string type, string name)
    {
        if (!Factories.TryGetValue(type, out var factory))
            throw new Exception("Invalid prop type " + type);
        return factory.Invoke(scene, type, name);
    }

    /// <summary>
    /// Register a prop factory
    /// </summary>
    /// <param name="id">The prop tyoe</param>
    /// <param name="factory">The prop factory</param>
    public static void RegisterFactory(string id, PropFactory factory)
    {
        Factories[id] = factory;
    }
}

public delegate Prop PropFactory(Scene scene, string type, string name);
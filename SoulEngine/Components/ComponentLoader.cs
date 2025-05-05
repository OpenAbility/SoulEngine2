using System.Collections;
using System.Reflection;
using OpenAbility.Logging;
using SoulEngine.Components;
using SoulEngine.Core;
using SoulEngine.Entities;
using SoulEngine.Util;
using BindingFlags = System.Reflection.BindingFlags;

namespace SoulEngine.Props;

/// <summary>
/// Handles loading and creation of components
/// </summary>
public static class ComponentLoader
{
    private static readonly Logger Logger = Logger.Get("ComponentLoader");

    private static readonly Dictionary<string, ComponentFactory> Factories = new Dictionary<string, ComponentFactory>();
    
    static ComponentLoader()
    {
        foreach (var type in EngineUtility.GetAllTypeAttributes<ComponentAttribute>())
        {
            if (!type.Type.IsAssignableTo(typeof(Component)))
            {
                Logger.Error("Registered component attribute at non-component type '{}'", type.Type);
                continue;
            }

            ConstructorInfo? ctor = type.Type.GetConstructor(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                [typeof(Entity)]);

            if (ctor == null)
            {
                Logger.Error("Component type '{}' does not expose constructor!", type.Type);
                continue;
            }

            Factories[type.Attribute.ID] = (entity, _) => (Component)ctor.Invoke([entity]);
        }
    }

    public static IEnumerable<string> Types => Factories.Keys;
    
    public static Component Create(Entity entity, string type)
    {
        if (!Factories.TryGetValue(type, out var factory))
            throw new Exception("Invalid component type " + type);
        return factory.Invoke(entity, type);
    }
    
    public static void RegisterFactory(string id, ComponentFactory factory)
    {
        Factories[id] = factory;
    }
}

public delegate Component ComponentFactory(Entity entity, string type);
using SoulEngine.Components;
using SoulEngine.Core;
using SoulEngine.Resources;

namespace SoulEngine.Entities;

public static class EntityTemplateFactory
{
    private static readonly Dictionary<string, EntityFactoryInitializer> Initializers = new ();

    static EntityTemplateFactory()
    {
        Register("entity", (_, name, scene) => scene.AddEntity(name));
        
        Register("dynamic_model", (_, name, scene) =>
        {
            Entity entity = scene.AddEntity(name);
            entity.Attach(new DynamicModelComponent(entity));
            return entity;
        });
        
        Register("static_model", (_, name, scene) =>
        {
            Entity entity = scene.AddEntity(name);
            entity.Attach(new StaticModelComponent(entity));
            return entity;
        });
    }
    
    public static void Register(string name, EntityFactoryInitializer initializer)
    {
        Initializers[name] = initializer;
    }

    public static IEnumerable<string> TemplateNames => Initializers.Keys;

    public static Entity? Initialize(string templateName, string name, Scene scene)
    {
        if (Initializers.TryGetValue(templateName, out var initializer))
        {
            return initializer(templateName, name, scene);
        }

        return null;
    }
}

public delegate Entity EntityFactoryInitializer(string type, string name, Scene scene);
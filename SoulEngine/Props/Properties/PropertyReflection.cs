using System.Reflection;
using SoulEngine.Core;

namespace SoulEngine.Props;

public static class PropertyReflection
{
    public static void RegisterProperties(Scene scene, object target, Action<SerializedProperty> adder)
    {

        Type type = target.GetType();
        
        foreach (PropertyInfo property in type.GetProperties())
        {
            SerializedPropertyAttribute? attribute = property.GetCustomAttribute<SerializedPropertyAttribute>();
            if(attribute == null)
                continue;
            
            adder(new AutomaticProperty(scene, attribute.ID, property, target));
        }
    }
}
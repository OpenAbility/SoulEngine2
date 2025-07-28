using System.Reflection;
using SoulEngine.Core;
using SoulEngine.Resources;
using SoulEngine.Util;

namespace SoulEngine.Props;

public static class PropertyReflection
{
    public static void RegisterProperties(Scene scene, object target, Action<SerializedProperty> adder)
    {

        Type type = target.GetType();
        
        foreach (MemberWrapper wrapper in MemberWrapper.GetWrappers(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy))
        {
            SerializedPropertyAttribute? attribute = wrapper.MemberInfo.GetCustomAttribute<SerializedPropertyAttribute>();
            if(attribute == null)
                continue;

            if (wrapper.MemberType.IsAssignableTo(typeof(Resource)))
            {

                Type propertyType = typeof(AutomaticResourceProperty<>).MakeGenericType(wrapper.MemberType);
                ConstructorInfo constructor =
                    propertyType.GetConstructor([typeof(string), typeof(MemberWrapper), typeof(object)])!;

                SerializedProperty prop = (SerializedProperty)constructor.Invoke([attribute.ID, wrapper, target]);
                adder(prop);
                continue;
            }
            
            adder(new AutomaticProperty(scene, attribute.ID, wrapper, target));
        }
    }
}
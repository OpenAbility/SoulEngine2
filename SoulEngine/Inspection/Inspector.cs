
using SoulEngine.Core;
using SoulEngine.Util;

namespace SoulEngine.Inspection;

public abstract class Inspector
{

    private static readonly Dictionary<Type, Inspector> inspectors = new Dictionary<Type, Inspector>();
    
    static Inspector()
    {
        foreach (var type in EngineUtility.GetAllTypeAttributes<InspectorAttribute>())
        {
            if(!type.Type.IsAssignableTo(typeof(Inspector)))
                continue;
            
            if(!type.Type.CanInstance())
                continue;
            
            inspectors[type.Attribute.Type] = type.Type.Instantiate<Inspector>()!;
        }
    }

    private static Inspector GetInspector(Type type)
    {
        if(inspectors.TryGetValue(type, out Inspector? inspector))
            return inspector;
        
        int lowest = Int32.MaxValue;
        foreach (var existing in inspectors)
        {
            int level = EngineUtility.GetInheritanceLevel(type, existing.Key);
            if(level == -1)
                continue;

            if (level < lowest)
            {
                lowest = level;
                inspector = existing.Value;
            }
        }

        if (inspector == null)
            throw new Exception("Unable to fit existing inspector to type " + type);

        inspectors[type] = inspector;
        return inspector;
    }
    
    public abstract object? Edit(object? instance, InspectionContext context);

    public static T? Inspect<T>(T? instance, string? associatedName, out bool edited)
    {
        return (T?)Inspect(typeof(T), instance, associatedName, out edited);
    }
    
    public static object? Inspect(object instance, string? associatedName, out bool edited)
    {
        return Inspect(instance.GetType(), instance, associatedName, out edited);
    }
    
    public static object? Inspect(Type type, object? instance, string? associatedName, out bool edited)
    {
        InspectionContext inspectionContext = new InspectionContext(null, associatedName, type);
        object? value = GetInspector(type).Edit(instance, inspectionContext);
        edited = inspectionContext.Edited;
        return value;
    }
    
    public static object? Inspect(Scene scene, Type type, object? instance, string? associatedName, out bool edited)
    {
        InspectionContext inspectionContext = new InspectionContext(scene, associatedName, type);
        object? value = GetInspector(type).Edit(instance, inspectionContext);
        edited = inspectionContext.Edited;
        return value;
    }
}

public abstract class Inspector<T> : Inspector
{
    public override object? Edit(object? instance, InspectionContext context)
    {
        return Edit((T?)instance, context);
    }
    
    public abstract T? Edit(T? instance, InspectionContext context);
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InspectorAttribute(Type type) : Attribute
{
    public readonly Type Type = type;
}
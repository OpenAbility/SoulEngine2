namespace SoulEngine.Props;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ComponentAttribute : Attribute
{
    public readonly string ID;

    public ComponentAttribute(string id)
    {
        ID = id;
    }
}
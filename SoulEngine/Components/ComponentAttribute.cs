namespace SoulEngine.Components;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ComponentAttribute : Attribute
{
    public readonly string ID;
    public string Icon = "none";

    public ComponentAttribute(string id)
    {
        ID = id;
    }
}
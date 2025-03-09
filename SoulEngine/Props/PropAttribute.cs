namespace SoulEngine.Props;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class PropAttribute : Attribute
{
    public readonly string ID;
    public string Icon = "object";

    public PropAttribute(string id)
    {
        ID = id;
    }
}
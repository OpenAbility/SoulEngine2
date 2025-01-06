namespace SoulEngine.Props;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class PropAttribute : Attribute
{
    public readonly string ID;

    public PropAttribute(string id)
    {
        ID = id;
    }
}
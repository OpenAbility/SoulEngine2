namespace SoulEngine.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class DirectorAttribute : Attribute
{
    public readonly string ID;

    public DirectorAttribute(string id)
    {
        ID = id;
    }
}
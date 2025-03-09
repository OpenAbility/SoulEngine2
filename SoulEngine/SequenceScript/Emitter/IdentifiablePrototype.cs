namespace SoulEngine.SequenceScript.Emitter;

public abstract class IdentifiablePrototype
{

    public readonly string Name;
    public readonly string PackageName;

    public IdentifiablePrototype(string name, string packageName)
    {
        Name = name;
        PackageName = packageName;
    }

}
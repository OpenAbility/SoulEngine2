namespace SoulEngine.SequenceScript.Machine;

public interface IModuleResolver
{
    public Stream LoadModule(string resolvePath);
}
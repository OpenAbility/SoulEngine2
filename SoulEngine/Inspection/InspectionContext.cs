using SoulEngine.Core;

namespace SoulEngine.Inspection;

public class InspectionContext
{
    public readonly Scene? Scene;
    public readonly string? AssociatedName;
    public readonly Type Type;

    public InspectionContext(Scene? scene, string? associatedName, Type type)
    {
        Scene = scene;
        AssociatedName = associatedName;
        Edited = false;
        Type = type;
    }

    public bool Edited { get; private set; }

    public void MarkEdited()
    {
        Edited = true;
    }
}
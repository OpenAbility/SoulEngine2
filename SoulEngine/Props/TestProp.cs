using SoulEngine.Core;

namespace SoulEngine.Props;

public class TestProp : Prop
{
    public TestProp(Scene scene, string type, string name) : base(scene, type, name)
    {
    }

    public new void Register<T>(T property) where T : PropProperty
    {
        base.Register(property);
    }
}
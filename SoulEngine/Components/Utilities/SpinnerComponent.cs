using OpenTK.Mathematics;
using SoulEngine.Entities;

namespace SoulEngine.Components.Utilities;

[Component("util_spinner")]
public class SpinnerComponent(Entity entity) : Component(entity)
{
    [SerializedProperty("speed")] public Vector3 Speed;
    
    public override void Update(float deltaTime)
    {
        Entity.Rotate(Speed);
    }
}
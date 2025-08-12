using SoulEngine.Entities;

namespace SoulEngine.Components.Lighting;

public abstract class LightComponent : Component
{
    protected LightComponent(Entity entity) : base(entity)
    {
    }
}
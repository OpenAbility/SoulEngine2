using SoulEngine.Components;
using SoulEngine.Entities;

namespace SoulEngine.Core;

public interface IEntityCollection
{
    public IEnumerable<Entity> EntityEnumerable { get; }
    
    public CameraComponent? Camera { get; }
    
    
    
    public IEnumerable<T> GetComponents<T>() where T : Component
    {
        return EntityEnumerable.SelectMany(e => e.GetComponents<T>());
    }
}
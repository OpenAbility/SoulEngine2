using SoulEngine.Components;
using SoulEngine.Content;
using SoulEngine.Data.NBT;
using SoulEngine.Entities;
using SoulEngine.Props;
using SoulEngine.Resources;

namespace SoulEngine.Core;

public class Scene : IEntityCollection
{

    public bool Running = true;
    public readonly Game Game;

    public Director? Director;

    public List<Entity> Entities = new List<Entity>();
    
    
    public IEnumerable<Entity> EntityEnumerable => Entities;
    
    public CameraComponent? Camera => GetComponents<CameraComponent>().OrderDescending().FirstOrDefault();
    

    public Scene(Game game)
    {
        Game = game;
    }

    public void Reset()
    {
        Director?.Reset();
        
        foreach (var entity in Entities)
        {
            entity.Reset();
        }
    }

    public void Update(float deltaTime)
    {
        if(!Running)
            return;
        
        Director?.Update(deltaTime);

        foreach (var entity in Entities)
        {
            entity.Update(deltaTime);
        }
    }
    
    public Entity AddEntity(string name)
    {
        Entity entity = new Entity(null!, name);
        AddEntity(entity);
        return entity;
    }

    public void AddEntity(Entity entity)
    {
        if(entity.Scene == this)
            return;
        if(entity.Scene != null!)
            entity.Scene.DeleteEntity(entity);
        
        entity.Scene = this;
        Entities.Add(entity);
        entity.EnterScene();
    }

    public CompoundTag Write()
    {
        CompoundTag tag = new CompoundTag("scene");

        CompoundTag entitesTag = new CompoundTag("entities");
        tag.Add(entitesTag);

        foreach (var entity in Entities)
        {
            entitesTag[entity.Name] = entity.Save();
        }

        if(Director != null)
            tag["director"] = Director.Save();

        return tag;
    }


    public void DeleteEntity(string name)
    {
        Entity? e = GetEntity(name);
        if(e != null)
            DeleteEntity(e);
    }

    public void DeleteEntity(Entity entity)
    {
        entity.LeaveScene();
        Entities.Remove(entity);
    }

    public Entity? GetEntity(string name)
    {
        return Entities.Find(e => e.Name == name);
    }
    
    public bool HasEntity(Entity entity)
    {
        return Entities.Contains(entity);
    }
    
    public IEnumerable<T> GetComponents<T>() where T : Component
    {
        return EntityEnumerable.SelectMany(e => e.GetComponents<T>());
    }


}
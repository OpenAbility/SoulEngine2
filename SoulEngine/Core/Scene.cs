using SoulEngine.Components;
using SoulEngine.Content;
using SoulEngine.Data.NBT;
using SoulEngine.Entities;
using SoulEngine.Props;
using SoulEngine.Resources;

namespace SoulEngine.Core;

[Resource("e.scn", typeof(Loader))]
[ExpectedExtensions(".scene")]
public class Scene : Resource, IEntityCollection
{

    public bool Running = true;
    public readonly Game Game;

    public Director? Director;

    public new List<Entity> Entities = new List<Entity>();
    
    
    public IEnumerable<Entity> EntityEnumerable => Entities;
    
    public CameraComponent? Camera => GetComponents<CameraComponent>().OrderDescending().FirstOrDefault();
    public ShadowCameraComponent? ShadowCamera => GetComponents<ShadowCameraComponent>().FirstOrDefault();
    

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
    
    
    public class Loader : IResourceLoader<Scene>
    {

        public static Scene Load(Game game, CompoundTag sceneTag)
        {
            Scene scene = new Scene(game);
            
            CompoundTag? entitiesTag = sceneTag.GetTag<CompoundTag>("entities");

            if (entitiesTag != null)
            {
                foreach (string name in entitiesTag.Keys)
                {
                    Entity entity = new Entity(scene, name);
                    scene.Entities.Add(entity);
                }
                
                foreach (var entity in scene.Entities)
                {
                    CompoundTag entityTag = (CompoundTag)entitiesTag[entity.Name];
                    entity.Load(entityTag);
                }
            }
            
            CompoundTag? directorTag = sceneTag.GetTag<CompoundTag>("director");
            if(directorTag != null) {
                string directorType = directorTag.GetString("$_type")!;

                Director director = DirectorLoader.Create(scene, directorType);
                scene.Director = director;
                
                director.Load(directorTag);
            }

            return scene;   
        }
        
        public Scene LoadResource(ResourceData data)
        {
            return Load(data.ResourceManager.Game, (CompoundTag)TagIO.ReadCompressed(data.ResourceStream, false));
        }
    }



    public Entity AddEntity(string name)
    {
        Entity entity = new Entity(this, name);
        Entities.Add(entity);
        return entity;
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
        Entities.RemoveAll(e => e.Name == name);
    }
    

    public Entity? GetEntity(string name)
    {
        return Entities.Find(e => e.Name == name);
    }
    
    public IEnumerable<T> GetComponents<T>() where T : Component
    {
        return EntityEnumerable.SelectMany(e => e.GetComponents<T>());
    }

}
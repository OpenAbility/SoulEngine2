using SoulEngine.Core;
using SoulEngine.Data.NBT;
using SoulEngine.Entities;
using SoulEngine.Props;

namespace SoulEngine.Resources;

[Resource("e.scn", typeof(Loader))]
[ExpectedExtensions(".scene")]
public class SceneInfo : Resource
{

    private readonly CompoundTag tag;
    private readonly Game game;

    public SceneInfo(Game game, CompoundTag tag)
    {
        this.tag = tag;
        this.game = game;
    }

    public Scene Instantiate()
    {
        Scene scene = new Scene(game);
            
        CompoundTag? entitiesTag = tag.GetTag<CompoundTag>("entities");

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
            
        CompoundTag? directorTag = tag.GetTag<CompoundTag>("director");
        if(directorTag != null) {
            string directorType = directorTag.GetString("$_type")!;

            Director director = DirectorLoader.Create(scene, directorType);
            scene.Director = director;
                
            director.Load(directorTag);
        }

        return scene;
    }


    public class Loader : IResourceLoader<SceneInfo>
    {
        public SceneInfo LoadResource(ResourceData data)
        {
            return new SceneInfo(data.ResourceManager.Game, (CompoundTag)TagIO.ReadCompressed(data.ResourceStream, false));
        }
    }
}
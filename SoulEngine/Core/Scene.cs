using SoulEngine.Content;
using SoulEngine.Data.NBT;
using SoulEngine.Props;
using SoulEngine.Resources;

namespace SoulEngine.Core;

[Resource(typeof(Loader))]
public class Scene : Resource
{

    public bool Running = true;
    public readonly Game Game;

    public Director? Director;

    public List<Prop> Props = new List<Prop>();

    public Scene(Game game)
    {
        Game = game;
    }

    public void Reset()
    {
        Director?.Reset();
        
        foreach (var prop in Props)
        {
            prop.Reset();
        }
    }

    public void Update(float deltaTime)
    {
        if(!Running)
            return;
        
        Director?.Update(deltaTime);

        foreach (var prop in Props)
        {
            prop.Update(deltaTime);
        }
    }
    
    
    public class Loader : IResourceLoader<Scene>
    {

        public static Scene Load(Game game, CompoundTag sceneTag)
        {
            Scene scene = new Scene(game);
            
            CompoundTag propsTag = (CompoundTag)sceneTag["props"];

            foreach (string name in propsTag.Keys)
            {
                CompoundTag propTag = (CompoundTag)propsTag[name];

                string propType = propTag.GetString("$_type")!;

                Prop prop = PropLoader.Create(scene, propType, name);
            
                prop.Load(propTag);
            
                scene.Props.Add(prop);
            }
            
            CompoundTag directorTag = (CompoundTag)sceneTag["director"];
            {
                string directorType = directorTag.GetString("$_type")!;

                Director director = DirectorLoader.Create(scene, directorType);
            
                director.Load(directorTag);

                scene.Director = director;
            }

            return scene;   
        }
        
        public Scene LoadResource(ResourceManager resourceManager, string id, ContentContext content)
        {
            return Load(resourceManager.Game, (CompoundTag)TagIO.ReadCompressed(content.Load(id)!, false));
        }
    }

    public CameraProp? Camera => Props.FirstOrDefault(p => p is CameraProp) as CameraProp;

    public void AddProp(string type, string name)
    {
        Prop prop = PropLoader.Create(this, type, name);
        Props.Add(prop);
    }

    public CompoundTag Write()
    {
        CompoundTag tag = new CompoundTag("scene");

        CompoundTag propsTag = new CompoundTag("props");
        tag.Add(propsTag);

        foreach (var prop in Props)
        {
            propsTag[prop.Name] = prop.Save();
        }

        if(Director != null)
            tag["director"] = Director.Save();

        return tag;
    }
}
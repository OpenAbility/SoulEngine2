using SoulEngine.Content;
using SoulEngine.Data.NBT;
using SoulEngine.Props;
using SoulEngine.Resources;

namespace SoulEngine.Core;

public class Scene : Resource
{

    public bool Running = true;
    public readonly Game Game;

    public Director? Director;

    public Dictionary<string, Prop> Props = new Dictionary<string, Prop>();

    public Scene(Game game)
    {
        Game = game;
    }

    public void Reset()
    {
        Director?.Reset();
        
        foreach (var prop in Props.Values)
        {
            prop.Reset();
        }
    }

    public void Update(float deltaTime)
    {
        if(!Running)
            return;
        
        Director?.Update(deltaTime);

        foreach (var prop in Props.Values)
        {
            prop.Update(deltaTime);
        }
    }
    
    
    public override Task Load(ResourceManager resourceManager, string id, ContentContext content)
    {
        Props.Clear();
        
        CompoundTag sceneTag = (CompoundTag)TagIO.ReadCompressed(content.Load(id)!, false);

        CompoundTag propsTag = (CompoundTag)sceneTag["props"];

        foreach (string name in propsTag.Keys)
        {
            CompoundTag propTag = (CompoundTag)propsTag[name];

            string propType = propTag.GetString("$_type")!;

            Prop prop = PropLoader.Create(this, propType, name);
            
            prop.Load(propTag);
            
            Props.Add(prop.Name, prop);
        }

        return Task.CompletedTask;
    }
}
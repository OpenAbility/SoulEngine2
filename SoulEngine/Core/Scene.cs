using SoulEngine.Content;
using SoulEngine.Resources;

namespace SoulEngine.Core;

public class Scene : Resource
{

    public void Update(float deltaTime)
    {
        
    }
    
    public override Task Load(ResourceManager resourceManager, string id, ContentContext content)
    {


        return Task.CompletedTask;
    }
}
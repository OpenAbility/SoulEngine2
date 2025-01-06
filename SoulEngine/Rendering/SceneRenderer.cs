using SoulEngine.Core;

namespace SoulEngine.Rendering;

/// <summary>
/// Renders a scene
/// </summary>
public class SceneRenderer
{
    public readonly Scene Scene;

    public SceneRenderer(Scene scene)
    {
        Scene = scene;
    }

    public void Render(IRenderSurface surface, float deltaTime)
    {
        surface.BindFramebuffer();

        foreach (var prop in Scene.Props.Values)
        {
            prop.Render(deltaTime);
        }
        
    }
}
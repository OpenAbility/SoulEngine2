using System.Numerics;
using System.Reflection;
using Hexa.NET.ImGui;
using SoulEngine.Core;
using SoulEngine.Resources;

namespace SoulEngine.Content;

public class ContentBrowser<T> : EngineObject where T : Resource
{
    private readonly string[] extensions;
    private readonly Game game;
    
    public ContentBrowser(Game game)
    {
        this.game = game;
        

        extensions = typeof(T).GetCustomAttribute<ExpectedExtensionsAttribute>()?.Extensions ?? [""];
    }

    private bool shown = false;
    public Action<T?> Callback;
    
    public void Show()
    {
        shown = true;
        game.UpdateHooks.Add(Update);
    }

    public void Hide()
    {
        shown = false;
        game.UpdateHooks.Remove(Update);
    }

    private void Update()
    {
        if(!shown)
            Hide();

        ImGui.SetNextWindowSize(new Vector2(400, 200), ImGuiCond.Appearing);
        if (ImGui.Begin("Content Browser##" + ObjectID, ref shown, ImGuiWindowFlags.AlwaysVerticalScrollbar))
        {
            foreach (var content in game.Content.Search())
            {
                if(!extensions.Any(e => content.EndsWith(e)))
                    continue;
                
                if (ImGui.Selectable(content))
                {
                    Hide();
                    T res = game.ResourceManager.Load<T>(content);
                    Callback(res);
                }

            }
        }

        ImGui.End();
    }
}
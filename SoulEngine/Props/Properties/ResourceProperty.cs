using ImGuiNET;
using SoulEngine.Core;
using SoulEngine.Data.NBT;
using SoulEngine.Resources;

namespace SoulEngine.Props;

public class ResourceProperty<T> : SerializedProperty<T?> where T : Resource
{
    private readonly Game game;
    private string id;
    
    public ResourceProperty(string name, string defaultValue, Game game) : base(name, null)
    {
        this.game = game;
        id = defaultValue;

        LoadResource();
    }

    private void LoadResource()
    {
        if (id == "")
            Value = null;
        else
            Value = game.ResourceManager.Load<T>(id);
    }

    public override void Edit()
    {
        
        ImGui.InputText(Name, ref id, 2048);
        ImGui.SameLine();
        if(ImGui.Button("Reload"))
            LoadResource();
        
    }

    public override Tag Save()
    {
        return new StringTag(Name, id);
    }

    public override void Load(Tag tag)
    {
        id = ((StringTag)tag).Data;
        
        LoadResource();
    }
}
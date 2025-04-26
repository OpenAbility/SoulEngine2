using System.Numerics;
using Hexa.NET.ImGui;
using SoulEngine.Core;
using SoulEngine.Data.NBT;
using SoulEngine.Resources;

namespace SoulEngine.Props;

public class ResourceProperty<T> : SerializedProperty<T?> where T : Resource
{
    private readonly Game game;
    private string id;

    private Exception? exception;
    
    public ResourceProperty(string name, string defaultValue, Game game) : base(name, null)
    {
        this.game = game;
        id = defaultValue;

        LoadResource();
    }

    private void LoadResource()
    {
        exception = null;
        if (id == "")
            Value = null;
        else
        {
            Value = null;
            try
            {
                Value = game.ResourceManager.Load<T>(id);
            }
            catch (Exception e)
            {
                
            }
        }
           
    }

    public override void Edit()
    {
        if(ImGui.InputText(Name, ref id, 2048, ImGuiInputTextFlags.EnterReturnsTrue))
            LoadResource();
        if(exception != null)
            ImGui.TextColored(new Vector4(1, 0, 0, 1), exception.ToString());
    }

    public void Load(string id)
    {
        this.id = id;
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
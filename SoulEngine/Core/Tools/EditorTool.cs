using SoulEngine.Data.NBT;

namespace SoulEngine.Core.Tools;

public abstract class EditorTool
{

    public readonly Game Game;
    public readonly Workspace Workspace;

    public bool Enabled = true;

    public Guid ID;
    public string ToolTypeID;

    private static readonly Dictionary<string[], string> IDMapping = new Dictionary<string[], string>();
    private static readonly Dictionary<string, Factory> ToolFactories = new Dictionary<string, Factory>();
    
    public EditorTool(Game game, Workspace workspace)
    {
        ID = Guid.NewGuid();
        
        Game = game;
        Workspace = workspace;

        ToolTypeID = "";
    }
    
    public abstract void Perform();

    public delegate EditorTool Factory(Game game, Workspace workspace);

    public static void Register(string id, Factory factory, params string[] path)
    {
        ToolFactories[id] = factory;
        IDMapping[path] = id;
    }

    public static EditorTool Create(string id, Game game, Workspace workspace)
    {
        EditorTool tool = ToolFactories[id].Invoke(game, workspace);
        tool.ToolTypeID = id;
        return tool;
    }

    public static void DrawMenus(MenuContext context)
    {
        if (Workspace.Current == null)
            return;

        foreach (var tool in IDMapping)
        {
            if (context.IsPressed(tool.Key))
            {
                Workspace.Current.AddTool(tool.Value);
            }
        }
    }

    public void Save(CompoundTag tag)
    {
        OnSave(tag);
    }

    public void Load(CompoundTag tag)
    {
        OnLoad(tag);
    }

    public virtual void OnSave(CompoundTag tag)
    {
        
    }

    public virtual void OnLoad(CompoundTag? tag)
    {
        
    }
}


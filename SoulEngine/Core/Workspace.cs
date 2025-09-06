using Hexa.NET.ImGui;
using SoulEngine.Core.Tools;
using SoulEngine.Data.NBT;
using SoulEngine.Entities;
using ImGuiWindow = SoulEngine.Rendering.ImGuiWindow;

namespace SoulEngine.Core;

public class Workspace
{
    private Guid ID;
    
    public string Name;

    private List<EditorTool> activeTools = new List<EditorTool>();

    public static readonly List<Workspace> Workspaces = new List<Workspace>();

    public static Workspace? Current;

    public Entity? CurrentEntity;

    public ImGuiWindow? GameWindow;
    public ImGuiWindow? SceneWindow;
    public SceneCamera? SceneCamera;

    private Game game;

    public Workspace(Game game)
    {
        ID = Guid.NewGuid();
        Name = "Workspace " + ID;

        this.game = game;
    }
    
    public void Update()
    {
        bool open = true;

        ImGui.PushID(ID.ToString());
        
        if (ImGui.BeginTabItem(Name + "##" + ID, ref open))
        {
            uint id = ImGui.GetID("DOCKSPACE_WORKSPACE_" + ID);
            ImGui.DockSpace(id);
            
            Current = this;
            
            foreach (var tool in activeTools)
            {
                tool.Perform();
            }

            activeTools.RemoveAll(t => !t.Enabled);

            ImGui.EndTabItem();
        }
        
        ImGui.PopID();
    }

    public EditorTool AddTool(string id)
    {
        EditorTool tool = EditorTool.Create(id, game, this);
        activeTools.Add(tool);
        tool.OnLoad(null!);
        return tool;
    }

    public Tag Save()
    {
        CompoundTag compound = new CompoundTag(ID.ToString());
        
        compound.SetString("name", Name);

        CompoundTag toolsTag = new CompoundTag("tools");
        compound.Add(toolsTag);

        foreach (var tool in activeTools)
        {
            CompoundTag toolTag = new CompoundTag(tool.ID.ToString());
            tool.Save(toolTag);
            toolTag.SetString("$_type", tool.ToolTypeID);
            
            toolsTag.Add(toolTag);
        }

        return compound;
    }

    public void Load(Tag tag)
    {
        if(tag is not CompoundTag compound)
            return;

        ID = Guid.Parse(compound.Name!);
        Name = compound.GetString("name") ?? "Workspace " + ID;

        CompoundTag toolsTag = compound.GetTag<CompoundTag>("tools")!;

        foreach (var tool in toolsTag.Values)
        {
            CompoundTag toolTag = (CompoundTag)tool;
            
            EditorTool toolInstance = EditorTool.Create(toolTag.GetString("$_type")!, game, this);
            toolInstance.ID = Guid.Parse(toolTag.Name!);
            toolInstance.Load(toolTag);
            
            activeTools.Add(toolInstance);
        }
    }

    private static void CreateDefaultSetup(Game game)
    {
        Stream? stream = game.Content.Load("editor/default_workspaces.workspace.nbt");
        
        CompoundTag compound = (CompoundTag)TagIO.ReadCompressed(stream!, false);
        
        foreach (var workspace in compound.Values)
        {
            Workspace workspaceInstance = new Workspace(game);
            workspaceInstance.Load(workspace);
            Workspaces.Add(workspaceInstance);
        }
    }

    public static void Load(Game game)
    {
        if (!File.Exists("workspaces.snbt"))
        {
            CreateDefaultSetup(game);
            return;
        }

        Tag tag = TagIO.ReadSNBT(File.ReadAllText("workspaces.snbt"));

        if (tag is not CompoundTag compound)
        {
            CreateDefaultSetup(game);
            return;
        }

        foreach (var workspace in compound.Values)
        {
            Workspace workspaceInstance = new Workspace(game);
            workspaceInstance.Load(workspace);
            Workspaces.Add(workspaceInstance);
        }
        
    }

    public static void Save(Game game)
    {
        CompoundTag compound = new CompoundTag("tools");

        foreach (var workspace in Workspaces)
        {
            compound.Add(workspace.Save());
        }
        
        File.WriteAllText("_workspaces.snbt", TagIO.WriteSNBT(compound));
        File.Copy("_workspaces.snbt", "workspaces.snbt", true);
    }
}
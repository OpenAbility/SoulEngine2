using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Entities;
using SoulEngine.Renderer;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.Util;
using Vector2 = System.Numerics.Vector2;

namespace SoulEngine.Core.Tools;

public class EventSequenceEditor : EditorTool
{
    private Scene? baseScene;
    private SceneInfo? baseSceneInfo;
    private readonly List<Entity> additionalEntities = new List<Entity>();
    
    private readonly ContentBrowser<SceneInfo> browser;
    private bool editCamera;
    private Entity? selectedEntity;
    private readonly SceneCamera sceneCamera;
    
    private Framebuffer? viewportBuffer;
    

    public EventSequenceEditor(Game game, Workspace workspace) : base(game, workspace)
    {
        browser = new ContentBrowser<SceneInfo>(game);

        browser.Callback += SetScene;

        sceneCamera = new SceneCamera(game);

    }

    public override void Perform()
    {
        uint dockspaceID = ImGui.GetID(ID.ToString());

        if (ImGui.Begin("Event Sequence Editor##" + ID, ref Enabled, ImGuiWindowFlags.MenuBar))
        {
            ImGui.DockSpace(dockspaceID);

            DrawView();
            DrawTimeline();
            DrawOutline();
        }

        ImGui.End();
    }

    private void DrawView()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(0, 0));

        if (ImGui.Begin("Viewport##" + ID, ImGuiWindowFlags.MenuBar))
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.MenuItem("Toggle Edit Camera"))
                {
                    editCamera = !editCamera;
                }
                
                ImGui.EndMenuBar();
            }
           
            
            Vector2 regionAvail = ImGui.GetContentRegionAvail();
            Vector2 position = ImGui.GetCursorScreenPos();


            if (baseScene == null)
            {
                ImGui.Text("No Scene Loaded! Provide a base scene in the outline!");
                goto ViewEnd;
            }

            if (viewportBuffer == null || viewportBuffer.FramebufferSize.X != (int)regionAvail.X ||
                viewportBuffer.FramebufferSize.Y != (int)regionAvail.Y)
            {
                viewportBuffer = new Framebuffer(Game,
                    new Vector2i((int)regionAvail.X, (int)regionAvail.Y));
            }

            SceneRenderInformation renderInformation = new SceneRenderInformation();
            renderInformation.CameraSettings = editCamera ? sceneCamera.CreateCameraSettings(viewportBuffer, true, selectedEntity) : CameraSettings.Game;
            renderInformation.DeltaTime = Game.DeltaTime;
            renderInformation.EnableCulling = false;
            renderInformation.EntityCollection = baseScene;
            renderInformation.PerformCullingPass = false;
            renderInformation.PostProcessing = true;
            renderInformation.RenderContext = Game.RenderContext;
            if (baseScene.Director != null)
                renderInformation.RenderUI = baseScene.Director.RenderUI;
            renderInformation.RenderPipeline = Game.RenderPipeline;
            renderInformation.TargetSurface = viewportBuffer;
            renderInformation.UIContext = Game.UIContext;
            Game.SceneRenderer.PerformGameRender(renderInformation);

            ImGui.Image(new ImTextureID(viewportBuffer.ColourBuffer),
                new System.Numerics.Vector2(viewportBuffer.FramebufferSize.X, viewportBuffer.FramebufferSize.Y),
                new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
            
            ImGuizmo.SetDrawlist();
            ImGuizmo.SetRect(position.X, position.Y, regionAvail.X, regionAvail.Y);
            
            if (selectedEntity != null && editCamera)
            {
                selectedEntity.RenderMoveGizmo(sceneCamera.GetView(), sceneCamera.GetProjection(regionAvail.X / regionAvail.Y));
            }
            sceneCamera.Update(Game.DeltaTime, ImGui.IsWindowFocused());


        }

        ViewEnd:

        ImGui.PopStyleVar();
        ImGui.End();
    }

    protected void DrawTimeline()
    {
        if (ImGui.Begin("Timeline##" + ID))
        {

        }

        ImGui.End();
    }

    protected void DrawOutline()
    {
        if (ImGui.Begin("Outline##" + ID))
        {
            string baseSceneID = baseSceneInfo?.ResourceID ?? "";
            if (ImGui.InputText("Base Scene", ref baseSceneID, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                SetScene(Game.ResourceManager.Load<SceneInfo>(baseSceneID));
            }

            ImGui.SameLine();
            if (ImGui.Button("Browse"))
            {
                browser.Show();
            }

            if (baseScene != null && ImGui.BeginChild("Additional Actors",
                    new Vector2(ImGui.GetContentRegionAvail().X, 0),
                    ImGuiChildFlags.AutoResizeX | ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.Borders))
            {
                ImGui.SeparatorText("Additional Actors");

                bool hoveredButton = false;

                foreach (var entity in additionalEntities)
                {
                    if (ImGuiUtil.ImageSelectable(entity.Icon, entity.Name + "##" + entity.GetHashCode(),
                            Workspace.CurrentEntity == entity))
                        selectedEntity = entity;

                    if (ImGui.IsItemHovered())
                        hoveredButton = true;
                }

                if (!hoveredButton && ImGui.BeginPopupContextWindow())
                {
                    foreach (var type in EntityTemplateFactory.TemplateNames)
                    {
                        if (ImGui.Selectable(type))
                        {
                            Entity? e = EntityTemplateFactory
                                .Initialize(type, type + " (" + Random.Shared.Next(1000, 9999) + ")", baseScene);

                            if (e != null)
                            {
                                e.Mark("actor_added");
                                additionalEntities.Add(e);
                            }
                        }
                    }

                    ImGui.EndPopup();
                }

                ImGui.EndChild();
            }

            if (baseScene != null && selectedEntity != null && ImGui.BeginChild("Actor Setup",
                    new Vector2(ImGui.GetContentRegionAvail().X, 0),
                    ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY))
            {
                selectedEntity.Edit();
                
                
                if (ImGui.Button("Delete"))
                {
                    baseScene.DeleteEntity(selectedEntity);
                    selectedEntity = null;
                }
                
                ImGui.EndPopup();
            }

        }

        ImGui.End();
    }

    private void SetScene(SceneInfo? info)
    {
        if (baseScene != null)
        {
            foreach (var entity in additionalEntities)
            {
                baseScene.DeleteEntity(entity);
            }
        }

        baseSceneInfo = info;
        baseScene = info?.Instantiate();

        if (baseScene != null)
        {
            foreach (var entity in additionalEntities)
            {
                baseScene.AddEntity(entity);
            }
        }


    }
}
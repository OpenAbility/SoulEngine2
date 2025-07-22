using System.Drawing;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using OpenTK.Mathematics;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Core.Tools;
using SoulEngine.Entities;
using SoulEngine.Input;
using SoulEngine.Mathematics;
using SoulEngine.Renderer;
using SoulEngine.Rendering;
using SoulEngine.Resources;
using SoulEngine.Util;
using Vector2 = System.Numerics.Vector2;

namespace SoulEngine.Sequences;

public class EventSequenceEditor : EditorTool
{
    private Scene? baseScene;
    private SceneInfo? baseSceneInfo;
    private readonly List<Entity> additionalEntities = new List<Entity>();

    private readonly List<SequenceEvent> events = new List<SequenceEvent>();
    
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

        SequenceEvent seq = new TestEvent();
        seq.DisplayName = "Test Event";
        seq.StartTime = TimeSpan.FromSeconds(10);
        
        events.Add(seq);
        events.Sort();

    }

    public override void Perform()
    {
  
            DrawView();
            DrawTimeline();
            DrawOutline();
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

    private float sidebarWidth = 100;
    private float timelineZoomFactor = 0.25f;
    private float timelineScroll = 0;
    private float timelineRowSize = 16;
    private float timelineRowMargin = 1;
    private float timelineRowPadding = 2;
    private float timelineRowScroll = 0;

    protected void DrawTimeline()
    {
        if (ImGui.Begin("Timeline##" + ID))
        {
            ImGuiIOPtr io = ImGui.GetIO();
            var drawList = ImGui.GetWindowDrawList();

            Vector2 canvasPos = ImGui.GetCursorScreenPos();
            Vector2 canvasSize = ImGui.GetContentRegionAvail();

            Rect windowRectangle = new Rect(canvasPos.X, canvasPos.Y, canvasSize.X, canvasSize.Y);

            float headerSize = ImGui.GetFontSize() + 4;
            
            if (windowRectangle.Inset(0, 0, 0, 0).Inside(ImGui.GetMousePos().Tk()))
            {
                if (Game.Keys.LeftShift.Down || Game.Keys.RightShift.Down)
                {
                    timelineScroll += io.MouseWheel;
                }
                else if (Game.Keys.LeftControl.Down || Game.Keys.RightControl.Down)
                {
                    timelineZoomFactor += io.MouseWheel * 0.1f * timelineZoomFactor;
                }
                else
                {
                    timelineRowScroll += io.MouseWheel;
                }
                
                timelineScroll += io.MouseWheelH;
            }

            if (timelineScroll < 0)
                timelineScroll = 0;

            if (timelineRowScroll < 0)
                timelineRowScroll = 0;

            if (timelineZoomFactor < 0.01f)
                timelineZoomFactor = 0.01f;
            
            drawList.AddRectFilled(canvasPos, canvasPos + new Vector2(canvasSize.X, headerSize), ImGui.GetColorU32(ImGuiCol.TableHeaderBg));
            drawList.AddLine(new Vector2(canvasPos.X, canvasPos.Y + headerSize), new Vector2(canvasPos.X + canvasSize.X, canvasPos.Y + headerSize), ImGui.GetColorU32(ImGuiCol.Border));
            drawList.AddText(canvasPos + Vector2.One * 2, ImGui.GetColorU32(ImGuiCol.Text), "Timeline"u8);
            
            drawList.AddLine(canvasPos + new Vector2(sidebarWidth, 0), canvasPos + new Vector2(sidebarWidth, canvasSize.Y), ImGui.GetColorU32(ImGuiCol.Border), 2);

            float timeBarProgression = MathF.Floor(timelineScroll);
            
            drawList.PushClipRect(new Vector2(canvasPos.X + sidebarWidth, canvasPos.Y), new Vector2(canvasPos.X + canvasSize.X, canvasPos.Y + headerSize));
            
            while (true)
            {
                float x = canvasPos.X + sidebarWidth + CalculateTimelineOffset(timeBarProgression);
                
                if(x - canvasPos.X >= canvasSize.X)
                    break;

                float height = timeBarProgression % 10 == 0 ? 1 : 0.25f;
                float width = timeBarProgression % 10 == 0 ? 2 : 1;
        
                drawList.AddLine(new Vector2(x, canvasPos.Y), new Vector2(x, canvasPos.Y + headerSize * height), ImGui.GetColorU32(ImGuiCol.Text), width);

                if (timeBarProgression % 20 == 0)
                {
                    TimeSpan timespan = TimeSpan.FromSeconds(timeBarProgression);

                    string display = ((int)timespan.TotalSeconds).ToString();
                    
                    drawList.AddText(new Vector2(x + 4, canvasPos.Y + headerSize - ImGui.GetFontSize()), ImGui.GetColorU32(ImGuiCol.Text), display);
                }
                
                timeBarProgression += 1;
            }

            for (int i = (int)MathF.Floor(timelineRowScroll); i < Int32.MaxValue; i++)
            {
                
                float y = canvasPos.Y + CalculateRowY(i) + headerSize;
                
                if(y > canvasSize.Y + canvasPos.Y)
                    break;

                uint colour = i % 2 == 0
                    ? ImGui.GetColorU32(ImGuiCol.TableRowBg)
                    : ImGui.GetColorU32(ImGuiCol.TableRowBgAlt);
                
                drawList.AddRectFilled(new Vector2(canvasPos.X + sidebarWidth + 2, y), new Vector2(canvasPos.X + canvasSize.X, y + timelineRowSize + timelineRowPadding * 2), colour);
                drawList.AddLine(new Vector2(canvasPos.X + sidebarWidth + 2, y + timelineRowSize + timelineRowPadding * 2), new Vector2(canvasPos.X + canvasSize.X, y + timelineRowSize + timelineRowPadding * 2), ImGui.GetColorU32(ImGuiCol.BorderShadow));
            }

            for (int i = 0; i < events.Count; i++)
            {
                SequenceEvent sequenceEvent = events[i];
                
                float x = canvasPos.X + sidebarWidth + CalculateTimelineOffset((float)sequenceEvent.StartTime.TotalSeconds);
                float y = canvasPos.Y + CalculateRowY(sequenceEvent.TimelineID) + headerSize + timelineRowPadding;
                if(x - canvasPos.X >= canvasSize.X)
                    break;

                float width = CalculateTimelineSize((float)sequenceEvent.Duration.TotalSeconds);

                if (x + width < canvasPos.X)
                    continue;

                if (sequenceEvent.RenderMode == EventRenderMode.Circle)
                {
                    float radius = timelineRowSize * 0.4f;
                    drawList.AddCircleFilled(new Vector2(x, y + timelineRowSize / 2f), radius, sequenceEvent.Colour.ToUint32());
                }
            }
       
            
        }

        ImGui.End();
    }

    private float CalculateTimelineOffset(float seconds)
    {
        return CalculateTimelineSize(seconds - timelineScroll);
    }
    
    private float CalculateTimelineSize(float duration)
    {
        return (duration) * 100 * timelineZoomFactor;
    }

    private float CalculateRowY(int row)
    {
        return (row - timelineRowScroll) * (timelineRowSize + timelineRowPadding + timelineRowPadding * 2);
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
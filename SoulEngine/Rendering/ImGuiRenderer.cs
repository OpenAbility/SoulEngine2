// We only need ImGui in dev builds

using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Hexa.NET.ImPlot;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Events;
using SoulEngine.Input;
using SoulEngine.Mathematics;
using ResourceManager = SoulEngine.Resources.ResourceManager;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace SoulEngine.Rendering;

public unsafe class ImGuiRenderer : EngineObject
{

    public readonly ImGuiContextPtr Context;
    public readonly IntPtr GizmoContext;
    private readonly ImPlotContextPtr PlotContext;
    public ImGuiIOPtr IO { get; private set; }

    private int fontTexture = -1;
    private int vao;
    private int vbo;
    private int ebo;

    private int vboSize = -1;
    private int eboSize = -1;

    // Pairs of 2, (Shift, Control, Alt, Super) Left, Right
    private bool[] specialKeys = new bool[8];
    

    private Shader shader;

    public ImGuiRenderer(ResourceManager resourceManager)
    {
        Context = ImGui.CreateContext();

        ImGui.SetCurrentContext(Context);
        ImGuizmo.SetImGuiContext(Context);
        PlotContext = ImPlot.CreateContext();

        IO = ImGui.GetIO();

        
        if (EngineVarContext.Global.Exists("imgui_font"))
            IO.Fonts.AddFontFromFileTTF(EngineVarContext.Global.GetString("imgui_font"), EngineVarContext.Global.GetInt("imgui_font_size", 21));
        else
            IO.Fonts.AddFontDefault();

        IO.Fonts.Build();
        
        IO.BackendFlags = ImGuiBackendFlags.RendererHasVtxOffset;
        IO.ConfigFlags = ImGuiConfigFlags.DockingEnable;

        BuildFontTexture();

        shader = resourceManager.Load<Shader>("shader/imgui.program");

        vbo = GL.CreateBuffer();
        ebo = GL.CreateBuffer();
        
        GL.NamedBufferData(vbo, 2000, null, BufferUsage.StreamDraw);
        GL.NamedBufferData(ebo, 2000, null, BufferUsage.StreamDraw);

        vboSize = 2000;
        eboSize = 2000;

        vao = GL.CreateVertexArray();

        GL.EnableVertexArrayAttrib(vao, 0);
        GL.EnableVertexArrayAttrib(vao, 1);
        GL.EnableVertexArrayAttrib(vao, 2);

        GL.VertexArrayAttribFormat(vao, 0, 2, VertexAttribType.Float, false, 0);
        GL.VertexArrayAttribFormat(vao, 1, 2, VertexAttribType.Float, false, 8);
        GL.VertexArrayAttribFormat(vao, 2, 4, VertexAttribType.UnsignedByte, true, 16);

        GL.VertexArrayAttribBinding(vao, 0, 0);
        GL.VertexArrayAttribBinding(vao, 1, 0);
        GL.VertexArrayAttribBinding(vao, 2, 0);

        GL.VertexArrayVertexBuffer(vao, 0, vbo, 0, sizeof(ImDrawVert));
        GL.VertexArrayElementBuffer(vao, ebo);
    }


    public void BuildFontTexture()
    {
        byte* pixels = null;
        int width = 0;
        int height = 0;
        int bytesPerPixel = 0;
        
        IO.Fonts.GetTexDataAsRGBA32(ref pixels, ref width, ref height, ref bytesPerPixel);

        int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

        fontTexture = GL.CreateTexture(TextureTarget.Texture2d);
        GL.TextureStorage2D(fontTexture, mips, SizedInternalFormat.Rgba8, width, height);
        GL.TextureSubImage2D(fontTexture, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

        GL.GenerateTextureMipmap(fontTexture);

        GL.TextureParameteri(fontTexture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TextureParameteri(fontTexture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.TextureParameteri(fontTexture, TextureParameterName.TextureMaxLevel, mips - 1);

        GL.TextureParameteri(fontTexture, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TextureParameteri(fontTexture, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        IO.Fonts.SetTexID(new ImTextureID(fontTexture));
        IO.Fonts.ClearTexData();
    }

    public void BeginFrame(Window window, float deltaTime)
    {
        IO = ImGui.GetIO();
        
        Vector2i fbSize = window.WindowSize;

        IO.DisplaySize = new Vector2(fbSize.X, fbSize.Y);
        IO.DeltaTime = deltaTime;

        IO.KeyShift = specialKeys[0] || specialKeys[1];
        IO.KeyCtrl = specialKeys[2] || specialKeys[3];
        IO.KeyAlt = specialKeys[4] || specialKeys[5];
        IO.KeySuper = specialKeys[6] || specialKeys[7];

        ImGui.NewFrame();
        ImGuizmo.BeginFrame();
    }

    public void EndFrame(IRenderSurface surface, bool hide)
    {
        ImGui.Render();
        if(!hide)
            RenderDrawData(ImGui.GetDrawData(), surface);
    }

    public void OnInputEvent(InputEvent inputEvent, bool unhandled)
    {
        if(!unhandled)
            return;

        if (inputEvent is CursorEvent cursorEvent)
        {
            IO.MousePos = cursorEvent.Position;
            if (IO.WantCaptureMouse)
                cursorEvent.Handle();
        } else if (inputEvent is MouseEvent mouseEvent && mouseEvent.Action != ButtonAction.Repeat)
        {
            IO.AddMouseButtonEvent((int)mouseEvent.Button, mouseEvent.Action == ButtonAction.Press);
            if (IO.WantCaptureMouse)
                mouseEvent.Handle();
        } else if (inputEvent is KeyEvent keyEvent && keyEvent.Action != ButtonAction.Repeat)
        {

            if (keyEvent.Key == KeyCode.LeftShift) specialKeys[0] = keyEvent.Action == ButtonAction.Press;
            else if (keyEvent.Key == KeyCode.RightShift) specialKeys[1] = keyEvent.Action == ButtonAction.Press;
            else if (keyEvent.Key == KeyCode.LeftControl) specialKeys[2] = keyEvent.Action == ButtonAction.Press;
            else if (keyEvent.Key == KeyCode.RightControl) specialKeys[3] = keyEvent.Action == ButtonAction.Press;
            else if (keyEvent.Key == KeyCode.LeftAlt) specialKeys[4] = keyEvent.Action == ButtonAction.Press;
            else if (keyEvent.Key == KeyCode.RightAlt) specialKeys[5] = keyEvent.Action == ButtonAction.Press;
            else if (keyEvent.Key == KeyCode.LeftSuper) specialKeys[6] = keyEvent.Action == ButtonAction.Press;
            else if (keyEvent.Key == KeyCode.RightSuper) specialKeys[7] = keyEvent.Action == ButtonAction.Press;
            
            IO.AddKeyEvent(TranslateKey(keyEvent.Key), keyEvent.Action == ButtonAction.Press);
            if (IO.WantCaptureKeyboard)
                keyEvent.Handle();
        } else if (inputEvent is TypeEvent typeEvent)
        {
            IO.AddInputCharactersUTF8(typeEvent.Text);
            if (IO.WantCaptureKeyboard)
                typeEvent.Handle();
        } else if (inputEvent is ScrollEvent scrollEvent)
        {
            IO.AddMouseWheelEvent(scrollEvent.Delta.X, scrollEvent.Delta.Y);
            if (ImGui.IsAnyItemHovered())
                scrollEvent.Handle();
        }

    }


    private void RenderDrawData(ImDrawDataPtr drawData, IRenderSurface surface)
    {

        if (drawData.CmdListsCount == 0)
            return;

        surface.BindFramebuffer();
        
        shader.Bind();
        GL.BindVertexArray(vao);

        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.ScissorTest);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);

        Vector2i surfaceSize = surface.FramebufferSize;

        for (int i = 0; i < drawData.CmdListsCount; i++)
        {

            ImDrawListPtr drawList = drawData.CmdLists[i];

            int vtxSize = drawList.VtxBuffer.Size * sizeof(ImDrawVert);
            if (vtxSize > vboSize)
            {
                vboSize = (int)(vtxSize * 1.5f);
                GL.NamedBufferData(vbo, vboSize, null, BufferUsage.StreamDraw);
            }

            int idxSize = drawList.IdxBuffer.Size * sizeof(ushort);
            if (idxSize > eboSize)
            {
                eboSize = (int)(idxSize * 1.5f);
                GL.NamedBufferData(ebo, eboSize, null, BufferUsage.StreamDraw);
            }

            GL.NamedBufferSubData(vbo, 0, drawList.VtxBuffer.Size * sizeof(ImDrawVert), drawList.VtxBuffer.Data);
            GL.NamedBufferSubData(ebo, 0, drawList.IdxBuffer.Size * sizeof(ushort), drawList.IdxBuffer.Data);

            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(0, IO.DisplaySize.X, IO.DisplaySize.Y, 0, -1.0f, 1.0f);

            shader.Uniform1i("ut_fontTexture", 0);
            shader.Matrix("um_projection", mvp, false);

            for (int j = 0; j < drawList.CmdBuffer.Size; j++)
            {

                ImDrawCmd drawCommand = drawList.CmdBuffer[j];

                GL.BindTextureUnit(0, (int)drawCommand.TextureId.Handle);

                Vector4 clip = drawCommand.ClipRect;
                Vector4i clipI = new Vector4i((int)clip.X, (int)clip.Y, (int)clip.Z, (int)clip.W);
                GL.Scissor(clipI.X, surfaceSize.Y - clipI.Y - clipI.W, clipI.Z, clipI.W);

                GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)drawCommand.ElemCount,
                    DrawElementsType.UnsignedShort, (IntPtr)(drawCommand.IdxOffset * sizeof(ushort)),
                    (int)drawCommand.VtxOffset);
            }
        }

        GL.BindVertexArray(0);

        GL.Disable(EnableCap.ScissorTest);
        GL.Disable(EnableCap.Blend);

    }

    private static ImGuiKey TranslateKey(KeyCode key)
    {
        if (key >= KeyCode.D0 && key <= KeyCode.D9)
            return key - KeyCode.D0 + ImGuiKey.Key0;

        if (key >= KeyCode.A && key <= KeyCode.Z)
            return key - KeyCode.A + ImGuiKey.A;

        if (key >= KeyCode.KeyPad0 && key <= KeyCode.KeyPad9)
            return key - KeyCode.KeyPad0 + ImGuiKey.Keypad0;

        if (key >= KeyCode.F1 && key <= KeyCode.F24)
            return key - KeyCode.F1 + ImGuiKey.F24;

        switch (key)
        {
            case KeyCode.Tab: return ImGuiKey.Tab;
            case KeyCode.Left: return ImGuiKey.LeftArrow;
            case KeyCode.Right: return ImGuiKey.RightArrow;
            case KeyCode.Up: return ImGuiKey.UpArrow;
            case KeyCode.Down: return ImGuiKey.DownArrow;
            case KeyCode.PageUp: return ImGuiKey.PageUp;
            case KeyCode.PageDown: return ImGuiKey.PageDown;
            case KeyCode.Home: return ImGuiKey.Home;
            case KeyCode.End: return ImGuiKey.End;
            case KeyCode.Insert: return ImGuiKey.Insert;
            case KeyCode.Delete: return ImGuiKey.Delete;
            case KeyCode.Backspace: return ImGuiKey.Backspace;
            case KeyCode.Space: return ImGuiKey.Space;
            case KeyCode.Enter: return ImGuiKey.Enter;
            case KeyCode.Escape: return ImGuiKey.Escape;
            case KeyCode.Apostrophe: return ImGuiKey.Apostrophe;
            case KeyCode.Comma: return ImGuiKey.Comma;
            case KeyCode.Minus: return ImGuiKey.Minus;
            case KeyCode.Period: return ImGuiKey.Period;
            case KeyCode.Slash: return ImGuiKey.Slash;
            case KeyCode.Semicolon: return ImGuiKey.Semicolon;
            case KeyCode.Equal: return ImGuiKey.Equal;
            case KeyCode.LeftBracket: return ImGuiKey.LeftBracket;
            case KeyCode.Backslash: return ImGuiKey.Backslash;
            case KeyCode.RightBracket: return ImGuiKey.RightBracket;
            case KeyCode.GraveAccent: return ImGuiKey.GraveAccent;
            case KeyCode.CapsLock: return ImGuiKey.CapsLock;
            case KeyCode.ScrollLock: return ImGuiKey.ScrollLock;
            case KeyCode.NumLock: return ImGuiKey.NumLock;
            case KeyCode.PrintScreen: return ImGuiKey.PrintScreen;
            case KeyCode.Pause: return ImGuiKey.Pause;
            case KeyCode.KeyPadDecimal: return ImGuiKey.KeypadDecimal;
            case KeyCode.KeyPadDivide: return ImGuiKey.KeypadDivide;
            case KeyCode.KeyPadMultiply: return ImGuiKey.KeypadMultiply;
            case KeyCode.KeyPadSubtract: return ImGuiKey.KeypadSubtract;
            case KeyCode.KeyPadAdd: return ImGuiKey.KeypadAdd;
            case KeyCode.KeyPadEnter: return ImGuiKey.KeypadEnter;
            case KeyCode.KeyPadEqual: return ImGuiKey.KeypadEqual;
            case KeyCode.LeftShift: return ImGuiKey.LeftShift;
            case KeyCode.LeftControl: return ImGuiKey.LeftCtrl;
            case KeyCode.LeftAlt: return ImGuiKey.LeftAlt;
            case KeyCode.LeftSuper: return ImGuiKey.LeftSuper;
            case KeyCode.RightShift: return ImGuiKey.RightShift;
            case KeyCode.RightControl: return ImGuiKey.RightCtrl;
            case KeyCode.RightAlt: return ImGuiKey.RightAlt;
            case KeyCode.RightSuper: return ImGuiKey.RightSuper;
            case KeyCode.Menu: return ImGuiKey.Menu;
            default: return ImGuiKey.None;
        }
    }


}
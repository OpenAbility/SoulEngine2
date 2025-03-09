// We only need ImGui in dev builds

using System.Resources;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SoulEngine.Events;
using SoulEngine.Mathematics;
using ResourceManager = SoulEngine.Resources.ResourceManager;
using Vector2 = System.Numerics.Vector2;

namespace SoulEngine.Rendering;

public unsafe class ImGuiRenderer
{

    public readonly ImGuiContextPtr Context;
    public readonly IntPtr GizmoContext;
    public readonly ImGuiIOPtr IO;

    private int fontTexture = -1;
    private int vao;
    private int vbo;
    private int ebo;

    private int vboSize = -1;
    private int eboSize = -1;

    private Shader shader;

    public ImGuiRenderer(ResourceManager resourceManager)
    {
        Context = ImGui.CreateContext();

        ImGui.SetCurrentContext(Context);
        ImGuizmo.SetImGuiContext(Context);

        IO = ImGui.GetIO();

        
        if (resourceManager.Game.EngineVar.Exists("imgui_font"))
            IO.Fonts.AddFontFromFileTTF(resourceManager.Game.EngineVar.GetString("imgui_font"), resourceManager.Game.EngineVar.GetInt("imgui_font_size", 21));
        else
            IO.Fonts.AddFontDefault();

        IO.Fonts.Build();
        
        IO.BackendFlags = ImGuiBackendFlags.RendererHasVtxOffset;
        IO.ConfigFlags = ImGuiConfigFlags.DockingEnable;

        BuildFontTexture();

        shader = resourceManager.Load<Shader>("shader/imgui.program");

        vbo = GL.CreateBuffer();
        ebo = GL.CreateBuffer();
        
        GL.NamedBufferData(vbo, 2000, null, VertexBufferObjectUsage.StreamDraw);
        GL.NamedBufferData(ebo, 2000, null, VertexBufferObjectUsage.StreamDraw);

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

    public void BeginFrame(IRenderSurface renderSurface, float deltaTime)
    {
        Vector2i fbSize = renderSurface.FramebufferSize;


        IO.DisplaySize = new Vector2(fbSize.X, fbSize.Y);
        IO.DeltaTime = deltaTime;

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
        } else if (inputEvent is MouseEvent mouseEvent && mouseEvent.Action != InputAction.Repeat)
        {
            IO.AddMouseButtonEvent((int)mouseEvent.Button, mouseEvent.Action == InputAction.Press);
            if (IO.WantCaptureMouse)
                mouseEvent.Handle();
        } else if (inputEvent is KeyEvent keyEvent && keyEvent.Action != InputAction.Repeat)
        {

            IO.KeyAlt = (keyEvent.Modifier & KeyModifiers.Alt) != 0;
            IO.KeyCtrl = (keyEvent.Modifier & KeyModifiers.Control) != 0;
            IO.KeyShift = (keyEvent.Modifier & KeyModifiers.Shift) != 0;
            IO.KeySuper = (keyEvent.Modifier & KeyModifiers.Super) != 0;
            
            IO.AddKeyEvent(TranslateKey(keyEvent.Key), keyEvent.Action == InputAction.Press);
            if (IO.WantCaptureKeyboard)
                keyEvent.Handle();
        } else if (inputEvent is TypeEvent typeEvent)
        {
            IO.AddInputCharacter(typeEvent.Codepoint);
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
                GL.NamedBufferData(vbo, vboSize, null, VertexBufferObjectUsage.StreamDraw);
            }

            int idxSize = drawList.IdxBuffer.Size * sizeof(ushort);
            if (idxSize > eboSize)
            {
                eboSize = (int)(idxSize * 1.5f);
                GL.NamedBufferData(ebo, eboSize, null, VertexBufferObjectUsage.StreamDraw);
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

                var clip = drawCommand.ClipRect;
                GL.Scissor((int)clip.X, surfaceSize.Y - (int)clip.W, (int)(clip.Z), (int)(clip.W));

                GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)drawCommand.ElemCount,
                    DrawElementsType.UnsignedShort, (IntPtr)(drawCommand.IdxOffset * sizeof(ushort)),
                    (int)drawCommand.VtxOffset);
            }
        }

        GL.BindVertexArray(0);

        GL.Disable(EnableCap.ScissorTest);
        GL.Disable(EnableCap.Blend);

    }

    private static ImGuiKey TranslateKey(Keys key)
    {
        if (key >= Keys.D0 && key <= Keys.D9)
            return key - Keys.D0 + ImGuiKey.Key0;

        if (key >= Keys.A && key <= Keys.Z)
            return key - Keys.A + ImGuiKey.A;

        if (key >= Keys.KeyPad0 && key <= Keys.KeyPad9)
            return key - Keys.KeyPad0 + ImGuiKey.Keypad0;

        if (key >= Keys.F1 && key <= Keys.F24)
            return key - Keys.F1 + ImGuiKey.F24;

        switch (key)
        {
            case Keys.Tab: return ImGuiKey.Tab;
            case Keys.Left: return ImGuiKey.LeftArrow;
            case Keys.Right: return ImGuiKey.RightArrow;
            case Keys.Up: return ImGuiKey.UpArrow;
            case Keys.Down: return ImGuiKey.DownArrow;
            case Keys.PageUp: return ImGuiKey.PageUp;
            case Keys.PageDown: return ImGuiKey.PageDown;
            case Keys.Home: return ImGuiKey.Home;
            case Keys.End: return ImGuiKey.End;
            case Keys.Insert: return ImGuiKey.Insert;
            case Keys.Delete: return ImGuiKey.Delete;
            case Keys.Backspace: return ImGuiKey.Backspace;
            case Keys.Space: return ImGuiKey.Space;
            case Keys.Enter: return ImGuiKey.Enter;
            case Keys.Escape: return ImGuiKey.Escape;
            case Keys.Apostrophe: return ImGuiKey.Apostrophe;
            case Keys.Comma: return ImGuiKey.Comma;
            case Keys.Minus: return ImGuiKey.Minus;
            case Keys.Period: return ImGuiKey.Period;
            case Keys.Slash: return ImGuiKey.Slash;
            case Keys.Semicolon: return ImGuiKey.Semicolon;
            case Keys.Equal: return ImGuiKey.Equal;
            case Keys.LeftBracket: return ImGuiKey.LeftBracket;
            case Keys.Backslash: return ImGuiKey.Backslash;
            case Keys.RightBracket: return ImGuiKey.RightBracket;
            case Keys.GraveAccent: return ImGuiKey.GraveAccent;
            case Keys.CapsLock: return ImGuiKey.CapsLock;
            case Keys.ScrollLock: return ImGuiKey.ScrollLock;
            case Keys.NumLock: return ImGuiKey.NumLock;
            case Keys.PrintScreen: return ImGuiKey.PrintScreen;
            case Keys.Pause: return ImGuiKey.Pause;
            case Keys.KeyPadDecimal: return ImGuiKey.KeypadDecimal;
            case Keys.KeyPadDivide: return ImGuiKey.KeypadDivide;
            case Keys.KeyPadMultiply: return ImGuiKey.KeypadMultiply;
            case Keys.KeyPadSubtract: return ImGuiKey.KeypadSubtract;
            case Keys.KeyPadAdd: return ImGuiKey.KeypadAdd;
            case Keys.KeyPadEnter: return ImGuiKey.KeypadEnter;
            case Keys.KeyPadEqual: return ImGuiKey.KeypadEqual;
            case Keys.LeftShift: return ImGuiKey.LeftShift;
            case Keys.LeftControl: return ImGuiKey.LeftCtrl;
            case Keys.LeftAlt: return ImGuiKey.LeftAlt;
            case Keys.LeftSuper: return ImGuiKey.LeftSuper;
            case Keys.RightShift: return ImGuiKey.RightShift;
            case Keys.RightControl: return ImGuiKey.RightCtrl;
            case Keys.RightAlt: return ImGuiKey.RightAlt;
            case Keys.RightSuper: return ImGuiKey.RightSuper;
            case Keys.Menu: return ImGuiKey.Menu;
            default: return ImGuiKey.None;
        }
    }


}
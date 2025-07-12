using System.Collections.Concurrent;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SDL3;

namespace SoulEngine.Rendering;

/// <summary>
/// Abstraction for an underlying GPU - meant to be multithreaded
/// </summary>
public unsafe class GPUDevice
{
    private bool running;
    private bool awake;

    private readonly Thread deviceThread;

    private readonly ConcurrentQueue<GPUTask> tasks = new ConcurrentQueue<GPUTask>();
    
    private EnableCaps enableCaps;
    private BindState currentBindState;

    private readonly Lock submitLock = new Lock();
    private bool lockEngaged = false;

    private readonly Fence setupFence;

    public IntPtr Context { get; private set; }

    public bool IsOnDeviceThread => Thread.CurrentThread == deviceThread;


    public GPUDevice(Window window)
    {
        running = true;
        setupFence = new Fence(this);
        
        deviceThread = new Thread(() => DeviceMainLoop(window));
        deviceThread.IsBackground = false;
        deviceThread.Priority = ThreadPriority.Highest;
        deviceThread.Name = "GPU Thread";
        singleCommandList = BuildCommandList();
        deviceThread.Start();

    }

    public void WaitReady()
    {
        setupFence.Wait(UInt64.MaxValue);
    }
    
    private void BuildState()
    {
        for (int i = 0; i < EnableCaps.Caps.Length; i++)
        {
            enableCaps[EnableCaps.Caps[i]] = GL.IsEnabled(EnableCaps.Caps[i]);
        }

        currentBindState = new BindState();

    }

    public EnableCaps GetCaps()
    {
        return enableCaps;
    }

    public BindState GetState()
    {
        return currentBindState;
    }

    private void SetCaps(EnableCaps caps)
    {
        for (int i = 0; i < EnableCaps.Caps.Length; i++)
        {
            EnableCap cap = EnableCaps.Caps[i];
            bool shouldEnable = caps[cap];
            if (enableCaps[cap] != shouldEnable)
            {
                enableCaps[cap] = shouldEnable;
                if(shouldEnable)
                    GL.Enable(cap);
                else
                    GL.Disable(cap);
            }
        }
    }

    private void SetBindState(BindState bindState)
    {
        if(bindState.VertexArray != currentBindState.VertexArray)
            GL.BindVertexArray(bindState.VertexArray);
        
        if(bindState.Framebuffer != currentBindState.Framebuffer)
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, bindState.Framebuffer);
        
        if(bindState.DrawFramebuffer != currentBindState.DrawFramebuffer)
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, bindState.DrawFramebuffer);
        
        if(bindState.ReadFramebuffer != currentBindState.ReadFramebuffer)
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, bindState.ReadFramebuffer);
        
        if(bindState.ShaderProgram != currentBindState.ShaderProgram)
            GL.UseProgram(bindState.ShaderProgram);

        currentBindState = bindState;
    }

    private void DeviceMainLoop(Window window)
    {
        
        SDL.GLSetAttribute(SDL.GLAttr.ContextMajorVersion, 4);
        SDL.GLSetAttribute(SDL.GLAttr.ContextMinorVersion, 5);
        SDL.GLSetAttribute(SDL.GLAttr.ContextProfileMask, (int)SDL.GLProfile.Core);
        //SDL.GLSetAttribute(SDL.GLAttr.FrameBufferSRGBCapable, 1);
        SDL.GLSetAttribute(SDL.GLAttr.DoubleBuffer, 0);
        SDL.GLSetAttribute(SDL.GLAttr.AcceleratedVisual, 1);
#if !RELEASE
        SDL.GLSetAttribute(SDL.GLAttr.ContextFlags, (int)SDL.GLContextFlag.Debug);
#endif

        Context = SDL.GLCreateContext(window.Handle);
        SDL.GLMakeCurrent(window.Handle, Context);
        GLLoader.LoadBindings(new SDLBindingsContext());
        
        BuildState();
        
        setupFence.Signal();
        
        
        while (running)
        {
            if (tasks.TryDequeue(out var task))
            {
                Console.WriteLine("Running task!");
                RunTask(task);
                Console.WriteLine("Finished task!");
            }
            
        }
    }

    private void RunTask(GPUTask task)
    {
        if (task.Type == SpecialTaskType.Regular)
        {
            SetCaps(task.Caps);
            SetBindState(task.BindState);
            task.TaskDelegate(ref currentBindState, ref enableCaps);
        } else if (task.Type == SpecialTaskType.StateRebuilding)
        {
            BuildState();
        } else if (task.Type == SpecialTaskType.NoDelegate)
        {
            SetCaps(task.Caps);
            SetBindState(task.BindState);
        } else if (task.Type == SpecialTaskType.NoState)
        {
            task.TaskDelegate(ref currentBindState, ref enableCaps);
        }
    }

    public void Lock()
    {
        submitLock.Enter();
        lockEngaged = true;
    }

    public void Unlock()
    {
        submitLock.Exit();
        lockEngaged = false;
    }

    public CommandList BuildCommandList()
    {
        return new CommandList(this);
    }

    private readonly CommandList singleCommandList;

    public int GetInteger(GetPName name)
    {
        return WaitTaskStateless(() => GL.GetInteger(name), UInt64.MaxValue);
    }
    
    public string? GetString(StringName name)
    {
        return WaitTaskStateless(() => GL.GetString(name), UInt64.MaxValue);
    }

    public void WaitIdle(ulong timeout)
    {
        WaitTaskStateless(GL.Finish, timeout);
    }

    public void WaitTask(GPUTask task, ulong timeout)
    {
        singleCommandList.Clear();
        singleCommandList.Enqueue(task);
        singleCommandList.Fence(out var fence);
        singleCommandList.Submit();
        fence.Wait(timeout);
    }
    
    public void WaitTaskStateless(GPUTaskDelegate task, ulong timeout)
    {
        singleCommandList.Clear();
        singleCommandList.Enqueue(new GPUTask()
        {
            Type = SpecialTaskType.NoState,
            TaskDelegate = task
        });
        singleCommandList.Fence(out var fence);
        singleCommandList.Submit();
        fence.Wait(timeout);
    }
    
    public void WaitTaskStateless(Action task, ulong timeout)
    {
        singleCommandList.Clear();
        singleCommandList.Enqueue(new GPUTask()
        {
            Type = SpecialTaskType.NoState,
            TaskDelegate = (ref state, ref caps) =>
            {
                task();
            }
        });
        singleCommandList.Fence(out var fence);
        singleCommandList.Submit();
        fence.Wait(timeout);
    }
    
    public T WaitTaskStateless<T>(Func<T> task, ulong timeout)
    {
        T returnValue = default!;
        
        singleCommandList.Clear();
        singleCommandList.Enqueue(new GPUTask()
        {
            Type = SpecialTaskType.NoState,
            TaskDelegate = (ref state, ref caps) =>
            {
                returnValue = task();
            }
        });
        singleCommandList.Fence(out var fence);
        singleCommandList.Submit();
        fence.Wait(timeout);
        
        return returnValue;
    }

    public void MakeCurrent(IntPtr window)
    {
        SDL.GLMakeCurrent(window, Context);
    }

    public void Submit(GPUTask task)
    {
        if (IsOnDeviceThread)
        {
            RunTask(task);
            return;
        }
        
        if (submitLock.IsHeldByCurrentThread)
        {
            tasks.Enqueue(task);
            return;
        }

        if (!lockEngaged)
        {
            tasks.Enqueue(task);
            return;
        }
        
        Lock();
        tasks.Enqueue(task);
        Unlock();
    }

    public void Kill()
    {
        running = false;
    }
    
    
}

public struct GPUTask
{
    public EnableCaps Caps;
    public BindState BindState;
    public GPUTaskDelegate TaskDelegate;

    public SpecialTaskType Type;
}

public delegate void GPUTaskDelegate(ref BindState state, ref EnableCaps caps);

public enum SpecialTaskType
{
    Regular,
    NoDelegate,
    StateRebuilding,
    NoState
}


public struct BindState
{
    public int DrawFramebuffer;
    public int ReadFramebuffer;
    public int Framebuffer;
    public int VertexArray;

    public int ShaderProgram;
}

public struct EnableCaps
{
    public bool LineSmooth;
    public bool PolygonSmooth;
    public bool CullFace;
    public bool DepthTest;
    public bool StencilTest;
    public bool Dither;
    public bool Blend;
    public bool ColorLogicOp;
    public bool ScissorTest;
    public bool Texture1d;
    public bool Texture2d;
    public bool PolygonOffsetPoint;
    public bool PolygonOffsetLine;
    public bool ClipDistance0;
    public bool ClipDistance1;
    public bool ClipDistance2;
    public bool ClipDistance3;
    public bool ClipDistance4;
    public bool ClipDistance5;
    public bool ClipDistance6;
    public bool ClipDistance7;
    public bool Convolution1dExt;
    public bool Convolution2dExt;
    public bool Separable2dExt;
    public bool HistogramExt;
    public bool MinmaxExt;
    public bool PolygonOffsetFill;
    public bool RescaleNormalExt;
    public bool Texture3dExt;
    public bool VertexArray;
    public bool InterlaceSgix;
    public bool Multisample;
    public bool SampleAlphaToCoverage;
    public bool SampleAlphaToOne;
    public bool SampleCoverage;
    public bool TextureColorTableSgi;
    public bool ColorTableSgi;
    public bool PostConvolutionColorTableSgi;
    public bool PostColorMatrixColorTableSgi;
    public bool Texture4dSgis;
    public bool PixelTexGenSgix;
    public bool SpriteSgix;
    public bool ReferencePlaneSgix;
    public bool IrInstrument1Sgix;
    public bool CalligraphicFragmentSgix;
    public bool FramezoomSgix;
    public bool FogOffsetSgix;
    public bool SharedTexturePaletteExt;
    public bool DebugOutputSynchronous;
    public bool AsyncHistogramSgix;
    public bool PixelTextureSgis;
    public bool AsyncTexImageSgix;
    public bool AsyncDrawPixelsSgix;
    public bool AsyncReadPixelsSgix;
    public bool FragmentLightingSgix;
    public bool FragmentColorMaterialSgix;
    public bool FragmentLight0Sgix;
    public bool FragmentLight1Sgix;
    public bool FragmentLight2Sgix;
    public bool FragmentLight3Sgix;
    public bool FragmentLight4Sgix;
    public bool FragmentLight5Sgix;
    public bool FragmentLight6Sgix;
    public bool FragmentLight7Sgix;
    public bool TextureRectangle;
    public bool TextureCubeMap;
    public bool ProgramPointSize;
    public bool DepthClamp;
    public bool TextureCubeMapSeamless;
    public bool SampleShading;
    public bool RasterizerDiscard;
    public bool PrimitiveRestartFixedIndex;
    public bool FramebufferSrgb;
    public bool SampleMask;
    public bool PrimitiveRestart;
    public bool DebugOutput;
    public bool ShadingRateImagePerPrimitiveNv;

    public bool this[EnableCap cap]
    {
        get => GetCap(cap);
        set => SetCap(cap, value);
    }

    public bool GetCap(EnableCap cap)
    {
        return cap switch
        {
            EnableCap.LineSmooth => this.LineSmooth,
            EnableCap.PolygonSmooth => this.PolygonSmooth,
            EnableCap.CullFace => this.CullFace,
            EnableCap.DepthTest => this.DepthTest,
            EnableCap.StencilTest => this.StencilTest,
            EnableCap.Dither => this.Dither,
            EnableCap.Blend => this.Blend,
            EnableCap.ColorLogicOp => this.ColorLogicOp,
            EnableCap.ScissorTest => this.ScissorTest,
            EnableCap.Texture1d => this.Texture1d,
            EnableCap.Texture2d => this.Texture2d,
            EnableCap.PolygonOffsetPoint => this.PolygonOffsetPoint,
            EnableCap.PolygonOffsetLine => this.PolygonOffsetLine,
            EnableCap.ClipDistance0 => this.ClipDistance0,
            EnableCap.ClipDistance1 => this.ClipDistance1,
            EnableCap.ClipDistance2 => this.ClipDistance2,
            EnableCap.ClipDistance3 => this.ClipDistance3,
            EnableCap.ClipDistance4 => this.ClipDistance4,
            EnableCap.ClipDistance5 => this.ClipDistance5,
            EnableCap.ClipDistance6 => this.ClipDistance6,
            EnableCap.ClipDistance7 => this.ClipDistance7,
            EnableCap.Convolution1dExt => this.Convolution1dExt,
            EnableCap.Convolution2dExt => this.Convolution2dExt,
            EnableCap.Separable2dExt => this.Separable2dExt,
            EnableCap.HistogramExt => this.HistogramExt,
            EnableCap.MinmaxExt => this.MinmaxExt,
            EnableCap.PolygonOffsetFill => this.PolygonOffsetFill,
            EnableCap.RescaleNormalExt => this.RescaleNormalExt,
            EnableCap.Texture3dExt => this.Texture3dExt,
            EnableCap.VertexArray => this.VertexArray,
            EnableCap.InterlaceSgix => this.InterlaceSgix,
            EnableCap.Multisample => this.Multisample,
            EnableCap.SampleAlphaToCoverage => this.SampleAlphaToCoverage,
            EnableCap.SampleAlphaToOne => this.SampleAlphaToOne,
            EnableCap.SampleCoverage => this.SampleCoverage,
            EnableCap.TextureColorTableSgi => this.TextureColorTableSgi,
            EnableCap.ColorTableSgi => this.ColorTableSgi,
            EnableCap.PostConvolutionColorTableSgi => this.PostConvolutionColorTableSgi,
            EnableCap.PostColorMatrixColorTableSgi => this.PostColorMatrixColorTableSgi,
            EnableCap.Texture4dSgis => this.Texture4dSgis,
            EnableCap.PixelTexGenSgix => this.PixelTexGenSgix,
            EnableCap.SpriteSgix => this.SpriteSgix,
            EnableCap.ReferencePlaneSgix => this.ReferencePlaneSgix,
            EnableCap.IrInstrument1Sgix => this.IrInstrument1Sgix,
            EnableCap.CalligraphicFragmentSgix => this.CalligraphicFragmentSgix,
            EnableCap.FramezoomSgix => this.FramezoomSgix,
            EnableCap.FogOffsetSgix => this.FogOffsetSgix,
            EnableCap.SharedTexturePaletteExt => this.SharedTexturePaletteExt,
            EnableCap.DebugOutputSynchronous => this.DebugOutputSynchronous,
            EnableCap.AsyncHistogramSgix => this.AsyncHistogramSgix,
            EnableCap.PixelTextureSgis => this.PixelTextureSgis,
            EnableCap.AsyncTexImageSgix => this.AsyncTexImageSgix,
            EnableCap.AsyncDrawPixelsSgix => this.AsyncDrawPixelsSgix,
            EnableCap.AsyncReadPixelsSgix => this.AsyncReadPixelsSgix,
            EnableCap.FragmentLightingSgix => this.FragmentLightingSgix,
            EnableCap.FragmentColorMaterialSgix => this.FragmentColorMaterialSgix,
            EnableCap.FragmentLight0Sgix => this.FragmentLight0Sgix,
            EnableCap.FragmentLight1Sgix => this.FragmentLight1Sgix,
            EnableCap.FragmentLight2Sgix => this.FragmentLight2Sgix,
            EnableCap.FragmentLight3Sgix => this.FragmentLight3Sgix,
            EnableCap.FragmentLight4Sgix => this.FragmentLight4Sgix,
            EnableCap.FragmentLight5Sgix => this.FragmentLight5Sgix,
            EnableCap.FragmentLight6Sgix => this.FragmentLight6Sgix,
            EnableCap.FragmentLight7Sgix => this.FragmentLight7Sgix,
            EnableCap.TextureRectangle => this.TextureRectangle,
            EnableCap.TextureCubeMap => this.TextureCubeMap,
            EnableCap.ProgramPointSize => this.ProgramPointSize,
            EnableCap.DepthClamp => this.DepthClamp,
            EnableCap.TextureCubeMapSeamless => this.TextureCubeMapSeamless,
            EnableCap.SampleShading => this.SampleShading,
            EnableCap.RasterizerDiscard => this.RasterizerDiscard,
            EnableCap.PrimitiveRestartFixedIndex => this.PrimitiveRestartFixedIndex,
            EnableCap.FramebufferSrgb => this.FramebufferSrgb,
            EnableCap.SampleMask => this.SampleMask,
            EnableCap.PrimitiveRestart => this.PrimitiveRestart,
            EnableCap.DebugOutput => this.DebugOutput,
            EnableCap.ShadingRateImagePerPrimitiveNv => this.ShadingRateImagePerPrimitiveNv,
            _ => throw new NotSupportedException(),
        };
    }

    public void SetCap(EnableCap cap, bool value)
    {
        if (cap == EnableCap.LineSmooth) this.LineSmooth = value;
        else if (cap == EnableCap.PolygonSmooth) this.PolygonSmooth = value;
        else if (cap == EnableCap.CullFace) this.CullFace = value;
        else if (cap == EnableCap.DepthTest) this.DepthTest = value;
        else if (cap == EnableCap.StencilTest) this.StencilTest = value;
        else if (cap == EnableCap.Dither) this.Dither = value;
        else if (cap == EnableCap.Blend) this.Blend = value;
        else if (cap == EnableCap.ColorLogicOp) this.ColorLogicOp = value;
        else if (cap == EnableCap.ScissorTest) this.ScissorTest = value;
        else if (cap == EnableCap.Texture1d) this.Texture1d = value;
        else if (cap == EnableCap.Texture2d) this.Texture2d = value;
        else if (cap == EnableCap.PolygonOffsetPoint) this.PolygonOffsetPoint = value;
        else if (cap == EnableCap.PolygonOffsetLine) this.PolygonOffsetLine = value;
        else if (cap == EnableCap.ClipDistance0) this.ClipDistance0 = value;
        else if (cap == EnableCap.ClipDistance1) this.ClipDistance1 = value;
        else if (cap == EnableCap.ClipDistance2) this.ClipDistance2 = value;
        else if (cap == EnableCap.ClipDistance3) this.ClipDistance3 = value;
        else if (cap == EnableCap.ClipDistance4) this.ClipDistance4 = value;
        else if (cap == EnableCap.ClipDistance5) this.ClipDistance5 = value;
        else if (cap == EnableCap.ClipDistance6) this.ClipDistance6 = value;
        else if (cap == EnableCap.ClipDistance7) this.ClipDistance7 = value;
        else if (cap == EnableCap.Convolution1dExt) this.Convolution1dExt = value;
        else if (cap == EnableCap.Convolution2dExt) this.Convolution2dExt = value;
        else if (cap == EnableCap.Separable2dExt) this.Separable2dExt = value;
        else if (cap == EnableCap.HistogramExt) this.HistogramExt = value;
        else if (cap == EnableCap.MinmaxExt) this.MinmaxExt = value;
        else if (cap == EnableCap.PolygonOffsetFill) this.PolygonOffsetFill = value;
        else if (cap == EnableCap.RescaleNormalExt) this.RescaleNormalExt = value;
        else if (cap == EnableCap.Texture3dExt) this.Texture3dExt = value;
        else if (cap == EnableCap.VertexArray) this.VertexArray = value;
        else if (cap == EnableCap.InterlaceSgix) this.InterlaceSgix = value;
        else if (cap == EnableCap.Multisample) this.Multisample = value;
        else if (cap == EnableCap.SampleAlphaToCoverage) this.SampleAlphaToCoverage = value;
        else if (cap == EnableCap.SampleAlphaToOne) this.SampleAlphaToOne = value;
        else if (cap == EnableCap.SampleCoverage) this.SampleCoverage = value;
        else if (cap == EnableCap.TextureColorTableSgi) this.TextureColorTableSgi = value;
        else if (cap == EnableCap.ColorTableSgi) this.ColorTableSgi = value;
        else if (cap == EnableCap.PostConvolutionColorTableSgi) this.PostConvolutionColorTableSgi = value;
        else if (cap == EnableCap.PostColorMatrixColorTableSgi) this.PostColorMatrixColorTableSgi = value;
        else if (cap == EnableCap.Texture4dSgis) this.Texture4dSgis = value;
        else if (cap == EnableCap.PixelTexGenSgix) this.PixelTexGenSgix = value;
        else if (cap == EnableCap.SpriteSgix) this.SpriteSgix = value;
        else if (cap == EnableCap.ReferencePlaneSgix) this.ReferencePlaneSgix = value;
        else if (cap == EnableCap.IrInstrument1Sgix) this.IrInstrument1Sgix = value;
        else if (cap == EnableCap.CalligraphicFragmentSgix) this.CalligraphicFragmentSgix = value;
        else if (cap == EnableCap.FramezoomSgix) this.FramezoomSgix = value;
        else if (cap == EnableCap.FogOffsetSgix) this.FogOffsetSgix = value;
        else if (cap == EnableCap.SharedTexturePaletteExt) this.SharedTexturePaletteExt = value;
        else if (cap == EnableCap.DebugOutputSynchronous) this.DebugOutputSynchronous = value;
        else if (cap == EnableCap.AsyncHistogramSgix) this.AsyncHistogramSgix = value;
        else if (cap == EnableCap.PixelTextureSgis) this.PixelTextureSgis = value;
        else if (cap == EnableCap.AsyncTexImageSgix) this.AsyncTexImageSgix = value;
        else if (cap == EnableCap.AsyncDrawPixelsSgix) this.AsyncDrawPixelsSgix = value;
        else if (cap == EnableCap.AsyncReadPixelsSgix) this.AsyncReadPixelsSgix = value;
        else if (cap == EnableCap.FragmentLightingSgix) this.FragmentLightingSgix = value;
        else if (cap == EnableCap.FragmentColorMaterialSgix) this.FragmentColorMaterialSgix = value;
        else if (cap == EnableCap.FragmentLight0Sgix) this.FragmentLight0Sgix = value;
        else if (cap == EnableCap.FragmentLight1Sgix) this.FragmentLight1Sgix = value;
        else if (cap == EnableCap.FragmentLight2Sgix) this.FragmentLight2Sgix = value;
        else if (cap == EnableCap.FragmentLight3Sgix) this.FragmentLight3Sgix = value;
        else if (cap == EnableCap.FragmentLight4Sgix) this.FragmentLight4Sgix = value;
        else if (cap == EnableCap.FragmentLight5Sgix) this.FragmentLight5Sgix = value;
        else if (cap == EnableCap.FragmentLight6Sgix) this.FragmentLight6Sgix = value;
        else if (cap == EnableCap.FragmentLight7Sgix) this.FragmentLight7Sgix = value;
        else if (cap == EnableCap.TextureRectangle) this.TextureRectangle = value;
        else if (cap == EnableCap.TextureCubeMap) this.TextureCubeMap = value;
        else if (cap == EnableCap.ProgramPointSize) this.ProgramPointSize = value;
        else if (cap == EnableCap.DepthClamp) this.DepthClamp = value;
        else if (cap == EnableCap.TextureCubeMapSeamless) this.TextureCubeMapSeamless = value;
        else if (cap == EnableCap.SampleShading) this.SampleShading = value;
        else if (cap == EnableCap.RasterizerDiscard) this.RasterizerDiscard = value;
        else if (cap == EnableCap.PrimitiveRestartFixedIndex) this.PrimitiveRestartFixedIndex = value;
        else if (cap == EnableCap.FramebufferSrgb) this.FramebufferSrgb = value;
        else if (cap == EnableCap.SampleMask) this.SampleMask = value;
        else if (cap == EnableCap.PrimitiveRestart) this.PrimitiveRestart = value;
        else if (cap == EnableCap.DebugOutput) this.DebugOutput = value;
        else if (cap == EnableCap.ShadingRateImagePerPrimitiveNv) this.ShadingRateImagePerPrimitiveNv = value;
    }

    public static readonly EnableCap[] Caps =
    [
        EnableCap.LineSmooth,
        EnableCap.PolygonSmooth,
        EnableCap.CullFace,
        EnableCap.DepthTest,
        EnableCap.StencilTest,
        EnableCap.Dither,
        EnableCap.Blend,
        EnableCap.ColorLogicOp,
        EnableCap.ScissorTest,
        EnableCap.Texture1d,
        EnableCap.Texture2d,
        EnableCap.PolygonOffsetPoint,
        EnableCap.PolygonOffsetLine,
        EnableCap.ClipDistance0,
        EnableCap.ClipDistance1,
        EnableCap.ClipDistance2,
        EnableCap.ClipDistance3,
        EnableCap.ClipDistance4,
        EnableCap.ClipDistance5,
        EnableCap.ClipDistance6,
        EnableCap.ClipDistance7,
        EnableCap.Convolution1dExt,
        EnableCap.Convolution2dExt,
        EnableCap.Separable2dExt,
        EnableCap.HistogramExt,
        EnableCap.MinmaxExt,
        EnableCap.PolygonOffsetFill,
        EnableCap.RescaleNormalExt,
        EnableCap.Texture3dExt,
        EnableCap.VertexArray,
        EnableCap.InterlaceSgix,
        EnableCap.Multisample,
        EnableCap.SampleAlphaToCoverage,
        EnableCap.SampleAlphaToOne,
        EnableCap.SampleCoverage,
        EnableCap.TextureColorTableSgi,
        EnableCap.ColorTableSgi,
        EnableCap.PostConvolutionColorTableSgi,
        EnableCap.PostColorMatrixColorTableSgi,
        EnableCap.Texture4dSgis,
        EnableCap.PixelTexGenSgix,
        EnableCap.SpriteSgix,
        EnableCap.ReferencePlaneSgix,
        EnableCap.IrInstrument1Sgix,
        EnableCap.CalligraphicFragmentSgix,
        EnableCap.FramezoomSgix,
        EnableCap.FogOffsetSgix,
        EnableCap.SharedTexturePaletteExt,
        EnableCap.DebugOutputSynchronous,
        EnableCap.AsyncHistogramSgix,
        EnableCap.PixelTextureSgis,
        EnableCap.AsyncTexImageSgix,
        EnableCap.AsyncDrawPixelsSgix,
        EnableCap.AsyncReadPixelsSgix,
        EnableCap.FragmentLightingSgix,
        EnableCap.FragmentColorMaterialSgix,
        EnableCap.FragmentLight0Sgix,
        EnableCap.FragmentLight1Sgix,
        EnableCap.FragmentLight2Sgix,
        EnableCap.FragmentLight3Sgix,
        EnableCap.FragmentLight4Sgix,
        EnableCap.FragmentLight5Sgix,
        EnableCap.FragmentLight6Sgix,
        EnableCap.FragmentLight7Sgix,
        EnableCap.TextureRectangle,
        EnableCap.TextureCubeMap,
        EnableCap.ProgramPointSize,
        EnableCap.DepthClamp,
        EnableCap.TextureCubeMapSeamless,
        EnableCap.SampleShading,
        EnableCap.RasterizerDiscard,
        EnableCap.PrimitiveRestartFixedIndex,
        EnableCap.FramebufferSrgb,
        EnableCap.SampleMask,
        EnableCap.PrimitiveRestart,
        EnableCap.DebugOutput,
        EnableCap.ShadingRateImagePerPrimitiveNv,
    ];

}
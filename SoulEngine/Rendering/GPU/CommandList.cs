using OpenTK.Graphics.OpenGL;

namespace SoulEngine.Rendering;

public class CommandList
{
    private readonly Queue<GPUTask> tasks = new Queue<GPUTask>();
    private readonly GPUDevice device;

    
    public CommandList(GPUDevice device)
    {
        this.device = device;
    }

    private EnableCaps capState;
    private BindState bindState;
    
    private bool buildingState;
    
    public void SetState(EnableCaps capState, BindState bindState)
    {
        this.capState = capState;
        this.bindState = bindState;
    }

    public void SetCap(EnableCap cap, bool enabled)
    {
        capState[cap] = enabled;
        StateBuildingCheck();
    }

    public void BindShader(int shader)
    {
        bindState.ShaderProgram = shader;
        StateBuildingCheck();
    }

    public void BindVertexArray(int vertexArray)
    {
        bindState.VertexArray = vertexArray;
        StateBuildingCheck();
    }

    public void BindFramebuffer(int framebuffer)
    {
        bindState.Framebuffer = framebuffer;
        StateBuildingCheck();
    }

    private void StateBuildingCheck()
    {
        if (!buildingState)
        {
            Enqueue(new GPUTask
            {
                Type = SpecialTaskType.NoDelegate,
                BindState = bindState,
                Caps = capState
            });
        }
    }

    public void BeginBuildingState()
    {
        buildingState = true;
    }

    public void EndBuildingState()
    {
        buildingState = false;
        
        Enqueue(new GPUTask
        {
            Type = SpecialTaskType.NoDelegate,
            BindState = bindState,
            Caps = capState
        });
    }

    public void EnsureExecute()
    {
        Enqueue(new GPUTask
        {
            Type = SpecialTaskType.NoState,
            TaskDelegate = (ref state, ref caps) =>
            {
                GL.Flush();
            }
        });
    }
    
    public void WaitFinish()
    {
        Enqueue(new GPUTask
        {
            Type = SpecialTaskType.NoState,
            TaskDelegate = (ref state, ref caps) =>
            {
                GL.Finish();
            }
        });
    }

    public void Fence(out Fence fence)
    {
        Fence fenceInstance = new Fence(device);
        
        Enqueue(new GPUTask
        {
            Type = SpecialTaskType.NoState,
            TaskDelegate = (ref state, ref caps) =>
            {
                fenceInstance.Signal();
            }
        });

        fence = fenceInstance;
    }

    public void Enqueue(GPUTask task)
    {
        tasks.Enqueue(task);
    }
    
    public void EnqueueStated(GPUTaskDelegate task) 
    {
        Enqueue(new GPUTask
        {
            Type = SpecialTaskType.Regular,
            BindState = bindState,
            Caps = capState,
            TaskDelegate = task
        });    
    }
    
    public void EnqueueStateless(GPUTaskDelegate task) 
    {
        Enqueue(new GPUTask
        {
            Type = SpecialTaskType.NoState,
            TaskDelegate = task
        });    
    }

    public void Submit()
    {
        device.Lock();
        foreach (var task in tasks)
        {
            device.Submit(task);
        }
        device.Unlock();
    }

    public void Clear()
    {
        tasks.Clear();
        bindState = new BindState();
        capState = new EnableCaps();
        buildingState = false;
    }
}
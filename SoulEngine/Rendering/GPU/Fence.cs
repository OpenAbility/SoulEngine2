using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SoulEngine.Rendering;

public class Fence : IDisposable
{
    private bool disposed;
    
    internal GLSync GPUFenceHandle = new GLSync(IntPtr.Zero);
    private GPUDevice device;
    private bool signaled = false;

    public Fence(GPUDevice device)
    {
        this.device = device;
    }

    ~Fence()
    {
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        if(disposed)
            return;
        disposed = true;
        
        if(GPUFenceHandle.Value == IntPtr.Zero)
            return;
        
        device.Submit(new GPUTask()
        {
            Type = SpecialTaskType.NoState,
            TaskDelegate = (ref state, ref caps) =>
            {
                GL.DeleteSync(GPUFenceHandle);
            }
        });
    }

    internal void Signal()
    {
        if (GPUFenceHandle.Value != IntPtr.Zero)
            throw new Exception("Cannot signal GPU-side fence!");
        signaled = true;
    }

    public void Wait(ulong timeout)
    {
        if (GPUFenceHandle.Value != IntPtr.Zero)
        {
            SyncStatus status = GL.ClientWaitSync(GPUFenceHandle, SyncObjectMask.SyncFlushCommandsBit, timeout);

            if (status == SyncStatus.TimeoutExpired)
                throw new TimeoutException();
        }
        else
        {
            DateTime started = DateTime.Now;
            while (!signaled)
            {
                if((DateTime.Now - started).TotalNanoseconds > timeout)
                    throw new TimeoutException();
            }
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}
using System.Runtime.InteropServices;
using OpenTK;
using SDL3;

namespace SoulEngine.Rendering;

public class SDLBindingsContext : IBindingsContext
{
    public IntPtr GetProcAddress(string procName)
    {
        // Reverse SDL func pointer
        return Marshal.GetFunctionPointerForDelegate(SDL.GLGetProcAddress(procName));
    }
}
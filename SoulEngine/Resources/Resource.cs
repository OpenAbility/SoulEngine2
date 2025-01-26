using OpenTK.Graphics.Egl;
using SoulEngine.Content;
using SoulEngine.Core;

namespace SoulEngine.Resources;

/// <summary>
/// Base resource class
/// </summary>
public abstract class Resource : EngineObject
{
    public string ResourceID { get; internal set; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ResourceAttribute : Attribute
{
    public Type LoaderType;

    public ResourceAttribute(Type loaderType)
    {
        LoaderType = loaderType;
    }
}
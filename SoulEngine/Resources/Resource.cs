using OpenTK.Graphics.Egl;
using SoulEngine.Content;
using SoulEngine.Core;

namespace SoulEngine.Resources;

/// <summary>
/// Base resource class
/// </summary>
public abstract class Resource : EngineObject
{
    public string ResourceID { get; internal set; } = null!;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ResourceAttribute : Attribute
{
    public readonly Type? LoaderType;
    public readonly string TypeID;

    public ResourceAttribute(string typeID, Type? loaderType)
    {
        TypeID = typeID;
        LoaderType = loaderType;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ExpectedExtensionsAttribute : Attribute
{
    public readonly string[] Extensions;

    public ExpectedExtensionsAttribute(params string[] extensions)
    {
        Extensions = extensions;
    }
}
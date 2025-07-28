using System.Text;
using SoulEngine.Content;

namespace SoulEngine.Resources;

/// <summary>
/// Data passed into a resource to aid with loading the resource
/// </summary>
public readonly struct ResourceData(
    string resourcePath,
    string resourceTypeID,
    Stream resourceStream,
    ResourceManager resourceManager,
    ContentContext content)
{
    public readonly string ResourcePath = resourcePath;
    public readonly string ResourceTypeID = resourceTypeID;
    public readonly ResourceManager ResourceManager = resourceManager;
    public readonly ContentContext Content = content;

    public readonly Stream ResourceStream = resourceStream;

    public byte[] ReadResourceData()
    {
        if (ResourceStream is MemoryStream existingMemoryStream)
            return existingMemoryStream.ToArray();

        using var memoryStream = new MemoryStream();
        ResourceStream.CopyTo(memoryStream);
        
        return memoryStream.ToArray();
    }

    public string ReadResourceString() => ReadResourceString(Encoding.UTF8);
    
    public string ReadResourceString(Encoding encoding)
    {
        return encoding.GetString(ReadResourceData());
    }
}
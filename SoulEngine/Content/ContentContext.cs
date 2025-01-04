using System.Text;

namespace SoulEngine.Content;

public class ContentContext
{
    private readonly HashSet<IContentSource> contentSources = new HashSet<IContentSource>();

    public ContentContext()
    {
        
    }

    public void Mount(IContentSource contentSource)
    {
        contentSources.Add(contentSource);
    }

    public Stream? Load(string id)
    {
        foreach (var source in contentSources)
        {
            Stream? stream = source.LoadContent(id);
            if (stream != null)
                return stream;
        }

        return null;
    }

    public Stream[] LoadAll(string id)
    {
        List<Stream> streams = new List<Stream>();
        
        foreach (var source in contentSources)
        {
            Stream? stream = source.LoadContent(id);
            if (stream != null)
                streams.Add(stream);
        }

        return streams.ToArray();
    }

    private static byte[] LoadB(Stream stream)
    {
        MemoryStream ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public byte[] LoadBytes(string id)
    {
        Stream? stream = Load(id);
        if (stream == null)
            return [];

        return LoadB(stream);
    }
    
    public string LoadString(string id)
    {
        return Encoding.UTF8.GetString(LoadBytes(id));
    }
    
    public byte[][] LoadAllBytes(string id)
    {
        List<byte[]> arrays = new List<byte[]>();

        Stream[] streams = LoadAll(id);

        foreach (var s in streams)
        {
            arrays.Add(LoadB(s));
        }

        return arrays.ToArray();
    }
    
    public string[] LoadAllStrings(string id)
    {
        List<string> strings = new List<string>();

        byte[][] streams = LoadAllBytes(id);

        foreach (var s in streams)
        {
            strings.Add(Encoding.UTF8.GetString(s));
        }

        return strings.ToArray();
    }
    
}
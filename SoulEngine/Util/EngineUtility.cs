using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Web;
using OpenAbility.Logging;

namespace SoulEngine.Util;

public static class EngineUtility
{
    public static string JsonEscape(string s)
    {
        return HttpUtility.JavaScriptStringEncode(s);
    }
    
    public static string FormatBytes(long bytes)
    {
        string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
        int i;
        double dblSByte = bytes;
        for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024) 
        {
            dblSByte = bytes / 1024.0;
        }

        return $"{dblSByte:0.##} {Suffix[i]}";
    }
    
    public static IEnumerable<T> Run<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        IEnumerable<T> values = enumerable.ToList();
        foreach (T value in values)
        {
            action(value);
        }
        return values;
    }
    
    public static bool CanInstance(	
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicConstructors 
            | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] this Type type)
    {
        return type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes) != null && !type.IsAbstract;
    }

    public static object? Instantiate([DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors 
        | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] this Type type)
    {
        if (type.CanInstance())
            return Activator.CreateInstance(type);
        return false;
    }
	
    public static T? Instantiate<T>([DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors 
        | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] this Type type) where T : class
    {
        return type.Instantiate() as T;
    }
    
    public static IEnumerable<AttributeQueryResult<T>> GetAllTypeAttributes<T>() where T : Attribute
    {
        Logger logger = Logger.Get("TypeSearcher");
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                T? attrib = type.GetCustomAttribute<T>();
                if (attrib != null)
                    yield return new AttributeQueryResult<T>(type.GetCustomAttribute<T>()!, type);
            }
        }
    }

    public static string ReadToString(this Stream stream)
    {
        return stream.ReadToString(stream.Length);
    }
	
    public static string ReadToString(this Stream stream, long length)
    {
        StringBuilder result = new StringBuilder();
        byte[] readBuffer = ArrayPool<byte>.Shared.Rent((int)length);
        Array.Clear(readBuffer);
        int read;
        while ((read = stream.Read(readBuffer, 0, (int)length)) != 0)
        {
            result.Append(Encoding.UTF8.GetString(readBuffer));
        }
        ArrayPool<byte>.Shared.Return(readBuffer);
        return result.ToString();
    }

    public static T PickRandom<T>(this IEnumerable<T> enumerable)
    {
        var list = enumerable.ToList();
        return list[Random.Shared.Next(list.Count)];
    }
	
    public static T PickRandom<T>(this List<T> list)
    {
        return list[Random.Shared.Next(list.Count)];
    }
}

public struct AttributeQueryResult<T> where T : Attribute
{
    public T Attribute;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type Type;

    public AttributeQueryResult(T attribute, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
    {
        Attribute = attribute;
        Type = type;
    }
}
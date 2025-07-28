using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Web;
using OpenAbility.Logging;
using OpenTK.Mathematics;
using SoulEngine.Data.NBT;

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
    
    public static Matrix4 ArrayToMatrix(ReadOnlySpan<float> data)
    {
        Matrix4 matrix = new Matrix4();
        int idx = 0;
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                matrix[x, y] = data[idx++];
            }
        }
        return matrix;
    }
	
    public static Matrix4 ArrayToMatrix(float[,] data)
    {
        Matrix4 matrix = new Matrix4();
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                matrix[x, y] = data[x, y];
            }
        }
        return matrix;
    }
    
    public static void MatrixToArray(this Matrix4 matrix4, ref float[,] matrix)
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                matrix[x, y] = matrix4[x, y];
            }
        }
    }
	
    public static void MatrixToArray(this Matrix4 matrix4, ref float[] matrix)
    {
        int idx = 0;
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                matrix[idx++] = matrix4[x, y];
            }
        }
    }

    public static void CopyDirectory(string source, string destination)
    {
        var dir = new DirectoryInfo(source);

        if(!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(destination);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destination, file.Name);
            file.CopyTo(targetFilePath);
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            string newDestination = Path.Combine(destination, subDir.Name);
            CopyDirectory(subDir.FullName, newDestination);
        }
    }

    public static ListTag ToTag(this Matrix4 matrix4, string name)
    {
        ListTag matrixTag = new ListTag(name);
        float[] m = new float[16];
        
        matrix4.MatrixToArray(ref m);
        for (int i = 0; i < 16; i++)
        {
            matrixTag.Add(new FloatTag(null, m[i]));
        }

        return matrixTag;
    }
    
    public static Matrix4 ToMatrix(this ListTag matrixTag)
    {
        float[] m = new float[16];
        for (int i = 0; i < 16; i++)
        {
            m[i] = (matrixTag[i] as FloatTag)!.Data;
        }

        return ArrayToMatrix(m);
    }
    
    /// <summary>
    /// Get the level of inheritance from one type to another
    /// </summary>
    /// <param name="type">The type lower down in the chain</param>
    /// <param name="parentType">The base type</param>
    /// <returns>The number of types between, or -1 if no connection is found</returns>
    /// <remarks>Interfaces will not work</remarks>
    public static int GetInheritanceLevel(Type type, Type parentType)
    {
        if (!parentType.IsClass)
            return -1;
		
        int level = 0;
        Type? currentType = type;

        while (currentType != parentType)
        {
            if (currentType == typeof(object))
                return -1;
            if (currentType == null)
                return -1;
            currentType = currentType.BaseType;
        }
		
        return level;
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
using System.Text;
using SoulEngine.Core;

namespace SoulEngine.Content;

public class PackedContent
{
    private const uint Magic = 0x1BADDA7A;
    private const uint ExpectedVersion = 1;
    // The soft max limit to an archive - 128 megs
    private const ulong MaxArchiveSize = 1024 * 1024 * 128;
    // Just to identify the file
    private const uint ArchiveMagic = 0x2BADDA7A;
    
    private readonly string Name;

    private readonly Dictionary<ulong, FileLocation> files = new Dictionary<ulong, FileLocation>();
    private readonly Directory RootDirectory;

    public PackedContent(string name)
    {
        Name = name;

        // We load in the file
        using BinaryReader reader = new BinaryReader(File.OpenRead(Name + "_dir.cpak"), Encoding.UTF8, false);

        uint magic = reader.ReadUInt32();
        uint version = reader.ReadUInt32();
        
        if (magic != Magic)
            throw new Exception("Invalid file magic!");

        if (version != ExpectedVersion)
            throw new Exception("Unsupported CPAK version!");
        
        // Then we can start loading in the directory structure

        RootDirectory = LoadDirectory(reader, "", true);
    }
    
    public byte[] LoadIntoMemory(string id)
    {
        ulong hash = HashString(id);
        if (!files.TryGetValue(hash, out FileLocation fileLocation))
            return [];

        using FileStream fileStream = File.OpenRead(Name + "_" + fileLocation.ArchiveIndex + ".cpak");

        byte[] allocated = new byte[fileLocation.FileSize];

        fileStream.Position = (long)fileLocation.ArchiveOffset;
        fileStream.ReadExactly(allocated);

        return allocated;
    }

    public bool Has(string id)
    {
        ulong hash = HashString(id);
        return files.ContainsKey(hash);
    }
 

    private static ulong HashString(string s)
    {
        ulong hashedValue = 3074457345618258791ul;
        for (int i = 0; i < s.Length; i++)
        {
            hashedValue += s[i];
            hashedValue *= 3074457345618258799ul;
        }

        return hashedValue;
    }

    private Directory LoadDirectory(BinaryReader reader, string path, bool root)
    {
        Directory directory = new Directory();
        directory.Name = reader.ReadString();
        
        if(!root)
            path += directory.Name + "/";
        
        uint fileCount = reader.ReadUInt32();
        
        for (uint i = 0; i < fileCount; i++)
        {
            FileLocation fileLocation = new FileLocation();
            fileLocation.Name = reader.ReadString();
            fileLocation.ArchiveIndex = reader.ReadUInt32();
            fileLocation.ArchiveOffset = reader.ReadUInt64();
            fileLocation.FileSize = reader.ReadUInt64();

            string fullPath = root ? fileLocation.Name : path + fileLocation.Name;
            
            files[HashString(fullPath)] = fileLocation;
        }

        uint directoryCount = reader.ReadUInt32();
        for (int i = 0; i < directoryCount; i++)
        {
            Directory dir = LoadDirectory(reader, path, false);

            directory.directories[dir.Name] = dir;
        }

        return directory;
    }

    /// <summary>
    /// Packs the contents of a directory into a .cpak structure
    /// </summary>
    /// <param name="directory">The input directory to use</param>
    /// <param name="output">The output file(without any extension, just like how you'd do in the constructor)</param>
    public static void PackDirectory(string directory, string output)
    {
        using ContentPackContext context = new ContentPackContext(output, new DirectoryInfo(directory));
        context.Pack();
    }
    
    
    private class ContentPackContext : IDisposable
    {
        private readonly string basePath;
        private readonly BinaryWriter directoryWriter;
        private readonly DirectoryInfo rootDirectory;
        
        private uint currentArchiveIndex = 0;
        private Stream? currentArchive;
        private ulong currentArchiveOffset = 0;
        
        public ContentPackContext(string basePath, DirectoryInfo input)
        {
            this.basePath = basePath;
            this.rootDirectory = input;

            directoryWriter = new BinaryWriter(File.OpenWrite(basePath + "_dir.cpak"), Encoding.UTF8, false);
            directoryWriter.Write(Magic);
            directoryWriter.Write(ExpectedVersion);
        }

        public void Pack()
        {
            BeginArchive(0);
            AddDirectory(rootDirectory);
        }

        private void BeginArchive(uint index)
        {
            if (currentArchive != null)
            {
                currentArchive.Flush();
                currentArchive.Close();
            }

            currentArchiveIndex = index;
            currentArchive = File.OpenWrite(basePath + "_" + currentArchiveIndex + ".cpak");

            BinaryWriter writer = new BinaryWriter(currentArchive, Encoding.UTF8, true);
            writer.Write(ArchiveMagic);
            writer.Dispose();

            currentArchiveOffset = sizeof(uint);
        }

        private void AddToArchive(Stream stream)
        {
            if (currentArchiveOffset > MaxArchiveSize)
            {
                BeginArchive(currentArchiveIndex + 1);
            }
            
            stream.CopyTo(currentArchive!);
            currentArchiveOffset += (ulong)stream.Length;
        }

        private void AddFile(FileInfo fileInfo)
        {
            directoryWriter.Write(fileInfo.Name);
            directoryWriter.Write(currentArchiveIndex);
            directoryWriter.Write(currentArchiveOffset);
            directoryWriter.Write((ulong)fileInfo.Length);

            using FileStream stream = fileInfo.OpenRead();
            AddToArchive(stream);
        }

        private void AddDirectory(DirectoryInfo directoryInfo)
        {
            directoryWriter.Write(directoryInfo.Name);


            FileInfo[] files = directoryInfo.GetFiles();
            
            directoryWriter.Write((uint)files.Length);

            for (int i = 0; i < files.Length; i++)
            {
                AddFile(files[i]);
            }

            DirectoryInfo[] directories = directoryInfo.GetDirectories();

            directoryWriter.Write((uint)directories.Length);
            for (int i = 0; i < directories.Length; i++)
            {
                AddDirectory(directories[i]);
            }
        }

        public void Dispose()
        {
            directoryWriter.Dispose();
            currentArchive?.Dispose();
        }
    }
    
    
    
    
    
    private struct FileLocation
    {
        public string Name;
        
        public uint ArchiveIndex;
        public ulong ArchiveOffset;
        public ulong FileSize;
    }
    
    private struct Directory
    {
        public string Name;
        public Dictionary<string, FileLocation> files = new Dictionary<string, FileLocation>();
        public Dictionary<string, Directory> directories = new Dictionary<string, Directory>();

        public Directory()
        {
            Name = "";
        }
    }
}




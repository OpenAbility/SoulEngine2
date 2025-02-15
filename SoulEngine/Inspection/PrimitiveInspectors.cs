using ImGuiNET;
using SoulEngine.Data;
using SoulEngine.Data.NBT;
using SoulEngine.Resources;

namespace SoulEngine.Inspection;

[Serializable]
[Inspector(typeof(string))]
[NBTSerializer(typeof(string))]
public class StringInspector : Inspector<string>, INBTSerializer<string>
{
    public override string? Edit(string? instance, InspectionContext context)
    {
        instance ??= "";
        if (ImGui.InputText(context.AssociatedName ?? "", ref instance, 1024 * 1024))
            context.MarkEdited();
        return instance;
    }

    public Tag Serialize(string value, NBTSerializationContext context)
    {
        return new StringTag(null, value);
    }

    public string? Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (tag is StringTag stringTag)
            return stringTag.Value;
        return null;
    }
}

[Serializable]
[Inspector(typeof(bool))]
[NBTSerializer(typeof(bool))]
public class BoolInspector : Inspector<bool>, INBTSerializer<bool>
{
    public override bool Edit(bool instance, InspectionContext context)
    {
        if (ImGui.Checkbox(context.AssociatedName ?? "", ref instance))
            context.MarkEdited();
        return instance;
    }

    public Tag Serialize(bool value, NBTSerializationContext context)
    {
        return new ByteTag(null, value ? (byte)1 : (byte)0);
    }

    public bool Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (tag is ByteTag byteTag)
            return byteTag.Value != 0;
        return false;
    }
}

[Serializable]
[Inspector(typeof(byte))]
[NBTSerializer(typeof(byte))]
public unsafe class ByteInspector : Inspector<byte>, INBTSerializer<byte>
{
    public override byte Edit(byte instance, InspectionContext context)
    {
        if (ImGui.InputScalar(context.AssociatedName ?? "", ImGuiDataType.U8, new IntPtr(&instance)))
            context.MarkEdited();
        return instance;
    }

    public Tag Serialize(byte value, NBTSerializationContext context)
    {
        return new ByteTag(null, value);
    }

    public byte Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (tag is ByteTag byteTag)
            return byteTag.Value;
        return 0;
    }
}

[Serializable]
[Inspector(typeof(short))]
[NBTSerializer(typeof(short))]
public unsafe class ShortInspector : Inspector<short>, INBTSerializer<short>
{
    public override short Edit(short instance, InspectionContext context)
    {
        if (ImGui.InputScalar(context.AssociatedName ?? "", ImGuiDataType.S16, new IntPtr(&instance)))
            context.MarkEdited();
        return instance;
    }

    public Tag Serialize(short value, NBTSerializationContext context)
    {
        return new ShortTag(null, value);
    }

    public short Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (tag is ShortTag shortTag)
            return shortTag.Value;
        return 0;
    }
}

[Serializable]
[Inspector(typeof(int))]
[NBTSerializer(typeof(int))]
public unsafe class IntInspector : Inspector<int>, INBTSerializer<int>
{
    public override int Edit(int instance, InspectionContext context)
    {
        if (ImGui.InputScalar(context.AssociatedName ?? "", ImGuiDataType.S32, new IntPtr(&instance)))
            context.MarkEdited();
        return instance;
    }

    public Tag Serialize(int value, NBTSerializationContext context)
    {
        return new IntTag(null, value);
    }

    public int Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (tag is IntTag byteTag)
            return byteTag.Value;
        return 0;
    }
}

[Serializable]
[Inspector(typeof(long))]
[NBTSerializer(typeof(long))]
public unsafe class LongInspector : Inspector<long>, INBTSerializer<long>
{
    public override long Edit(long instance, InspectionContext context)
    {
        if (ImGui.InputScalar(context.AssociatedName ?? "", ImGuiDataType.S64, new IntPtr(&instance)))
            context.MarkEdited();
        return instance;
    }

    public Tag Serialize(long value, NBTSerializationContext context)
    {
        return new LongTag(null, value);
    }

    public long Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (tag is LongTag byteTag)
            return byteTag.Value;
        return 0;
    }
}

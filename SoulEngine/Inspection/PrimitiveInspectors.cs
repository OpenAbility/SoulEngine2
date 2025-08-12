using System.Numerics;
using Hexa.NET.ImGui;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Data.NBT;
using SoulEngine.Resources;
using SoulEngine.Util;
using Quaternion = OpenTK.Mathematics.Quaternion;
using Vector3 = OpenTK.Mathematics.Vector3;

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
        if (ImGui.InputScalar(context.AssociatedName ?? "", ImGuiDataType.U8, &instance))
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
        if (ImGui.InputScalar(context.AssociatedName ?? "", ImGuiDataType.S16, &instance))
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
        if (ImGui.InputScalar(context.AssociatedName ?? "", ImGuiDataType.S32, &instance))
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
        if (ImGui.InputScalar(context.AssociatedName ?? "", ImGuiDataType.S64, &instance))
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


[Serializable]
[Inspector(typeof(float))]
[NBTSerializer(typeof(float))]
public unsafe class FloatInspector : Inspector<float>, INBTSerializer<float>
{
    public override float Edit(float instance, InspectionContext context)
    {
        if (ImGui.InputScalar(context.AssociatedName ?? "", ImGuiDataType.Float, &instance))
            context.MarkEdited();
        return instance;
    }

    public Tag Serialize(float value, NBTSerializationContext context)
    {
        return new FloatTag(null, value);
    }

    public float Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (tag is FloatTag byteTag)
            return byteTag.Value;
        return 0;
    }
}

[Serializable]
[Inspector(typeof(Vector3))]
[NBTSerializer(typeof(Vector3))]
public unsafe class Vector3Inspector : Inspector<Vector3>, INBTSerializer<Vector3>
{
    public override Vector3 Edit(Vector3 instance, InspectionContext context)
    {
        if (ImGui.InputFloat3(context.AssociatedName ?? "", (float*)&instance))
            context.MarkEdited();
        return instance;
    }

    public Tag Serialize(Vector3 value, NBTSerializationContext context)
    {
        CompoundTag compound = new CompoundTag(null);
        compound.SetFloat("x", value.X);
        compound.SetFloat("y", value.Y);
        compound.SetFloat("z", value.Z);
        return compound;
    }

    public Vector3 Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (tag is CompoundTag compound)
            return new Vector3(compound.GetFloat("x") ?? 0, compound.GetFloat("y") ?? 0, compound.GetFloat("z") ?? 0);
        return Vector3.Zero;
    }
}

[Serializable]
[Inspector(typeof(Quaternion))]
[NBTSerializer(typeof(Quaternion))]
public unsafe class QuaternionInspector : Inspector<Quaternion>, INBTSerializer<Quaternion>
{
    public override Quaternion Edit(Quaternion instance, InspectionContext context)
    {
        if (ImGui.InputFloat4(context.AssociatedName ?? "", (float*)&instance))
            context.MarkEdited();
        return instance;
    }

    public Tag Serialize(Quaternion value, NBTSerializationContext context)
    {
        CompoundTag compound = new CompoundTag(null);
        compound.SetFloat("x", value.X);
        compound.SetFloat("y", value.Y);
        compound.SetFloat("z", value.Z);
        compound.SetFloat("w", value.W);
        return compound;
    }

    public Quaternion Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (tag is CompoundTag compound)
            return new Quaternion(compound.GetFloat("x") ?? 0, compound.GetFloat("y") ?? 0, compound.GetFloat("z") ?? 0,
                compound.GetFloat("w") ?? 0);
        return Quaternion.Identity;
    }
}

[Serializable]
[Inspector(typeof(Colour))]
[NBTSerializer(typeof(Colour))]
public unsafe class ColourInspector : Inspector<Colour>, INBTSerializer<Colour>
{
    public override Colour Edit(Colour instance, InspectionContext context)
    {
        if (ImGui.ColorEdit4(context.AssociatedName ?? "", (float*)&instance))
            context.MarkEdited();
        return instance;
    }

    public Tag Serialize(Colour value, NBTSerializationContext context)
    {
        CompoundTag compound = new CompoundTag(null);
        compound.SetFloat("r", value.R);
        compound.SetFloat("g", value.G);
        compound.SetFloat("b", value.B);
        compound.SetFloat("a", value.A);
        return compound;
    }

    public Colour Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (tag is CompoundTag compound)
            return new Colour(compound.GetFloat("r") ?? 0, compound.GetFloat("g") ?? 0, compound.GetFloat("b") ?? 0,
                compound.GetFloat("a") ?? 0);
        return Colour.Blank;
    }
    
}

[Serializable]
public unsafe class InvalidInspector : Inspector, INBTSerializer<object>
{
    public override object? Edit(object? instance, InspectionContext context)
    {
        ImGui.Text(instance?.ToString() ?? "null");
        return instance;
    }

    public Tag Serialize(object value, NBTSerializationContext context)
    {
        CompoundTag compound = new CompoundTag(null);
        return compound;
    }

    public object Deserialize(Tag tag, NBTSerializationContext context)
    {
        return null!;
    }
}

[Inspector(typeof(EngineObjectInspector))]
[NBTSerializer(typeof(EngineObjectInspector))]
[Serializable]
public class EngineObjectInspector : Inspector<EngineObject>, INBTSerializer<EngineObject>
{
    public override EngineObject? Edit(EngineObject? instance, InspectionContext context)
    {
        instance?.Edit();
        return instance;
    }

    public Tag Serialize(EngineObject value, NBTSerializationContext context)
    {
        return value.Save();
    }

    public EngineObject? Deserialize(Tag tag, NBTSerializationContext context)
    {
        EngineObject? instance = context.Type.Instantiate<EngineObject>();
        instance?.Load((CompoundTag)tag);
        return instance;
    }
}

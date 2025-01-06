using ImGuiNET;
using OpenTK.Mathematics;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class Vector3Property : SerializedProperty<Vector3>
{
    public Vector3Property(string name, Vector3 defaultValue) : base(name, defaultValue)
    {
    }


    public override unsafe void Edit()
    {
        System.Numerics.Vector3 num = new System.Numerics.Vector3(Value.X, Value.Y, Value.Z);
        if (ImGui.InputFloat3(Name, ref num))
        {
            Value.X = num.X;
            Value.Y = num.Y;
            Value.Z = num.Z;
        }
    }


    public override Tag Save()
    {
        CompoundTag compoundTag = new CompoundTag(Name);
        compoundTag.SetFloat("x", Value.X);
        compoundTag.SetFloat("y", Value.Y);
        compoundTag.SetFloat("z", Value.Z);
        return compoundTag;
    }

    public override void Load(Tag tag)
    {
        CompoundTag compoundTag = (CompoundTag)tag;
        Value = new Vector3(
            compoundTag.GetFloat("x") ?? 0,
            compoundTag.GetFloat("y") ?? 0,
            compoundTag.GetFloat("z") ?? 0
        );
    }
}
using Hexa.NET.ImGui;
using OpenTK.Mathematics;
using SoulEngine.Data.NBT;
using SoulEngine.Mathematics;
using Vector2 = System.Numerics.Vector2;

namespace SoulEngine.Props;

public class QuaternionProperty : SerializedProperty<Quaternion>
{
    public QuaternionProperty(string name, Quaternion defaultValue) : base(name, defaultValue)
    {
    }


    public override unsafe void Edit()
    {

        if (ImGui.BeginChild(Name, new Vector2(0, 0), ImGuiChildFlags.AlwaysAutoResize | ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeX | ImGuiChildFlags.AutoResizeY))
        {
            ImGui.Text(Name);

            var euler = Value.ToEulerAngles() * Mathf.Rag2Deg;
            System.Numerics.Vector3 eulerProp = new System.Numerics.Vector3(euler.X, euler.Y, euler.Z);
            if (ImGui.InputFloat3("Euler", ref eulerProp))
            {
                Value = Quaternion.FromEulerAngles(eulerProp.X * Mathf.Deg2Rad, eulerProp.Y * Mathf.Deg2Rad,
                    eulerProp.Z * Mathf.Deg2Rad);
            }


            System.Numerics.Vector4 num = new System.Numerics.Vector4(Value.X, Value.Y, Value.Z, Value.W);
            if (ImGui.InputFloat4("Quaternion", ref num))
            {
                Value.X = num.X;
                Value.Y = num.Y;
                Value.Z = num.Z;
                Value.W = num.W;
            }
            
            ImGui.EndChild();
        }
    }


    public override Tag Save()
    {
        CompoundTag compoundTag = new CompoundTag(Name);
        compoundTag.SetFloat("x", Value.X);
        compoundTag.SetFloat("y", Value.Y);
        compoundTag.SetFloat("z", Value.Z);
        compoundTag.SetFloat("w", Value.W);
        return compoundTag;
    }

    public override void Load(Tag tag)
    {
        CompoundTag compoundTag = (CompoundTag)tag;
        Value = new Quaternion(
            compoundTag.GetFloat("x") ?? 0,
            compoundTag.GetFloat("y") ?? 0,
            compoundTag.GetFloat("z") ?? 0,
            compoundTag.GetFloat("w") ?? 0
        );
    }
}
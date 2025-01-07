using System.Numerics;
using System.Reflection;
using ImGuiNET;
using SoulEngine.Data.NBT;

namespace SoulEngine.Props;

public class EnumProperty<T> : SerializedProperty<T> where T : struct, Enum
{
    private static readonly bool Flags;
    private static readonly T[] Values;

    static EnumProperty()
    {
        Flags = typeof(T).GetCustomAttribute<FlagsAttribute>() != null;
        Values = Enum.GetValues<T>();
    }
    
    public EnumProperty(string name, T defaultValue) : base(name, defaultValue)
    {
        
    }

    
    public override unsafe void Edit()
    {
        if (Flags)
        {
            
            ImGui.Text(Name);
            if (ImGui.BeginChild(Name + "##Basket", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                    ImGuiChildFlags.Border | ImGuiChildFlags.AutoResizeY))
            {
                uint v = Convert.ToUInt32(Value);

                foreach (var value in Values)
                {
                    uint fv = Convert.ToUInt32(value);
                    ImGui.CheckboxFlags(value.ToString(), ref v, fv);
                }

                Value = (T)Enum.ToObject(typeof(T), v);

            }
            ImGui.EndChild();
            
        }
        else
        {

            if (ImGui.BeginCombo(Name, Value.ToString()))
            {

                for (int i = 0; i < Values.Length; i++)
                {
                    bool selected = Equals(Values[i], Value);
                    if (ImGui.Selectable(Values[i].ToString(), selected))
                        Value = Values[i];
                    if(selected)
                        ImGui.SetItemDefaultFocus();
                }
                
                ImGui.EndCombo();
            }
            
        }
    }
    

    public override Tag Save()
    {
        return new LongTag(Name, Convert.ToUInt32(Value));
    }

    public override void Load(Tag tag)
    {
        var rep = ((LongTag)tag).Data;
        Value = (T)Enum.ToObject(typeof(T), rep);
    }
}
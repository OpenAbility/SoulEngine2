using ImGuiNET;
using SoulEngine.Data;
using SoulEngine.Data.NBT;
using SoulEngine.Props;

namespace SoulEngine.Inspection;

[Inspector(typeof(Prop))]
[NBTSerializer(typeof(Prop))]
[Serializable]
public class PropInspector : Inspector<Prop>, INBTSerializer<Prop>
{
    public override Prop? Edit(Prop? instance, InspectionContext context)
    {
        if (context.Scene == null)
        {
            ImGui.Text(context.AssociatedName ?? "" + " = "  + (instance == null ? "null" : instance.Name));
            return instance;
        }
        
        if (ImGui.BeginCombo(context.AssociatedName ?? "", instance?.Name ?? ""))
        {
            foreach (var prop in context.Scene.Props)
            {
                if (context.Type.IsInstanceOfType(prop))
                {
                    if (ImGui.Selectable(prop.Name))
                    {
                        instance = prop;
                        context.MarkEdited();
                    }

                    if (ImGui.BeginItemTooltip())
                    {
                        ImGui.Text(prop.ToString() ?? "no string tooltip found");
                        ImGui.EndTooltip();
                    }
                }
            }
            
            ImGui.EndCombo();
        }

        return instance;
    }

    public Tag Serialize(Prop value, NBTSerializationContext context)
    {
        return new StringTag(value.Name);
    }

    public Prop? Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (context.Scene == null)
            return null;
        return context.Scene.GetProp(((StringTag)tag).Data, context.Type);
    }
}
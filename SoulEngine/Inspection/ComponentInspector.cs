using Hexa.NET.ImGui;
using SoulEngine.Components;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Data.NBT;
using SoulEngine.Entities;

namespace SoulEngine.Inspection;

[Inspector(typeof(Component))]
[NBTSerializer(typeof(Component))]
[Serializable]
public class ComponentInspector : Inspector<Component>, INBTSerializer<Component>
{
    public override Component? Edit(Component? instance, InspectionContext context)
    {
        if (context.Scene == null)
        {
            ImGui.Text(context.AssociatedName ?? "" + " = "  + (instance == null ? "null" : instance.Entity.Name));
            return instance;
        }

        if (ImGui.BeginCombo(context.AssociatedName ?? "", instance?.Entity.Name ?? ""))
        {
            foreach (var entity in context.Scene.Entities)
            {

                int index = 0;
                foreach (var component in entity.GetComponents<Component>())
                {
                    if (context.Type.IsInstanceOfType(component))
                    {
                        if (ImGui.Selectable(entity.Name + " (" + index + ")"))
                        {
                            instance = component;
                            context.MarkEdited();
                        }

                        if (ImGui.BeginItemTooltip())
                        {
                            ImGui.Text(entity.ToString() ?? "no string tooltip found");
                            ImGui.EndTooltip();
                        }
                    }
                }

            }

            ImGui.EndCombo();
        }

        return instance;
    }

    public Tag Serialize(Component value, NBTSerializationContext context)
    {
        CompoundTag compoundTag = new CompoundTag("PROP");
        compoundTag.SetString("entity", value.Entity.Name);
        compoundTag.SetInt("index", value.Entity.IndexOfComponent(value));

        return compoundTag;
    }

    public Component? Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (context.Scene == null)
            return null;

        CompoundTag compoundTag = (CompoundTag)tag;

        Entity entity = context.Scene.GetEntity(compoundTag.GetString("entity")!)!;
        return entity.IndexedComponent(compoundTag.GetInt("index")!.Value);
    }
}
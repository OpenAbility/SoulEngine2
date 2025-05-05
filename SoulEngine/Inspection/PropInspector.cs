using Hexa.NET.ImGui;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Data.NBT;
using SoulEngine.Entities;
using SoulEngine.Props;

namespace SoulEngine.Inspection;

[Inspector(typeof(Entity))]
[NBTSerializer(typeof(Entity))]
[Serializable]
public class EntityInspector : Inspector<Entity>, INBTSerializer<Entity>
{
    public override Entity? Edit(Entity? instance, InspectionContext context)
    {
        if (context.Scene == null)
        {
            ImGui.Text(context.AssociatedName ?? "" + " = "  + (instance == null ? "null" : instance.Name));
            return instance;
        }
        
        if (ImGui.BeginCombo(context.AssociatedName ?? "", instance?.Name ?? ""))
        {
            foreach (var entity in context.Scene.Entities)
            {
                if (context.Type.IsInstanceOfType(entity))
                {
                    if (ImGui.Selectable(entity.Name))
                    {
                        instance = entity;
                        context.MarkEdited();
                    }

                    if (ImGui.BeginItemTooltip())
                    {
                        ImGui.Text(entity.ToString() ?? "no string tooltip found");
                        ImGui.EndTooltip();
                    }
                }
            }
            
            ImGui.EndCombo();
        }

        return instance;
    }

    public Tag Serialize(Entity value, NBTSerializationContext context)
    {
        return new StringTag(value.Name);
    }

    public Entity? Deserialize(Tag tag, NBTSerializationContext context)
    {
        if (context.Scene == null)
            return null;
        return context.Scene.GetEntity(((StringTag)tag).Data);
    }
}
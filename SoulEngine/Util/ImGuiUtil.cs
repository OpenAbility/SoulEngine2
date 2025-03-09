using System.Numerics;
using Hexa.NET.ImGui;
using SoulEngine.Rendering;

namespace SoulEngine.Util;

public static class ImGuiUtil
{
    public static bool ImageSelectable(Texture? texture, string label, bool selected)
    {
        return ImageSelectable(texture, ImGui.GetFontSize(), label, selected);
    }
    
    public static bool ImageSelectable(Texture? texture, float imageSize, string label, bool selected)
    {
        var cursorPos = ImGui.GetCursorPos();

        if (texture != null)
        {
            label = new string(' ', (int)(imageSize / ImGui.GetFont().GetCharAdvance(' '))) + " " + label;
        }

        bool marked = ImGui.Selectable(label, selected);
        
        ImGuiLastItemDataPtr ptr = ImGuiP.ImGuiLastItemData();
        
        var afterCursor = ImGui.GetCursorPos();

        ImGui.Dummy(afterCursor - cursorPos);
        ImGui.SetCursorPos(cursorPos);

        
        if(texture != null)
            ImGui.Image(new ImTextureID(texture.Handle), new Vector2(imageSize), new Vector2(0, 1), new Vector2(1, 0));


        
        //ImGuiP.SetLastItemData(ImGui.GetID(label), ptr.ItemFlags, ptr.StatusFlags, new ImRect(cursorPos, afterCursor));
        
        ImGui.SetCursorPos(afterCursor);
        return marked;
    }
}
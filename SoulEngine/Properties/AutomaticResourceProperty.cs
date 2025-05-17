using System.Reflection;
using Hexa.NET.ImGui;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Data.NBT;
using SoulEngine.Resources;
using SoulEngine.Util;

namespace SoulEngine.Props;

public class AutomaticResourceProperty<T> : SerializedProperty where T : Resource
{

    private readonly MemberWrapper wrapper;
    private T? resetValue;
    private readonly object? target;

    private string? editedID;

    private readonly ContentBrowser<T> browser;
    
    public AutomaticResourceProperty(string name, MemberWrapper wrapper, object? target) : base(name)
    {
        this.wrapper = wrapper;
        this.target = target;

        browser = new ContentBrowser<T>(Game.Current);
        browser.Callback = (value) =>
        {
            wrapper.SetValue(target, value);
            editedID = value?.ResourceID;
        };
    }

    public override void Edit()
    {
        editedID ??= (wrapper.GetValue(target) as T)?.ResourceID ?? "";
        
        if (ImGui.InputText(Name, ref editedID, 2048, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            editedID = editedID.Trim();

            if (editedID == "")
            {
                wrapper.SetValue(target, null);
            }
            else
            {
                T value = ResourceManager.Global.Load<T>(editedID);
                wrapper.SetValue(target, value);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Browse"))
        {
            browser.Show();
        }
    }

    public override void MakeCurrentReset()
    {
        resetValue = wrapper.GetValue(target) as T;
    }

    public override void Reset()
    {
        wrapper.SetValue(target, resetValue);
    }
    

    public override Tag Save()
    {
        T? value = wrapper.GetValue(target) as T;
        return new StringTag(null!, value?.ResourceID ?? "!null");
    }

    public override void Load(Tag tag)
    {
        if (tag is StringTag stringTag)
        {
            if (stringTag.Value == "!null")
            {
                wrapper.SetValue(target, null);
                resetValue = null;
            }
            else
            {
                T value = ResourceManager.Global.Load<T>(stringTag.Value);
                wrapper.SetValue(target, value);
            }
        }
    }
}
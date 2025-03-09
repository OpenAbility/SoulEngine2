using Hexa.NET.ImGui;
using OpenAbility.Logging;

namespace SoulEngine;

public class MenuContext
{
	private static readonly Logger Logger = Logger.Get<MenuContext>();
	
	private readonly Dictionary<string, MenuObject> menus = new Dictionary<string, MenuObject>();

	public bool WillDraw
	{
		get
		{
			return menus.Count > 0;
		}
	}
	
	public bool IsFlagSet(string path)
	{
		return IsFlagSet(path.Split("/"));
	}
	
	public bool IsFlagSet(params string[] path)
	{
		return GetMenu(path, MenuObjectType.Checkbox).Value is true;
	}
	
	public void IsFlagSet(ref bool flag, params string[] path)
	{
		flag = GetMenu(path, MenuObjectType.Checkbox, flag).Value is true;
	}
	
	public bool IsPressed(string path)
	{
		return IsPressed(path.Split("/"));
	}
	
	public bool IsPressed(params string[] path)
	{
		return GetMenu(path, MenuObjectType.Button).Value is true;
	}
	
	public void SetVisible(bool visible, string path)
	{
		SetVisible(visible, path.Split("/"));
	}

	public void SetVisible(bool visible, params string[] path)
	{
		GetMenu(path, MenuObjectType.Button).Hidden = !visible;
	}

	private MenuObject GetMenu(string[] path, MenuObjectType type, bool defaultValue = false)
	{
		if (path.Length < 2)
			throw new Exception("Invalid menu path: Not child of root menu");

		if (!menus.TryGetValue(path[0], out MenuObject? menu))
		{
			Logger.Debug("Creating new root menu: {}", path[0]);
			menu = new MenuObject
			{
				ID = path[0],
				Type = MenuObjectType.Container
			};
			menus[path[0]] = menu;
		}
		
		for (int i = 1; i < path.Length; i++)
		{
			bool last = i == path.Length - 1;
			
			if (!menu.Children.TryGetValue(path[i], out MenuObject? child))
			{
				Logger.Debug("Creating new item: {}", path[i]);
				child = new MenuObject
				{
					ID = path[i],
					Type = last ? type : MenuObjectType.Container,
					Value = last ? defaultValue : null
				};
				menu.Children[path[i]] = child;
			}

			if (!last && child.Type != MenuObjectType.Container)
				throw new Exception("Cannot get child of non-container menu!");

			if (last && child.Type != type)
				throw new Exception("Menu is already predefined as other type");

			menu = child;
		}

		return menu;
	}

	public void Draw()
	{
		foreach (var m in menus)
		{
			m.Value.Render();
		}
	}

	private class MenuObject
	{
		public Dictionary<string, MenuObject> Children = new Dictionary<string, MenuObject>();
		public object? Value;
		public string ID;
		public bool Hidden;
		public MenuObjectType Type = MenuObjectType.Container;

		public void NotRendered()
		{
			if (Type == MenuObjectType.Container)
			{
				foreach (var c in Children)
				{
					c.Value.NotRendered();
				}
			}
			else if (Type == MenuObjectType.Button)
			{
				Value = false;
			}
		}

		public void Render()
		{
			if(Hidden)
				return;
			if (Type == MenuObjectType.Container)
			{
				if (ImGui.BeginMenu(ID))
				{
					foreach (var c in Children)
					{
						c.Value.Render();
					}
					ImGui.EndMenu();
				}
				else
				{
					foreach (var c in Children)
					{
						c.Value.NotRendered();
					}
				}
				
			} else if (Type == MenuObjectType.Button)
			{
				Value = ImGui.MenuItem(ID);
			} else if (Type == MenuObjectType.Checkbox)
			{
				if (Value == null)
					Value = false;
				bool enabled = (bool)Value;
				
				if (ImGui.Checkbox(ID, ref enabled))
				{
					Value = enabled;
				}
			}
		}
	}

	private enum MenuObjectType
	{
		Container,
		Checkbox,
		Button,
	}
}
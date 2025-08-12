using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Components;
using SoulEngine.Core;
using SoulEngine.Data.NBT;
using SoulEngine.Mathematics;
using SoulEngine.Props;
using SoulEngine.Renderer;
using SoulEngine.Rendering;
using SoulEngine.Util;

namespace SoulEngine.Entities;

[Serializable]
public class Entity : EngineObject, ITransformable
{
    
    private readonly List<Component> components = new List<Component>();
    private readonly HashSet<string> tags = new HashSet<string>();

    [SerializedProperty("position")] public Vector3 Position { get; set; } = Vector3.Zero;
    

    [SerializedProperty("rotation")]  public Quaternion RotationQuat { get; set; } = Quaternion.Identity;

    public Vector3 RotationEuler
    {
        get => RotationQuat.ToEulerAngles() * Mathx.Rag2Deg;
        set => RotationQuat = Quaternion.FromEulerAngles(value * Mathx.Deg2Rad);
    }

    [SerializedProperty("scale")] public Vector3 Scale { get; set; } = Vector3.One;

    public Matrix4 LocalMatrix
    {
        get => Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(RotationQuat) *
               Matrix4.CreateTranslation(Position);
        set
        {
            Scale = value.ExtractScale();
            Position = value.ExtractTranslation();
            RotationQuat = value.ExtractRotation();
        }
    } 

    public ITransformable? Parent;

    public Matrix4 GlobalMatrix
    {
        get =>  Parent?.GlobalMatrix ?? Matrix4.Identity * LocalMatrix;
        set => LocalMatrix = (Parent?.GlobalMatrix ?? Matrix4.Identity).Inverted() * value;
    }
    
    
    public Vector3 Forward => (RotationQuat * -Vector3.UnitZ).Normalized();
    public Vector3 Up => (RotationQuat * Vector3.UnitY).Normalized();
    public Vector3 Right => (RotationQuat * Vector3.UnitX).Normalized();
    
    /// <summary>
    /// The name of the property
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The scene that this prop is in
    /// </summary>
    public Scene Scene { get; internal set; }

    private Texture? icon;

    internal Texture? Icon
    {
        get
        {
            if (icon == null)
            {
                foreach (var component in GetComponents<Component>())
                {
                    if (component.propIcon != null)
                        icon ??= component.propIcon;
                }

                if (icon == null)
                    icon = Scene.Game.ResourceManager.Load<Texture>("icons/object.png");
            }

            return icon;
        }
    }

    public bool WasCulled { get; internal set; } = false;


    private readonly Dictionary<string, SerializedProperty> properties = new Dictionary<string, SerializedProperty>();
    
    // Events
    public event Action<Entity> OnEnterScene = e => { };
    public event Action<Entity> OnLeaveScene = e => { };
    public event Action<Entity, IRenderPipeline, float> OnRender = (e, pipeline, dt) => { };
    public event Action<Entity, float> OnUpdate = (e, dt) => { };
    public event Action<Entity> OnReset = (e) => { };
    public event Action<Entity> OnEdit = (e) => { };



    public Entity(Scene scene, string name)
    {
        Name = name;
        Scene = scene;
        
        
        PropertyReflection.RegisterProperties(scene, this, p => Register(p));
    }

    public void Rotate(Vector3 euler)
    {
        RotationQuat *= Quaternion.FromEulerAngles(euler);
    }

    public void Rotate(Quaternion quaternion)
    {
        RotationQuat *= quaternion;
    }
    
    private T Register<T>(T property) where T : SerializedProperty
    {
        properties[property.Name] = property;
        return property;
    }
    
    

    public T Attach<T>(T component) where T : Component
    {
        icon = null;
        components.Add(component);
        return component;
    }
    
    public void Detach(Component component)
    {
        icon = null;
        components.Remove(component);
    }
    
    public int IndexOfComponent(Component value)
    {
        return components.IndexOf(value);
    }

    public Component IndexedComponent(int index)
    {
        return components[index];
    }
    

    public T? GetComponent<T>() where T : Component
    {
        return components.FirstOrDefault(c => c is T) as T;
    }
    
    public IEnumerable<T> GetComponents<T>() where T : Component
    {
        return components.Where(c => c is T).Cast<T>();
    }

    public override CompoundTag Save()
    {
        CompoundTag tag = new CompoundTag(Name);
        

        foreach (var p in properties)
        {
            tag[p.Key] = p.Value.Save();
        }
        
        ListTag componentsTag = new ListTag("components");
        tag.Add(componentsTag);

        foreach (var component in components.ToArray())
        {
            componentsTag.Add(component.Save());
        }

        return tag;
    }

    public override void Load(CompoundTag tag)
    {
        foreach (var p in properties)
        {
            if (tag.TryGetValue(p.Key, out Tag? propertyTag))
            {
                p.Value.Load(propertyTag);
                p.Value.MakeCurrentReset();
            }
        }

        ListTag componentsTag = tag.GetTag<ListTag>("components")!;

        List<(Component component, CompoundTag tag)> tags = new List<(Component component, CompoundTag tag)>();
        
        foreach (var componentTag in componentsTag.Cast<CompoundTag>())
        {
            string componentType = componentTag.GetString("$_type")!;
            Component component = ComponentLoader.Create(this, componentType);
            components.Add(component);
            
            tags.Add((component, componentTag));
        }

        foreach (var ctag in tags)
        {
            ctag.component.Load(ctag.tag);
        }
    }

    public void Render(IRenderPipeline renderPipeline, float deltaTime)
    {
        foreach (var component in components.ToArray())
        {
            component.Render(renderPipeline, deltaTime);
        }
    }

    public void Update(float deltaTime)
    {
        foreach (var component in components.ToArray())
        {
            component.Update(deltaTime);
        }

        OnUpdate(this, deltaTime);
    }

    public void EnterScene()
    {
        foreach (var component in components.ToArray())
        {
            component.EnterScene();
        }

        OnEnterScene(this);
    }
    
    public void LeaveScene()
    {
        foreach (var component in components.ToArray())
        {
            component.LeaveScene();
        }

        OnLeaveScene(this);
    }

    
    public override void Edit()
    {
        string n = Name;
        if (ImGui.InputText("name", ref n, 2048))
            Name = n;
        ImGui.SeparatorText("properties");
        
        foreach (var p in properties.Values)
        {
            p.Edit();
        }
        
        ImGui.SeparatorText("Components");
        foreach (var component in components.ToArray())
        {
            component.Edit();
        }

        OnEdit(this);
        
        if (ImGui.BeginPopupContextWindow())
        {
            foreach (var component in ComponentLoader.Types)
            {
                if (ImGui.Selectable(component))
                {
                    components.Add(ComponentLoader.Create(this, component));
                }
            }
                    
            ImGui.EndPopup();
        }
    }

    public void Reset()
    {
        foreach (var p in properties.Values)
        {
            p.Reset();
        }
        
        foreach (var component in components.ToArray())
        {
            component.Reset();
        }

        OnReset(this);
    }
    
    /// <summary>
    /// Returns the bounding box used for rendering, does not need to be translated to world-space as the entity handles that
    /// </summary>
    /// <returns>The component AABB</returns>
    public AABB RenderingBoundingBox()
    {
        AABB aabb = AABB.InvertedInfinity;
        
        foreach (var component in components)
        {
            AABB componentBox = component.RenderingBoundingBox();
            aabb.PushPoint(componentBox.Min);
            aabb.PushPoint(componentBox.Max);
        }

        if (aabb.Invalid)
            aabb = new AABB();

        aabb = aabb.Translated(GlobalMatrix);

        return aabb;
    }

    public void RenderGizmo(GizmoContext context)
    {
        
        if(Icon != null)
            context.BillboardedSprite(Icon);
        context.Begin(PrimitiveType.Lines);
        
        context.Vertex(Vector3.Zero, Colour.Red);
        context.Vertex(Vector3.UnitX, Colour.Red);
        
        context.Vertex(Vector3.Zero, Colour.Green);
        context.Vertex(Vector3.UnitY, Colour.Green);
        
        context.Vertex(Vector3.Zero, Colour.Blue);
        context.Vertex(-Vector3.UnitZ, Colour.Blue);

        context.End();
        
        
        foreach (var component in components.ToArray())
        {
            component.RenderGizmo(context);
        }
    }
    
    
    public void RenderMoveGizmo(Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        float[] view = new float[16];
        viewMatrix.MatrixToArray(ref view);

        float[] projection = new float[16];
        projectionMatrix.MatrixToArray(ref projection);

        float[] model = new float[16];
        LocalMatrix.MatrixToArray(ref model);

        ImGuizmo.SetID(GetHashCode());
        if (ImGuizmo.Manipulate(ref view[0], ref projection[0],
                ImGuizmoOperation.Translate | ImGuizmoOperation.Rotate | ImGuizmoOperation.Scale, ImGuizmoMode.World, ref model[0]))
        {
            Matrix4 newModel = EngineUtility.ArrayToMatrix(model);

            Position = newModel.ExtractTranslation();
            Scale = newModel.ExtractScale();
            RotationQuat = newModel.ExtractRotation();
        }
    }
    
    // Tags

    public void Mark(string tag)
    {
        tags.Add(tag);
    }

    public void Unmark(string tag)
    {
        tags.Remove(tag);
    }

    public bool Marked(string tag)
    {
        return tags.Contains(tag);
    }
}
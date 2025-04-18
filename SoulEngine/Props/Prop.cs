using System.Reflection;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SoulEngine.Components;
using SoulEngine.Core;
using SoulEngine.Data.NBT;
using SoulEngine.Mathematics;
using SoulEngine.Renderer;
using SoulEngine.Rendering;
using SoulEngine.Util;

namespace SoulEngine.Props;

/// <summary>
/// Scene object such as some static geometry or a moving chair.
/// </summary>
public abstract class Prop : ITransformable
{
    /// <summary>
    /// The name of the property
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The scene that this prop is in
    /// </summary>
    public readonly Scene Scene;

    public readonly string Type;

    private readonly Dictionary<string, SerializedProperty> properties = new Dictionary<string, SerializedProperty>();

    private readonly Vector3Property positionProperty;

    internal readonly Texture? propIcon;
    
    public Vector3 Position
    {
        get => positionProperty.Value;
        set => positionProperty.Value = value;
    }

    private readonly QuaternionProperty rotationProperty;

    public Quaternion RotationQuat
    {
        get => rotationProperty.Value;
        set => rotationProperty.Value = value;
    }

    public Vector3 RotationEuler
    {
        get => rotationProperty.Value.ToEulerAngles() * Mathf.Rag2Deg;
        set => rotationProperty.Value = Quaternion.FromEulerAngles(value * Mathf.Deg2Rad);
    }

    private readonly Vector3Property scaleProperty;
    
    public Vector3 Scale
    {
        get => scaleProperty.Value;
        set => scaleProperty.Value = value;
    }

    public Matrix4 LocalMatrix => Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(RotationQuat) *
                                  Matrix4.CreateTranslation(Position);

    public ITransformable? Parent;
    public Matrix4 GlobalMatrix => Parent?.GlobalMatrix ?? Matrix4.Identity * LocalMatrix;
    
    
    public Vector3 Forward => RotationQuat * -Vector3.UnitZ;
    public Vector3 Up => RotationQuat * Vector3.UnitY;
    public Vector3 Right => RotationQuat * Vector3.UnitX;

    
    
    /// <summary>
    /// Create a new prop
    /// </summary>
    /// <param name="scene">The scene to create the prop in</param>
    /// <param name="type">The prop type</param>
    /// <param name="name">The name of the property</param>
    public Prop(Scene scene, string type, string name)
    {
        Scene = scene;
        Name = name;
        Type = type;

        positionProperty = Register(new Vector3Property("position", new Vector3(0, 0, 0)));
        rotationProperty = Register(new QuaternionProperty("rotation", Quaternion.Identity));
        scaleProperty = Register(new Vector3Property("scale", Vector3.One));

        PropertyReflection.RegisterProperties(scene, this, p => Register(p));
        
#if DEVELOPMENT

        if (this is Entity entity)
        {
            foreach (var component in entity.GetComponents<Component>())
            {
                if (component.propIcon != null)
                    propIcon ??= component.propIcon;
            }
            
            if(propIcon == null)
                propIcon = Scene.Game.ResourceManager.Load<Texture>("icons/object.png");
            
        }
        else
        {
            string icon = GetType().GetCustomAttribute<PropAttribute>()?.Icon ?? "object";

            if(icon != "none")
                propIcon = Scene.Game.ResourceManager.Load<Texture>("icons/" + icon + ".png");
        }
        

#endif
    }

    /// <summary>
    /// Loads this prop from the serialized tag
    /// </summary>
    /// <param name="tag">The tag to load from</param>
    public void Load(CompoundTag tag)
    {
        foreach (var p in properties)
        {
            if (tag.TryGetValue(p.Key, out Tag? propertyTag))
            {
                p.Value.Load(propertyTag);
                p.Value.MakeCurrentReset();
            }
        }
        
        OnLoad(tag);
    }

    /// <summary>
    /// Saves this prop to a tag
    /// </summary>
    /// <returns>The tag to save this prop as</returns>
    public CompoundTag Save()
    {
        CompoundTag tag = new CompoundTag(Name);
        
        tag.SetString("$_type", Type);
        
        foreach (var p in properties)
        {
            tag[p.Key] = p.Value.Save();
        }

        OnSave(tag);

        return tag;
    }


    /// <summary>
    /// Called after the prop is finished loading
    /// </summary>
    /// <param name="tag">The tag that we loaded from</param>
    public virtual void OnLoad(CompoundTag tag)
    {
        
    }
    
    /// <summary>
    /// Called after the prop is finished loading
    /// </summary>
    /// <param name="tag">The tag that we loaded from</param>
    public virtual void OnSave(CompoundTag tag)
    {
        
    }
    
    /// <summary>
    /// Update inner prop logic. Do not update render logic!
    /// </summary>
    /// <param name="deltaTime">The time that has passed since last update</param>
    public virtual void Update(float deltaTime)
    {
        
    }
    
    /// <summary>
    /// The new render functionality
    /// </summary>
    /// <param name="renderPipeline"></param>
    /// <param name="renderData"></param>
    /// <param name="deltaTime"></param>
    public virtual void Render(IRenderPipeline renderPipeline, SceneRenderData renderData, float deltaTime)
    {
        
    }

    public void Reset()
    {
        foreach (var p in properties.Values)
        {
            p.Reset();
        }
        OnReset();
    }
    
    protected virtual void OnReset()
    {
        
    }
    
    public void Edit()
    {
        string n = Name;
        if (ImGui.InputText("name", ref n, 2048))
            Name = n;
        ImGui.TextDisabled(Type);
        ImGui.SeparatorText("properties");
        
        foreach (var p in properties.Values)
        {
            p.Edit();
        }
        
        OnEdit();
    }

    protected virtual void OnEdit()
    {
        
    }

    public virtual void RenderGizmo(GizmoContext context)
    {
        if(propIcon != null)
            context.BillboardedSprite(propIcon);
        context.Begin(PrimitiveType.Lines);
        
        context.Vertex(Vector3.Zero, Colour.Red);
        context.Vertex(Vector3.UnitX, Colour.Red);
        
        context.Vertex(Vector3.Zero, Colour.Green);
        context.Vertex(Vector3.UnitY, Colour.Green);
        
        context.Vertex(Vector3.Zero, Colour.Blue);
        context.Vertex(-Vector3.UnitZ, Colour.Blue);

        context.End();
    }

    public virtual void RenderMoveGizmo(Matrix4 viewMatrix, Matrix4 projectionMatrix)
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




    protected T Register<T>(T property) where T : SerializedProperty
    {
        properties[property.Name] = property;
        return property;
    }
}
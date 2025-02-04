using System.Reflection;
using ImGuiNET;
using ImGuizmoNET;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Data.NBT;
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
        get => rotationProperty.Value.ToEulerAngles();
        set => rotationProperty.Value = Quaternion.FromEulerAngles(value);
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
    /// Render this prop. Do not update non-render logic!
    /// </summary>
    /// <param name="renderContext"></param>
    /// <param name="sceneRenderData">The scene render data</param>
    /// <param name="deltaTime">The time that has passed since last render</param>
    public virtual void Render(RenderContext renderContext, SceneRenderData sceneRenderData, float deltaTime)
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
                OPERATION.TRANSLATE | OPERATION.ROTATE | OPERATION.SCALE, MODE.LOCAL, ref model[0]))
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
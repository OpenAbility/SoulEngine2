using OpenTK.Mathematics;
using SoulEngine.Props;

namespace SoulEngine.Rendering;

public struct CameraSettings
{
    public CameraMode CameraMode;
    
    public Vector3 CameraPosition;
    public Vector3 CameraDirection;

    public Matrix4 ViewMatrix;
    public Matrix4 ProjectionMatrix;

    public bool ShowGizmos;
    public Prop? SelectedProp;

    public bool ShowUI;

    public static readonly CameraSettings Game = new CameraSettings()
    {
        CameraMode = CameraMode.GameCamera,
        ShowUI = true
    };
}

public enum CameraMode
{
    // Usual camera view
    GameCamera,
    // Moves the view matrix and cull points etc
    FlyCamera,
    // Only moves the view matrix
    FreeCamera
}
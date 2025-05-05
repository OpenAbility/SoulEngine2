using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Entities;
using SoulEngine.Props;

namespace SoulEngine.Rendering;

public struct CameraSettings
{
    public CameraMode CameraMode;
    
    public Vector3 CameraPosition;
    public Vector3 CameraDirection;

    public Vector3 CameraUp;
    public Vector3 CameraRight;

    public Matrix4 ViewMatrix;
    public Matrix4 ProjectionMatrix;
    
    public float FieldOfView;
    public float NearPlane;
    public float FarPlane;
    

    public bool ShowGizmos;
    public Entity? SelectedEntity;

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
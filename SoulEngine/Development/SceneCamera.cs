using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Mathematics;
using SoulEngine.Rendering;

namespace SoulEngine.Development;

public class SceneCamera : EngineObject
{
    public CameraMode CameraMode = CameraMode.FreeCamera;

    public Vector3 Position;
    public float Yaw;
    public float Pitch;

    public float FOV = 90;

    public float Near = 0.1f;
    public float Far = 1000f;
    
    public Quaternion Rotation => Quaternion.FromEulerAngles(0, Yaw * Mathf.Deg2Rad,0) * Quaternion.FromEulerAngles(Pitch * Mathf.Deg2Rad, 0, 0);

    public Vector3 Forward => Rotation * -Vector3.UnitZ;
    public Vector3 Right => Rotation * Vector3.UnitX;
    public Vector3 Up => Rotation * Vector3.UnitY;
    
    private readonly Game game;
    
    public SceneCamera(Game game)
    {
        this.game = game;
    }

    public Matrix4 GetView()
    {
        return Matrix4.LookAt(Position, Position + Forward, Vector3.UnitY);
    }

    public Matrix4 GetProjection(float aspect)
    {
        return Matrix4.CreatePerspectiveFieldOfView(FOV * MathF.PI / 180f, aspect, Near, Far);
    }

    private bool mouseCaptured;

    public void Update(float deltaTime, bool inputEnabled)
    {
        if (inputEnabled && !mouseCaptured && game.BuiltinActions.SceneCameraToggle.Down)
        {
            mouseCaptured = true;
            game.MainWindow.MouseCaptured = true;
        }

        if (mouseCaptured && !game.BuiltinActions.SceneCameraToggle.Down)
        {
            mouseCaptured = false;
            game.MainWindow.MouseCaptured = false;
        }
        
        
        Vector3 movementVector = new Vector3();
        if (mouseCaptured)
        {
            if (game.BuiltinActions.SceneCameraForward.Down)
                movementVector += Forward;
            if (game.BuiltinActions.SceneCameraBackward.Down)
                movementVector -= Forward;
        
            if (game.BuiltinActions.SceneCameraRight.Down)
                movementVector += Right;
            if (game.BuiltinActions.SceneCameraLeft.Down)
                movementVector -= Right;

            if (game.BuiltinActions.SceneCameraDown.Down)
                movementVector.Y -= 1;
            if (game.BuiltinActions.SceneCameraUp.Down)
                movementVector.Y += 1;
        
            if(movementVector.Length > 0)
                movementVector.Normalize();

            movementVector *= 2;
            
            if (game.BuiltinActions.SceneCameraSprint.Down)
                movementVector *= 10;

            Yaw -= game.InputManager.MouseDelta.X;
            Pitch -= game.InputManager.MouseDelta.Y;

            Pitch = Math.Clamp(Pitch, -89f, 89f);
        }

        
        Position += movementVector * deltaTime;
        

    }
}
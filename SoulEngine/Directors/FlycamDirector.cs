using OpenTK.Mathematics;
using SoulEngine.Core;
using SoulEngine.Data.NBT;
using SoulEngine.Props;

namespace SoulEngine.Directors;

[Director("builtin_flycam")]
public class FlycamDirector : Director
{
    public readonly FloatProperty SpeedProperty;
    public readonly FloatProperty SprintSpeedProperty;
    
    public FlycamDirector(Scene scene, string type) : base(scene, type)
    {
        SpeedProperty = Register(new FloatProperty("speed", 1));
        SprintSpeedProperty = Register(new FloatProperty("speed_sprint", 2));
    }

    public override void OnLoad(CompoundTag tag)
    {
        
    }

    public override void OnSave(CompoundTag tag)
    {
        
    }

    public override void Update(float deltaTime)
    {
        UpdateCamera(deltaTime);
    }

    private float yaw;
    private float pitch;
    
    private void UpdateCamera(float deltaTime)
    {
        Game.MainWindow.MouseCaptured = Game.BuiltinActions.CameraToggle.Down;
        
        if(!Game.BuiltinActions.CameraToggle.Down)
            return;
        
        CameraProp camera = FindProp<CameraProp>()!;
        
        Vector3 movementVector = new Vector3();
        if (Game.BuiltinActions.CameraForward.Down)
            movementVector += camera.Forward;
        if (Game.BuiltinActions.CameraBackward.Down)
            movementVector -= camera.Forward;
        
        if (Game.BuiltinActions.CameraRight.Down)
            movementVector += camera.Right;
        if (Game.BuiltinActions.CameraLeft.Down)
            movementVector -= camera.Right;

        if (Game.BuiltinActions.CameraDown.Down)
            movementVector.Y -= 1;
        if (Game.BuiltinActions.CameraUp.Down)
            movementVector.Y += 1;
        
        if(movementVector.Length > 0)
            movementVector.Normalize();

        if (Game.BuiltinActions.CameraSprint.Down)
            movementVector *= SprintSpeedProperty.Value;

        movementVector *= SpeedProperty.Value;
        
        camera.Position += movementVector * deltaTime;
        
        yaw -= Game.InputManager.MouseDelta.X / 100f;
        pitch -= Game.InputManager.MouseDelta.Y / 100f;

        camera.RotationQuat = Quaternion.FromEulerAngles(0, yaw,0) * Quaternion.FromEulerAngles(pitch, 0, 0);
    }
}
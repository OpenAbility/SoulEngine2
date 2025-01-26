using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SoulEngine.Input;

internal class BuiltinActions
{

    public readonly InputAction CameraForward;
    public readonly InputAction CameraBackward;
    public readonly InputAction CameraLeft;
    public readonly InputAction CameraRight;
    public readonly InputAction CameraUp;
    public readonly InputAction CameraDown;
    public readonly InputAction CameraSprint;
    
    public readonly InputAction CameraToggle;
    
    public readonly InputAction LeftAlt;
    public readonly InputAction Enter;
    
    public readonly InputAction SceneCameraForward;
    public readonly InputAction SceneCameraBackward;
    public readonly InputAction SceneCameraLeft;
    public readonly InputAction SceneCameraRight;
    public readonly InputAction SceneCameraUp;
    public readonly InputAction SceneCameraDown;
    public readonly InputAction SceneCameraSprint;
    
    public readonly InputAction SceneCameraToggle;
    
    public BuiltinActions(InputManager inputManager)
    {
        CameraForward = inputManager.Action("builtin.camera.forward", Keys.W);
        CameraBackward = inputManager.Action("builtin.camera.backward", Keys.S);
        CameraLeft = inputManager.Action("builtin.camera.left", Keys.A);
        CameraRight = inputManager.Action("builtin.camera.right", Keys.D);
        CameraUp = inputManager.Action("builtin.camera.up", Keys.Space);
        CameraDown = inputManager.Action("builtin.camera.down", Keys.LeftControl);
        CameraToggle = inputManager.Action("builtin.camera.toggle", MouseButton.Right);
        CameraSprint = inputManager.Action("builtin.camera.sprint", Keys.LeftShift);
        
        SceneCameraForward = inputManager.Action("builtin.camera.forward", Keys.W).IgnoreWindow();
        SceneCameraBackward = inputManager.Action("builtin.camera.backward", Keys.S).IgnoreWindow();
        SceneCameraLeft = inputManager.Action("builtin.camera.left", Keys.A).IgnoreWindow();
        SceneCameraRight = inputManager.Action("builtin.camera.right", Keys.D).IgnoreWindow();
        SceneCameraUp = inputManager.Action("builtin.camera.up", Keys.Space).IgnoreWindow();
        SceneCameraDown = inputManager.Action("builtin.camera.down", Keys.LeftControl).IgnoreWindow();
        SceneCameraToggle = inputManager.Action("builtin.camera.toggle", MouseButton.Right).IgnoreWindow();
        SceneCameraSprint = inputManager.Action("builtin.camera.sprint", Keys.LeftShift).IgnoreWindow();

        LeftAlt = inputManager.Action("builtin.key.left_alt", Keys.LeftAlt).IgnoreWindow();
        Enter = inputManager.Action("builtin.key.enter", Keys.Enter).IgnoreWindow();
    }
}
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

    public readonly InputAction Space;
    
    public BuiltinActions(InputManager inputManager)
    {
        CameraForward = inputManager.Action().Name("builtin.camera.forward").Bind(Keys.W).Finish();
        CameraBackward = inputManager.Action().Name("builtin.camera.backward").Bind(Keys.S).Finish();
        CameraLeft = inputManager.Action().Name("builtin.camera.left").Bind(Keys.A).Finish();
        CameraRight = inputManager.Action().Name("builtin.camera.right").Bind(Keys.D).Finish();
        CameraUp = inputManager.Action().Name("builtin.camera.up").Bind(Keys.Space).Finish();
        CameraDown = inputManager.Action().Name("builtin.camera.down").Bind(Keys.LeftControl).Finish();
        CameraToggle = inputManager.Action().Name("builtin.camera.toggle").Bind(MouseButton.Right).Finish();
        CameraSprint = inputManager.Action().Name("builtin.camera.sprint").Bind(Keys.LeftShift).Finish();
        
        SceneCameraForward = inputManager.Action().Name("builtin.camera.forward").Bind(Keys.W).Finish().IgnoreWindow();
        SceneCameraBackward = inputManager.Action().Name("builtin.camera.backward").Bind(Keys.S).Finish().IgnoreWindow();
        SceneCameraLeft = inputManager.Action().Name("builtin.camera.left").Bind(Keys.A).Finish().IgnoreWindow();
        SceneCameraRight = inputManager.Action().Name("builtin.camera.right").Bind(Keys.D).Finish().IgnoreWindow();
        SceneCameraUp = inputManager.Action().Name("builtin.camera.up").Bind(Keys.Space).Finish().IgnoreWindow();
        SceneCameraDown = inputManager.Action().Name("builtin.camera.down").Bind(Keys.LeftControl).Finish().IgnoreWindow();
        SceneCameraToggle = inputManager.Action().Name("builtin.camera.toggle").Bind(MouseButton.Right).Finish().IgnoreWindow();
        SceneCameraSprint = inputManager.Action().Name("builtin.camera.sprint").Bind(Keys.LeftShift).Finish().IgnoreWindow();

        LeftAlt = inputManager.Action().Name("builtin.key.left_alt").Bind(Keys.LeftAlt).Finish().IgnoreWindow();
        Enter = inputManager.Action().Name("builtin.key.enter").Bind(Keys.Enter).Finish().IgnoreWindow();
    }
}
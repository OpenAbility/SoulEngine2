using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SoulEngine.Input;

internal class BuiltinActions(InputManager inputManager)
{
    public readonly InputAction SceneCameraForward = inputManager.Action().Name("builtin.camera.forward").Bind(Keys.W).Finish().IgnoreWindow();
    public readonly InputAction SceneCameraBackward = inputManager.Action().Name("builtin.camera.backward").Bind(Keys.S).Finish().IgnoreWindow();
    public readonly InputAction SceneCameraLeft = inputManager.Action().Name("builtin.camera.left").Bind(Keys.A).Finish().IgnoreWindow();
    public readonly InputAction SceneCameraRight = inputManager.Action().Name("builtin.camera.right").Bind(Keys.D).Finish().IgnoreWindow();
    public readonly InputAction SceneCameraUp = inputManager.Action().Name("builtin.camera.up").Bind(Keys.Space).Finish().IgnoreWindow();
    public readonly InputAction SceneCameraDown = inputManager.Action().Name("builtin.camera.down").Bind(Keys.LeftControl).Finish().IgnoreWindow();
    public readonly InputAction SceneCameraSprint = inputManager.Action().Name("builtin.camera.sprint").Bind(Keys.LeftShift).Finish().IgnoreWindow();
    
    public readonly InputAction SceneCameraToggle = inputManager.Action().Name("builtin.camera.toggle").Bind(MouseButton.Right).Finish().IgnoreWindow();
}
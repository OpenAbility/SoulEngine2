# SoulEngine
SoulEngine is a small "in-house" engine currently in the works to make a single game. It
follows a theatre-style workflow, where the scene is divided up into props and directors.

Just attempting to clone and build this repo will not work(I can't even guarantee that the solution is up to date),
as the engine is meant to be a dependency of the game.

To get started, create a C# project, and add the engine source code as a submodule(or just copy it into a directory).
Then, ensure you have 3 different solution configurations:
- Release - The release builds of your game
- Debug - A build without dev tools, but with debugging symbols etc(to test the release environment)
- Development - All builds of the engine with this enabled will have a full-blown editor baked into them which allows you
    to use specific tooling and more.

I don't really know how to ensure that these propagate down to the engine, but it's these 3 that we use in-engine to
change up what parts compile etc.

Then, you simply need a class inheriting from `SoulEngine.Core.Game` called whatever you want(e.g `MyGame`).

In order to start the game you then just need to instance that class,
call `.Run()`, and then `.FinalizeEngine()`(for cleanup).

A short example of a main function(using the modern top-level statements) looks like this:

```csharp
using MyStudio.GameProject;

// Catch exceptions and print them to console just in cas
AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
    Console.Error.WriteLine(eventArgs.ExceptionObject);
};

TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
{
    Console.Error.WriteLine(eventArgs.Exception);
};

// We use a try-catch block for this
MyGame? game = null;

try
{
    // Creates the game instance
    game = new MyGame();
    // Actually starts it
    game.Run();
}
finally
{
    // Cleanup of device resources
    game?.FinalizeEngine();
}
```

More documentation to follow.
using JoltPhysicsSharp;
using SoulEngine.Core;

namespace SoulEngine.Physics;

public class PhysicsSimulator
{
    public readonly Game Game;

    public PhysicsSimulator(Game game)
    {
        Game = game;

        // Init physics
        if (!Foundation.Init())
        {
            throw new Exception("Could not initialize physics");
        }
        
    }
}
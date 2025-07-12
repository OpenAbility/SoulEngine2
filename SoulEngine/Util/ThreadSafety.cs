using SoulEngine.Core;

namespace SoulEngine.Util;

/// <summary>
/// Provides utilities for running functions on the main thread
/// </summary>
public class ThreadSafety
{
    public static ThreadSafety Instance { get; private set; } = null!;
    
    public readonly Game Game;

    public bool OnMain => Game.MainThread == Thread.CurrentThread;
    
    public ThreadSafety(Game game)
    {
        Game = game;
        Instance = this;
    }

    public T EnsureMain<T>(Func<T> func)
    {
        T returnValue = default(T)!;
        EnsureMain(() =>
        {
            returnValue = func();
        });
        return returnValue;
    }
    

    public void EnsureMain(Action action)
    {
        if (OnMain)
            action();
        else
        {
            bool awaiting = true;
            tasks.Enqueue(() =>
            {
                action();
                awaiting = false;
            });

            while (awaiting)
            {
                Thread.Yield();
            }
            
        }
    }

    private Queue<Action> tasks = new Queue<Action>();

    public void RunTasks()
    {
        if (!OnMain)
            throw new Exception("Not on main thread!");

        while (tasks?.Count > 0)
        {
            tasks?.Dequeue()?.Invoke();
        }
    }
}
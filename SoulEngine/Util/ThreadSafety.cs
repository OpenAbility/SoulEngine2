using SoulEngine.Core;

namespace SoulEngine.Util;

/// <summary>
/// Provides utilities for running functions on the main thread
/// </summary>
public class ThreadSafety
{
    public readonly Game Game;

    public bool OnMain => Game.MainThread == Thread.CurrentThread;
    
    public ThreadSafety(Game game)
    {
        Game = game;
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
    
    public async Task<T> EnsureMainAsync<T>(Func<T> func)
    {
        T returnValue = default(T)!;
        await EnsureMainAsync(() =>
        {
            returnValue = func();
        });
        return returnValue;
    }

    public async Task EnsureMainAsync(Action action)
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
                await Task.Delay(1);
            }
        }
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

        while (tasks.Count > 0)
        {
            tasks.Dequeue().Invoke();
        }
    }
}
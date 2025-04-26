using System.Collections.Concurrent;
using OpenAbility.Logging;
using SoulEngine.Resources;

namespace SoulEngine.Processing;

/// <summary>
/// Enqueues multiple tasks onto multiple threads
/// </summary>
public class ThreadPool
{
    public static readonly ThreadPool Global = new ThreadPool("Global", 8);
    
    private readonly Thread[] executingThreads;
    private readonly ConcurrentQueue<Action> tasks = new ConcurrentQueue<Action>();
    private readonly Logger Logger;
    private bool running;

    public ThreadPool(string name, int size)
    {
        executingThreads = new Thread[size];
        Logger = Logger.Get("ThreadPool", name);

        running = true;
        
        for (int i = 0; i < size; i++)
        {
            executingThreads[i] = new Thread(ThreadLoop);
            executingThreads[i].Start();
            executingThreads[i].IsBackground = true;
            executingThreads[i].Name = name + "-Thread" + i;
        }
    }

    private void ThreadLoop()
    {
        while (running)
        {
            if (tasks.TryDequeue(out Action? task))
            {
                try
                {
                    task();
                }
                catch (Exception e)
                {
                    Logger.Error("Task threw: \n{}", e);                    
                }
            }
        }
    }

    public void Enqueue(Action task)
    {
        tasks.Enqueue(task);
    }

    public ExecutionPromise<T> EnqueuePromise<T>(Func<T> func)
    {
        ExecutionPromise<T>.PromiseTrigger trigger = ExecutionPromise<T>.Create();
        
        Enqueue(() =>
        {
            try
            {
                T result = func();
                trigger.Complete(result);
            }
            catch (Exception e)
            {
                trigger.Fail(e);
                throw;
            }
        });

        return trigger.Promise;
    }
}
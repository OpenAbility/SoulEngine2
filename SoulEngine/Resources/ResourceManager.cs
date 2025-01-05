using OpenAbility.Logging;
using SoulEngine.Core;

namespace SoulEngine.Resources;

public class ResourceManager
{
    
    private static readonly Logger Logger = Logger.Get<ResourceManager>();
    
    private Dictionary<string, WeakReference<Resource>> resourceCache =
        new Dictionary<string, WeakReference<Resource>>();

    private Dictionary<string, (Resource resource, Task loadingTask)> resourceLoadingTasks = new Dictionary<string, (Resource resource, Task loadingTask)>();


    private readonly Lock loadCacheLock = new Lock();

    public readonly Game Game;

    public ResourceManager(Game game)
    {
        Game = game;
    }

    private Task<T> GetLoadTask<T>(string id, ResourceFactory<T> resourceFactory, bool synchronized) where T : Resource
    {
        using var scope = loadCacheLock.EnterScope();
        
        if (synchronized)
        {
            Logger.Info("Loading {}", id);
            T resource = resourceFactory();
            resource.Load(this, id, Game.Content);
            resourceCache[id] = new WeakReference<Resource>(resource);
            return Task.FromResult(resource);
        }

        if (resourceLoadingTasks.TryGetValue(id, out var existing))
        {
            return Task.Run(async () =>
            {
                await existing.loadingTask;
                return (T)existing.resource;
            });
        }

        T resourceInstance = resourceFactory();

        resourceCache[id] = new WeakReference<Resource>(resourceInstance);

        Task loadingTask = Task.Run(() => resourceInstance.Load(this, id, Game.Content));
        resourceLoadingTasks[id] = (resourceInstance, loadingTask);

        return Task.Run(async () =>
        {
            await loadingTask;
            return resourceInstance;
        });
    }

    /// <summary>
    /// Asynchronously loads a resource
    /// </summary>
    /// <param name="id">The resource ID to load</param>
    /// <param name="factory">The resource factory</param>
    /// <typeparam name="T">The type of the resource</typeparam>
    /// <returns>The loading task</returns>
    public Task<T> LoadAsync<T>(string id, ResourceFactory<T> factory) where T : Resource
    {
        if (resourceCache.TryGetValue(id, out WeakReference<Resource>? loaded))
        {
            if (loaded.TryGetTarget(out Resource? loadedTarget))
                return Task.FromResult((T)loadedTarget);
            resourceCache.Remove(id);
        }

        return GetLoadTask(id, factory, false);
    }
    
    /// <summary>
    /// Synchronously loads a resource
    /// </summary>
    /// <param name="id">The resource ID to load</param>
    /// <param name="factory">The resource factory</param>
    /// <typeparam name="T">The type of the resource</typeparam>
    /// <returns>The loaded resource</returns>
    public T Load<T>(string id, ResourceFactory<T> factory) where T : Resource
    {
        if (resourceCache.TryGetValue(id, out WeakReference<Resource>? loaded))
        {
            if (loaded.TryGetTarget(out Resource? loadedTarget))
                return (T)loadedTarget;
            resourceCache.Remove(id);
        }
        
        return GetLoadTask(id, factory, true).Result;
    }
}

public delegate T ResourceFactory<out T>() where T : Resource;
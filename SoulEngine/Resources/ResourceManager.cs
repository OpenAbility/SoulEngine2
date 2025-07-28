using System.Diagnostics;
using System.Reflection;
using OpenAbility.Logging;
using SoulEngine.Content;
using SoulEngine.Core;
using SoulEngine.Data;
using SoulEngine.Util;
using ThreadPool = SoulEngine.Processing.ThreadPool;

namespace SoulEngine.Resources;

public class ResourceManager : EngineObject
{
    
    private static readonly Logger Logger = Logger.Get<ResourceManager>();
    
    private Dictionary<string, WeakReference<Resource>> resourceCache =
        new Dictionary<string, WeakReference<Resource>>();

    private Dictionary<Type, object> resourceLoaders = new ();
    
    private Dictionary<string, ExecutionPromise> resourceLoadingTasks = new Dictionary<string, ExecutionPromise>();


    private readonly Lock loadCacheLock = new Lock();

    public readonly Game Game;

    public readonly TemporaryResourceSource Temporary;

    public static ResourceManager Global { get; internal set; } = null!;

    public ResourceManager(Game game)
    {
        Game = game;

        Temporary = new TemporaryResourceSource();
        Game.Content.Mount(Temporary);
    }

    private IResourceLoader<T> GetLoader<T>()
    {
        if (resourceLoaders.TryGetValue(typeof(T), out object? existing))
            return (IResourceLoader<T>)existing;

        Type type = typeof(T);

        ResourceAttribute? attribute = type.GetCustomAttribute<ResourceAttribute>();
        if (attribute == null)
            throw new Exception("No resource attribute found!");

        if (!attribute.LoaderType!.IsAssignableTo(typeof(IResourceLoader<T>)))
            throw new Exception("Loader type not compatible!");

        if (!attribute.LoaderType.CanInstance())
            throw new Exception("Loader type is not instantiatable!");

        IResourceLoader<T> loader = attribute.LoaderType.Instantiate<IResourceLoader<T>>()!;

        resourceLoaders[type] = loader;

        return loader;
    }

    private ExecutionPromise<T> GetLoadTask<T>(string id, bool synchronized) where T : Resource
    {
        //using var scope = loadCacheLock.EnterScope();

        if (!Game.Content.Exists(id))
            Logger.Warning("File '{}' not present in context!", id);

        IResourceLoader<T> loader = GetLoader<T>();
        
        //Logger.Info("Loading '{}'", id);
        
        if (synchronized)
        {
            using var segment = Profiler.Instance.Segment("resources.load");
            
            T resource = null!;
            
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                
                resource = loader.LoadResource(new ResourceData(id, "_nil", Game.Content.Load(id)!, this, Game.Content));
                resource.ResourceID = id;
                
                //Logger.Info("(S) Loading '{}' took {} ms", id, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Logger.Error("Loading '{}' threw: \n{}", id, e);
            }
            
            resourceCache[id] = new WeakReference<Resource>(resource);
            return  ExecutionPromise<T>.Completed(resource);
        }

        if (resourceLoadingTasks.TryGetValue(id, out var existing))
        {
            return (ExecutionPromise<T>)existing;
        }
        
        ExecutionPromise<T> promise = ThreadPool.Global.EnqueuePromise(() =>
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            T instance = loader.LoadResource(new ResourceData(id, "_nil", Game.Content.Load(id)!, this, Game.Content));;
            instance.ResourceID = id;
                    
            resourceCache[id] = new WeakReference<Resource>(instance);

            
            //Logger.Info("(A) Loading '{}' took {} ms", id, stopwatch.ElapsedMilliseconds);
            return instance;

        });
        
        resourceLoadingTasks[id] = promise;

        return promise;
    }

    /// <summary>
    /// Asynchronously loads a resource
    /// </summary>
    /// <param name="id">The resource ID to load</param>
    /// <typeparam name="T">The type of the resource</typeparam>
    /// <returns>The loading task</returns>
    public ExecutionPromise<T> LoadAsync<T>(string id) where T : Resource
    {
        if (resourceCache.TryGetValue(id, out WeakReference<Resource>? loaded))
        {
            if (loaded.TryGetTarget(out Resource? loadedTarget))
                return ExecutionPromise<T>.Completed((T)loadedTarget);
            resourceCache.Remove(id);
        }

        return GetLoadTask<T>(id, EngineVarContext.Global.GetBool("e_force_load_sync"));
    }
    
    /// <summary>
    /// Synchronously loads a resource
    /// </summary>
    /// <param name="id">The resource ID to load</param>
    /// <typeparam name="T">The type of the resource</typeparam>
    /// <returns>The loaded resource</returns>
    public T Load<T>(string id) where T : Resource
    {
        if (resourceCache.TryGetValue(id, out WeakReference<Resource>? loaded))
        {
            if (loaded.TryGetTarget(out Resource? loadedTarget))
                return (T)loadedTarget;
            else
                resourceCache.Remove(id);
        }
        
        return GetLoadTask<T>(id, true).ReturnValue!;
    }

    /// <summary>
    /// Adds a resource to the cache
    /// </summary>
    /// <param name="id">The ID of the resource</param>
    /// <param name="resource">The resource to add</param>
    public void RegisterResource(string id, Resource resource)
    {
        resourceCache[id] = new WeakReference<Resource>(resource);
    }
    
}

public delegate T ResourceFactory<out T>() where T : Resource;
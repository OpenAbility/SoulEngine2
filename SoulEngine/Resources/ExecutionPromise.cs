namespace SoulEngine.Resources;

public class ExecutionPromise
{
    public Exception? Exception { get; protected set; }
    public PromiseState State { get; protected set; }
    
    public event OnPromiseReturnedEvent OnComplete = (promise) => {};

    protected virtual void TriggerComplete()
    {
        OnComplete?.Invoke(this);
    }

    public async Task WaitCompletedAsync()
    {
        while (State == PromiseState.AwaitingCompleted)
        {
            await Task.Yield();
        }

        if (State == PromiseState.Failed)
        {
            if (Exception != null)
                throw new AggregateException("Promise failed execution!", Exception);
            else
                throw new AggregateException("Promise failed execution!");
        }
    }
    
    public static PromiseTrigger Create()
    {
        ExecutionPromise promise = new ExecutionPromise();
        PromiseTrigger trigger = new PromiseTrigger(promise);

        return trigger;
    }
    
    public static ExecutionPromise Completed()
    {
        ExecutionPromise promise = new ExecutionPromise();
        promise.State = PromiseState.Completed;

        return promise;
    }
    
    public sealed class PromiseTrigger
    {
        public readonly ExecutionPromise Promise;

        internal PromiseTrigger(ExecutionPromise promise)
        {
            Promise = promise;
        }
        
        public void Complete()
        {
            if (Promise.State != PromiseState.AwaitingCompleted)
                throw new Exception("Cannot retrigger triggered promise!");
            
            Promise.State = PromiseState.Completed;
            Promise.Exception = null;
            
            Promise.TriggerComplete();
        }
        
        public void Fail(Exception? exception)
        {
            if (Promise.State != PromiseState.AwaitingCompleted)
                throw new Exception("Cannot retrigger triggered promise!");
            
            Promise.State = PromiseState.Completed;
            Promise.Exception = exception;
            
            Promise.TriggerComplete();
        }
    }
    
}

public sealed class ExecutionPromise<T> : ExecutionPromise
{
    public T? ReturnValue { get; private set; }

    public new event OnPromiseCompleteEvent<T> OnComplete = (value) => { };

    private ExecutionPromise()
    {
    }
    
    public new async Task<T> WaitCompletedAsync()
    {
        while (State == PromiseState.AwaitingCompleted)
        {
            await Task.Yield();
        }

        if (State == PromiseState.Failed)
        {
            if (Exception != null)
                throw new AggregateException("Promise failed execution!", Exception);
            else
                throw new AggregateException("Promise failed execution!");
        }

        return ReturnValue!;
    }

    protected override void TriggerComplete()
    {
        base.TriggerComplete();
        OnComplete?.Invoke(this);
    }

    public new static PromiseTrigger Create()
    {
        ExecutionPromise<T> promise = new ExecutionPromise<T>();
        PromiseTrigger trigger = new PromiseTrigger(promise);

        return trigger;
    }
    
    public static ExecutionPromise<T> Completed(T value)
    {
        ExecutionPromise<T> promise = new ExecutionPromise<T>();
        promise.State = PromiseState.Completed;
        promise.ReturnValue = value;

        return promise;
    }
    
    public new sealed class PromiseTrigger
    {
        public readonly ExecutionPromise<T> Promise;

        internal PromiseTrigger(ExecutionPromise<T> promise)
        {
            Promise = promise;
        }
        
        public void Complete(T returnValue)
        {
            if (Promise.State != PromiseState.AwaitingCompleted)
                throw new Exception("Cannot retrigger triggered promise!");
            
            Promise.State = PromiseState.Completed;
            Promise.Exception = null;
            Promise.ReturnValue = returnValue;
            
            Promise.TriggerComplete();
        }
        
        public void Fail(Exception? exception)
        {
            if (Promise.State != PromiseState.AwaitingCompleted)
                throw new Exception("Cannot retrigger triggered promise!");
            
            Promise.State = PromiseState.Completed;
            Promise.Exception = exception;
            Promise.ReturnValue = default;
            
            Promise.TriggerComplete();
        }
    }
    

}

public enum PromiseState
{
    AwaitingCompleted,
    Completed,
    Failed
}

public delegate void OnPromiseCompleteEvent<T>(ExecutionPromise<T> promise);
public delegate void OnPromiseReturnedEvent(ExecutionPromise promise);
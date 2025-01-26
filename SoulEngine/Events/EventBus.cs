using System.Numerics;

namespace SoulEngine.Events;

/// <summary>
/// Provides event dispatching
/// </summary>
/// <typeparam name="T">The type of all event objects</typeparam>
public class EventBus<T>
{
	private Queue<T> eventQueue = new Queue<T>();
	private Queue<(EventListener<T>, bool)> listenQueue = new ();
	private List<EventListener<T>> handlers = new List<EventListener<T>>();

	public event Action<EventBus<T>> OnDispatch = (bus) => { };

	private readonly bool immediate;

	/// <summary>
	/// Creates a default event bus
	/// </summary>
	public EventBus()
	{
		immediate = false;
	}
	
	/// <summary>
	/// Creates an event bus
	/// </summary>
	/// <param name="immediate">Should all events be dispatched immediately</param>
	public EventBus(bool immediate)
	{
		this.immediate = immediate;
	}

	/// <summary>
	/// Dispatches all events on the bus
	/// </summary>
	public void Dispatch()
	{
		// Dirty code but it'll work
		AddListeners();
		
		OnDispatch.Invoke(this);
		
		Queue<T> events = eventQueue;
		eventQueue = new Queue<T>();

		while (events.Count != 0)
		{
			T eventInstance = events.Dequeue();
			DispatchEvent(eventInstance);
		}
	}

	private void AddListeners()
	{
		while (listenQueue.Count != 0)
		{
			(EventListener<T> listener, bool additive) = listenQueue.Dequeue();
			if(additive)
				handlers.Add(listener);
			else
				handlers.Remove(listener);
		}
	}
	
	private void DispatchEvent(T eventInstance) {
		if (eventInstance is HandledEvent consumableEvent)
		{

			for (int i = 0; i < handlers.Count; i++)
			{
				handlers[i](eventInstance, false);
				if(consumableEvent.Handled)
					break;
			}
				
			if(consumableEvent.Handled)
				return;
				
			for (int i = 0; i < handlers.Count; i++)
			{
				handlers[i](eventInstance, true);
				if(consumableEvent.Handled)
					break;
			}
				
		}
		else
		{
			for (int i = 0; i < handlers.Count; i++)
			{
				handlers[i](eventInstance, false);
			}
		}
	}

	public void Event(T eventObject)
	{
		eventQueue.Enqueue(eventObject);
		if(immediate)
			Dispatch();
	}
	
	public void EventNow(T eventObject)
	{
		AddListeners();
		DispatchEvent(eventObject);
	}

	/// <summary>
	/// Begins listening for events on this bus
	/// </summary>
	/// <param name="listener">The listener to add</param>
	/// <returns>This bus, for call chaining</returns>
	public EventBus<T> BeginListen(EventListener<T> listener)
	{
		listenQueue.Enqueue((listener, true));
		return this;
	}
	
	/// <summary>
	/// Stops listening for events on this bus
	/// </summary>
	/// <param name="listener">The listener to remove</param>
	/// <returns>This bus, for call chaining</returns>
	public EventBus<T> EndListen(EventListener<T> listener)
	{
		listenQueue.Enqueue((listener, false));
		return this;
	}
}

/// <summary>
/// Listens for events on an event bus. <see cref="HandledEvent">Handled events</see> will be dispatched twice
/// unless consumed the first time around. The second dispatch will set the <c>unhandled</c> flag to true.
/// </summary>
/// <typeparam name="T">The event object type</typeparam>
public delegate void EventListener<in T>(T eventObject, bool unhandled);

/// <summary>
/// Provides an event that may be handled
/// </summary>
public class HandledEvent
{
	public bool Handled { get; private set; } = false;

	public void Handle()
	{
		Handled = true;
	}
}

/// <summary>
/// Base class for all engine events
/// </summary>
public class Event
{
	/// <summary>
	/// The event ID
	/// </summary>
	public readonly string ID;
	
	/// <summary>
	/// The event user data
	/// </summary>
	public readonly object? UserData;

	/// <summary>
	/// Creates a new event
	/// </summary>
	/// <param name="id">The event ID</param>
	/// <param name="userData">The event data</param>
	public Event(string id, object? userData)
	{
		this.ID = id;
		this.UserData = userData;
	}

	public static Event Of(string name)
	{
		return new Event(name, null);
	}
	
	public static Event Of(string name, object? userData)
	{
		return new Event(name, userData);
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

public class Pool<T> : IPool<T> where T : class
{
	private readonly Queue<T> _freeInstances;
	private readonly HashSet<T> _usingInstances = new HashSet<T>();
	private readonly Func<T> _objectCreator;
	private readonly Action<T> _onSpawned, _onDespawned;
	private readonly int _extendAmount;
	
	public Pool(int initialCapacity, int extendAmount, Func<T> objectCreator, Action<T> onSpawned = null, Action<T> onDespawned = null)
	{
		_freeInstances = new Queue<T>(initialCapacity);
		_extendAmount = extendAmount;
		_objectCreator = objectCreator;
		_onSpawned = onSpawned;
		_onDespawned = onDespawned;
		
		ExtendCapacity(initialCapacity);
	}
	
	private void ExtendCapacity(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			T obj = _objectCreator();
			_freeInstances.Enqueue(obj);
		}
	}
		
	public T Spawn()
	{
        if (_freeInstances.Count == 0)
        {
            if (_extendAmount > 0)
            {
                ExtendCapacity(_extendAmount);
            }
            else if (_usingInstances.Count > 0)
            {
                Despawn(_usingInstances.First());
            }
            else
            {
                return null;
            }
        }

        T obj = _freeInstances.Dequeue();
        _usingInstances.Add(obj);
		_onSpawned?.Invoke(obj);
		return obj;
	}
	
	public void Despawn(T obj)
	{
		_freeInstances.Enqueue(obj);
		_usingInstances.Remove(obj);
		_onDespawned?.Invoke(obj);
	}

    public IReadOnlyCollection<T> FreeInstances => _freeInstances;
    public IReadOnlyCollection<T> UsingInstances => _usingInstances;
}
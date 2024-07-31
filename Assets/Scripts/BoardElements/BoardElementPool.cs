using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BoardElementPool<T> where T: BoardElement
{
    private Stack<T> _pool;

    public T GetInstance (T prefab)
    {
        if (_pool == null)
        {
            _pool = new();
        }
		
        if (_pool.TryPop(out T instance))
        {
            instance.gameObject.SetActive(true);
        }
        else
        {
            instance = Object.Instantiate(prefab);
        }
        return instance;
        
    }

    public void Recycle (T instance)
    {
        _pool.Push(instance);
        instance.gameObject.SetActive(false);
    }
}

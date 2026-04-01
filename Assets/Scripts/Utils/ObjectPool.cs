using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generic object pool to avoid frequent Instantiate/Destroy calls.
/// Supports any Component type.
/// </summary>
public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Queue<T> _pool = new Queue<T>();

    /// <summary>
    /// Creates a pool with an optional warm-up count.
    /// </summary>
    public ObjectPool(T prefab, Transform parent = null, int warmUp = 0)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < warmUp; i++)
        {
            T obj = Create();
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Returns an instance from the pool, creating one if necessary.
    /// </summary>
    public T Get(Vector3 position, Quaternion rotation)
    {
        T obj = _pool.Count > 0 ? _pool.Dequeue() : Create();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Returns an object back to the pool.
    /// </summary>
    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }

    private T Create()
    {
        T obj = Object.Instantiate(_prefab, _parent);
        return obj;
    }
}

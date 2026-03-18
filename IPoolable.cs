using UnityEngine;
using UnityEngine.Pool;

public interface IPoolable
{
    public void OnSpawn();
    public void OnDeSpawn();
}

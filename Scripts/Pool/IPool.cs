
public interface IPool<T>
{
    T Spawn();
    void Despawn(T obj);
}

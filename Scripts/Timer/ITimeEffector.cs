
public interface ITimeEffector
{
    void AddFactor(ITimeFactor factor, bool compute = true);
    void RemoveFactor(ITimeFactor factor, bool compute = true);
    float Compute();
}
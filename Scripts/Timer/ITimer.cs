using System;

public interface ITimer
{
    event Action<float> OnTimeScaleChanged;
    float LocalScale { get; set; }
    float TotalScale { get; }
    float FixedDelta { get; }
    float Delta { get; }
}

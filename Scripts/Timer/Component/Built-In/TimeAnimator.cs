using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TimeAnimator : TimeComponent<Animator, TimeAnimator.Snapshot>
{
    public struct Snapshot
    {
        public float Speed { get; set; }
    }

    protected override Snapshot TakeSnapshot()
    {
        return new Snapshot()
        {
            Speed = Component.speed
        };
    }

    protected override void ApplySnapshot(Snapshot snapshot, float scaleRatio)
    {
        Component.speed = snapshot.Speed * scaleRatio;
    }
}
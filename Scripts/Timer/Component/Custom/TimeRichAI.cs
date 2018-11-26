using UnityEngine;
using Pathfinding;

public class TimeRichAI : TimeComponent<RichAI, TimeRichAI.Snapshot>
{
    public struct Snapshot
    {
        public float MaxSpeed { get; set; }
        public float RotationSpeed { get; set; }
        public Vector3 Gravity { get; set; }
    }

    protected override Snapshot TakeSnapshot()
    {
        return new Snapshot()
        {
            MaxSpeed = Component.maxSpeed,
            RotationSpeed = Component.rotationSpeed,
            Gravity = Component.gravity
        };
    }

    protected override void ApplySnapshot(Snapshot snapshot, float scaleRatio)
    {
        Component.maxSpeed = snapshot.MaxSpeed * scaleRatio;
        Component.rotationSpeed = snapshot.RotationSpeed * scaleRatio;
        Component.gravity = snapshot.Gravity * scaleRatio;
    }
}
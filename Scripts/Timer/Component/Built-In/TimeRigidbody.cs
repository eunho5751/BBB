using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
public class TimeRigidbody : TimeComponent<Rigidbody, TimeRigidbody.Snapshot>
{
    public struct Snapshot
    {
        public float Drag { get; set; }
        public float AngularDrag { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 AngularVelocity { get; set; }
    }

    [SerializeField]
    private bool _useGravity = true;

    protected override void Awake()
    {
        base.Awake();
        
    }

    protected override void Start()
    {
        base.Start();
        
        if (_useGravity)
            Component.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (_useGravity && !Component.isKinematic)
        {
            float deltaTime = TimeScale * Time.fixedDeltaTime;
            Vector3 deltaDistance = Physics.gravity * deltaTime;
            Component.velocity += deltaDistance;
        }
    }

    protected override Snapshot TakeSnapshot()
    {
        return new Snapshot()
        {
            Drag = Component.drag,
            AngularDrag = Component.angularDrag,
            Velocity = Component.velocity,
            AngularVelocity = Component.angularVelocity
        };
    }

    protected override void ApplySnapshot(Snapshot snapshot, float scaleRatio)
    {
        Component.drag = snapshot.Drag * scaleRatio;
        Component.angularDrag = snapshot.AngularDrag * scaleRatio;
        Component.velocity = snapshot.Velocity * scaleRatio;
        Component.angularVelocity = snapshot.AngularVelocity * scaleRatio;
    }

    public bool UseGravity
    {
        get { return _useGravity; }
        set { _useGravity = value; }
    }
}
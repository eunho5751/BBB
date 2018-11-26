using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class TimeParticleSystem : TimeComponent<ParticleSystem, TimeParticleSystem.Snapshot>
{
    public struct Snapshot
    {
        public float SimulationSpeed { get; set; }
    }

    protected override Snapshot TakeSnapshot()
    {
        return new Snapshot()
        {
            SimulationSpeed = Component.main.simulationSpeed
        };
    }

    protected override void ApplySnapshot(Snapshot snapshot, float scaleRatio)
    {
        var module = Component.main;
        module.simulationSpeed = snapshot.SimulationSpeed * scaleRatio;
    }
}
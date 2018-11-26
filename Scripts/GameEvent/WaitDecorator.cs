using System.Collections;
using UnityEngine;

public class WaitDecorator : GameEventDecorator
{
    [SerializeField]
    private float _seconds = 0f;

    public override void Trigger()
    {
        CoroutineManager.Start(Wait(_seconds));
    }

    private IEnumerator Wait(float seconds)
    {
        float elapsedTime = 0f;
        while (elapsedTime < seconds)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
        }
        
        base.Trigger();
    }
}
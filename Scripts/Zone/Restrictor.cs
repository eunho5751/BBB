using System;
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-1)]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class Restrictor : MonoBehaviour
{
    private BoxCollider _collider;
    private MeshRenderer _renderer;
    private Vector3 _enterPosition;
    private event Action _onRestrictorPassed;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _renderer = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _enterPosition = other.transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        Vector3 diff = other.transform.position - _enterPosition;
        if (diff.x > 0f)
            _onRestrictorPassed?.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        StopCoroutine("Coloring");
        StartCoroutine("Coloring", _renderer.sharedMaterial);
    }

    private IEnumerator Coloring(Material sharedMat)
    {
        Color originalColor = sharedMat.color;
        Material mat = _renderer.material;
        Color targetColor = new Color(0.05f, 0.05f, 0.05f);
        float elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            mat.color = Color.Lerp(originalColor, targetColor, elapsedTime / 0.2f);
            elapsedTime += MainTimer.RawDelta;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            mat.color = Color.Lerp(targetColor, originalColor, elapsedTime / 0.2f);
            elapsedTime += MainTimer.RawDelta;
            yield return null;
        }

        mat.color = originalColor;
    }

    public void SetRestrictorActive(bool active)
    {
        _renderer.enabled = active;
        _collider.isTrigger = !active;
    }

    public event Action OnRestrictorPassed
    {
        add { _onRestrictorPassed += value; }
        remove { _onRestrictorPassed -= value; }
    }
}
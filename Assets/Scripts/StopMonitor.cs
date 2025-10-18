using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reusable component that monitors colliders' attached rigidbodies and invokes a callback
/// when the object's speed stays below a threshold for a specified duration.
/// Attach this to trigger GameObjects and call StartMonitoring/StopMonitoring from trigger scripts.
/// </summary>
public class StopMonitor : MonoBehaviour
{
    private class Watch
    {
        public Coroutine coroutine;
        public Rigidbody rb3;
        public Rigidbody2D rb2;
    }

    private readonly Dictionary<Collider, Watch> watches = new Dictionary<Collider, Watch>();

    /// <summary>
    /// Start monitoring a collider. If it stays below stopThreshold for holdTime seconds, onStopped is invoked with the collider.
    /// If there is already a watch for this collider it will be restarted.
    /// </summary>
    public void StartMonitoring(Collider collider, float stopThreshold, float holdTime, Action<Collider> onStopped)
    {
        if (collider == null) return;

        // If already watching this collider, stop the previous watch first
        StopMonitoring(collider);

        var watch = new Watch();
        watch.rb3 = collider.attachedRigidbody;
        if (watch.rb3 == null)
        {
            watch.rb2 = collider.GetComponent<Rigidbody2D>() ?? collider.GetComponentInParent<Rigidbody2D>();
        }

        // If no rigidbody found, do not start monitoring
        if (watch.rb3 == null && watch.rb2 == null)
        {
            return;
        }

        watch.coroutine = StartCoroutine(MonitorCoroutine(collider, watch, stopThreshold, holdTime, onStopped));
        watches[collider] = watch;
    }

    /// <summary>
    /// Stop monitoring a specific collider.
    /// </summary>
    public void StopMonitoring(Collider collider)
    {
        if (collider == null) return;
        if (watches.TryGetValue(collider, out var watch))
        {
            if (watch.coroutine != null)
            {
                StopCoroutine(watch.coroutine);
            }
            watches.Remove(collider);
        }
    }

    /// <summary>
    /// Stop all active watches.
    /// </summary>
    public void StopAllMonitoring()
    {
        foreach (var w in watches.Values)
        {
            if (w.coroutine != null) StopCoroutine(w.coroutine);
        }
        watches.Clear();
    }

    private System.Collections.IEnumerator MonitorCoroutine(Collider collider, Watch watch, float stopThreshold, float holdTime, Action<Collider> onStopped)
    {
        float timer = 0f;
        while (true)
        {
            float speed = 0f;
            if (watch.rb3 != null)
            {
                speed = watch.rb3.linearVelocity.magnitude;
            }
            else if (watch.rb2 != null)
            {
                speed = watch.rb2.linearVelocity.magnitude;
            }

            if (speed <= stopThreshold)
            {
                timer += Time.deltaTime;
                if (timer >= holdTime)
                {
                    // Fire the callback and remove the watch
                    onStopped?.Invoke(collider);
                    watches.Remove(collider);
                    yield break;
                }
            }
            else
            {
                timer = 0f; // reset
            }

            yield return null;
        }
    }

    private void OnDisable()
    {
        StopAllMonitoring();
    }
}

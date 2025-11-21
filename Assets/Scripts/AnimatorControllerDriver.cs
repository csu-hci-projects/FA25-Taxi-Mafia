using UnityEngine;
using UnityEngine.UI;

public class AnimatorControllerDriver : MonoBehaviour
{
    // assign automatically from the same GameObject
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator not found on " + gameObject.name);
    }

    // Example: crossfade to a state by name (optional)
    public void CrossfadeTo(string stateName, float transitionDuration = 0.1f, int layer = 0)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is null, cannot CrossFade.");
            return;
        }

        animator.CrossFade(stateName, transitionDuration, layer);
    }

}

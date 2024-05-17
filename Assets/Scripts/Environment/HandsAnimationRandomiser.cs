using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationKeyframeRandomiser : MonoBehaviour
{
    private void Start()
    {
        foreach (Transform child in transform)
        {
            // Get the Animator component
            if (child.TryGetComponent<Animator>(out var animator))
            {
                float startKeyframe = Random.Range(0f, 1f);
                animator.Play("Armature_Hand_roll_1", 0, startKeyframe);
            }
        }
    }
}
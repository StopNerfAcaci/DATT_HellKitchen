using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform targetTransform;

    public void SetTargetTransform(Transform target)
    {
        this.targetTransform = target;
    }

    private void LateUpdate()
    {
        if (this.targetTransform == null)
        {
            return;
        }
        transform.position = this.targetTransform.position;
        transform.rotation = this.targetTransform.rotation;
    }
}

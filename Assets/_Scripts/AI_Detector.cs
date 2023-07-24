using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class AI_Detector : MonoBehaviour
{
    public event EventHandler<Transform> DetectedInRange;

    public bool IsTargetInArea { get; private set; }
    [HideInInspector] public Vector2 directionToTarget => (Target.position - transform.position).normalized;

    [Header("Detection Parameters")]
    public Vector2 detectorOffset = Vector2.zero;
    public float detectionDelay = 0.5f;
    public float detectorSize = 5f;
    public LayerMask detectionTag;

    [Header("Gizmo Parameters")]
    public Color GizmoIdleColor = Color.red;
    public Color GizmoDetectedColor = Color.green;
    public bool GizmoEnabled = true;

    public Transform Target { get => target; private set { target = value; IsTargetInArea = target != null; } }
    private Transform target;

    private WaitForSeconds _detectionDelay;

    private void Start()
    {
        _detectionDelay = new WaitForSeconds(detectionDelay);
        StartCoroutine(StartDetecting());
    }
    private void OnDrawGizmos()
    {
        if (GizmoEnabled)
        {
            if (IsTargetInArea)
                Gizmos.color = GizmoDetectedColor;
            else
                Gizmos.color = GizmoIdleColor;

            Gizmos.DrawWireSphere((Vector2)transform.position + detectorOffset, detectorSize);
        }
    }

    private void PerformDetection()
    {
        Collider2D collider = Physics2D.OverlapCircle((Vector2)transform.position + detectorOffset, detectorSize, detectionTag);

        if (collider && collider.gameObject.GetComponent<Player>())
            Target = collider.transform;
        else
            Target = null;

        DetectedInRange?.Invoke(this, Target);
    }

    private IEnumerator StartDetecting()
    {
        yield return _detectionDelay;
        PerformDetection();
        StartCoroutine(StartDetecting());
    }

}
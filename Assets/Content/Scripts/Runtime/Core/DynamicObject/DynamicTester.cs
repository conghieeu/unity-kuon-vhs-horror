using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UHFPS.Tools;
using UHFPS.Runtime;
using TMPro;
using static UHFPS.Runtime.DynamicObject;

public class DynamicTester : MonoBehaviour
{
    public Transform target;
    public TransformType transformType;
    public MinMax openLimits;
    public float startingAngle;
    public Axis targetHinge = Axis.Y;
    public Axis targetForward = Axis.Z;
    public float openSpeed = 1f;
    public bool globalAxis;

    private float currentAngle;
    private float targetAngle;
    private bool isOpened;

    private Vector3 hingeAxis;
    private Vector3 forwardAxis;

    private void Awake()
    {
        if (globalAxis)
        {
            hingeAxis = targetHinge.Convert();
            forwardAxis = targetForward.Convert();
        }
        else
        {
            hingeAxis = target.Direction(targetHinge);
            forwardAxis = target.Direction(targetForward);
        }

        currentAngle = GetStartingAngle();
        targetAngle = currentAngle;
    }

    private void Update()
    {
        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, Time.deltaTime * openSpeed * 10);
        SetOpenableAngle(currentAngle);
    }

    [ContextMenu("Switch Opened")]
    public void SwitchOpened()
    {
        isOpened = !isOpened;
        targetAngle = isOpened ? openLimits.max : openLimits.min;
    }

    private void SetOpenableAngle(float angle)
    {
        Quaternion rotation = Quaternion.AngleAxis(angle, hingeAxis);

        //Vector3 finalForward = Quaternion.AngleAxis(angle, hingeAxis) * forwardAxis;
        //Quaternion finalRotation = Quaternion.LookRotation(finalForward);

        if (transformType == TransformType.Local)
        {
            target.localRotation = rotation;
        }
        else
        {
            target.rotation = rotation;
        }
    }

    private float GetStartingAngle()
    {
        float _startingAngle = startingAngle;
        return _startingAngle;
    }

    private void OnDrawGizmos()
    {
        if (target == null)
            return;

        Vector3 forward = Application.isPlaying ? forwardAxis : target.Direction(targetForward);
        Vector3 upward = Application.isPlaying ? hingeAxis : target.Direction(targetHinge);

        HandlesDrawing.DrawLimits(
            target.position,
            openLimits,
            forward,
            upward,
            false,
            false,
            0.3f
        );

        Vector3 startingDir = Quaternion.AngleAxis(GetStartingAngle(), upward) * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(target.position, startingDir * (0.4f));
    }
}
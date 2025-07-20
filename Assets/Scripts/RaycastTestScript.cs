using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class RaycastTestScript : MonoBehaviour
{
    
    public bool In1;
    public bool In2;
    public Logic.Operators Operator = new();
    public float Angle;
    public Vector3 dir;
    Ray ray = new Ray();
    public RaycastHit2D rayhit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dir = Vector3.up;
    }

    // Update is called once per frame
    void Update()
    {
        rayhit = Physics2D.Raycast(ray.origin, ray.direction,5000);
    }

    private void OnDrawGizmosSelected()
    {
        Func<bool, bool, bool> func = Operator;
        ray.origin = transform.position;
        dir = Quaternion.AngleAxis(Angle, new Vector3(0, 0, 1)) * dir;
        ray.direction = dir;

        Type[] types = new Type[]{ typeof(UltimateCurveGenerationScript) , typeof(Light2D) };

        rayhit = Logic.RaycastByTypes(types, Operator, ray.origin, ray.direction, 5000);

        float dist = (rayhit) ? rayhit.distance : 5000;

        Gizmos.DrawRay(ray.origin, ray.direction.normalized * dist);
    }

}

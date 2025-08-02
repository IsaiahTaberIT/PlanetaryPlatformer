using UnityEngine;
using System.Collections.Generic;
using UnityEditor;


[ExecuteInEditMode]
public class OrbitScript : MonoBehaviour
{
    public bool UseObjectRotation = true;
    public bool Simulate;
    public List<Orbiter> Orbiters = new();

#if UNITY_EDITOR
    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (!Application.isPlaying && Simulate)
        {

            MovePlanets(0.01f);

            SceneView.RepaintAll();
        }
    }
#endif
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < Orbiters.Count; i++)
        {
            Gizmos.color = Color.blue;

            for (int j = 0; j < 30; j++)
            {

                float angle = (j / 30f) * 360f;
                Vector3 pos = Vector3.zero;

                if (UseObjectRotation)
                {
                    pos = transform.TransformDirection(Quaternion.AngleAxis(angle, new Vector3(0, 1, 0)) * Vector3.right * Orbiters[i].PolarCoordinates.y) + transform.position;

                }
                else
                {
                    pos = (Quaternion.Euler(Orbiters[i].ManualOrbitAxisRotation) * (Quaternion.AngleAxis(angle, new Vector3(0, 1, 0)) * Vector3.right * Orbiters[i].PolarCoordinates.y)) + transform.position;

                }
                Gizmos.DrawSphere((pos),2);
            }
        }
    }
    private void OnValidate()
    {
        MovePlanets(0);
    }


    public void MovePlanets(float timeStep)
    {
        for (int i = 0; i < Orbiters.Count; i++)
        {
            Orbiter orbiter = Orbiters[i];

            float orbitAngle = orbiter.OrbitTimer.GetRatio() * 360;
            orbiter.OrbitTimer.Step(timeStep);
            float revolutionAngle = orbiter.RevolutionTimer.GetRatio() * 360;
            orbiter.RevolutionTimer.Step(timeStep);
            orbiter.SetAngle(orbitAngle);
            Vector3 pos = Vector3.zero;
            if (UseObjectRotation)
            {
                pos = transform.TransformDirection(Quaternion.AngleAxis(orbitAngle, new Vector3(0, 1, 0)) * Vector3.right * Orbiters[i].PolarCoordinates.y) + transform.position;
                orbiter.Object.transform.localRotation = Quaternion.AngleAxis(revolutionAngle, transform.TransformDirection(Vector3.up));
                orbiter.Object.transform.localRotation *= transform.rotation;
            }
            else
            {
                pos = (Quaternion.Euler(orbiter.ManualOrbitAxisRotation) * (Quaternion.AngleAxis(orbitAngle, new Vector3(0, 1, 0)) * Vector3.right * Orbiters[i].PolarCoordinates.y)) + transform.position;
                orbiter.Object.transform.localRotation = Quaternion.AngleAxis(revolutionAngle, Quaternion.Euler(orbiter.ManualOrbitAxisRotation) * Vector3.up);
                orbiter.Object.transform.localRotation *= Quaternion.Euler(orbiter.ManualOrbitAxisRotation);
            }

            orbiter.Object.transform.position = pos;

            Orbiters[i] = orbiter;

        }
    }
    
    private void Update()
    {
        if (Application.isPlaying)
        {
            MovePlanets(Time.deltaTime);

        }


    }


    [System.Serializable]
    
    public struct Orbiter
    {
        public GameObject Object;
        public Vector2 PolarCoordinates;
        public Logic.Timer OrbitTimer;
        public Logic.Timer RevolutionTimer;
        public Vector3 ManualOrbitAxisRotation;


        /*
        public Orbiter(GameObject obj, float time, float angle, float distance)
        {
            RotationTime = 0;
            ElapsedRotationTime = 0;
            ElapsedTime = 0;
            Object = obj;
            CompletionTime = time;
            PolarCoordinates = new Vector2(angle, distance);
        }
        public Orbiter(GameObject obj, float time, Vector2 polarCoords)
        {
            ElapsedTime = 0;
            Object = obj;
            CompletionTime = time;
            PolarCoordinates = polarCoords;
        }
        */

     
        public void SetAngle(float angle)
        {
            PolarCoordinates.x = angle;
        }

        public void SetDistance(float distance)
        {
            PolarCoordinates.y = distance;
        }
    }
}

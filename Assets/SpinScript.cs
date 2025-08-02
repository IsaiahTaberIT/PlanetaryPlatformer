using UnityEngine;

public class SpinScript : MonoBehaviour
{
    public Logic.Timer RevolutionTimer;
    public Vector3 RotationAxis;
    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.AngleAxis(RevolutionTimer.GetRatio() * 360, Quaternion.Euler(RotationAxis) * Vector3.up);
        RevolutionTimer.Step();

    }
}

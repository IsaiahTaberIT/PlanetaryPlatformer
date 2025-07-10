using UnityEngine;
using static MovementScript;
public class MovingPlatform : MonoBehaviour
{

    public bool ResetOnStart = false;
    public bool SpinInEditor = true;
    [HideInInspector] public bool DisplayFunctions;
    public Logic.Timer RevolutionTimer;
    public bool Spin;
    public bool Translate;
    public AlternatingTranslation Movementcontroller;
    public Vector3 Vel;
    private Vector3 _lastPos;
    public float Angle;
    public Rigidbody2D Myrigidbody;

    
    public bool ActiveAndSpinning()
    {
        if (this.isActiveAndEnabled && Spin && !(RevolutionTimer.Clamp && RevolutionTimer.Ratio == 1))
        {
            return true;
        }

        return false;
    }

    public void InvertSpinningDirection()
    {
        RevolutionTimer.EndTime *= -1;
        
    }


    private void Start()
    {
        if (ResetOnStart)
        {
            RevolutionTimer.Time = 0;
        }

        TryGetComponent(out Myrigidbody);
    }

    public void TrySpinPlatform()
    {
        if (Spin)
        {
            Angle = RevolutionTimer.Ratio * 360;

            if (Myrigidbody != null)
            {
                Myrigidbody.MoveRotation(Angle);
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, Angle);

            }

            RevolutionTimer.Step();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    void FixedUpdate()
    {

        TrySpinPlatform();



        if (Translate)
        {
            Vel = transform.position - _lastPos;
            Vel /= Time.deltaTime;
            _lastPos = transform.position;

            Movementcontroller.StepTowardsNextTarget(transform);    
        }

    }
}

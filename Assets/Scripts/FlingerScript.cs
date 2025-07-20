
using UnityEngine;

[ExecuteInEditMode]

public class FlingerScript : MonoBehaviour
{
    [SerializeField] public Vector3 Direction = Vector3.right;
    [SerializeField] private float _FlingerForce = 100;
    [SerializeField] private float _Acceleration = 1;
    public float test;
    public float Angle;
    public Rigidbody2D body;
    private float _OldAngle;
    private Vector3 _OldDirection;
    public float AlignedAngle;
    public Logic.IAngle iAngle;
    [SerializeField] private MonoBehaviour referenceScript;
    [SerializeField] private bool _PreserveMomentum = true;

    public float OffsetAngle;
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<MovementScript>(out MovementScript movementScript))
        {
            body = collision.gameObject.GetComponent<Rigidbody2D>();
            Fling();
        }
    }

    void Fling()
    {
        if (_PreserveMomentum)
        {
            if (_Acceleration < 100)
            {
                Vector3 forceInDirection = Vector3.Dot(body.linearVelocity, Direction.normalized) * Direction;
                Vector2 TargetVelocity = Logic.LerpVector(forceInDirection, Direction.normalized * _FlingerForce, Time.deltaTime * _Acceleration);

                body.linearVelocity -= (Vector2)forceInDirection;
                body.linearVelocity += TargetVelocity;

            }
            else
            {
                Vector3 forceInDirection = Vector3.Dot(body.linearVelocity, Direction.normalized) * Direction;
                body.linearVelocity -= (Vector2)forceInDirection;
                body.linearVelocity += (Vector2)(Direction.normalized * _FlingerForce);
            }
        }
        else
        {
            if (_Acceleration < 100)
            {
                body.linearVelocity = Logic.LerpVector(body.linearVelocity, Direction.normalized * _FlingerForce, Time.deltaTime * _Acceleration);
            }
            else
            {
                body.linearVelocity = Direction.normalized * _FlingerForce;

            }
        }

    }

    [ContextMenu("Find ALignment Script")]
    void FindAlgimentscript()
    {
        TryGetComponent<PlanetAlignmentScript>(out PlanetAlignmentScript align);
        if (align != null)
        {
            referenceScript = align;
        }
    }

        [OnEditorMoved]
    void CalculateState()
    {
        if (referenceScript != null && referenceScript is Logic.IAngle)
        {
          //  Debug.Log("CalculateState");
            if (iAngle == null)
            {
                iAngle = referenceScript as Logic.IAngle;
            }

            AlignedAngle = iAngle.FloatValue;

            Angle = AlignedAngle + OffsetAngle;
            test = AlignedAngle + OffsetAngle;
        }


        if (_OldDirection != Direction)
        {
            Angle = -Vector2.SignedAngle(Direction, Vector2.up);
        }
        if (_OldAngle != Angle)
        {
            Direction = Quaternion.AngleAxis(Angle, new Vector3(0, 0, 1)) * new Vector3(0, 1, 0);
        }

        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        SpriteRenderer circleSpriteRenderer = gameObject.transform.Find("Circle").GetComponent<SpriteRenderer>();


        byte red = (byte)Mathf.Lerp(60, 255, (_FlingerForce < 300) ? Mathf.InverseLerp(0, 150, _FlingerForce) : Mathf.InverseLerp(1000, 300, _FlingerForce));
        byte green = (byte)Mathf.Lerp(255, 20, Mathf.InverseLerp(150, 300, _FlingerForce));
        byte blue = (byte)Mathf.Lerp(160, 20, Mathf.InverseLerp(0, 300, _FlingerForce));

        spriteRenderer.color = new Color32(red, green, 0, (byte)(20 + (140 * Mathf.InverseLerp(0, 70, Mathf.Pow(_Acceleration, 2)))));
        circleSpriteRenderer.color = new Color32(red, green, 0, (byte)(20 + (80 * Mathf.InverseLerp(0, 70, Mathf.Pow(_Acceleration, 2)))));


        gameObject.transform.Find("Arrow").GetComponent<Transform>().localEulerAngles = new Vector3(0, 0, Angle);

        _OldAngle = Angle;
        _OldDirection = Direction;
    }

    private void OnValidate()
    {
        CalculateState();
    }

}


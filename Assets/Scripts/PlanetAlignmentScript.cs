
using UnityEngine;

[ExecuteInEditMode]


public class PlanetAlignmentScript : MonoBehaviour, Logic.IAngle
{

    public bool AlignToRadius = false;
    private bool LastAlignToRadius = true;
    [Tooltip("if true raduis wiil be fixed to a specific value, otherwise it will snap to the closest value within the step size")]
    public float Radius;
    public float RadiusSnapSize =1;
    public bool SnapAngle = false;
    public float AngleSnapSize = 1;

    public bool FixedRadius;
    public bool AlignToTarget = true;
    public bool AlignToSurface = true;

    public bool AutoGenerateAlignmentTarget = false;

    public float SurfaceCheckDistance = 0.5f;
    public float VerticalOffset = 0.5f;
    public float _CurrentRadius;
    public LayerMask SurfaceLayer;

    private SpriteRenderer spriteRenderer;
    public GameObject BoundsCalculationTarget;

    public GameObject AlignmentTarget;
    public float Angle;
    public float FloatValue => Angle;
    public Vector3 Size;
    private Vector3 _Lastpos;
    public Gravity[] _Planets;
    private Logic.RayRenderer Rayrenderer;
    [ContextMenu("Refresh Planet Array")]
    void RefreshPlanetArray()
    {
        _Planets = new Gravity[0];

    }






    [OnEditorMoved]
    void GetBestAligmentTarget()
    {
        if (AutoGenerateAlignmentTarget)
        {
            if (!Application.isPlaying)
            {
                if (_Planets.Length == 0)
                {
                    _Planets = FindObjectsByType<Gravity>(FindObjectsSortMode.None);

                }

                if (_Planets.Length > 0)
                {
                    float maxgravity = 0;
                    float planetgravity;
                    Gravity planet = _Planets[0];
                    for (int i = 0; i < _Planets.Length; i++)
                    {
                        planetgravity = _Planets[i].PlanetGravityVector(transform.position).sqrMagnitude;
                        if (planetgravity > maxgravity)
                        {
                            maxgravity = planetgravity;
                            planet = _Planets[i];
                        }

                    }

                    AlignmentTarget = planet.gameObject;
                }
             

            }
        }
    }

    [OnEditorMoved]

    void AlignAngle()
    {
        if(SnapAngle)
        {
            Angle = -Vector2.SignedAngle(AlignmentTarget.transform.position - transform.position, Vector2.down);
            _CurrentRadius = Vector2.Distance(transform.position, AlignmentTarget.transform.position);
            float localRadius = (AlignToRadius) ? Mathf.RoundToInt(_CurrentRadius / RadiusSnapSize) * RadiusSnapSize : _CurrentRadius;

            Angle = Mathf.RoundToInt(Angle / (AngleSnapSize / localRadius)) * (AngleSnapSize/ localRadius);
            Vector3 position = Quaternion.AngleAxis(Angle + 180, new Vector3(0, 0, 1)) * (localRadius * Vector2.down);
            position += AlignmentTarget.transform.position;
            transform.position = position;
        }
    }


    [OnEditorMoved]
    void AlignRadius()
    {
        if (!LastAlignToRadius && AlignToRadius)
        {
            LastAlignToRadius = true;

           
        }

        LastAlignToRadius = AlignToRadius;

        if (AlignToRadius && AlignmentTarget != null)
        {
            _CurrentRadius = Vector2.Distance(transform.position, AlignmentTarget.transform.position);

            if (transform.position != _Lastpos)
            {
                //  Radius = _CurrentRadius;
                if (SnapAngle)
                {
                    Angle = Mathf.RoundToInt(Angle / (AngleSnapSize / Radius)) * (AngleSnapSize / Radius);
                }
                else
                {
                    Angle = -Vector2.SignedAngle(AlignmentTarget.transform.position - transform.position, Vector2.down);

                }
            }

            if (FixedRadius)
            {

                if (_CurrentRadius == 0)
                {
                    Radius = _CurrentRadius;
                }

               
            }
            else
            {
                if(RadiusSnapSize <= 0)
                {
                    RadiusSnapSize = 0.01f;
                }
                Radius = Mathf.RoundToInt(_CurrentRadius / RadiusSnapSize) * RadiusSnapSize;
            }

            Vector3 position = Quaternion.AngleAxis(Angle + 180, new Vector3(0, 0, 1)) * (Radius * Vector2.down);
            position += AlignmentTarget.transform.position;
            transform.position = position;

        }
    }


   // float RoundRadius()
  //  {
    //    if (_CurrentRadius / RadiusSnapSize)
     //   {

      //  }
//    }
        



    private void OnValidate()
    {
        AlignRadius();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Logic.RenderRayRenderer(Rayrenderer);
    }

    [OnEditorMoved]
    public void AlignSurface()
    {
        if (AlignToSurface)
        {
            LayerMask layer = gameObject.layer;
            gameObject.layer = 0;
            Vector3 direction = Quaternion.AngleAxis(Angle, new Vector3(0, 0, 1)) * new Vector3(0, -1, 0);


            RaycastHit2D detectionRay = Physics2D.Raycast(transform.position, direction.normalized, SurfaceCheckDistance, SurfaceLayer);
            Rayrenderer = new Logic.RayRenderer(new Ray(transform.position, direction.normalized), 100);
           // Debug.Log(direction);


            if (detectionRay)
            {
               // Debug.Log(true);

                Vector3 rayHitPoint = detectionRay.point;
                // Debug.Log(rayHitPoint);

                float z = gameObject.transform.position.z;
                Vector3 newPos = rayHitPoint + VerticalOffset * -direction;
                newPos.z = z;

                gameObject.transform.position = newPos;
            }


            gameObject.layer = layer;
        }

    }





    [OnEditorMoved]
    public void Align()
    {
        if (AlignmentTarget != null)
        {
            Angle = -Vector2.SignedAngle(AlignmentTarget.transform.position - transform.position, Vector2.down);

          
  
        }
        else
        {
            Angle = 0;
        }

        if (AlignToTarget)
        {
            Vector3 currentrot = transform.eulerAngles;
            currentrot.z = Angle;
            transform.eulerAngles = currentrot;
        }


    }

    [ContextMenu("Calculate Vertical Offset")]

    void CalculateVerticalOffset()
    {
        if (spriteRenderer == null)
        {
            TryGetComponent(out spriteRenderer);
        }


        if (spriteRenderer != null)
        {
            if (BoundsCalculationTarget != null)
            {

            }
            else
            {
                Size = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit * transform.lossyScale;

                VerticalOffset = Size.y / 2;
            }
                

        }
    }

}

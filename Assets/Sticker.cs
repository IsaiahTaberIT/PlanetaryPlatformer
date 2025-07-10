
using UnityEngine;
using UnityEngine.Rendering.Universal;
[ExecuteInEditMode]
public class Sticker : MonoBehaviour
{
    public bool SimpleCircle = false;
    private bool _ParentCanMove = false;
    public delegate void RepairColliders();
    public static event RepairColliders ColliderRepair;
    private MovingPlatform _Platform;
    [ContextMenu("Probaly Wont Work")]

    private void Start()
    {
        _ParentCanMove = transform.parent.TryGetComponent<MovingPlatform>(out _Platform);
    }
    public void InvokeRepair()
    {
        ColliderRepair.Invoke();
    }
    void OnEnable()
    {
        ColliderRepair += UpdateCollider;
    }


    void OnDisable()
    {
        ColliderRepair -= UpdateCollider;

    }
    public interface IStickerable
    {
        static public void StickerCollision(Collider2D collision, ref StickerAbleInfo StickerInfo, bool _MovingLeft, bool _MovingRight, Transform transform , LayerMask GroundLayer , Rigidbody2D Body)
        {
          
            if (collision.TryGetComponent(out Sticker sticker))
            {
                if (sticker.SimpleCircle)
                {
                    //  RaycastHit2D closestpoint = Physics2D.Raycast(transform.position, Direction, 5, GroundLayer);
                    if (StickerInfo.StickerCooldown.GetRatio() >= 1)
                    {
                        
                        StickerInfo._Stickered = true;
                        RaycastHit2D rayhit = Physics2D.Raycast(transform.position, sticker.Parentcollider.transform.position - transform.position , 5, GroundLayer);

                        if (rayhit)
                        {
                            StickerInfo.StickerPull = (rayhit.point - (Vector2)transform.position).normalized;
                            Body.AddForce(StickerInfo.StickerPull * (sticker.Adhesion));
                            StickerInfo.StickerAngle = Vector2.SignedAngle(Vector2.up, -StickerInfo.StickerPull);
                        }
            
                    }
                      

                }
                else
                {
                    Vector3 MovementVel = Vector3.zero;
                    if (sticker._ParentCanMove)
                    {
                        MovementVel = sticker._Platform.Vel;
                    }

                    Vector2 lastStickerNormal = StickerInfo.StickerNormal;
                    Vector2 movingDir = (_MovingRight) ? Vector2.right : (_MovingLeft) ? Vector2.left : Vector2.zero;
                    movingDir /= 10;

                    Vector2 Direction = (sticker.Parentcollider.ClosestPoint(transform.position + transform.TransformDirection(movingDir)) - (Vector2)transform.position).normalized;
                    Vector3 start = transform.position;
                    RaycastHit2D normalcheckCenter = Physics2D.Raycast(start, Direction, 5, GroundLayer);

                    bool WallEvaluation = sticker.HandleWallIntersection ? true : (normalcheckCenter.collider.gameObject == sticker.Parentcollider.gameObject);

                    if (normalcheckCenter && StickerInfo.StickerCooldown.GetRatio() >= 1 && WallEvaluation)
                    {
                        StickerInfo._Stickered = true;

                        // Body.AddForce(StickerInfo.StickerPull);
                        //  Debug.Log(sticker._ParentCanMove);
                        float movemetvelreduction = Vector3.Dot(-StickerInfo.StickerPull.normalized, MovementVel);
                        // Debug.Log(movemetvelreduction);

                        Body.AddForce(StickerInfo.StickerPull * (sticker.Adhesion - movemetvelreduction * 20));

                        StickerInfo.StickerNormal = (normalcheckCenter.normal);
                        StickerInfo.StickerPull = Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.up, StickerInfo.StickerNormal), new Vector3(0, 0, 1)) * Vector2.down;
                        StickerInfo.StickerAngle = Vector2.SignedAngle(Vector2.up, StickerInfo.StickerNormal);

                    }


                    if (Vector2.Angle(lastStickerNormal, StickerInfo.StickerNormal) > 45f)
                    {
                        if (_MovingRight)
                        {
                            Body.AddForce(transform.TransformDirection(Vector2.right * 100));
                        }
                    }

                    if (Vector2.Angle(lastStickerNormal, StickerInfo.StickerNormal) > 45f)
                    {
                        if (_MovingLeft)
                        {
                            Body.AddForce(transform.TransformDirection(Vector2.left * 100));
                        }
                    }
                }
              
            }
        }

    }

    [System.Serializable]
    public struct StickerAbleInfo
    {
        public Logic.Timer StickerCooldown;
        public float StickerAngle;
        public Vector2 StickerNormal;
        public Vector3 StickerPull;
        public bool _Stickered;
        public bool _WasStickered;
    }

    public bool HandleWallIntersection = true;
    [SerializeField] private float _EdgeExpansion;
    public bool Glowing;
    public Collider2D Parentcollider;
    public Collider2D MyCollider;
    public float Adhesion = 500;
    public Light2D MyLight;

    private void OnTriggerEnter2D(Collider2D collision)
    {
     
        if (collision.TryGetComponent(out IStickerable target))
        {
            if (MyCollider is CircleCollider2D)
            {
                (MyCollider as CircleCollider2D).radius += _EdgeExpansion;
            }
            else if (MyCollider is EdgeCollider2D)
            {
                (MyCollider as EdgeCollider2D).edgeRadius += _EdgeExpansion;
            }
            else if (MyCollider is BoxCollider2D)
            {
                (MyCollider as BoxCollider2D).edgeRadius += _EdgeExpansion;
            }

        }
    }

    

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IStickerable target))
        {

            if (MyCollider is CircleCollider2D)
            {
                (MyCollider as CircleCollider2D).radius -= _EdgeExpansion;
            }
            else if (MyCollider is EdgeCollider2D)
            {
                (MyCollider as EdgeCollider2D).edgeRadius -= _EdgeExpansion;
            }
            else if (MyCollider is BoxCollider2D)
            {
                (MyCollider as BoxCollider2D).edgeRadius -= _EdgeExpansion;
            }

            if (collision.TryGetComponent(out PlayerScript player))
            {

                player.CyoteTime.Time = player.CyoteTime.EndTime;
                player.isCyoteGrounded = false;
                player.IsGrounded = false;
            }

        }

     

    }

    void UpdateCollider()
    {





        try
        {
            if (transform.parent.gameObject == null)
            {
                return;
            }
        }
        catch
        {
            return;

        }
        GameObject parent = transform.parent.gameObject;
        Parentcollider = parent.GetComponent<Collider2D>();
        MyCollider = GetComponent<Collider2D>();

        
        if (MyCollider is EdgeCollider2D)
        {
            Vector2[] points = new Vector2[0];

            if (Parentcollider is EdgeCollider2D)
            {
                points = (Parentcollider as EdgeCollider2D).points;
            }
            else if (Parentcollider is PolygonCollider2D)
            {
                points = (Parentcollider as PolygonCollider2D).points;
            }
            else
            {
                return;

            }

           (MyCollider as EdgeCollider2D).points = points;

            AssignLightPoints(points);
        }
        else if (MyCollider is PolygonCollider2D)
        {
            Vector2[] points = new Vector2[0];

            if (Parentcollider is EdgeCollider2D)
            {
                points = (Parentcollider as EdgeCollider2D).points;
            }
            else if (Parentcollider is PolygonCollider2D)
            {
                points = (Parentcollider as PolygonCollider2D).points;
            }
            else
            {
                return;

            }

            (MyCollider as PolygonCollider2D).points = points;
            AssignLightPoints(points);

        }
        
    }

    void AssignLightPoints(Vector2[] points)
    {
        
        if (TryGetComponent(out MyLight))
        {
            if (Glowing)
            {
                if (!MyLight.enabled)
                {
                    MyLight.enabled = true;

                }

                Vector3[] V3Array = new Vector3[points.Length - 1];

                for (int i = 0; i < V3Array.Length; i++)
                {
                    V3Array[i] = points[V3Array.Length - i];
                }

                MyLight.SetShapePath(V3Array);
            }
            else
            {
                MyLight.enabled = false;
            }
            
        }
        
    
    }

    private void OnValidate()
    {
        UpdateCollider();
    }
    private void Awake()
    {
    
    }


}

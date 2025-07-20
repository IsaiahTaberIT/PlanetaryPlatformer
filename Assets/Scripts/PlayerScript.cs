
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerScript : MovementScript , Sticker.IStickerable
{

    [Header("Player Movement")]
    [SerializeField] private bool _TriedJump;
    [Space]
    public Sticker.StickerAbleInfo StickerInfo;
    private bool _MovingLeft;
    private bool _MovingRight;
    public float VelocityMagnitude;
    private Vector3 _LastPos = Vector3.zero;
    public bool isCyoteGrounded;
    public bool Moving = false;
    public Logic.Timer CyoteTime = new(0.06f);
    public Vector3 MousePosition;
    [Space]

    [Header("Launch")]
    private bool Launch = false;
    public float LaunchForce = 5000;
    public int RemaingLaunches = 0;
    public int MaxLaunches = 1;
    public Vector3 LookDirection;
    private Vector3 _RelativeDirection;

    [Space]

    [Header("Movement Prediction")]
    [Space]
    public MovementPrediction prediction;

  
    float predictionTimer = 0;
    [SerializeField] private List<Logic.RayRenderer> RenderRays = new List<Logic.RayRenderer>(0);
    [Space]
    [SerializeField] private int SavedEndPoints = 0;
    [SerializeField] private float AverageDist = 0;
    [SerializeField] private float AverageEndGravity = 0;
    [SerializeField] private float DotGravAndVel = 0;
    [SerializeField] private float _InitialThreshold = 100000;
    [SerializeField] private float _TrueThreshold = 100000;
    public Logic.Timer EscapeConfidence = new(20);
    public Logic.Timer EscapeTimer = new(5, 0);
    [Space]
    [Header("GravityArrows")]
    [Space]
    public GravityArrowObjects ArrowObjects;

    [Space]
    private Vector3 _TargetAngle = Vector3.zero;
    private Vector3 _LerpedAngle;
    public float PlayerRunningGravityMagnitudes;
    [Space]
    [Header("Misc Object References")]
    [Space]
    public GameObject MyLight;
    public AudioSource DeathSound;
    [SerializeField] ContactGetterScript MyContactGetterScript;
    [SerializeField] private AudioEvent DeathEvent2;
    [SerializeField] private GameObject Particle;
    public GameObject MainCamera;
    public SoftBodyScript SoftBody;
    public LightsBakeable2D MyBaked;
    public float CameraFollowRateDistanceFactor;
    public float CameraFollowForce = 0.02f;
    [SerializeField] private float _SlowFall = 2;
    [SerializeField] private float _HorizontalBoostFromJumping;

    [SerializeField] private float _JumpCoolDown = 1;
    private float _jumpTimer;
    [SerializeField] private float _SafetyWindow = 100f;
    public Vector2 direction;
    private GameLogicScript.GravityInfo _GravityInfo;
    private List<Vector2> _IntersectList = new List<Vector2>(0);
    [SerializeField] private float _AngleTransitionRate;
    [SerializeField] private float _AngleTransition;
    [SerializeField] private Vector2 _LastValidNormal;
    [SerializeField] private ParticleSystem MovementParticles;

    private Transform _MyTransform;
    
    //public MovementPrediction.PredictionVars PlayerPredictionVars;

    [System.Serializable]
    public struct GravityArrowObjects
    {
        public GameObject Arrow1;
        public GameObject Arrow2;
        public GameObject Arrow3;
        public SpriteRenderer Arrow1SpriteRenderer;
        public SpriteRenderer Arrow2SpriteRenderer;
        public SpriteRenderer Arrow3SpriteRenderer;
    }
    void CheckIfEscaped()
    {
        AverageDist = 0;
        AverageEndGravity = 0;

        if (prediction.PredictionEndGravity.Count > SavedEndPoints)
        {
            prediction.PredictionEndGravity.RemoveRange(0, prediction.PredictionEndGravity.Count - SavedEndPoints);
        }

        if (prediction.PredictionEndDistances.Count > SavedEndPoints)
        {
            prediction.PredictionEndDistances.RemoveRange(0, prediction.PredictionEndDistances.Count - SavedEndPoints);

        }

        for (int i = 0; i < prediction.PredictionEndDistances.Count && i < prediction.PredictionEndGravity.Count; i++)
        {
            AverageDist += prediction.PredictionEndDistances[i];
            AverageEndGravity += prediction.PredictionEndGravity[i].magnitude;
        }

        AverageDist /= SavedEndPoints;
        AverageEndGravity /= SavedEndPoints;


        float velocityMagnitude = Body.linearVelocity.magnitude;

        DotGravAndVel = Vector2.Dot(Body.linearVelocity, -GravityDown.normalized) + velocityMagnitude;
        DotGravAndVel /= velocityMagnitude * 2;
        DotGravAndVel++;
       

        AverageDist /= (GravityDown.sqrMagnitude + AverageEndGravity) / 2;
        AverageDist *= DotGravAndVel;
        if (AverageDist > _InitialThreshold)
        {
            EscapeConfidence.Time += Time.deltaTime * Mathf.Clamp(AverageDist / _InitialThreshold, 0,3);
        }
        else
        {
            EscapeConfidence.Time = 0;
        }

        if (EscapeConfidence.Time >= EscapeConfidence.EndTime)
        {

            if (EscapeTimer.EndTime > EscapeTimer.Time)
            {
                if (_TrueThreshold <= AverageDist)
                {
                    MyGamelogicScript.EscapeUI.SetActive(true);

                    MyGamelogicScript.SetCountDown(EscapeTimer.EndTime - EscapeTimer.Time);
                    EscapeTimer.Time += Time.deltaTime;

                }
                else
                {
                    MyGamelogicScript.EscapeUI.SetActive(false);
                    EscapeTimer.Time = 0;
                  

                }
            }
            else
            {
                MyGamelogicScript.GameOver();
            }
           
        }
        else
        {
            if (MyGamelogicScript.EscapeUI != null)
            {
                MyGamelogicScript.EscapeUI.SetActive(false) ;    

            }

            EscapeTimer.Time = 0;
        }


        float targetSize = Mathf.Lerp(0.8f, 0.4f, EscapeConfidence.Time / EscapeConfidence.EndTime);
        float currentSize = MyGamelogicScript.shaderMaterial.GetFloat("_Size");
        float lerpRate = (targetSize < currentSize) ? 0.1f : 0.01f;

        targetSize = Mathf.Lerp(currentSize, targetSize, lerpRate);
        MyGamelogicScript.shaderMaterial.SetFloat("_Size", (targetSize));
        

    }

    // Start is called before the first frame updater
    void Start()
    {
        if (MainCamera == null)
        {
            MainCamera = Camera.main.gameObject;
        }
        _MyTransform = transform;
        Body = GetComponent<Rigidbody2D>();
        MyGamelogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();
        _GravityInfo = GameLogicScript.GravityDirection(_MyTransform.position, Vector3.down * MyGamelogicScript.NormalGravity);
        prediction.Body = Body;
        prediction.MyTransform = _MyTransform;

    }

    // Update is called once per frame
    void FixedUpdate() 
    {
        if (_GravityInfo.VectorArray == null)
        {
            _GravityInfo = GameLogicScript.GravityDirection(_MyTransform.position, Vector3.down * MyGamelogicScript.NormalGravity);

        }
        if (MyGamelogicScript.RunGame)
        {
            if (!IsGameRunning)
            {
                Body.linearVelocity = PrePauseVelocity;
                IsGameRunning = true;
            }

            if (_IntersectList.Count > 130)
            {
                _IntersectList.Clear();
            }


            //DoGravity

            _MovingLeft = false;
            _MovingRight = false;

            //gravityinfo is a struct that contains all the information the player needs for its functionality

            _GravityInfo = GameLogicScript.GravityDirection(_MyTransform.position, Vector3.down * MyGamelogicScript.NormalGravity);

            StickerInfo.StickerCooldown.Step();

            if (StickerInfo._Stickered && StickerInfo.StickerCooldown.GetRatio() >= 1)
            {
                GravityDown = StickerInfo.StickerPull.normalized * 1;

                _GravityInfo.Gravity = GravityDown;
            }
            else
            {
                GravityDown = _GravityInfo.Gravity;
            }

            GravityDownMagnitude = GravityDown.magnitude;

            LookRotation = TargetAngleBasedOnGravityDirection(GravityDown);

            LookDirection = new Vector3(0, 0, Mathf.LerpAngle(MainCamera.transform.localEulerAngles.z, LookRotation, 0.05f));

            Body.AddForce(GravityDown * Body.mass);


            //allowing physics if player cant move but game isnt paused

            if (MyGamelogicScript.CanMove)
            {
              //  Physics2D.RaycastAll
                PlayerMovement(true,true);
            }
            else
            {
                Velocity = Body.linearVelocity;
            }

            ApplyVelocity();
        }
        else
        {
            if (IsGameRunning)
            {
                PrePauseVelocity = Body.linearVelocity;
                IsGameRunning = false;
            }

            Body.linearVelocity = Vector2.zero;
        }

        if (!isCyoteGrounded)
        {
            if (MyGamelogicScript.EscapeUI != null)
            {
                CheckIfEscaped();
            }

            if (Body != null && predictionTimer >= prediction.PredictionFrequency)
            {
                PredictMotion();
                predictionTimer = 0;
            }
            else
            {
                predictionTimer += Time.deltaTime;
            }

        }
        else
        {
            if(EscapeTimer.Time == 0 || EscapeConfidence.Time == 0)
            {
                //this is to refresh the EscapeUiStuff when the player has hit the ground
                EscapeTimer.Time = 0;
                EscapeConfidence.Time = 0;
                CheckIfEscaped();

            }

        }

        StickerInfo._Stickered = false;
    }

    //end Gravity
   


    void PlayerMovement(bool canMove, bool runGame)
    {
        // float oldTorque = torque;

        MainCamera.transform.localEulerAngles = LookDirection;

        Vector3 WorldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, MainCamera.transform.position.z * -1));
        Vector2 lastdir = _RelativeDirection;
        _RelativeDirection = new Vector3(WorldMousePos.x - _MyTransform.position.x, WorldMousePos.y - _MyTransform.position.y).normalized;
        MousePosition = _RelativeDirection;
     
        Moving = false;

        // Perform ground check

        Body.MoveRotation(LookRotation);

         IsGrounded = CheckGrounded(GravityDown, GroundCheckDistance, GroundLayer, ref StickerInfo._Stickered);

      //  IsGrounded = true;
       // Debug.Log(obj1 + " , " + obj2);


        if (IsGrounded)
        {
            RemaingLaunches = MaxLaunches;
            CyoteTime.Time = 0;
            isCyoteGrounded = true;
        }
        else
        {
            if (CyoteTime.Time <= CyoteTime.EndTime)
            {
                CyoteTime.Time += Time.fixedDeltaTime;
                isCyoteGrounded = true;
            }
            else
            {
                isCyoteGrounded = false;
            }
        }


     
        // if the player hits a flinger this prevents the player from jumping

        isCyoteGrounded = (IsFlingered && isCyoteGrounded) ? false : isCyoteGrounded;


        // im leaving this for now because while it is not needed anymore, it is kinda cool

        /*

        RaycastHit2D[] rays = new RaycastHit2D[_Raycount];
        float rayspacing = 0.05f;


        if (isCloseToground)
        {
            _Normal = Vector2.zero;
            _NormalDivisor = 1;
        }


        if (isCloseToground)
        {
            if (_AngleTransition < 1)
            {
                _AngleTransition += (_AngleTransitionRate / 100);

            }

            float Angle = -Vector2.SignedAngle(LookDirection, Vector2.up);
            float contactY = 0;
            float PlayerY = ((Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * _MyTransform.position).y - 0.5f);

            for (int i = 0; i < _Raycount; i++)
            {
                rays[i] = Physics2D.Raycast(_MyTransform.position + Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * GravityDown.normalized * ((rayspacing * _Raycount / -2) + rayspacing * i), GravityDown, 5, GroundLayer);
                if (rays[i].collider != null)
                {
                    //  Debug.Log(rays[i].normal);
                    contactY = (Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * rays[i].point).y;
                    _Normal += rays[i].normal;
                    _NormalDivisor++;
                    _IntersectList.Add(rays[i].point);
                }
            }

            contactY /= _NormalDivisor - 1;

            Vector3 modpos = (Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * transform.position);

            modpos.y = contactY - 0.55f;



            modpos = (Quaternion.AngleAxis(Angle, new Vector3(0, 0, 1)) * modpos);



            //transform.position = modpos;
        }
        else
        {
            if (_AngleTransition > 0)
            {
                _AngleTransition -= (_AngleTransitionRate / 100);
            }
        }


        if (_Normal != Vector2.zero)
        {
            _LastValidNormal = _Normal;
        }

       // _MyTransform.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(LookRotation, -Vector2.SignedAngle(((_Normal == Vector2.zero) ? _LastValidNormal : _Normal) / _NormalDivisor, Vector2.up), _AngleTransition));

        */

        Velocity = Body.linearVelocity;


        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            Velocity -= GravityDown.normalized * _SlowFall;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            Velocity += GravityDown.normalized * _SlowFall;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            Moving = true;
            _MovingLeft = true;

        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            Moving = true;
            _MovingRight = true;
        }


        if (_MovingRight ^ _MovingLeft)
        {
            
            if (_MovingLeft)
            {
                Velocity = MoveLeft(runGame, canMove);
            }
            else
            {
                Velocity = MoveRight(runGame, canMove);
            }
        }
        else
        {
            _MovingLeft = false;
            _MovingRight = false;

        }

        // Ground Drag when not trying to move, to eliminate unwanted horizontal velocity

        if (isCyoteGrounded && !IsFlingered)
        {
            if (Moving == false)
            {
                if (Mathf.Abs(Velocity.sqrMagnitude - GroundBodyVelocity.sqrMagnitude) > 1f)
                {
                    Vector3 Relativeright = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * Vector2.right);
                    Relativeright.y *= -1;
                  //  Debug.Log(Relativeright);

                    Velocity = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * Velocity);
                    Velocity.x = Mathf.Lerp(Velocity.x, Vector2.Dot(GroundBodyVelocity, Relativeright), 0.5f * GroundDrag / Mathf.Sqrt(Mathf.Abs(Velocity.x)));
                    Velocity = (Quaternion.AngleAxis(LookRotation, new Vector3(0, 0, 1)) * Velocity);
                }
                else
                {
                    Velocity = GroundBodyVelocity;
                    Body.linearVelocity = GroundBodyVelocity;
                }

            }
        }

        //jump

        if (StickerInfo._Stickered)
        {
            StickerInfo._WasStickered = true;
        }

        _jumpTimer += Time.deltaTime;

        if (isCyoteGrounded)
        {
            //Debug.Log("Caught");

            if (Input.GetKey(KeyCode.Space) && _JumpCoolDown <= _jumpTimer )
            {
                if (StickerInfo._Stickered == false || _TriedJump)
                {
                    _jumpTimer = 0;
                    CyoteTime.Time = CyoteTime.EndTime + 1;
                    Velocity = Jump(Velocity, GravityDown, JumpHeight);

                    if (StickerInfo._Stickered == true)
                    {
                        StickerInfo.StickerCooldown.Time = 0;
                        StickerInfo._Stickered = false;
                    }
                
                    _TriedJump = false;
                    Body.totalForce = Vector2.zero;
                    StickerInfo._WasStickered = false;
                }
            }

            _TriedJump = false;
        }
        else
        {
            StickerInfo._WasStickered = false;
        }
          

    }

    void ApplyVelocity()
    {
        // applying velocity changes from jumping and camera-relative horizontal movement

        Body.linearVelocity = Velocity;

        //Execution of left click launch functionality propels the player in the direction of the mouse
        //Launch Bool is set in Update Because it is not a held input key and fixed update tends to eat inputs that are instantaneous

        if (Launch == true)
        {
            RemaingLaunches--;
            isCyoteGrounded = false;
            IsGrounded = false;
            CyoteTime.Time = CyoteTime.EndTime + 10;
            Launch = false;

            Vector3 forceInDirection = Vector3.Dot(Body.linearVelocity, _RelativeDirection.normalized) * _RelativeDirection.normalized;

            forceInDirection.x *= (_RelativeDirection.x * forceInDirection.x < 0) ? 1 : 0;
            forceInDirection.y *= (_RelativeDirection.y * forceInDirection.y < 0) ? 1 : 0;


            Body.linearVelocity -= (Vector2)forceInDirection;

            Body.AddForce(_RelativeDirection.normalized * LaunchForce * Body.mass);
        }

        // this is to stop perpetual and infinitessimal sliding 

        if (Body.linearVelocity.magnitude <= 0.1f)
        {
            Body.linearVelocity = Vector3.zero;
        }

        VelocityMagnitude = Body.linearVelocity.magnitude;
        IsFlingered = false;

        // interpolating the camera position

        float cameraDistance = Vector3.Distance(MainCamera.transform.position, _MyTransform.position);

        CameraFollowRateDistanceFactor = CameraFollowForce * Mathf.InverseLerp(50, _SafetyWindow, cameraDistance) + 0.05f;

        CameraFollowRateDistanceFactor *= CameraFollowForce;

        MainCamera.transform.position = new Vector3(Mathf.Lerp(MainCamera.transform.position.x, _MyTransform.position.x, CameraFollowRateDistanceFactor), Mathf.Lerp(MainCamera.transform.position.y, _MyTransform.position.y, CameraFollowRateDistanceFactor), (MainCamera.transform.position.z));

        if (Body.linearVelocity.magnitude < 0.1f)
        {
            Body.linearVelocity = Vector2.zero;
            Velocity = Vector2.zero;
            if (Vector3.Distance(_LastPos, _MyTransform.position) < 0.05f)
            {
               _MyTransform.position = _LastPos;
            }

        }

        _LastPos = _MyTransform.position;
    }
    void Update()
    {
        if (MovementParticles != null)
        {
            var main = MovementParticles.main;
            var emmission = MovementParticles.emission;
            main.emitterVelocity = (Body.linearVelocity - (Vector2)GroundBodyVelocity) * 2;

            if (IsGrounded)
            {
                emmission.rateOverDistance = 0.5f;

                SoftBody.Movementbobbing += ((Body.linearVelocity - (Vector2)GroundBodyVelocity).magnitude) * (SoftBody.BobbingRate / 1000); ;
                SoftBody.Movementbobbing %= 2 * Mathf.PI;
            }
            else
            {
                emmission.rateOverDistance = 0.1f;

                SoftBody.Movementbobbing = Mathf.Lerp(SoftBody.Movementbobbing, 0, Time.deltaTime);
            }

        }

        // Logic.RenderRayToLineRenderer(RenderRays, MyGamelogicScript.MyLineRenderer);

        if (MyLight != null)
        {
            MyLight.transform.eulerAngles = new Vector3(0, 0, -Vector2.SignedAngle(_RelativeDirection, Vector2.up));

            if (MyBaked != null && MyGamelogicScript.CanMove)
            {
                MyBaked.GenerateMesh(false);
            }

        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _TriedJump = true;
        }
      
        if (!MyGamelogicScript.IsGameOver)
        {
            GravityArrows(_GravityInfo.VectorArray);

        }

        // this is in update and not fixed update because fixed update eats inputs that only occur for a single frame
        if (Input.GetMouseButtonDown(0) && RemaingLaunches > 0)
        {
            Launch = true;
        }


    }
    public override Vector3 Jump(Vector3 vel, Vector3 grav, float jumpPower)
    {
        vel = base.Jump(vel, grav, jumpPower);

        //this might get re-added

        if (_MovingLeft ^ _MovingRight)
        {
     //       vel = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * vel);

      //      vel.x += _HorizontalBoostFromJumping * (_MovingRight ? 1 : -1);

     //       vel = (Quaternion.AngleAxis(LookRotation, new Vector3(0, 0, 1)) * vel);
        }

        return vel;
    }

    public void GravityArrows(Vector3[] Gravity)
    {
       
        PlayerRunningGravityMagnitudes = _GravityInfo.RunningGravityMagnitudes;

        float scooch = 0.5f;

        _TargetAngle = new Vector3(-Vector2.SignedAngle(Gravity[0], Vector2.up), -Vector2.SignedAngle(Gravity[1], Vector2.up), -Vector2.SignedAngle(Gravity[2], Vector2.up));

        float LerpRate = 0.15f;
        _LerpedAngle.x = Mathf.LerpAngle(_LerpedAngle.x, _TargetAngle.x, LerpRate);
        _LerpedAngle.y = Mathf.LerpAngle(_LerpedAngle.y, _TargetAngle.y, LerpRate);
        _LerpedAngle.z = Mathf.LerpAngle(_LerpedAngle.z, _TargetAngle.z, LerpRate);

        ArrowObjects.Arrow1.transform.eulerAngles = new Vector3(0, 0, _LerpedAngle.x);
        ArrowObjects.Arrow2.transform.eulerAngles = new Vector3(0, 0, _LerpedAngle.y);
        ArrowObjects.Arrow3.transform.eulerAngles = new Vector3(0, 0, _LerpedAngle.z);

        ArrowObjects.Arrow1.transform.localScale = new Vector3(ArrowLength(Gravity[0]), ArrowLength(Gravity[0]), ArrowObjects.Arrow1.transform.localScale.z);
        ArrowObjects.Arrow1.transform.position = _MyTransform.position + Quaternion.AngleAxis(_LerpedAngle.x, new Vector3(0, 0, 1)) * new Vector3(0, 1, 0) * (ArrowLength(Gravity[0]) / 2 + scooch);

        ArrowObjects.Arrow2.transform.localScale = new Vector3(ArrowLength(Gravity[1]), ArrowLength(Gravity[1]), ArrowObjects.Arrow1.transform.localScale.z);
        ArrowObjects.Arrow2.transform.position = _MyTransform.position + Quaternion.AngleAxis(_LerpedAngle.y, new Vector3(0, 0, 1)) * new Vector3(0, 1, 0) * (ArrowLength(Gravity[1]) / 2 + scooch);

        ArrowObjects.Arrow3.transform.localScale = new Vector3(ArrowLength(Gravity[2]), ArrowLength(Gravity[2]), ArrowObjects.Arrow1.transform.localScale.z);
        ArrowObjects.Arrow3.transform.position = _MyTransform.position + Quaternion.AngleAxis(_LerpedAngle.z, new Vector3(0, 0, 1)) * new Vector3(0, 1, 0) * (ArrowLength(Gravity[2]) / 2 + scooch);



        float maxStickerAlpha = 0.5f;
        float StickerAlpha = (StickerInfo._Stickered) ? maxStickerAlpha : (StickerInfo.StickerCooldown.GetRatio() < 1) ? (1 - StickerInfo.StickerCooldown.GetRatio()) * maxStickerAlpha : 0;

        ArrowObjects.Arrow1SpriteRenderer.color = new Color(ArrowObjects.Arrow1SpriteRenderer.color.r, ArrowObjects.Arrow1SpriteRenderer.color.g, ArrowObjects.Arrow1SpriteRenderer.color.b, Mathf.Max(ArrowAlpha(PlayerRunningGravityMagnitudes, Gravity[0].magnitude, 0.5f, 1.37f), StickerAlpha));
        ArrowObjects.Arrow2SpriteRenderer.color = new Color(ArrowObjects.Arrow2SpriteRenderer.color.r, ArrowObjects.Arrow2SpriteRenderer.color.g , ArrowObjects.Arrow2SpriteRenderer.color.b , ArrowAlpha(Gravity[1].magnitude, PlayerRunningGravityMagnitudes, 0.5f, 20));
        ArrowObjects.Arrow3SpriteRenderer.color = new Color(ArrowObjects.Arrow3SpriteRenderer.color.r, ArrowObjects.Arrow3SpriteRenderer.color.g, ArrowObjects.Arrow3SpriteRenderer.color.b, ArrowAlpha(Gravity[2].magnitude, PlayerRunningGravityMagnitudes, 0.5f, 35));

        ArrowObjects.Arrow1SpriteRenderer.enabled = Gravity[0].magnitude < PlayerRunningGravityMagnitudes * 1.37f || (StickerInfo.StickerCooldown.GetRatio() < 1) || StickerInfo._Stickered;
        ArrowObjects.Arrow2SpriteRenderer.enabled = Gravity[1].magnitude > PlayerRunningGravityMagnitudes / 20;
        ArrowObjects.Arrow3SpriteRenderer.enabled = Gravity[2].magnitude > PlayerRunningGravityMagnitudes / 35;

    }
    float ArrowAlpha(float ArrowMagnitude, float gravMagnitude, float maxAlpha, float ratio)
    {
        return Mathf.Lerp(maxAlpha, 0, Mathf.Pow(Mathf.InverseLerp(gravMagnitude, gravMagnitude / ratio, ArrowMagnitude),10f));
    }
    float ArrowLength(Vector3 ArrowVector)
    {
        return Mathf.Pow(ArrowVector.magnitude * 1, (1f/3f));
    }

    private void OnDrawGizmos()
    {
        Logic.RenderRayRenderer(RenderRays, Color.blue);
        Gizmos.color = Color.blue;

        foreach (Vector2 pos in _IntersectList)
        {

            Gizmos.DrawSphere(pos, 0.05f);
        }
        _IntersectList.Clear();

    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public IEnumerator CallRestart(float delay)
    {
        bool activate = false;
        
        while (!activate)
        {
            activate = true;
            yield return new WaitForSeconds(delay);
        }

        MyGamelogicScript.Restart();

    }
    public IEnumerator DeathAnimation()
    {
        if (GravityDownMagnitude < 2000)
        {
            float duration = 0.25f;
            float timer = 0;
            Vector3 initialScale = SoftBody.transform.localScale;
            Vector3 scaledVector = initialScale * 2;
            int Particles = 1000;
            float particleSpeed = 40;
            DeathSound.Play();

            while (timer < duration)
            {
                SoftBody.transform.localScale = Logic.LerpVector(initialScale, scaledVector, timer / duration);
                timer += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }

            GameObject particleobj = Instantiate(SoftBody.SquishParticle, transform.position + transform.TransformDirection(Vector3.up), Quaternion.identity);
            ParticleSystem particle = particleobj.GetComponent<ParticleSystem>();
            AudioSource audio = SoftBody.Squeltch;
            audio.volume = 2;
            audio.Play();

            if (GravityDownMagnitude < 2000)
            {
                var main = particle.main;
                var emission = particle.emission;
                var collision = particle.collision;
                emission.rateOverTime = Particles;
                main.duration = 0.35f;
                main.maxParticles = 5000;
                main.startLifetime = new ParticleSystem.MinMaxCurve(5, 10);
                main.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeed * 0.75f, particleSpeed * 2);
                collision.maxKillSpeed = 0;
                particle.Play();
            }
           
        }
        else
        {
            DeathEvent2.Play(DeathSound);
        }

        DisableRendering();

        StartCoroutine(CallRestart(2));
    }
    void DisableRendering()
    {
        ArrowObjects.Arrow1SpriteRenderer.enabled = false;
        ArrowObjects.Arrow2SpriteRenderer.enabled = false;
        ArrowObjects.Arrow3SpriteRenderer.enabled = false;
        SoftBody.gameObject.GetComponent<MeshRenderer>().enabled = false;

    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        Sticker.IStickerable.StickerCollision(collision, ref StickerInfo, _MovingLeft, _MovingRight, transform, GroundLayer, Body);
    }
    public void PredictMotion()
    {
        
        prediction.PredictRoutine = StartCoroutine(prediction.PredictionRays());

        RenderRays = prediction.Rays;
    }

}

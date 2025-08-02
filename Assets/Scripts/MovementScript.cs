
using UnityEngine;
public class MovementScript : MonoBehaviour
{
    [Min(1)] public float Mult = 1;
    public float GroundCheckDistance = 0.1f; // Distance to cast the ray
    public Vector3 GroundBodyVelocity;
    public float GravityDownMagnitude;
    public LayerMask GroundLayer; // Assign the ground layer in the Inspector
    public GameLogicScript MyGamelogicScript;
    public Vector3 GravityDown;
    public float MaxVelocity = 20;
    public bool IsGrounded;
    public float JumpHeight = 10;
    public float LookRotation;
    public float InAirAccelReduction = 2;
    public float InAirMaxSpeedRatio= 2;

    public bool IsGameRunning;
    public Vector2 PrePauseVelocity;
    public bool IsFlingered;
    public Vector3 Velocity;
    public float GroundDrag = 1f;
    public float Acceleration = 0.1f;
    public Rigidbody2D Body;

    [System.Serializable]

    public class AlternatingTranslation
    {
        private bool _FirstTarget = true;
        [Range(1f, 10f)] public float Easing;
        public float Speed;
        public MoveDirection direction;
        public EdgeBehavior edgeBehavior;
        public Logic.Timer WaitAtStart;
        public Logic.Timer WaitAtTarget;
        public Logic.Timer TranslationTimer;
        public Path Mypath;
        public Vector3[] Targets = new Vector3[0];
        private Vector3 LastTarget;
        public int TargetIndex = 0;
        public float DistToNextTarget;

        public enum MoveDirection
        {
            Backward = -1,
            Forward = 1,

        }
        public enum EdgeBehavior
        {
            Cycle = 0,
            Flip = 1,

        }

        void SelectTarget()
        {
            if (Mypath != null && Mypath.Updated)
            {
                Mypath.Updated = false;
                Targets = Mypath.PathWorld.ToArray();
            }

            if (edgeBehavior == EdgeBehavior.Cycle)
            {
                if (TargetIndex == Targets.Length - 1)
                {
                    LastTarget = Targets[TargetIndex];

                    TargetIndex = 0;

                    DistToNextTarget = Vector3.Distance(LastTarget, Targets[TargetIndex]);


                }
                else
                {
                    LastTarget = Targets[TargetIndex];

                    TargetIndex++;

                    DistToNextTarget = Vector3.Distance(LastTarget, Targets[TargetIndex]);

                }
            }

            if (edgeBehavior == EdgeBehavior.Flip)
            {
                if (direction == MoveDirection.Forward)
                {
                    if (TargetIndex == Targets.Length - 1)
                    {
                        LastTarget = Targets[TargetIndex];

                        direction = MoveDirection.Backward;

                        TargetIndex += (int)direction;

                        DistToNextTarget = Vector3.Distance(LastTarget, Targets[TargetIndex]);
                    }
                    else
                    {
                        LastTarget = Targets[TargetIndex];

                        TargetIndex += (int)direction;

                        DistToNextTarget = Vector3.Distance(LastTarget, Targets[TargetIndex]);
                    }
                }
                else
                {
                    if (TargetIndex == 0)
                    {
                        direction = MoveDirection.Forward;

                        LastTarget = Targets[TargetIndex];

                        TargetIndex += (int)direction;

                        DistToNextTarget = Vector3.Distance(LastTarget, Targets[TargetIndex]);
                    }
                    else
                    {
                        LastTarget = Targets[TargetIndex];

                        TargetIndex += (int)direction;

                        DistToNextTarget = Vector3.Distance(LastTarget, Targets[TargetIndex]);
                    }
                }
            }

            if (!_FirstTarget)
            {
                TranslationTimer.Time = 0;
                WaitAtTarget.Time = 0;
            }
            else
            {
                _FirstTarget = false;
            }

        }

        public void StepTowardsNextTarget(Transform transform)
        {
            if (Targets.Length < 1)
            {
                return;
            }

            if (TranslationTimer.Ratio >= 1 || _FirstTarget)
            {
                if (WaitAtTarget.Ratio >= 1 || WaitAtTarget.EndTime <= 0 || _FirstTarget)
                {
                    SelectTarget();
                }
                else
                {
                    WaitAtTarget.Step();
                }

            }
            else
            {

                if (LastTarget == null)
                {
                    SelectTarget();

                }
                TranslationTimer.Step((Time.deltaTime / DistToNextTarget) * Speed);


                float eased = Logic.EaseInOut(TranslationTimer.Ratio, Easing);
                // eased = Logic.EaseInOutQuart(TranslationTimer.Ratio);
                transform.position = Logic.LerpVector(LastTarget, Targets[TargetIndex], eased);
            }
        }
    }

    private void OnEnable()
    {
        if (MyGamelogicScript == null)
        {
            MyGamelogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();
        }
    }

    static public float TargetAngleBasedOnGravityDirection(Vector3 gravity)
    {
        //return (gravity.x > 0) ? Vector2.Angle(-gravity, Vector2.up) : Mathf.Lerp(180, 360, Mathf.InverseLerp(-180, 0, -Vector2.Angle(-gravity, Vector2.up)));
        return -Vector2.SignedAngle(gravity, Vector2.down);
    }

    public bool CheckGrounded()
    {
        Transform cachedtransform = transform;

        Vector3 distanceToBottom = (cachedtransform.TransformDirection(Vector2.down) * -(cachedtransform.lossyScale.y / 2 + 0.02f));

        RaycastHit2D hit1 = Physics2D.Raycast(cachedtransform.position - distanceToBottom + Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * GravityDown.normalized * -0.2f, GravityDown, GroundCheckDistance, GroundLayer);

        RaycastHit2D hit2 = Physics2D.Raycast(cachedtransform.position - distanceToBottom + Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * GravityDown.normalized * 0.2f, GravityDown, GroundCheckDistance, GroundLayer);

        return (hit1.collider != null || hit2.collider != null);
    }
    public bool CheckGrounded(Vector3 gravity, float distance, LayerMask layer, ref bool stickered)
    {
        int count = 0;
        Transform cachedtransform = transform;

        Vector3 distanceToBottom = (cachedtransform.TransformDirection(Vector2.down) * -(cachedtransform.lossyScale.y / 2 + 0.02f));

        RaycastHit2D hit1 = Physics2D.Raycast(cachedtransform.position - distanceToBottom + Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * gravity.normalized * -0.2f, gravity, distance, layer);

        RaycastHit2D hit2 = Physics2D.Raycast(cachedtransform.position - distanceToBottom + Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * gravity.normalized * 0.2f, gravity, distance, layer);

        bool ReturnValue = (hit1.collider != null || hit2.collider != null);

        Vector3 runningbodyvel = Vector3.zero;


        if (hit1)
        {
            count++;

            if (hit1.collider.gameObject.GetRootParent().TryGetComponent<MovingPlatform>(out MovingPlatform mPlatform))
            {
                //hit1.collider.gameObject.GetRootParent();
                Vector3 TangentialVelocityAtPoint = Vector3.zero;

                if (mPlatform.ActiveAndSpinning())
                {
                    float RadiusAtPoint = Mathf.Abs(Vector2.Distance(hit1.point, mPlatform.transform.position));
                 
                    Vector3 DirectionFromCenter = hit1.point - (Vector2)mPlatform.transform.position;
                    Vector2 TangentDirection = Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * DirectionFromCenter;
                    TangentialVelocityAtPoint = (TangentDirection.normalized * (Time.deltaTime * (RadiusAtPoint + 0.5f) * 2 * Mathf.PI * 50)) / mPlatform.RevolutionTimer.EndTime;
                    Debug.DrawLine(mPlatform.transform.position, hit1.point, Color.green);
                }
                

                runningbodyvel += TangentialVelocityAtPoint;
                runningbodyvel += mPlatform.Vel;
            }

            if (hit1.collider.gameObject.CompareTag("Sticker"))
            {
                stickered = true;
            }

        }

        if (hit2)
        {

            count++;

            // Debug.Log(hit2.collider.gameObject.GetRootParent().name, hit2.collider.gameObject.GetRootParent());

            if (hit2.collider.gameObject.GetRootParent().TryGetComponent<MovingPlatform>(out MovingPlatform mPlatform))
            {
               // Debug.Log(hit2.collider.gameObject.GetRootParent().name, hit2.collider.gameObject.GetRootParent());

                Vector3 TangentialVelocityAtPoint = Vector3.zero;

                if (mPlatform.ActiveAndSpinning())
                {
                  
                    float RadiusAtPoint = Mathf.Abs(Vector2.Distance(hit2.point, mPlatform.transform.position));
       
                    Vector3 DirectionFromCenter = hit2.point - (Vector2)mPlatform.transform.position;
                    Vector2 TangentDirection = Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * DirectionFromCenter;


                   
                    TangentialVelocityAtPoint = (TangentDirection.normalized * Time.deltaTime * (RadiusAtPoint + 0.5f) * 2 * Mathf.PI * 50) / mPlatform.RevolutionTimer.EndTime;

                   // TangentialVelocityAtPoint = TangentDirection.normalized * ArcLengthOverTimeStep;

                }



                runningbodyvel += TangentialVelocityAtPoint;
                runningbodyvel += mPlatform.Vel;

            }

            if (hit2.collider.gameObject.CompareTag("Sticker"))
            {
                stickered = true;
            }
        }

      
            GroundBodyVelocity = runningbodyvel / 2;


     //   Debug.Log(count);

        return ReturnValue;


    }

    public bool CheckGrounded(Vector3 gravity,float distance, LayerMask layer)
    {
        Transform cachedtransform = transform;

        Vector3 distanceToBottom = (cachedtransform.TransformDirection(Vector2.down) * -(cachedtransform.lossyScale.y / 2 + 0.02f));

        RaycastHit2D hit1 = Physics2D.Raycast(cachedtransform.position - distanceToBottom + Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * gravity.normalized * -0.2f, gravity, distance, layer);

        RaycastHit2D hit2 = Physics2D.Raycast(cachedtransform.position - distanceToBottom + Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * gravity.normalized * 0.2f, gravity, distance, layer);

        return (hit1.collider != null || hit2.collider != null);

        
    }

    public virtual Vector3 Jump(Vector3 vel, Vector3 grav, float jumpPower)
    {
        Vector3 RelativeUp = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * Vector2.up);

        RelativeUp.y *= -1;

        float leftVelocityFromFloor = Vector2.Dot(GroundBodyVelocity, RelativeUp);

        jumpPower -= leftVelocityFromFloor;


        Vector3 forceInDirection = Vector3.Dot(vel, grav.normalized) * new Vector2(grav.normalized.x, grav.normalized.y);

        vel -= forceInDirection;

        vel += grav.normalized * -jumpPower;

        return vel;
    }

    public Vector3 MoveLeft(bool runGame, bool canMove)
    {
        if (runGame)
        {
            Vector3 RelativeLeft = (Quaternion.AngleAxis(LookRotation, new Vector3(0, 0, 1)) * Vector2.left);

            float leftVelocityFromFloor = Vector2.Dot(GroundBodyVelocity, RelativeLeft);

            float AdditiveMaxVelocity = MaxVelocity + leftVelocityFromFloor;

           // Debug.Log(AdditiveMaxVelocity);

            if (canMove)
            {

                float inAirInterpolationRate = IsGrounded ? 1 : 1f / InAirAccelReduction;

                Velocity = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * Velocity);

                Vector3 tempBodyVel = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * Body.linearVelocity);

             //   float directionmodifier = ((tempBodyVel.x < 0) ? 1 : 2);


                float XVelocity = Mathf.Lerp(tempBodyVel.x, -(IsGrounded ? AdditiveMaxVelocity : MaxVelocity * InAirMaxSpeedRatio),  Acceleration * inAirInterpolationRate);

                Velocity.x = XVelocity;

                if (Velocity.x > tempBodyVel.x)
                {
                    Velocity.x = tempBodyVel.x;
                }

            }

            if (Velocity.x < -AdditiveMaxVelocity && IsGrounded)
            {
                Velocity.x = Mathf.Lerp(Velocity.x, -AdditiveMaxVelocity, 0.1f * GroundDrag);
            }

            return (Quaternion.AngleAxis(LookRotation, new Vector3(0, 0, 1)) * Velocity);
        }

        return Vector3.zero;
    }

    public Vector3 MoveRight(bool runGame, bool canMove)
    {
        if (runGame)
        {
            Vector3 RelativeRight = (Quaternion.AngleAxis(LookRotation, new Vector3(0, 0, 1)) * Vector2.right);

            float RightVelocityFromFloor =  Vector2.Dot(GroundBodyVelocity, RelativeRight);

            float AdditiveMaxVelocity = MaxVelocity + RightVelocityFromFloor;

         //   Debug.Log(AdditiveMaxVelocity);


            if (canMove)
            {
                float inAirInterpolationRate = IsGrounded ? 1 : 1f / InAirAccelReduction;

                Velocity = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * Velocity);

                Vector3 tempBodyVel = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * Body.linearVelocity);

              //  float directionmodifier = ((tempBodyVel.x > 0 && !IsGrounded) ? 1 : 2);

         

                float XVelocity = Mathf.Lerp(tempBodyVel.x, (IsGrounded ? AdditiveMaxVelocity : MaxVelocity * InAirMaxSpeedRatio), Acceleration * inAirInterpolationRate);

                Velocity.x = XVelocity;


                if (Velocity.x <= tempBodyVel.x)
                {
                    Velocity.x = tempBodyVel.x;
                }

            }

            if (Velocity.x > AdditiveMaxVelocity && IsGrounded)
            {
                Velocity.x = Mathf.Lerp(Velocity.x, AdditiveMaxVelocity, 0.1f * GroundDrag);
            }

            return (Quaternion.AngleAxis(LookRotation, new Vector3(0, 0, 1)) * Velocity);
        }

        return Vector3.zero;

    }


    public Vector3 MoveLeft(bool runGame, bool canMove, Vector2 rigidBodyVel)
    {
        if (runGame)
        {
            if (canMove)
            {
                float inAirInterpolationRate = IsGrounded ? 1 : 0.5f / InAirAccelReduction;

                Velocity = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * Velocity);

                Vector3 tempBodyVel = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * rigidBodyVel);

                float XVelocity = Mathf.Lerp(tempBodyVel.x, -(IsGrounded ? MaxVelocity : MaxVelocity * InAirMaxSpeedRatio) * ((tempBodyVel.x < 0) ? 1 : 2), Acceleration * inAirInterpolationRate);

                Velocity.x = XVelocity;

                if (Velocity.x > tempBodyVel.x)
                {
                    Velocity.x = tempBodyVel.x;
                }

            }
            if (Velocity.x < -MaxVelocity && IsGrounded)
            {
                Velocity.x = Mathf.Lerp(Velocity.x, -MaxVelocity, 0.1f * GroundDrag);
            }

            return (Quaternion.AngleAxis(LookRotation, new Vector3(0, 0, 1)) * Velocity);
        }

        return Vector3.zero;
    }

    public Vector3 MoveRight(bool runGame, bool canMove, Vector2 rigidBodyVel)
    {
        if (runGame)
        {
            if (canMove)
            {
                float inAirInterpolationRate = IsGrounded ? 1 : 0.5f / InAirAccelReduction;

                Velocity = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * Velocity);

                Vector3 tempBodyVel = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * rigidBodyVel);


                float XVelocity = Mathf.Lerp(tempBodyVel.x, (IsGrounded ? MaxVelocity : MaxVelocity * InAirMaxSpeedRatio) * ((tempBodyVel.x > 0) ? 1 : 2), Acceleration * inAirInterpolationRate);

                Velocity.x = XVelocity;


                if (Velocity.x <= tempBodyVel.x)
                {
                    Velocity.x = tempBodyVel.x;
                }

            }

            if (Velocity.x > MaxVelocity && IsGrounded)
            {
                Velocity.x = Mathf.Lerp(Velocity.x, MaxVelocity, 0.1f * GroundDrag);
            }

            return (Quaternion.AngleAxis(LookRotation, new Vector3(0, 0, 1)) * Velocity);
        }

        return Vector3.zero;

    }

    public Vector3 MoveLeft(Vector3 vel,float targetSpeed,float angle,bool isGrounded,Vector3 rigidBodyVel,float airDrag,float groundDrag,float deafultInterpolationRate, bool canMove, bool runGame)
    {
        if (runGame)
        {
            if (canMove)
            {
                float inAirInterpolationRate = isGrounded ? 1 : 0.5f / InAirAccelReduction;

                vel = (Quaternion.AngleAxis(-angle, new Vector3(0, 0, 1)) * vel);

                Vector3 tempBodyVel = (Quaternion.AngleAxis(-angle, new Vector3(0, 0, 1)) * rigidBodyVel);

                float XVelocity = Mathf.Lerp(tempBodyVel.x, -(isGrounded ? targetSpeed : targetSpeed * InAirMaxSpeedRatio) * ((tempBodyVel.x < 0) ? 1 : 2), deafultInterpolationRate * inAirInterpolationRate);

                vel.x = XVelocity;

                if (vel.x > tempBodyVel.x)
                {
                    vel.x = tempBodyVel.x;
                }

            }
            if (vel.x < -targetSpeed && isGrounded)
            {
                vel.x = Mathf.Lerp(vel.x, -targetSpeed, 0.1f * groundDrag);
            }

            return (Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * vel);
        }

        return Vector3.zero;
    }


    public Vector3 MoveRight(Vector3 vel, float targetSpeed, float angle, bool isGrounded, Vector3 rigidBodyVel, float airDrag, float groundDrag, float deafultInterpolationRate, bool canMove, bool runGame)
    {
        if (runGame)
        {
            if (canMove)
            {
                float inAirInterpolationRate = isGrounded ? 1 : 0.5f / airDrag;

                vel = (Quaternion.AngleAxis(-angle, new Vector3(0, 0, 1)) * vel);

                Vector3 tempBodyVel = (Quaternion.AngleAxis(-angle, new Vector3(0, 0, 1)) * rigidBodyVel);


                float XVelocity = Mathf.Lerp(tempBodyVel.x, (isGrounded ? targetSpeed : targetSpeed * InAirMaxSpeedRatio) * ((tempBodyVel.x > 0) ? 1 : 2), deafultInterpolationRate * inAirInterpolationRate);

                vel.x = XVelocity;


                if (vel.x <= tempBodyVel.x)
                {
                    vel.x = tempBodyVel.x;
                }

            }

            if (vel.x > targetSpeed && isGrounded)
            {
                vel.x = Mathf.Lerp(vel.x, targetSpeed, 0.1f * groundDrag);
            }

            return (Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * vel);
        }

        return Vector3.zero;

    }

}

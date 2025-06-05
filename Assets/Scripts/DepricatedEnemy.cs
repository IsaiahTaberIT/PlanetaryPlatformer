using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DepricatedEnemy : MovementScript, Hurtful.IHurtful
{
    [SerializeField] private bool _SafeToJump;

    [Range(0f,1f)] [SerializeField] private float _Acceleration = 0.1f;
   // [SerializeField] private float;

    [SerializeField] private float _TargetVelocity;
    [SerializeField] private float _GroundCheckDistance;
    [SerializeField] private Rigidbody2D _Body;
    public MovementState AiMovementState;
    public Phase AiPhase;
    [SerializeField] private TargetType myTarget;

    private Coroutine Ai;
    private Coroutine PlayerChecker;
    public GameObject Player;
    public PlayerScript MyPlayerScript;
    public Transform PlayerTransform;

    public bool Reactivate;
    private float _HitDistance;
    public Collider2D WhatIHit;
    [SerializeField] private float _WaitBeforeTryingAgain;

    [SerializeField] private bool _PredictionHitEnd;
    [SerializeField] private float _HitRayAngle;
    [SerializeField] private float _RayAngle;
    [SerializeField] private float _InterestCooldown;
    [SerializeField] private float _InterestTime;
    [SerializeField] private bool _PlayerisAbove;
    [SerializeField] private bool _TryJump;
    [SerializeField] private float _Suspicion;
    [SerializeField] private float _SuspicionThreshold;
    [SerializeField] private LayerMask _LayersToIgnore;
    [SerializeField] private Target TargetPlayer;
    [SerializeField] private Target CurrentTarget;
    [SerializeField] private bool _ChaseMouse;
    [SerializeField] private Vector2 MousePos;
    [SerializeField] private float _SeekDuration;
    [SerializeField] private float FloorCheckDistance;
    [SerializeField] private float FloorCheckDistanceFar;
    [SerializeField] private float DecisionTime;
    [SerializeField] private float DecisionTimer;
    [SerializeField] private float FlingerStunTime;
    [SerializeField] private float FlingerStunTimer;
    [SerializeField] private bool  _CheckedLastPosition = true;
    [SerializeField] private Vector3 LastSafePosition;
    [SerializeField] private Vector3 LastKnownPositionOfPlayer;
    [SerializeField] private float _SearchPrecision = 5;
    [SerializeField] private List<Logic.RayRenderer> RenderRays = new List<Logic.RayRenderer>(0);
    [SerializeField] private List<Logic.RayRenderer> JumpPredictionRenderRays = new List<Logic.RayRenderer>(0);

    [SerializeField] private Target[] TargetArray = new Target[10];
    [SerializeField] private Vector3 TargetPosition;
    [SerializeField] private int TargetID;
    [SerializeField] private int TargetPriority;
    [SerializeField] private int CheckFrequency;
    [SerializeField] private float initialInvertedGravityPower = 13;
    [SerializeField] private float invertedGravityPower = 13;
    [SerializeField] private float initialSteps = 4;
    [SerializeField] private int Steps = 4;
    [SerializeField] private float Divisor = 2;
    [SerializeField] private List<RaycastHit2D> Points = new List<RaycastHit2D>(0);
    [SerializeField] private float MaxDecisionTIme = 2;
    [SerializeField] private int RayCount;

    public struct Target
    {
        public float TargetDistance;
        public Vector3 TargetPosition;
        public float AngleToTarget;
        public int TargetID;
        public int Priority;
        public float SeekTime ;



        public static Target empty
        {
            get
            {
                Target tempTarget = new Target();
                tempTarget.Priority = -5000000;
                return tempTarget;
            }
         
        }

        public static Target PlayerWithModdedPriority(Target target, int priority)
        {

            return new Target(target.TargetPosition, target.AngleToTarget, 0, priority, 0);
        }




        public static Target NonPlayerTarget(Vector3 pos, int priority, float seekTime)
        {
            return new Target(pos,0,1, priority,seekTime);
        }
            

        public Target(Vector3 pos, float angle,int id,int priority,float seekTime)
        {
            SeekTime = seekTime;
            Priority = priority;
            TargetDistance = 0;
            TargetPosition = pos;
            AngleToTarget = angle;
            TargetID = id;
        }
    }




    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public enum Phase
    {
        Wander = 1,
        Chase = 2,
        HeadToPoint = 3
    }


    public enum TargetType
    {
        Player = 0,
        Flinger = 1,
        LastHitPlayer = 2,
        LastSafePoint = 3,
        JumpPoint = 4
    }

    public enum MovementState
    {
        limp = 0,
        Left = 1,
        Right = 2,
        Stop = 3,
   
    }

    void PredictJump()
    {
        _PredictionHitEnd = false;
        JumpPredictionRenderRays.Clear();
         Points.Clear();
        if (MyGamelogicScript != null)
        {
            GravityDown = GameLogicScript.GravityDirection(transform.position, MyGamelogicScript.NormalGravity * Vector3.down).Gravity;

        }
        else
        {
            MyGamelogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();
            GravityDown = GameLogicScript.GravityDirection(transform.position, MyGamelogicScript.NormalGravity * Vector3.down).Gravity;
        }

       
        Vector3 point = Vector3.zero;
        Quaternion rotation = Quaternion.identity;


   

        Vector3 velocity = Vector3.zero;
        
        LookRotation = TargetAngleBasedOnGravityDirection(GravityDown);



        float rotVel = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * _Body.linearVelocity).x * 0.95f;


        velocity = (transform.TransformDirection(new Vector3(rotVel, 0, 0)) + (transform.TransformDirection(Vector3.up) * JumpHeight * 0.9f)) / Divisor;


        invertedGravityPower = initialInvertedGravityPower * Divisor * Divisor;
        Steps = Mathf.FloorToInt(initialSteps * Divisor);
        point = transform.position + transform.TransformDirection(Vector3.up);
        int lastpointindex = 0;
        RaycastHit2D potentialPoint = new RaycastHit2D();
        bool assigned = false;

        for (int i = 0; i < Steps; i++)
        {
            RaycastHit2D RaysegmentHit = Physics2D.Raycast(point, velocity, velocity.magnitude, ~6);

            float distance = velocity.magnitude;

            if (RaysegmentHit == true)
            {
                distance = RaysegmentHit.distance;
            }

            //  Gizmos.color = Color.red;
            // Debug.Log(RenderRays.Count);
            JumpPredictionRenderRays.Add(new Logic.RayRenderer(new Ray(point, velocity.normalized), distance, Color.red));
            // Debug.Log(RenderRays.Count);
            // Gizmos.DrawRay(point, velocity.normalized * distance);


            float pointRot = TargetAngleBasedOnGravityDirection(GameLogicScript.GravityDirection(point, Vector3.down * MyGamelogicScript.NormalGravity).Gravity);



            if (RaysegmentHit == true)
            {
                if (Vector2.Angle(Vector2.up, Quaternion.AngleAxis(-pointRot, new Vector3(0, 0, 1)) * RaysegmentHit.normal) > 89)
                {

                    // Project the current velocity onto the specified direction
                    float dotProduct = Vector2.Dot(velocity, RaysegmentHit.normal * -1);

                    // If the dot product is negative, velocity exists in the *opposite* direction.  Don't do anything
                    // If the dot product is positive, the current velocity has velocity in the correct direction.
                    if (dotProduct > 0)
                    {
                        // Subtract the velocity component in the specified direction
                        velocity -= (Vector3)(RaysegmentHit.normal * dotProduct * -1);
                        //point = RaysegmentHit.point;

                        //point = RaysegmentHit.point;
                    }
                    else
                    {
                        point += velocity;
                    }


                }
                else
                {
                    _PredictionHitEnd = true;
                    break;
                }


                //point = RaysegmentHit.point;
            }
            else
            {
                point += velocity;
            }

            Vector3 gravDeltaV = GameLogicScript.GravityDirection(point, MyGamelogicScript.NormalGravity * Vector3.down).Gravity / invertedGravityPower;

            pointRot = TargetAngleBasedOnGravityDirection(GameLogicScript.GravityDirection(point, MyGamelogicScript.NormalGravity * Vector3.down).Gravity);



            if (i % (CheckFrequency == 0 ? 1 : CheckFrequency) == 0)
            {

                float myYPosition = (Quaternion.AngleAxis(-pointRot, new Vector3(0, 0, 1)) * transform.position).y;

                float pointYPosition = ((Quaternion.AngleAxis(-pointRot, new Vector3(0, 0, 1)) * point).y);

                float differenceOfY = (Mathf.Abs(pointYPosition - myYPosition) + 40) / 2f;

                if (pointYPosition - myYPosition + 10 < 0)
                {
                    // break;
                }
                float platformDistance = differenceOfY;

                RaycastHit2D RayCheckForPlatform = Physics2D.Raycast(point, gravDeltaV, differenceOfY, ~6);

               

                if (RayCheckForPlatform == true)
                {
                    if (Vector2.Distance(RayCheckForPlatform.point, transform.position) > 9)
                    {
                        if (lastpointindex == i - 1 && assigned == true)
                        {
                            if (Vector2.Distance(potentialPoint.point, RayCheckForPlatform.point) < 2)
                            {
                                Points.Add(potentialPoint);
                            }
                        }

                        assigned = true;
                        lastpointindex = i;
                        potentialPoint = RayCheckForPlatform;
                       
                    }
                    platformDistance = RayCheckForPlatform.distance;
                }
                JumpPredictionRenderRays.Add(new Logic.RayRenderer(new Ray(point, gravDeltaV.normalized), platformDistance, Color.green));
            }






            velocity += gravDeltaV;

            
            Vector3 tempVel = Quaternion.AngleAxis(-pointRot, new Vector3(0, 0, 1)) * velocity;

            float xvelocity = Mathf.Lerp(tempVel.x, (MaxVelocity / InAirAccelReduction) / Divisor * Mathf.Sign(velocity.x), 0.2f);

            tempVel.x = xvelocity;

            tempVel = Quaternion.AngleAxis(pointRot, new Vector3(0, 0, 1)) * tempVel;

            if (tempVel.magnitude > velocity.magnitude)
            {

                velocity = tempVel;

            }

        }

    }

    private void OnDrawGizmos()
    {
        if (PlayerTransform != null)
        {
            Logic.RenderRayRenderer(RenderRays);
        }
    }



    private void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        MyPlayerScript = Player.GetComponent<PlayerScript>();
        PlayerTransform = Player.transform;
        _LayersToIgnore = ~_LayersToIgnore;
        _Body = GetComponent<Rigidbody2D>();
        MyGamelogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();

        Ai = StartCoroutine(AiRoutine());
        TargetPlayer = new Target(Player.transform.position, 0,0,100,0);
        
    }

    public IEnumerator AiCheckForPlayer()
    {
        
        while (true)
        {
            CurrentTarget = new Target();
            Vector3 PlayerStartingPosition = transform.position + (TargetPlayer.TargetPosition - transform.position).normalized * (transform.lossyScale.magnitude / 2) + transform.TransformDirection(Vector2.up * 1.24f);

            Vector3 PlayerDirection = ((PlayerStartingPosition + PlayerTransform.TransformDirection(Vector2.up * 0.45f)) - PlayerStartingPosition).normalized;

            RaycastHit2D PlayerOnlyRay = Physics2D.Raycast(PlayerStartingPosition, PlayerDirection, 50, _LayersToIgnore);


            RenderRays.Add(new Logic.RayRenderer(new Ray(PlayerStartingPosition, PlayerDirection), PlayerOnlyRay.distance, Color.yellow));

            if (PlayerOnlyRay.collider != null && PlayerOnlyRay.collider.gameObject == Player)
            {
                AiPhase = Phase.Chase;
                CurrentTarget = TargetPlayer;
               // Debug.Log("Found Player");
            }

            //Debug.Log("Faild To Find Player");

            yield return new WaitForSeconds(0.2f);
        }




    }

    public IEnumerator AiRoutine()
    {
        
        TargetPlayer = new Target(Player.transform.position, 0, 0, 100, 0);
        CurrentTarget = Target.empty;
       // Debug.Log("hi");

        //PlayerChecker = StartCoroutine(AiCheckForPlayer());

        while (true)
        {
            RenderRays.Clear();
            // PredictJump();

            if (MyGamelogicScript.RunGame)
            {
                if (!IsGameRunning)
                {
                    _Body.linearVelocity = PrePauseVelocity;
                    IsGameRunning = true;
                }
            }
            else
            {
                if (IsGameRunning)
                {
                    PrePauseVelocity = _Body.linearVelocity;
                    IsGameRunning = false;
                }

                yield return new WaitForSeconds(0.02f);

                _Body.linearVelocity = Vector2.zero;
            }
         


            RenderRays.AddRange(JumpPredictionRenderRays);

            // remove the current target if you get close enough to it

            if (AiPhase == Phase.HeadToPoint && Vector3.Distance(transform.position, CurrentTarget.TargetPosition) < _SearchPrecision)
            {
                if (CurrentTarget.TargetID != 1)
                {
                    TargetArray[CurrentTarget.TargetID].SeekTime = 0;

                }

            }




            TargetPlayer.TargetPosition = Player.transform.position;

            //if the flinger target is over 1.25x farther away than the player is from the current position then remove the fliner from the target list so the enemy doesnt default to it when the player is lost

            if (Vector2.Distance(TargetArray[(int)TargetType.Flinger].TargetPosition, transform.position) / 1.25f > Vector2.Distance(TargetPlayer.TargetPosition, transform.position))
            {
                TargetArray[(int)TargetType.Flinger].SeekTime = 0;
            }


            
            if (CurrentTarget.SeekTime <= 0 && CurrentTarget.TargetID != 0)
            {
                TargetArray[CurrentTarget.TargetID] = Target.empty;

            }


            Ray lookForPlayer = new Ray();

            lookForPlayer.origin = transform.position + (TargetPlayer.TargetPosition - transform.position).normalized * (transform.lossyScale.magnitude / 2) + transform.TransformDirection(Vector2.up * 1.24f);

            lookForPlayer.direction = ((TargetPlayer.TargetPosition + PlayerTransform.TransformDirection(Vector2.up * 0.45f)) - lookForPlayer.origin).normalized;

            RaycastHit2D CanSeePlayer = Physics2D.Raycast(lookForPlayer.origin,lookForPlayer.direction, 100 , _LayersToIgnore);

            bool hitplayer;

            RenderRays.Add(new Logic.RayRenderer(lookForPlayer, 100,Color.magenta));

            if (CanSeePlayer.collider != null && CanSeePlayer.collider.gameObject == Player)
            {
                if (CanSeePlayer.distance > 50)
                {
                    if (_Suspicion >= _SuspicionThreshold)
                    {
                        hitplayer = true;
                    }
                    else
                    {
                        hitplayer = false;
                    }
                }
              
                hitplayer = true;
            }
            else
            {
                hitplayer = false;
            }

            if (hitplayer)
            {
                LastKnownPositionOfPlayer = PlayerTransform.position;
                TargetArray[(int)TargetType.JumpPoint].SeekTime = 0;
            }

                TargetPlayer.TargetDistance = Vector2.Distance(TargetPlayer.TargetPosition, transform.position);


            if (TargetPlayer.TargetDistance > 50 && AiPhase == Phase.Chase)
            {
                GameObject[] flingers = GameObject.FindGameObjectsWithTag("Flinger");

                foreach (GameObject flinger in flingers)
                {
                    Vector3 flingerpos = flinger.transform.position;

                    float distance = Vector2.Distance(flingerpos, transform.position);

                    if (distance <= TargetPlayer.TargetDistance)
                    {
                        Ray flingefinder = new Ray();
                        flingefinder.origin = transform.position + (flinger.transform.position - transform.position).normalized * (transform.lossyScale.magnitude / 2);
                        flingefinder.direction = (flinger.transform.position - flingefinder.origin).normalized;
                        RaycastHit2D CanReachFlinger = Physics2D.Raycast(flingefinder.origin, flingefinder.direction, 50, _LayersToIgnore);

                       // Debug.Log("FlingerRaycast");





                        float angleDifference = Vector3.Angle(flinger.GetComponent<FlingerScript>().Direction, lookForPlayer.direction);

                     //  Debug.Log("angleDifference " + angleDifference);

                      //  Debug.Log("flingerAngle " + flinger.GetComponent<FlingerScript>().Angle + " playerAngle " + -Vector3.SignedAngle(lookForPlayer.direction,Vector3.up,new Vector3(0,0,1)) + 180);

                        RenderRays.Add(new Logic.RayRenderer(flingefinder, (CanReachFlinger.distance == 0) ? 50 : CanReachFlinger.distance + 10,Color.red));

                        if (CanReachFlinger.distance >= distance || CanReachFlinger.distance == 0)
                        {
                            if (angleDifference < 100)
                            {
                               // Debug.Log("flinger pos " + flingerpos);
                                AddTarget((new Target(flingerpos, 0, (int)TargetType.Flinger, 20, 3)));
                                AiPhase = Phase.HeadToPoint;
                                break;
                            }
                           
                        }
                    }
                }
               
            }

           


            if (FlingerStunTime > FlingerStunTimer)
            {
                FlingerStunTimer += Time.deltaTime;
            }

            if (IsFlingered)
            {
                _InterestTime = _InterestCooldown / 2;
                AiPhase = Phase.Chase;
                IsFlingered = false;
                CurrentTarget = TargetPlayer;

                if (CurrentTarget.TargetID == (int)TargetType.Flinger)
                {
                    TargetArray[CurrentTarget.TargetID].SeekTime = 0;
                }

                CurrentTarget.SeekTime = 0;
                FlingerStunTimer = 0;
            }

            if (_Suspicion > 0)
            {
                _Suspicion -= Time.deltaTime;
            }


            _TargetVelocity = MaxVelocity;
            _PlayerisAbove = false;

           
            if (AiPhase != Phase.HeadToPoint)
            {
                if (_InterestTime > 0)
                {
                   
                    if (hitplayer)
                    {
                       // Debug.Log(CanSeePlayer.collider.gameObject == Player);
                        _CheckedLastPosition = false;
                        AiPhase = Phase.Chase;
                        _InterestTime -= Time.deltaTime;

                    }
                    else 
                    {
                        if (_CheckedLastPosition == false)
                        {
                            AddTarget(new Target(LastKnownPositionOfPlayer, 0, (int)TargetType.LastHitPlayer, 5, 3f + _InterestTime));
                            _InterestTime = 0;
                            _CheckedLastPosition = true;
                        }
                    }

                }
                else
                {
             
                }

                
            }

              //  AmIStandingOverNothing();

            if (_ChaseMouse)
            {
                Vector3 WorldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z * -1));
                WorldMousePos.z = 0;

                AddTarget(new Target(WorldMousePos, 0, 1, 200, 1));

                //Debug.Log(CurrentTarget.TargetPosition);


            }



            float rayDistance = 100;

            Ray hitRay = new Ray();


          
            AddTarget(Target.PlayerWithModdedPriority(TargetPlayer, (int)TargetType.Player));
            

            if (hitplayer)
            {
                AddTarget(TargetPlayer);
               // Debug.Log(TargetPlayer.Priority);
            }
         
             
            CurrentTarget = HighestPriorityTarget();
            _SeekDuration = CurrentTarget.SeekTime;
            TargetID = CurrentTarget.TargetID;
            TargetPosition = CurrentTarget.TargetPosition;
            TargetPriority = CurrentTarget.Priority;
            myTarget = (TargetType)(CurrentTarget.TargetID);

        //    Debug.Log("TargetPlayer.Priority " + TargetPlayer.Priority + "Hit Player? " + hitplayer);

            
            if (AiPhase != Phase.Wander)
            {
                if(IsGrounded)
                {
                    PredictJump();

                    if (Points.Count > 0)
                    {
                        RaycastHit2D bestPoint = new RaycastHit2D();
                        float minimumDistance = 10000;

                        foreach (RaycastHit2D ray in Points)
                        {
                            float checkDistance = Vector3.Distance(CurrentTarget.TargetPosition, ray.point);

                            if (checkDistance < minimumDistance)
                            {
                                minimumDistance = checkDistance;
                                bestPoint = ray;
                            }
                        }

                        if (minimumDistance < Vector3.Distance(CurrentTarget.TargetPosition, transform.position))
                        {
                            AiPhase = Phase.HeadToPoint;
                            CurrentTarget.TargetPosition = bestPoint.point;
                        }

                    }
                }
               

             
            }




            if (CurrentTarget.TargetID == TargetPlayer.TargetID)
            {

                hitRay.origin = transform.position + (CurrentTarget.TargetPosition - transform.position).normalized * (transform.lossyScale.magnitude / 2) + transform.TransformDirection(Vector2.up * 1.24f);

                hitRay.direction = ((CurrentTarget.TargetPosition + PlayerTransform.TransformDirection(Vector2.up * 0.45f)) - hitRay.origin).normalized;
            }
            else
            {
              
                hitRay.origin = transform.position + (CurrentTarget.TargetPosition - transform.position).normalized * (transform.lossyScale.magnitude / 2) + transform.TransformDirection(Vector2.up * 1.24f);

                hitRay.direction = ((CurrentTarget.TargetPosition) - hitRay.origin).normalized;
            }

           




            TargetPosition = CurrentTarget.TargetPosition;

            LayerMask mask = (CurrentTarget.TargetID == 0) ? _LayersToIgnore : ~_LayersToIgnore;

            RaycastHit2D hit1 = Physics2D.Raycast(hitRay.origin, hitRay.direction, rayDistance, mask);

            _HitDistance = (hit1.distance != 0) ? hit1.distance : rayDistance;
            WhatIHit = hit1.collider;

            RenderRays.Add(new Logic.RayRenderer(hitRay, _HitDistance));


            CurrentTarget.AngleToTarget = Vector2.SignedAngle(GravityDown, (TargetPosition - transform.position).normalized) + 180;
            _HitRayAngle = CurrentTarget.AngleToTarget;



          //  Debug.Log(CurrentTarget.AngleToTarget);

          //  Debug.Log(AiPhase);
           // Debug.Log((hit1 && true));
            int range = 45;

            if (CurrentTarget.AngleToTarget > 360 - range || CurrentTarget.AngleToTarget < range)
            {
                if (AiPhase != Phase.Wander)
                {
                    _PlayerisAbove = true;

                }
            }
            else
           

            if (_Suspicion > _SuspicionThreshold)
            {
                _InterestTime = _InterestCooldown;
            }

            if (hit1.collider != null && hit1.collider.gameObject == Player)
            {
                 //Debug.Log("Hit Player");

               

                
           
              

                if(_Suspicion <= _SuspicionThreshold)
                {
                    _Suspicion += 2 * Time.deltaTime;
                }
               


                if (_HitRayAngle > 280 || _HitRayAngle < 80)
                {
                   
                }
                else
                {
                    if (_HitDistance <= 50)
                    {
                        _InterestTime = _InterestCooldown;

                    }
                    else
                    {
                        _Suspicion += (0.5f / (_Suspicion + 5)) * Time.deltaTime;
                    }


                }
            }


            if (AiPhase == Phase.Wander)
            {
                Transform cachedtransform = transform;


                if (DecisionTime <= DecisionTimer)
                {
                    AiMovementState = (MovementState)Random.Range(1, 8);
                    DecisionTime = Random.Range(0.5f, MaxDecisionTIme);
                    DecisionTimer = 0;
                }
                else
                {
                    DecisionTimer += Time.deltaTime;
                }

                EdgeDetection();
                WallDetection();

                //UnityEngine.Debug.Log(AiMovementState);
            }

            if (AiPhase == Phase.HeadToPoint || AiPhase == Phase.Chase)
            {
                if (_PlayerisAbove)
                {
                    _TryJump = true;
                }
                else
                {
                    _TryJump = false;
                }


                if (hit1 == true && hit1.collider.gameObject.layer == 6)
                {
                   // Debug.Log(hit1.normal);
                   // Debug.Log(Vector2.Angle(Vector2.up, transform.TransformDirection(hit1.normal)));

                    if (Vector2.Angle(Vector2.up, transform.TransformDirection(hit1.normal)) < 40)
                    {

                    }
                    else
                    {
                        _TryJump = true;
                    }

                }



                if (_WaitBeforeTryingAgain <=0)
                {
                    if (_HitRayAngle > 180)
                    {
                        AiMovementState = MovementState.Right;
                    }
                    else
                    {
                        AiMovementState = MovementState.Left;
                    }

                }
                else
                {
                    _WaitBeforeTryingAgain -= 0.1f;
                }






                if (FlingerStunTimer < FlingerStunTime)
                {
                    AiMovementState = MovementState.limp;
                }


                EdgeDetection();
                WallDetection();



            }

            // Debug.Log("Alive");






            yield return new WaitForSeconds(0.01f);

        }
    }

    private void FixedUpdate()
    {
        RayCount = RenderRays.Count;


        if (RenderRays.Count > 200)
        {
            RenderRays.Clear();
        }

        Vector3 runningVelocity = _Body.linearVelocity;
        GravityDown = GameLogicScript.GravityDirection(transform.position,MyGamelogicScript.NormalGravity * Vector3.down).Gravity;


        if (Reactivate == true)
        {
            StopAllCoroutines();
            Ai = StartCoroutine(AiRoutine());
            Reactivate = false;
        }
        LookRotation = TargetAngleBasedOnGravityDirection(GravityDown);
        IsGrounded = CheckGrounded(GravityDown, _GroundCheckDistance, GroundLayer);

        transform.localEulerAngles = new Vector3(0, 0, LookRotation);

        //bool canMove = MyGamelogicScript.CanMove;
        bool runGame = MyGamelogicScript.RunGame;




        switch ((int)AiMovementState)
        {
            case 0:
                break;


            case 1:
                runningVelocity = MoveLeft(runningVelocity, _TargetVelocity, LookRotation, IsGrounded, _Body.linearVelocity, InAirAccelReduction, 1, _Acceleration,true,runGame);

                break;

            case 2:
                runningVelocity = MoveRight(runningVelocity, _TargetVelocity, LookRotation, IsGrounded, _Body.linearVelocity, InAirAccelReduction, 1, _Acceleration, true, runGame);
                break;

            case 3:
                runningVelocity = MoveRight(runningVelocity, 0, LookRotation, IsGrounded, _Body.linearVelocity, InAirAccelReduction, 1, _Acceleration * 2, true, runGame);
                break;
        
            case int i when i > 3  && i < 10:


                    PredictJump();
                

                if (Points.Count > 0)
                {
                    Vector3 point = Points[Random.Range(0, Points.Count)].point;

                    if (Vector2.Distance(transform.position, (Vector2)point) > 4)
                    {
                        AiPhase = Phase.HeadToPoint;



                        AddTarget(new Target(point, 0, (int)TargetType.JumpPoint, 60, 5));


                        _TryJump = true;
                    }
                    else
                    {
                        DecisionTimer = 1;
                        AiMovementState = MovementState.Right;
                    }
                }
                else
                {
                    DecisionTimer = 1;
                    AiMovementState = MovementState.Right;
                }

                break;

            default:
                break;
        }

        if (_TryJump && runGame)
        {
            if (IsGrounded)
            {
                runningVelocity = Jump(runningVelocity, GravityDown, JumpHeight);
                _TryJump = false;
            }
        }

        

        Velocity = runningVelocity;
        if (runGame)
        {
            _Body.linearVelocity = Velocity;
            _Body.AddForce(GravityDown * _Body.mass);

        }
    }

    void AssignTarget(Vector3 pos,float SeekTime,int priority)
    {
        _SeekDuration = SeekTime;
        AiPhase = Phase.HeadToPoint;
        CurrentTarget = Target.NonPlayerTarget(pos, priority, SeekTime);
    }

    void AmIStandingOverNothing()
    {

        if(IsGrounded == false && _SafeToJump == false || _PredictionHitEnd == false)
        {

            float checkDistanceDown = 50;
            Vector3 distanceToBottom = (transform.TransformDirection(Vector2.down) * -(transform.lossyScale.y / 2 + 0.2f));


            Ray VoidCheck = new Ray();
            Ray VoidCheck2 = new Ray();

            VoidCheck.origin = transform.position - distanceToBottom;
            //VoidCheck2.origin = transform.position - distanceToBottom + Mathf.Sqrt(_Body.linearVelocity.magnitude) * (Vector3)_Body.linearVelocity.normalized;
            VoidCheck2.origin = transform.position - distanceToBottom;
            VoidCheck.direction = GravityDown;
            // VoidCheck2.direction = GravityDown;
            // VoidCheck2.direction = new Vector3(transform.TransformDirection(_Body.linearVelocity.magnitude * (Vector3)_Body.linearVelocity.normalized).x, transform.TransformDirection(GravityDown/2).y) ;
            VoidCheck2.direction = (_Body.linearVelocity + (Vector2)GravityDown) / 2;
            RaycastHit2D HitFloor = Physics2D.Raycast(VoidCheck.origin, VoidCheck.direction, checkDistanceDown, GroundLayer);
            RaycastHit2D HitFloor2 = Physics2D.Raycast(VoidCheck2.origin, VoidCheck2.direction, checkDistanceDown, GroundLayer &= ~(1 << 9));


            

            RenderRays.Add(new Logic.RayRenderer(VoidCheck, HitFloor.distance > 0 ? HitFloor.distance : checkDistanceDown, (Color.white + Color.blue * 2) / 3));
            RenderRays.Add(new Logic.RayRenderer(VoidCheck2, HitFloor2.distance > 0 ? HitFloor2.distance : checkDistanceDown, (Color.white + Color.red * 2) / 3));





            if (HitFloor2 == false)
            {



                if (HitFloor == true)
                {

                    if (AiMovementState == MovementState.Left)
                    {
                        AiMovementState = MovementState.Right;
                    }


                    if (AiMovementState == MovementState.Right)
                    {
                        AiMovementState = MovementState.Left;
                    }

                    //RaycastHit2D HitFloor3 = Physics2D.Raycast(VoidCheck2.origin, new Vector3(transform.TransformDirection(Mathf.Sqrt(_Body.linearVelocity.magnitude) * (Vector3)_Body.linearVelocity.normalized).x * -1, transform.TransformDirection(GravityDown.normalized * 6).y), checkDistanceDown, GroundLayer &= ~(1 << 9));
                    Vector2 moddedVel = transform.TransformDirection(_Body.linearVelocity);
                    moddedVel.x *= -1;
                    moddedVel = transform.InverseTransformDirection(moddedVel);
                    RaycastHit2D HitFloor3 = Physics2D.Raycast(VoidCheck2.origin,(moddedVel + (Vector2)GravityDown) / 2, checkDistanceDown, GroundLayer &= ~(1 << 9));


                    RenderRays.Add(new Logic.RayRenderer(new Ray(VoidCheck2.origin, (moddedVel + (Vector2)GravityDown) / 2), HitFloor3.distance > 0 ? HitFloor3.distance : checkDistanceDown, (Color.red + Color.yellow * 2) / 3));

                    //RenderRays.Add(new RayRenderer(new Ray(VoidCheck2.origin, new Vector3(transform.TransformDirection(Mathf.Sqrt(_Body.linearVelocity.magnitude) * (Vector3)_Body.linearVelocity.normalized).x * -1, transform.TransformDirection(GravityDown.normalized * 6).y)), HitFloor3.distance > 0 ? HitFloor3.distance : checkDistanceDown, (Color.red + Color.yellow * 2) / 3));


                    if (HitFloor3)
                    {
                        LastSafePosition = HitFloor3.point;
                        AddTarget(new Target(LastSafePosition, 0, (int)TargetType.LastSafePoint, 1000, 0.5f));
                    }
                    else
                    {
                        AddTarget(new Target(LastSafePosition, 0, (int)TargetType.LastSafePoint, 1000, 0.5f));

                    }
                    //  LastSafePosition = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * transform.position);

                    //  LastSafePosition.y = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * HitFloor.point).y;

                    // LastSafePosition = (Quaternion.AngleAxis(LookRotation, new Vector3(0, 0, 1)) * LastSafePosition);



                    //     LastSafePosition = HitFloor.point;
                }

            }
            else
            {
                if (HitFloor == false)
                {

                    LastSafePosition = HitFloor2.point;
                    AddTarget(new Target(LastSafePosition, 0, (int)TargetType.LastSafePoint, 10, 0.5f));
                }
                 
            }

            if (HitFloor == false)
            {
                _TargetVelocity = MaxVelocity * 2;

                AddTarget(new Target(LastSafePosition, 0, (int)TargetType.LastSafePoint, 10,0.5f));
            }

        }
        else
        {
            LastSafePosition = transform.position;
        }

     

    }

    void WallDetection()
    {
        int checkDirection = AiMovementState == MovementState.Right ? 1 : -1;
        Ray wallDetector = new Ray();
        wallDetector.origin = transform.position + transform.lossyScale.x * transform.TransformDirection(new Vector3(1.1f,0) * checkDirection) / 2 ;
        wallDetector.direction = transform.TransformDirection(Vector3.right * checkDirection);
        RaycastHit2D wallDetectorHit = Physics2D.Raycast(wallDetector.origin, wallDetector.direction, 0.5f, GroundLayer);
        RenderRays.Add(new Logic.RayRenderer(wallDetector, 0.5f));

        if (wallDetectorHit == true)
        {
            _TryJump = true;
        }


    }

    void EdgeDetection()
    {
        //Debug.Log("EdgeDetection");
        if (IsGrounded)
        {
            int checkDirection = (transform.TransformDirection(_Body.linearVelocity).x > 0) ? 1 : -1;
            float checkDistanceHorizontal = 2.5f;
            float checkDistanceDown = 50;
            float checkThreshold = 10;


            Ray NewRay = new Ray();
            Ray NewRayFar = new Ray();


            NewRay.origin = transform.position + transform.TransformDirection(Vector3.right * checkDistanceHorizontal * checkDirection);
            NewRayFar.origin = transform.position + transform.TransformDirection(Vector3.right * checkDistanceHorizontal * checkDirection * 2);
            NewRay.direction = GameLogicScript.GravityDirection(NewRay.origin, MyGamelogicScript.NormalGravity * Vector3.down).Gravity;
            NewRay.direction = GameLogicScript.GravityDirection(NewRayFar.origin, MyGamelogicScript.NormalGravity * Vector3.down).Gravity;



            RaycastHit2D checkForCliffShort = Physics2D.Raycast(NewRay.origin, NewRay.direction, checkDistanceDown, GroundLayer);
            RaycastHit2D checkForCliffFar = Physics2D.Raycast(NewRayFar.origin, NewRayFar.direction, checkDistanceDown, GroundLayer);


            RenderRays.Add(new Logic.RayRenderer(NewRay, (checkForCliffShort.distance > 0 ? checkForCliffShort.distance : 1)));
            RenderRays.Add(new Logic.RayRenderer(NewRayFar, (checkForCliffFar.distance > 0 ? checkForCliffFar.distance : 1)));



            FloorCheckDistanceFar = checkForCliffFar.distance;

            if ((checkForCliffFar.distance <= 0 && checkForCliffFar == false) || checkForCliffFar.distance > checkThreshold)
            {
                _TargetVelocity = MaxVelocity / 2;
            }

            FloorCheckDistance = (checkForCliffShort.distance == 0) ? checkDistanceDown : checkForCliffShort.distance;


            float myYPosition = (Quaternion.AngleAxis(-LookRotation, new Vector3(0, 0, 1)) * transform.position).y;


            float targetLookRotation = TargetAngleBasedOnGravityDirection(GameLogicScript.GravityDirection(CurrentTarget.TargetPosition, MyGamelogicScript.NormalGravity * Vector3.down).Gravity);

            float targeyYPosition = ((Quaternion.AngleAxis(-targetLookRotation, new Vector3(0, 0, 1)) * PlayerTransform.position).y);




            if ((checkForCliffShort.distance <= 0 && checkForCliffShort == false)  || checkForCliffShort.distance > checkThreshold)

            {
                if (AiPhase == Phase.Wander)
                {
                    AiMovementState = checkDirection == 1 ? MovementState.Left : MovementState.Right;
                }
                else if (AiPhase == Phase.Chase || AiPhase == Phase.HeadToPoint)
                {

                    PredictJump();
                    _SafeToJump = false;

                   // Debug.Log(Points.Count);
                    foreach (RaycastHit2D ray in Points)
                    {
                        float pointYrot = TargetAngleBasedOnGravityDirection(GameLogicScript.GravityDirection(ray.point, MyGamelogicScript.NormalGravity * Vector3.down).Gravity);
                        float pointY = (Quaternion.AngleAxis(-pointYrot, new Vector3(0, 0, 1)) * ray.point).y;
                       // Debug.Log(pointY + " , " + targeyYPosition);

                        if (Vector2.Distance(ray.point, CurrentTarget.TargetPosition) < 5 || Mathf.Abs(pointY - targeyYPosition) < 5)
                        {

                            _SafeToJump = true;
                            _TryJump = true;
                            break;
                        }

                    }


                    if (Mathf.Abs(checkForCliffShort.distance - Mathf.Abs(myYPosition - targeyYPosition)) > 5 && CurrentTarget.TargetID != 0)
                    {
                      
                        if (_TryJump == false)
                        {
                            AiMovementState = checkDirection == 1 ? MovementState.Left : MovementState.Right;
                            _WaitBeforeTryingAgain = 1f;
                        }
                    }
                    else
                    {
                        _TryJump = true;
                    }

                }

            }

        }
    }

    public Target HighestPriorityTarget()
    {
        Target HighestPriority = Target.empty;


        TargetArray[CurrentTarget.TargetID].SeekTime -= Time.deltaTime;

        for (int i = 0; i < TargetArray.Length; i++)
        {
            Target examinedTarget = TargetArray[i];

            if (examinedTarget.SeekTime <= 0 && examinedTarget.TargetID != 0)
            {
                TargetArray[i] = Target.empty;
            }
            else
            {
                HighestPriority = (TargetArray[i].Priority > HighestPriority.Priority) ? TargetArray[i] : HighestPriority;
                
            }
        }


        
        if (HighestPriority.TargetID == 0)
        {
            if (_InterestTime > 0)
            {
                AiPhase = Phase.Chase;
            }
            else
            {
               // Debug.Log("_InterestTime " +_InterestTime);
                AiPhase = Phase.Wander;
            }
        }
        else
        {
            AiPhase = Phase.HeadToPoint;
        }




            /*
            if (HighestPriority.TargetID != 0)
            {
                AiPhase = Phase.HeadToPoint;
                if (_SeekDuration <= 0)
                {
                    _SeekDuration = HighestPriority.SeekTime;
                }
            }
            else
            {

                _SeekDuration = 0;
            }
            */



            return HighestPriority;
    }

    void AddTarget(Target target)
    {
        TargetArray[target.TargetID] = target;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}

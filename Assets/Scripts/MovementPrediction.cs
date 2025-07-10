using UnityEngine;

using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class MovementPrediction
{
    public Coroutine PredictRoutine;
    public List<Vector2> PredictionEndGravity = new();
    public List<float> PredictionEndDistances = new();
    public Transform MyTransform;
    public Rigidbody2D Body;
    public List<Logic.RayRenderer> Rays = new List<Logic.RayRenderer>(0);
    public LayerMask PredictionGroundlayer;
    public float PredictionTime;
    public float PredictionFrequency;

    public float initialSteps;
    public float initialInvertedGravityPower;
    public Vector3 NormalGravity;
    public float InvertedGravityPower;
    public float Resolution;
    public float Steps;
    
    // Playerscript Movement Prediction

    public IEnumerator PredictionRays()
    {
        //Debug.Log("called");
        //Debug.Log(true);
        Vector3 gravity = GameLogicScript.GravityDirection(MyTransform.position, NormalGravity).Gravity;
    
        Vector3 gravDeltaV = Vector2.zero;

        Vector3 point = Vector3.zero;

        Vector3 velocity = Vector3.zero;

        velocity = Body.linearVelocity / Resolution;
        InvertedGravityPower = initialInvertedGravityPower * Resolution * Resolution;

        Steps = Mathf.FloorToInt(Mathf.Clamp(initialSteps * Resolution * (10 / (10 + velocity.magnitude)), 0, initialSteps * Resolution * 10));
        point = MyTransform.position;
        float waitTime = PredictionTime / Steps;
        int stepsPerFrame = 1;
        bool continueLoop = true;

        if (waitTime < 0.02f)
        {
            stepsPerFrame = Mathf.Clamp(Mathf.RoundToInt(0.02f / waitTime), 1, 1000);

        }

       // Debug.Log(Steps);

        for (int i = 0; i < Steps && continueLoop; i += stepsPerFrame)
        {
            
            if (Rays.Count > 500)
            {
                Rays.RemoveRange(0, Rays.Count - 500);
            }

        
            for (int j = 0; j < stepsPerFrame; j++)
            {
                RaycastHit2D RaysegmentHit = Physics2D.Raycast(point, velocity, velocity.magnitude, PredictionGroundlayer);

                float distance = velocity.magnitude;

                if (RaysegmentHit == true)
                {
                    distance = RaysegmentHit.distance;
                }


                Rays.Add(new Logic.RayRenderer(new Ray(point, velocity.normalized), distance, Color.red));

                float pointRot = MovementScript.TargetAngleBasedOnGravityDirection(GameLogicScript.GravityDirection(point, NormalGravity).Gravity);

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

                        }
                        else
                        {
                            point += velocity;
                        }


                    }
                    else
                    {
                        yield return new WaitForSeconds((PredictionTime / Steps) * (Steps - i));
                        continueLoop = false;
                        PredictionEndGravity.Add((Vector2)(gravDeltaV * InvertedGravityPower));
                        PredictionEndDistances.Add((point - MyTransform.position).sqrMagnitude);
                        break;
                    }


                }
                else
                {
                    point += velocity;
                }

                gravDeltaV = GameLogicScript.GravityDirection(point, NormalGravity).Gravity / InvertedGravityPower;

                velocity += gravDeltaV;

            }

            yield return new WaitForSeconds(stepsPerFrame * waitTime);
        }
        PredictionEndGravity.Add((Vector2)(gravDeltaV * InvertedGravityPower));
        PredictionEndDistances.Add((point - MyTransform.position).sqrMagnitude);

        //  Debug.Log("finished", gameObject);
    }







}

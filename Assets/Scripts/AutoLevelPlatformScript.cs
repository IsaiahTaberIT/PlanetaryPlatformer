#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

[ExecuteInEditMode]

public class AutoLevelPlatformScript : MonoBehaviour
{
    public bool Dynamic;
    public bool ReCenter;
    public float InitialLength = 1;
    public float size = 0.25f;
    public int Steps;
    private LastValues _MyLastValues;




    public GameLogicScript MyGameLogicScript;
    private Spline _MySpline;



    public List<Gravity> PlanetsOfInfluence = new();
    public Vector3[] Points;
    private SpriteShapeController _ShapeController;

  

    
    public struct LastValues
    {
        public Vector3 LastPos;
        public int LastSteps;
        public float LastInitialLength;
        public bool LastDynamic;
        public bool LastReCenter;

        public LastValues(Vector3 lastPos, int lastSteps, float lastInitialLength, bool lastdynamic, bool lastReCenter)
        {
            LastPos = lastPos;
            LastSteps = lastSteps;
            LastInitialLength = lastInitialLength;
            LastDynamic = lastdynamic;
            LastReCenter = lastReCenter;
        }

    }



   



    [ContextMenu("FindAllPlanets")]


   

    void FindAllPlanets()
    {
        PlanetsOfInfluence.Clear();
        PlanetsOfInfluence.AddRange(FindObjectsByType<Gravity>(FindObjectsSortMode.None));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //_MySpline = _ShapeController.spline;
        MyGameLogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();

    }

    bool ChangedValues()
    {
        if (_MyLastValues.LastInitialLength != InitialLength
           || _MyLastValues.LastSteps != Steps
           || _MyLastValues.LastPos != transform.position
           || _MyLastValues.LastDynamic != Dynamic
           || _MyLastValues.LastReCenter != ReCenter
           )
        {
            _MyLastValues.LastPos = transform.position;
            _MyLastValues.LastSteps = Steps;
            _MyLastValues.LastInitialLength = InitialLength;
            _MyLastValues.LastDynamic = Dynamic;
            _MyLastValues.LastReCenter = ReCenter;

            return true;
        }


        return false;

    }





    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < _MySpline.GetPointCount(); i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_MySpline.GetPosition(i) + transform.position, size);
        }
    }
    
     
    void CenteredOnEdge()
    {
        if (Dynamic && ReCenter && (Steps % 2 == 1))
        {
            Steps++;
        }

        Vector3 point = Vector3.zero;
        _MySpline.InsertPointAt(0, point);
        _MySpline.SetTangentMode(0, ShapeTangentMode.Continuous);


        for (int i = 0; i < Steps; i++)
        {
            Vector3 direction = Vector3.up;

            if (Dynamic)
            {
                direction = (Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * GameLogicScript.BasicGravityDirection(point + transform.position, PlanetsOfInfluence, MyGameLogicScript.NormalGravity * Vector3.down));
                float magnitude = direction.magnitude;
                direction = direction.normalized;
                magnitude = (40 / (magnitude / 10 + 1)) + 0.1f;
                direction *= magnitude;
            }
            else
            {
                direction = (Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * GameLogicScript.BasicGravityDirection(point + transform.position, PlanetsOfInfluence, MyGameLogicScript.NormalGravity * Vector3.down)).normalized * InitialLength;
            }
            Debug.Log(MyGameLogicScript.NormalGravity);

            point += direction;
            _MySpline.InsertPointAt(i + 1, point);
            _MySpline.SetTangentMode(i + 1, ShapeTangentMode.Continuous);
        }
    }


    void CenteredOnMiddle()
    {
        Vector3 point1 = Vector3.zero;
        Vector3 point2 = Vector3.zero;
        Vector3 direction1 = Vector3.up;
        Vector3 direction2 = Vector3.up;


        Points = new Vector3[Steps + 1];

        Points[Steps / 2] = point1;

        _MySpline.Clear();


        if (_MySpline.GetPointCount() != Steps)
        {
            for (int i = 0; i < Steps; i++)
            {
                _MySpline.InsertPointAt(i, Vector3.zero);

            }
        }

       


        for (int i = 1; i < Steps / 2 + 1; i++)
        {

            if (Dynamic)
            {
                direction1 = (Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * GameLogicScript.BasicGravityDirection(point1 + transform.position, PlanetsOfInfluence, MyGameLogicScript.NormalGravity * Vector3.down));
                float magnitude = direction1.magnitude;
                direction1 = direction1.normalized;
                magnitude = (40 / (magnitude / 10 + 1)) + 0.1f;
                direction1 *= magnitude;

                direction2 = (Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * GameLogicScript.BasicGravityDirection(point2 + transform.position, PlanetsOfInfluence, MyGameLogicScript.NormalGravity * Vector3.down));
                magnitude = direction2.magnitude;
                direction2 = direction2.normalized;
                magnitude = (40 / (magnitude / 10 + 1)) + 0.1f;
                direction2 *= magnitude;
            }
            else
            {
                direction1 = (Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * GameLogicScript.BasicGravityDirection(point1 + transform.position, PlanetsOfInfluence, MyGameLogicScript.NormalGravity * Vector3.down)).normalized * InitialLength;

                direction2 = (Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * GameLogicScript.BasicGravityDirection(point2 + transform.position, PlanetsOfInfluence, MyGameLogicScript.NormalGravity * Vector3.down)).normalized * -InitialLength;
            }

           // Debug.Log(MyGameLogicScript.NormalGravity);

            if (i == 1)
            {
                direction1 /= 2;
                direction2 /= 2;

            }

            point1 += direction1;
            point2 += direction2;

            

            Points[Steps / 2 + i] = point1;

            Points[Steps / 2 - i] = point2;

            
        }

        for (int i = 0; i < Steps; i++)
        {

            _MySpline.SetTangentMode(i, ShapeTangentMode.Continuous);

            if (Points[i] != Vector3.zero)
            {
                _MySpline.SetPosition(i, Points[i]);
               // Debug.Log(i + " , " + Points[i]);
            }
           


        }

    }

    void OnValidate()
    {
        CalculateSpline();
    }

    [ContextMenu("CalcSpline")]
    [OnEditorMoved]
    void CalculateSpline()
    {
        if(_ShapeController == null)
        {
            _ShapeController = GetComponent<SpriteShapeController>();
        }

        if (MyGameLogicScript == null)
        {



            GameObject obj = GameObject.FindGameObjectWithTag("Logic");

            if (obj == null)
            {
                return;
            }

            MyGameLogicScript = obj.GetComponent<GameLogicScript>();
          

           
        }





        _MySpline = _ShapeController.spline;
        _MySpline.Clear();

        if (ReCenter)
        {
            CenteredOnMiddle();
        }
        else
        {
            CenteredOnEdge();
        }




    }

}
#endif
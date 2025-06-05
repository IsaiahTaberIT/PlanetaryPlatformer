#if UNITY_EDITOR


using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;


public class UltimateCurveGenerationScript : MonoBehaviour
{
    [ExecuteInEditMode]


    [HideInInspector] public bool showGizmoSettings = false;
    [HideInInspector] public bool showButtons = false;
    private List<int> corners = new();

    //  [Min(0.01f)]public float divide;
    [HideInInspector] public float GizmoSize;
    [HideInInspector] public Color SelectedColor;
    [HideInInspector] public Color UnSelectedColor;

    [HideInInspector] public bool DrawAny;
    [HideInInspector] public bool DrawAll;
    [SerializeField] public CurveSegment DisplaySegment;
    [HideInInspector] public CurveSegment LastDisplaySegment;
    [HideInInspector] public int LastSegment;

    public List<CurveSegment> Segments = new (1);
    [SerializeField] public TargetInfo TargetSettings;
    [SerializeField] public List<Vector3> Points;
    [HideInInspector] public int CurrentSegment;
    [SerializeField] private SpriteShapeController ShapeController;
    [SerializeField] private Spline MySpline;
    [SerializeField] private bool _DisplayIcon;
    [SerializeField] private bool AutoGenerateVerts;
    [SerializeField] private float VertDensity = 2;
    [SerializeField] private ShapeTangentMode _TangentMode = ShapeTangentMode.Linear;
    [SerializeField] private List<Logic.RayRenderer> RayRenderers = new(0);
    public bool IslolateOnInsertion;
    public GameLogicScript MyGameLogicScript;
    [SerializeField] private List<Gravity> PlanetsOfInfluence = new();
    public float SnapSpacing;

    public void InsertSegment()
    {
        CurveSegment segment = new CurveSegment();
        segment.Type = Segments[CurrentSegment].Type;
        segment.SegmentPoints = new List<Vector3>(0);
        segment.Length = 2;
        segment.Verts = 2;
        segment.Isolate = IslolateOnInsertion;
        segment.OldDimentions = Vector2.zero;

        Segments.Insert(CurrentSegment + 1, segment);

        CurrentSegment++;
        DisplaySegment = segment;
        LastDisplaySegment = segment;

        GenerateSpriteShape();

    }

    public void DeleteSegment()
    {
        Segments.RemoveAt(CurrentSegment);

        if (CurrentSegment != 0)
        {
            CurrentSegment--;
        }
        DisplaySegmentRefresh();

        GenerateSpriteShape();
    }


    [ContextMenu("FindAllPlanets")]
    void FindAllPlanets()
    {
        PlanetsOfInfluence.Clear();
        PlanetsOfInfluence.AddRange(FindObjectsByType<Gravity>(FindObjectsSortMode.None));
    }

    [System.Serializable]
    public struct TargetInfo
    {
        [HideInInspector]
        public Vector3 transformPosition;
        public TargetType Type;
        public Vector3 TargetPosition;
        public GameObject TargetObject;

        public Vector3 GetTargetPos(Transform transform)
        {
            Vector3 Output = Vector3.zero;

            if (Type == TargetType.Vector)
            {
                Output = TargetPosition;
            }
            else if (Type == TargetType.Transform)
            {
                Output = transformPosition;
            }
            else if (Type == TargetType.Object)
            {
                Output = TargetObject.transform.position;
            }
            else if (Type == TargetType.Offset)
            {
                Output = transformPosition + TargetPosition;
            }
            else if (Type == TargetType.Parent)
            {
                if ((transform.GetRootParent() != null && TargetObject == null))
                {
                    TargetObject = transform.transform.GetRootParent().gameObject;
                }

                if (TargetObject != null)
                {
                    Output = TargetObject.transform.position;
                }
            }



            return Output;

        }
    }

    public void test()
    {
        Debug.Log(true);
    }

    public enum TargetType
    {
        Transform = 0,
        Vector = 1,
        Object = 2,
        Offset = 3,
        Parent = 4,
        None = 5,

    }

    public void FixTransformPosition()
    {

        TargetSettings.transformPosition = transform.position;

        if (TargetSettings.Type != TargetType.Transform && TargetSettings.Type != TargetType.None)
        {
            _DisplayIcon = true;

        }
        else
        {
            _DisplayIcon = false;

        }
    }

    public void DisplaySegmentRefresh()
    {
        FixTransformPosition();

        if (CurrentSegment > Segments.Count - 1 || CurrentSegment < 0)
        {
            CurrentSegment = Mathf.Clamp(CurrentSegment, 0, Segments.Count - 1);
        }

        if (CurrentSegment != LastSegment)
        {
            CurrentSegment = Mathf.Clamp(CurrentSegment, 0, Segments.Count - 1);
            DisplaySegment = Segments[CurrentSegment];
            LastSegment = CurrentSegment;
        }
        else
        {
            if (LastDisplaySegment.Equals(DisplaySegment))
            {
                if (!Segments[CurrentSegment].Equals(DisplaySegment))
                {
                    DisplaySegment = Segments[CurrentSegment];
                }
            }
            else
            {
                Segments[CurrentSegment] = DisplaySegment;
            }
        }

        LastDisplaySegment = DisplaySegment;
    }

    [ContextMenu("clear spline")]
    void ClearSpline()
    {
        ShapeController = null;

        ShapeController = GetComponent<SpriteShapeController>();


        MySpline = null;

        MySpline = ShapeController.spline;

    }
    void OnEnable()
    {
        MyGameLogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();

        ShapeController = GetComponent<SpriteShapeController>();


    }

    private void OnDrawGizmosSelected()
    {

        if (DrawAny)
        {

            if (DrawAll)
            {
                foreach (Vector3 point in Points)
                {

                    Gizmos.color = UnSelectedColor;
                    Gizmos.DrawSphere(Vector3.Scale(transform.TransformDirection(point), transform.lossyScale) + TargetSettings.GetTargetPos(transform), GizmoSize);
                }


                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(TargetSettings.GetTargetPos(transform), GizmoSize * 2);
            }


            foreach (Vector3 point in Segments[CurrentSegment].SegmentPoints)
            {
                // Debug.Log(CurrentSegment);
                Gizmos.color = SelectedColor;
                Gizmos.DrawSphere(Vector3.Scale(transform.TransformDirection(point), transform.lossyScale) + TargetSettings.GetTargetPos(transform), GizmoSize * 1.1f);
            }


        }
    }
    private void OnDrawGizmos()
    {


        if (_DisplayIcon)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawIcon(transform.position, "BlurredCobble.png", false);

        }

        Logic.RenderRayRenderer(RayRenderers);


    }
    private void OnValidate()
    {


        if (!Application.isPlaying)
        {
            RayRenderers.Clear();

            GenerateSpriteShape();
        }
        
    }

    public enum SegmentType
    {
        None = -1,
        Polar = 0,
        Euclidean = 1,
        SelfLeveling = 2,
        PolarConstLength = 3,
        Manual = 4,
        
    }
    void GeneratePolarConstLength(int index)
    {
        GeneratePolar(index);
    }


    void GenerateSegment(SegmentType segmentType, int index)
    {
        switch ((int)segmentType)
        {

            case 0:
                GeneratePolar(index);

                break;
            case 1:

                GenerateEuclidean(index);

                break;
            case 2:

                GenerateAutoLevel(index);

                break;
            case 3:

                GeneratePolarConstLength(index);
                break;
            case 4:


                break;
            case 5:


                break;
            default:
                break;
        }
        CurveSegment segment = Segments[index];
        segment.LastType = segmentType;
        Segments[index] = segment;

    }

    Vector3 GetStartingPosition(int index)
    {
        Vector3 startPos = Vector3.zero;

        CurveSegment lastSegment;

        if (index > 0 && Segments.Count >= index)
        {
            lastSegment = Segments[index - 1];

            if (lastSegment.SegmentPoints.Count > 0)
            {
                startPos = lastSegment.SegmentPoints[^1];
            }

            else
            {
                Debug.Log("No Points at: " + (index - 1));

            }


        }

        return startPos;
    }

    Vector3 GeneratePointsAutoLevel(int verts, ref CurveSegment currentSegment, float shiftX, float shiftY, float segmentArcLength, float radius, Vector2 LastPoint, Vector2 startpos, bool isOld)
    {
        Vector3 pointPosition = Vector3.zero;

        for (int i = 0; i < verts; i++)
        {
            Ray debugRay = new();

            float arcLength = i * segmentArcLength + shiftX;
            float radialStep = i * ((radius - currentSegment.InitHeight) / (verts - 1)) + shiftY + currentSegment.InitHeight;

            pointPosition = new Vector2(Mathf.Sin(arcLength), Mathf.Cos(arcLength)) * (radius + shiftY);

            Vector2 direction = (Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * GameLogicScript.BasicGravityDirection((Vector3)LastPoint + transform.position, PlanetsOfInfluence, MyGameLogicScript.NormalGravity * Vector3.down));

            Vector2 startingPosition = TargetSettings.GetTargetPos(transform);

            if (!isOld)
            {
                debugRay.origin = startingPosition;
                debugRay.direction = pointPosition * 2;

                RayRenderers.Add(new Logic.RayRenderer(debugRay, 1000, (Color.magenta + Color.red) / 2));

                debugRay.origin = LastPoint + startingPosition;
                debugRay.direction = direction - LastPoint;

                RayRenderers.Add(new Logic.RayRenderer(debugRay, 1000, Color.green));
            }
        
            pointPosition = Logic.IntersectionPoint(Vector2.zero, (Vector2)pointPosition, LastPoint, direction - LastPoint);

            LastPoint = pointPosition;

            if (i != 0 || (startpos - (Vector2)pointPosition).sqrMagnitude > 0.5)
            {
                if(!isOld)
                {
                    currentSegment.SegmentPoints.Add(pointPosition);

                }
            }

        }

        return pointPosition;

    }
    void GenerateAutoLevel(int index)
    {
        Vector3 oldPointPosition = Vector3.zero;
        CurveSegment currentSegment = Segments[index];
        SegmentType lastType = SegmentType.None;
        Vector2 startpos = GetStartingPosition(index);

        if (index != 0)
        {
            CurveSegment lastSegment = Segments[index - 1];

            lastType = lastSegment.Type;
        }

        currentSegment.IsolatedShift = (index == 0 || !Segments[index - 1].Isolate) ? Vector2.zero : currentSegment.IsolatedShift;
        Vector2 isolatedShift = (index == 0) ? Vector2.zero : currentSegment.IsolatedShift;

        Vector2 dims = Segments[index].Dimentions;
        float shiftX = 0;
        float shiftY = 0;
        float radius = dims.y - isolatedShift.y;
        int verts = Segments[index].Verts;

        Segments[index].SegmentPoints.Clear();


        if (lastType != SegmentType.None)
        {
            CurveSegment lastSegment = Segments[index - 1];

            if (lastType == SegmentType.Polar || lastType == SegmentType.PolarConstLength || lastType == SegmentType.SelfLeveling)
            {
                shiftY = lastSegment.PolarShift.y;
                shiftX = lastSegment.PolarShift.x;
            }
            else if (lastType == SegmentType.Euclidean)
            {
       
                shiftY = Vector2.Distance(startpos, Vector3.zero);
                shiftX = -Vector2.SignedAngle(Vector2.down, lastSegment.SegmentPoints[^1]) + 180;
                shiftX /= 180;
                shiftX *= Mathf.PI;
            }

        }


        float segmentArcLength = 0;
        float shiftedDimentions = (dims.x - isolatedShift.x);

        segmentArcLength = shiftedDimentions * Mathf.PI / 180;

        if (currentSegment.ReAssignVerts)
        {
            float totalRadius = Mathf.Abs(shiftY + radius);
            float squrtRadius = Mathf.Pow(2f, (-75f / (totalRadius + 10f))) * (50f / 360f) * VertDensity;

            currentSegment = CalculateVerts(currentSegment, shiftedDimentions * squrtRadius);         
        }
        else
        {
            if (verts < 2)
            {
                verts = 2;
            }
        }

        segmentArcLength /= (verts - 1);

        Vector2 LastPoint = startpos;
        //GenerateMainPoints

        Vector3 pointPosition = Vector3.zero;

        Vector2 startingPosition = TargetSettings.GetTargetPos(transform);

        for (int i = 0; i < verts; i++)
        {

            Ray debugRay = new();

            float arcLength = i * segmentArcLength + shiftX;
            float radialStep = i * ((radius - currentSegment.InitHeight) / (verts - 1)) + shiftY + currentSegment.InitHeight;

            pointPosition = new Vector2(Mathf.Sin(arcLength), Mathf.Cos(arcLength)) * (radius + shiftY);

            Vector2 direction = (GameLogicScript.BasicGravityDirection(transform.TransformDirection(LastPoint + LastPoint.normalized) + transform.position, PlanetsOfInfluence, MyGameLogicScript.NormalGravity * Vector3.down));


            debugRay.origin = transform.TransformDirection(LastPoint) + (Vector3)startingPosition;
            debugRay.direction = direction;

            RayRenderers.Add(new Logic.RayRenderer(debugRay, 100, (Color.blue + Color.white)/2));

           // Debug.Log(Vector2.Angle(direction, transform.TransformDirection(LastPoint)));

            Quaternion rotation = (Vector2.Angle(direction, transform.TransformDirection(LastPoint)) > 90) ? Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) : Quaternion.AngleAxis(-90, new Vector3(0, 0, 1));

            direction = rotation * direction;




            debugRay.origin = startingPosition;
            debugRay.direction = transform.TransformDirection(pointPosition);

            RayRenderers.Add(new Logic.RayRenderer(debugRay, 1000, (Color.magenta + Color.red) / 2));

            debugRay.origin = transform.TransformDirection(LastPoint) + (Vector3)startingPosition;
            debugRay.direction = direction;

            RayRenderers.Add(new Logic.RayRenderer(debugRay, 1000, Color.green));

            pointPosition = Logic.IntersectionPoint(Vector2.zero, pointPosition, LastPoint, transform.InverseTransformDirection(direction));

            LastPoint = pointPosition;

            if (i != 0 || (startpos - (Vector2)pointPosition).sqrMagnitude > 0.5)
            {


                currentSegment.SegmentPoints.Add(pointPosition);


            }

        }



        float startradius = Vector2.Distance(startpos, Vector2.zero);

        currentSegment.PolarShift.x = segmentArcLength * (verts - 1) + shiftX;
        currentSegment.PolarShift.y = startradius;

        Points.AddRange(currentSegment.SegmentPoints);


        if (index < Segments.Count - 1)
        {
            CurveSegment nextSegment = Segments[index + 1];

            if (nextSegment.Sloped)
            {
                nextSegment.InitHeight = Vector2.Distance(currentSegment.SegmentPoints[^1], Vector2.zero) - startradius;
            }

            Segments[index + 1] = nextSegment;
        }







        if (!currentSegment.Isolate)
        {
            // Assigning Offset from Isolation

            if (currentSegment.WasIsolated)
            {
                if (index < Segments.Count - 1)
                {
                    CurveSegment nextSegment = Segments[index + 1];

                    if (nextSegment.Isolate)
                    {
                        nextSegment.OldDimentions = nextSegment.OldDimentions - nextSegment.IsolatedShift;
                    }

                    nextSegment.SetDimentions(nextSegment.Dimentions - nextSegment.IsolatedShift);
                    nextSegment.IsolatedShift = Vector2.zero;

                    Segments[index + 1] = nextSegment;
                }
            }


            currentSegment.WasIsolated = false;
            currentSegment.OldDimentions = currentSegment.Dimentions;
            currentSegment.OldPosition = currentSegment.SegmentPoints[^1];



        }
        else
        {
            currentSegment.WasIsolated = true;

            if (index < Segments.Count - 1)
            {
                CurveSegment nextSegment = Segments[index + 1];

                float isloatedY = currentSegment.Dimentions.y - currentSegment.OldDimentions.y;
                float nextRadius = currentSegment.Dimentions.y + shiftY + nextSegment.Dimentions.y - isloatedY;

                if (nextSegment.Type == SegmentType.Euclidean)
                {
                  

                    //polar -> euclidean

                    nextSegment.IsolatedShift = currentSegment.SegmentPoints[^1] - currentSegment.OldPosition;



                }
                else if (nextSegment.Type == SegmentType.Polar || nextSegment.Type == SegmentType.SelfLeveling)
                {
                    nextSegment.IsolatedShift.x = currentSegment.Dimentions.x - currentSegment.OldDimentions.x;

                    nextSegment.IsolatedShift.y = 0;
                    // polar -> polar
                    //  Debug.Log("PolarPolar");
                }
                else if (nextSegment.Type == SegmentType.PolarConstLength)
                {
                    // polar -> constant

                    nextSegment.IsolatedShift.x = (currentSegment.Dimentions.x - currentSegment.OldDimentions.x) * nextRadius / (180f / (2f * Mathf.Pow(Mathf.PI, 2)));
                    nextSegment.IsolatedShift.y = 0;
                }


                Segments[index + 1] = nextSegment;
            }
            
        }

        Segments[index] = currentSegment;

    }


    void GeneratePolar(int index)
    {

        CurveSegment currentSegment = Segments[index];

        SegmentType lastType = SegmentType.None;
        Vector2 startpos = GetStartingPosition(index);

        if (index != 0)
        {
            CurveSegment lastSegment = Segments[index - 1];

            lastType = lastSegment.Type;
        }

        currentSegment.IsolatedShift = (index == 0 || !Segments[index - 1].Isolate) ? Vector2.zero : currentSegment.IsolatedShift;
        Vector2 isolatedShift = (index == 0) ? Vector2.zero : currentSegment.IsolatedShift;



        Vector2 dims = Segments[index].Dimentions;
        float shiftX = 0;
        float shiftY = 0;
        float radius = dims.y - isolatedShift.y;


        int verts = Segments[index].Verts;

        Segments[index].SegmentPoints.Clear();

        float circumference = 0;


        if (lastType != SegmentType.None)
        {
            CurveSegment lastSegment = Segments[index - 1];

            if (lastType == SegmentType.Polar || lastType == SegmentType.PolarConstLength || lastType == SegmentType.SelfLeveling)
            {                
                shiftY = lastSegment.PolarShift.y;
                shiftX = (index == 0) ? 0 : lastSegment.PolarShift.x;
            }
            else if (lastType == SegmentType.Euclidean)
            {
                if (lastSegment.Isolate)
                {
                    shiftY = 0;
                }

                shiftY = Vector2.Distance(startpos, Vector3.zero);
                shiftX = -Vector2.SignedAngle(Vector2.down, lastSegment.SegmentPoints[^1]) + 180;
                shiftX /= 180;
                shiftX *= Mathf.PI;
            }
                
        }

        circumference = ((shiftY + radius) * 2 * Mathf.PI);



        float segmentArcLength = 0;
        float shiftedDimentions = (dims.x - isolatedShift.x);



        if (currentSegment.Type == SegmentType.PolarConstLength)
        {
            segmentArcLength = shiftedDimentions / circumference;

        }
        else
        {
            segmentArcLength = shiftedDimentions * Mathf.PI / 180;

        }

    
        if (currentSegment.ReAssignVerts)
        {
            float totalRadius = Mathf.Abs(shiftY + radius);
            float squrtRadius = Mathf.Pow(2f,(-75f / (totalRadius + 10f) )) * (50f / 360f) * VertDensity;
            if (currentSegment.Type == SegmentType.Polar || currentSegment.Type == SegmentType.SelfLeveling)
            {
                currentSegment = CalculateVerts(currentSegment, shiftedDimentions * squrtRadius);
            }
            else
            {
                float angle = shiftedDimentions * ((180f / (2f * Mathf.Pow(Mathf.PI, 2))) / (radius + shiftY));
                currentSegment = CalculateVerts(currentSegment, angle * squrtRadius);

            }
        }
        else
        {
            if (verts < 2)
            {
                verts = 2;
            }
        }





        segmentArcLength /= (verts - 1);



        for (int i = 0; i < verts; i++)
        {
            float arcLength = i * segmentArcLength + shiftX;
            float radialStep = i * ((radius - currentSegment.InitHeight) / (verts - 1)) + shiftY + currentSegment.InitHeight;
            Vector3 pointPosition = Vector3.zero;


            if (currentSegment.Sloped)
            {
                pointPosition = new Vector2(Mathf.Sin(arcLength), Mathf.Cos(arcLength)) * (radialStep);

            }
            else
            {
                pointPosition = new Vector2(Mathf.Sin(arcLength), Mathf.Cos(arcLength)) * (radius + shiftY);
            }

            if (i != 0 || (startpos - (Vector2)pointPosition).sqrMagnitude > 0.5)
            {
                currentSegment.SegmentPoints.Add(pointPosition);
            }

        }

        currentSegment.PolarShift.x = segmentArcLength * (verts - 1) + shiftX;
        currentSegment.PolarShift.y = radius + shiftY;
        Points.AddRange(currentSegment.SegmentPoints);

        // Handling Isolation Logic

        if (!currentSegment.Isolate)
        {
            // Assigning Offset from Isolation

            if (currentSegment.WasIsolated)
            {
                if (index < Segments.Count - 1)
                {
                    CurveSegment nextSegment = Segments[index + 1];

                    if (nextSegment.Isolate)
                    {
                        nextSegment.OldDimentions = nextSegment.OldDimentions - nextSegment.IsolatedShift;

                    }
                    nextSegment.SetDimentions(nextSegment.Dimentions - nextSegment.IsolatedShift);
                    nextSegment.IsolatedShift = Vector2.zero;

                    Segments[index + 1] = nextSegment;
                }
            }


            currentSegment.WasIsolated = false;
            currentSegment.OldPosition = currentSegment.SegmentPoints[^1];
            currentSegment.OldDimentions = currentSegment.Dimentions;
        }
        else
        {
            currentSegment.WasIsolated = true;

            if (index < Segments.Count - 1)
            {
                CurveSegment nextSegment = Segments[index + 1];

                float isloatedY = currentSegment.Dimentions.y - currentSegment.OldDimentions.y;
                float nextRadius = currentSegment.Dimentions.y + shiftY + nextSegment.Dimentions.y - isloatedY;


                if (currentSegment.Type == SegmentType.PolarConstLength)
                {

                    if (nextSegment.Type == SegmentType.Euclidean)
                    {
                        // constant -> Euclid
                     
                        nextSegment.IsolatedShift = currentSegment.SegmentPoints[^1] - currentSegment.OldPosition;

                    }
                    else if (nextSegment.Type == SegmentType.Polar || nextSegment.Type == SegmentType.SelfLeveling)
                    {
                        //constant -> Polar

                        float oldRadius = currentSegment.OldDimentions.y - isolatedShift.y;
                        float oldcircumference = ((oldRadius + shiftY) * 2 * Mathf.PI);
                        circumference = ((radius + shiftY) * 2 * Mathf.PI);
                        float oldlength = (currentSegment.Dimentions.x - isolatedShift.x) / oldcircumference;
                        float length = (currentSegment.Dimentions.x - isolatedShift.x) / circumference;
                        float ShiftFromHeightChange = (oldlength * Mathf.Rad2Deg) - (length * Mathf.Rad2Deg);
                        ShiftFromHeightChange *= currentSegment.OldDimentions.x / dims.x;

                        nextSegment.IsolatedShift.x = (currentSegment.Dimentions.x - currentSegment.OldDimentions.x) * ((180f / (2f * Mathf.Pow(Mathf.PI, 2))) / (radius + shiftY)) - ShiftFromHeightChange;
                        nextSegment.IsolatedShift.y = (currentSegment.Dimentions.y - currentSegment.OldDimentions.y);
                        // Debug.Log((180f / (2f * Mathf.Pow(Mathf.PI, 2))) + " , " + (radius + shiftY));
                    }
                    else if (nextSegment.Type == SegmentType.PolarConstLength)
                    {
                        // constant -> constant
                        float oldRadius = currentSegment.OldDimentions.y - isolatedShift.y;
                        float oldcircumference = ((oldRadius + +shiftY) * 2 * Mathf.PI);
                        circumference = ((radius + shiftY) * 2 * Mathf.PI);
                        float oldlength = (currentSegment.Dimentions.x - isolatedShift.x) / oldcircumference;
                        float length = (currentSegment.Dimentions.x - isolatedShift.x) / circumference;
                        float ShiftFromHeightChange = (oldlength * Mathf.Rad2Deg) - (length * Mathf.Rad2Deg);
                        ShiftFromHeightChange *= (currentSegment.OldDimentions.x / (dims.x));

                        float oldNextRadius = currentSegment.OldDimentions.y + shiftY + nextSegment.Dimentions.y - isloatedY;

                        nextRadius = currentSegment.Dimentions.y + shiftY + nextSegment.Dimentions.y - isloatedY;
                        float ratioRadNDimy = (currentSegment.Dimentions.y / (currentSegment.Dimentions.y + shiftY));

                        float var1 = ((currentSegment.Dimentions.x - currentSegment.OldDimentions.x) * nextRadius / (currentSegment.Dimentions.y)) * ratioRadNDimy - (ShiftFromHeightChange * (nextRadius / (180f / (2f * Mathf.Pow(Mathf.PI, 2)))));
                        nextSegment.IsolatedShift.x = var1 + shiftX;
                        nextSegment.IsolatedShift.y = (isloatedY);


                    }
                }
                else if (currentSegment.Type == SegmentType.Polar)
                {


                    if (nextSegment.Type == SegmentType.Euclidean)
                    {
                        //polar -> euclidean
                        nextSegment.IsolatedShift = currentSegment.SegmentPoints[^1] - currentSegment.OldPosition;

                    }
                    else if (nextSegment.Type == SegmentType.Polar || nextSegment.Type == SegmentType.SelfLeveling)
                    {
                        nextSegment.IsolatedShift = currentSegment.Dimentions - currentSegment.OldDimentions;
                        // polar -> polar
                        //  Debug.Log("PolarPolar");
                    }
                    else if (nextSegment.Type ==  SegmentType.PolarConstLength)
                    {
                        // polar -> constant

                        nextSegment.IsolatedShift.x = (currentSegment.Dimentions.x - currentSegment.OldDimentions.x) * nextRadius / (180f / (2f * Mathf.Pow(Mathf.PI, 2)));
                        nextSegment.IsolatedShift.y = (isloatedY);
                    }
                }

                Segments[index + 1] = nextSegment;

            }
        }

        Segments[index] = currentSegment;
    }
    void GenerateEuclidean(int index)
    {
        CurveSegment currentSegment = Segments[index];

       

        Vector3 startPos = GetStartingPosition(index);

        currentSegment.IsolatedShift = (index == 0 || !Segments[index - 1].Isolate) ? Vector2.zero : currentSegment.IsolatedShift;
        Vector2 isolatedShift = (index == 0) ? Vector2.zero : currentSegment.IsolatedShift;

        Vector3 shiftedPos = (Vector2)startPos - isolatedShift;



        currentSegment.SegmentPoints.Clear();
        Vector3[] euclideanPoints = new Vector3[2];


        euclideanPoints[1] = Logic.RoundSnap(currentSegment.Dimentions + (Vector2)shiftedPos,SnapSpacing);

        if (!currentSegment.Isolate)
        {
            if (currentSegment.WasIsolated)
            {
                if (index < Segments.Count - 1)
                {
                    CurveSegment nextSegment = Segments[index + 1];

                    if (nextSegment.Isolate)
                    {
                        nextSegment.OldDimentions = nextSegment.OldDimentions - nextSegment.IsolatedShift;
                    }

                    nextSegment.SetDimentions(nextSegment.Dimentions - nextSegment.IsolatedShift);
                    nextSegment.IsolatedShift = Vector2.zero;

                    Segments[index + 1] = nextSegment;
                }
            }

            currentSegment.WasIsolated = false;
            currentSegment.OldPosition = euclideanPoints[1];
            currentSegment.OldDimentions = currentSegment.Dimentions;
        }
        else
        {
            currentSegment.WasIsolated = true;

            if (index < Segments.Count - 1)
            {
                CurveSegment nextSegment = Segments[index + 1];

                if (nextSegment.Type == SegmentType.Polar || nextSegment.Type == SegmentType.PolarConstLength || nextSegment.Type == SegmentType.SelfLeveling)
                {
                    float newradius = Vector2.Distance(currentSegment.Dimentions + (Vector2)shiftedPos, Vector2.zero);
                    float oldradius = Vector2.Distance(currentSegment.OldDimentions + (Vector2)shiftedPos, Vector2.zero);

                    float oldx = -Vector2.SignedAngle(Vector2.down, (Vector2)shiftedPos + currentSegment.OldDimentions);
                    float newx = -Vector2.SignedAngle(Vector2.down, (Vector2)shiftedPos + currentSegment.Dimentions);

                    Vector2 tempIsSh;

                    tempIsSh.x = newx - oldx;
                   
                    tempIsSh.y = newradius - oldradius;


                    if (oldx > 0 && newx < 0)
                    {
                        if (Mathf.Abs(oldx) > 90 && Mathf.Abs(newx) > 90)
                        {

                            tempIsSh.x += 360;

                        }
                    }

                    if (oldx < 0 && newx > 0)
                    {
                        if (Mathf.Abs(oldx) > 90 && Mathf.Abs(newx) > 90)
                        {
                            tempIsSh.x = 360 - (tempIsSh.x + 180);
                            tempIsSh.x *= -1;
                            tempIsSh.x -= 180;
                            tempIsSh.x %= 360;
                        }
                       
                    }

                    if (nextSegment.Type == SegmentType.PolarConstLength)
                    {
                        float nextRadius = (newradius + nextSegment.Dimentions.y) - tempIsSh.y;
                        tempIsSh.x *= (nextRadius / (180f / (2f * Mathf.Pow(Mathf.PI, 2))));
                    }

                           
                     nextSegment.IsolatedShift = tempIsSh;
                }
                else if (nextSegment.Type == SegmentType.Euclidean)
                {
                    nextSegment.IsolatedShift = currentSegment.Dimentions - currentSegment.OldDimentions;
                }


                Segments[index + 1] = nextSegment;


            }
        }



        if (currentSegment.Sloped)
        {
            euclideanPoints[0] = startPos + new Vector3(0, Logic.RoundSnap(currentSegment.InitHeight - currentSegment.IsolatedShift.y, SnapSpacing), 0);
        }
        else
        {
            euclideanPoints[0] = startPos + new Vector3(0, Logic.RoundSnap(currentSegment.Dimentions.y - currentSegment.IsolatedShift.y, SnapSpacing), 0);
            
        }

        currentSegment.SegmentPoints.AddRange(euclideanPoints);

        if (index == 0 || (shiftedPos - euclideanPoints[0]).sqrMagnitude > 0.5)
        {
            Points.Add(currentSegment.SegmentPoints[0]);
        }

        Points.Add(currentSegment.SegmentPoints[1]);

        Segments[index] = currentSegment;
    }
#if UNITY_EDITOR
    [ContextMenu("Spline")]
    public void GenerateSpriteShape()
    {
        
        if (ShapeController != null)
        {
            MySpline = ShapeController.spline;
        }
        else
        {
            ShapeController = GetComponent<SpriteShapeController>();
            MySpline = ShapeController.spline;


        }



        if (MySpline != null)
        {
            MySpline.Clear();

        }
        else
        {
            if (ShapeController != null)
            {
                MySpline = ShapeController.spline;
                MySpline.Clear();
            }
            else
            {
                MySpline = ShapeController.spline;

                return;
            }
        }


        if (AutoGenerateVerts)
        {
            PopulateVerts();
        }

        DisplaySegmentRefresh();

        Points.Clear();


      

        for (int i = 0; i < Segments.Count; i++)
        {

            GenerateSegment(Segments[i].Type, i);
 
        }        





        for (int i = 0; i < Points.Count; i++)
        {
            
            MySpline.InsertPointAt(i, Points[i] - transform.position + TargetSettings.GetTargetPos(transform));
            MySpline.SetTangentMode(i, _TangentMode);
            //MySpline.SetLeftTangent(i, Vector3.left);
           // MySpline.SetLeftTangent(i, Vector3.right);


        }

        DisplaySegmentRefresh();

        // Mark the controller as dirty so Unity knows it changed
        if (ShapeController != null)
        {
            EditorUtility.SetDirty(ShapeController);

            // VERY IMPORTANT: Record prefab instance changes
            PrefabUtility.RecordPrefabInstancePropertyModifications(ShapeController);
        }
 

    }
#endif

    [System.Serializable]
    public struct CurveSegment
    {
        
        public bool Isolate;
        public bool Sloped;

        public Vector2 PolarShift;
        [Range(2, 100)] public int Verts;

        [HideInInspector]
        public bool WasIsolated;
        public SegmentType Type;
        [HideInInspector]
        public SegmentType LastType;

        public float Length;
        public float Height;
        public Vector2 Dimentions => new Vector2(Length, Height);
        public List<Vector3> SegmentPoints;
        [HideInInspector]
        public Vector2 OldDimentions;
        public Vector2 IsolatedShift;
        public float InitHeight;
        [HideInInspector]
        public bool ReAssignVerts;
        [HideInInspector]
        public Vector3 OldPosition;
        public void SetDimentions(Vector2 dim)
        {
            Length = dim.x;
            Height = dim.y;
        }

        
        /*
        public CurveSegment() 
        {
            Dimentions = Vector2.zero;
            Verts = 2;
            type = SegmentType.Polar;
            Isolate = false;
            SegmentPoints = new Vector3[2];
            PolarShift = 0;
        }
        */
    }
    public void Clicked(Vector3 MousePosition)
    {

        MousePosition.z = 0;
        MousePosition.y *= -1;
        MousePosition.y += SceneView.currentDrawingSceneView.cameraViewport.height;

        

        // Debug.Log("Clicked");
        //Debug.Log(SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(MousePosition));
        //Debug.Log(MousePosition);
        float checkDistance;
        float minDistance = float.MaxValue;
        int Currenminindex = 0;
        for (int i = 0; i < Segments.Count; i++)
        {
            for (int j = 0; j < Segments[i].SegmentPoints.Count; j++)
            {
                Vector3 position = TargetSettings.GetTargetPos(transform);
                Vector3 pointPosition = Segments[i].SegmentPoints[j];



                Vector3 worldPointPosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(MousePosition);
                position.z = 0;
                pointPosition.z = 0;
                worldPointPosition.z = 0;



                checkDistance = Vector2.Distance(worldPointPosition, position + (Quaternion.AngleAxis(transform.rotation.eulerAngles.z, new Vector3(0, 0, 1)) * pointPosition));

                if (minDistance > checkDistance)
                {
               
                    Currenminindex = i;
                    minDistance = checkDistance;
                }
            }
        }
        CurrentSegment = Currenminindex;
       
    }

    CurveSegment CalculateVerts(CurveSegment segment, float length)
    {
        length = Mathf.Abs(length);
      //  Debug.Log(length + " , " + segment.Type);
        segment.Verts = Mathf.Clamp(Mathf.RoundToInt(length + 2f),2,100);
        if (!AutoGenerateVerts)
        {
        segment.ReAssignVerts = false;

        }
        return segment;

    }


    [ContextMenu("Populate Verts")]
    void PopulateVerts()
    {
        for (int i = 0; i < Segments.Count; i++)
        {
            CurveSegment segment = Segments[i];

            if (Segments[i].Type != SegmentType.Euclidean)
            {
                segment.ReAssignVerts = true;
                Segments[i] = segment;

            }
            else
            {
                segment.Verts = 2;
            }
        }
    }



}
#endif

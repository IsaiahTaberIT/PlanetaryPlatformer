
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;




public class AdvancedCurveGenerationScript : MonoBehaviour
{

    [SerializeField] private float _GizmoSize;
    [SerializeField] private bool DrawAny;
    [SerializeField] private bool DrawAll;
    [SerializeField] private bool _SubtractRotationRate;
    [SerializeField] private bool _SubtractRadius;
    [SerializeField] private bool _TrueLengthMode = false;
    [SerializeField] private float _MinimumRadius;
    [SerializeField] private float _InitialRadius;
    [SerializeField] private float _SlopeStartRadius;


    [Range(0,2)] [SerializeField] private int _TanMode;
    public int SegmentCount;
    [SerializeField] private int _SpriteIndex;
    [SerializeField] private Vector2Int _EdgeSpriteIndex;

    

    [HideInInspector]
    public int _LastSegment;

    public int _CurrentSegment;

    private SpriteShapeController ShapeController;
    private SpriteShape Shape;
    private Spline MySpline;
    [SerializeField] private bool _IsHorizontal = true;
    [SerializeField] private int _Points = 20;

    [SerializeField] private float _RotationRate = 10;
    [SerializeField] private float _Radius = 20;

    public SpriteShapeSegment[] Segments = new SpriteShapeSegment[1];
    [ExecuteInEditMode]

    public void CallValidatePlease()
    {
        Repair();
    }
       
     
    void Start()
    {
        ShapeController = GetComponent<SpriteShapeController>();
        MySpline = ShapeController.spline;
       
    }

    public void Repair()
    {
        
        Segments[0].IsFirst = true;

        ShapeController = GetComponent<SpriteShapeController>();
        MySpline = ShapeController.spline;

        if (_CurrentSegment < 0)
        {
            _CurrentSegment = 0;
        }


        if (_CurrentSegment >= SegmentCount)
        {
            _CurrentSegment = SegmentCount - 1;
        }


        if (_CurrentSegment != _LastSegment)
        {
            _IsHorizontal = Segments[_CurrentSegment].Horizontal;
            _Points = Segments[_CurrentSegment].PointCount;
            _Radius = Segments[_CurrentSegment].PolarCords.y;
            _RotationRate = Segments[_CurrentSegment].PolarCords.x * 60;
            _SubtractRotationRate = Segments[_CurrentSegment].SubtractRotationRate;
            _SubtractRadius = Segments[_CurrentSegment].SubtractRadius;
            _SpriteIndex = Segments[_CurrentSegment].SpriteIndex;
            _EdgeSpriteIndex = Segments[_CurrentSegment].EdgeSpriteIndex;
            _SlopeStartRadius = Segments[_CurrentSegment].SlopeStartRadius;

        }
        else
        {
            Segments[_CurrentSegment].Horizontal = _IsHorizontal;
            Segments[_CurrentSegment].PointCount = _Points;
            Segments[_CurrentSegment].PolarCords.y = _Radius;
            Segments[_CurrentSegment].PolarCords.x = _RotationRate / 60;
            Segments[_CurrentSegment].SubtractRotationRate = _SubtractRotationRate;
            Segments[_CurrentSegment].SubtractRadius = _SubtractRadius;
            Segments[_CurrentSegment].SpriteIndex = _SpriteIndex;
            Segments[_CurrentSegment].EdgeSpriteIndex = _EdgeSpriteIndex;
            Segments[_CurrentSegment].SlopeStartRadius = _SlopeStartRadius;

        }

        for (int i = 0; i < Segments.Length; i++)
        {
            if (Segments[i].myTransform == null)
            {
                Segments[i].myTransform = transform;

            }

            Segments[i].MinimumRadius = _MinimumRadius;
            Segments[i].TrueLengthMode = _TrueLengthMode;
            Segments[i].InitialRadius = _InitialRadius;

            if (i != 0)
            {
              
                

                Segments[i].IsFirst = false;

              

                Segments[i].PolarCordsModifier = Segments[i - 1].PolarCordsModifier;

                Segments[i].PolarCordsOffsetInput = Segments[i - 1].PolarCordsOffsetOutput;
                
            }
            else
            {
                
                Segments[i].PolarCordsOffsetInput = Vector2.zero;
            }

            
                Segments[i].MyMethod();
            
        }

        _LastSegment = _CurrentSegment;

        
            RecalculateSpline();
        
    }







    [System.Serializable]
    public class SpriteShapeSegment
    {
        private float _ScaleOffset;
        public Transform myTransform;
        public float InitialRadius;
        public float SlopeStartRadius;
        public bool IsFirst = false;
        public int SpriteIndex;
        public Vector2Int EdgeSpriteIndex;
        public float MinimumRadius;
        public Vector2 PolarCordsOffsetOutput;
        public Vector2 PolarCordsOffsetInput;
        public Vector2 InitialPolarCords = new Vector2(10,10);
        public Vector2 PolarCordsModifier;
        public Vector2 ModifiedPolarCords;
        public bool Horizontal = true;
        public Vector3 StartingPosition;
        public int PointCount;
        public Vector3[] SegmentPoints = new Vector3[0];
        public bool SubtractRotationRate;
        public bool SubtractRadius;
        public bool TrueLengthMode;
        public Vector2 PolarCords;

        public SpriteShapeSegment(float initialRadius,float minimumRadius, Vector2 startingCords)
        {
            

            PointCount = 5;
            MinimumRadius = minimumRadius;
            InitialRadius = initialRadius;
            PolarCords = startingCords;
        }
            


          

        public void MyMethod()
        {
          if (myTransform != null)
          {
                _ScaleOffset = (Mathf.Sqrt(myTransform.lossyScale.x));
                //_ScaleOffset = 1;
          }
          else
          {
                _ScaleOffset = 1;
          }
            

            PolarCords.y = IsFirst ? MinimumRadius / _ScaleOffset : PolarCords.y;
            float half = IsFirst ? 2 : 1;
            ModifiedPolarCords.x = (PolarCords.x) - PolarCordsModifier.x;
            ModifiedPolarCords.y = (PolarCords.y/ _ScaleOffset) - PolarCordsModifier.y;

            if (SubtractRotationRate == false)
            {
                InitialPolarCords.x = PolarCords.x;
            }


            if (SubtractRadius == false)
            {
                InitialPolarCords.y = PolarCords.y;
            }

            float consistantSizeModifier = (TrueLengthMode) ? ((ModifiedPolarCords.y + PolarCordsOffsetInput.y * 2) / 2) / (ModifiedPolarCords.y + InitialRadius + (PolarCordsOffsetInput.y * 2) - MinimumRadius) * 2: 1;
            //consistantSizeModifier = IsFirst ? MinimumRadius / InitialRadius : consistantSizeModifier;

            PolarCordsModifier.x = 0;
            PolarCordsModifier.y = 0;

            if (Horizontal)
            {

                SegmentPoints = new Vector3[PointCount];



                for (int i = 0; i < SegmentPoints.Length; i++)
                {
                    Vector3 pointPosition = new Vector3(Mathf.Sin(PolarCordsOffsetInput.x + i * ((ModifiedPolarCords.x) / (PointCount - 1)) / consistantSizeModifier), Mathf.Cos(PolarCordsOffsetInput.x + i * ((ModifiedPolarCords.x) / (PointCount - 1)) / consistantSizeModifier), 0) * (ModifiedPolarCords.y + PolarCordsOffsetInput.y * 2) / 2;
                    SegmentPoints[i] = pointPosition;

                }

                if (SubtractRotationRate)
                {
                    PolarCordsModifier.x = (PolarCords.x - InitialPolarCords.x) / consistantSizeModifier;
                }

                if (SubtractRadius)
                {
                    PolarCordsModifier.y = PolarCords.y - InitialPolarCords.y;
                }

                PolarCordsOffsetOutput = PolarCordsOffsetInput + new Vector2(((PointCount - 1) * (ModifiedPolarCords.x) / (PointCount - 1)) / consistantSizeModifier, ModifiedPolarCords.y / 2);

            }
            else
            {

                int offset = 1;

                if (Mathf.Abs(SlopeStartRadius) > 0.1f)
                {
                    offset = 0;
                }
               
                SegmentPoints = new Vector3[PointCount - offset];


                for (int i = 0; i < SegmentPoints.Length; i++)
                {
                    Vector3 pointPosition = new Vector3(Mathf.Sin(PolarCordsOffsetInput.x + (i + offset) * ((ModifiedPolarCords.x) / (PointCount - 1)) / consistantSizeModifier), Mathf.Cos(PolarCordsOffsetInput.x + (i + offset) * ((ModifiedPolarCords.x) / (PointCount - 1)) / consistantSizeModifier), 0) * ((i + offset) * ((ModifiedPolarCords.y ) / (PointCount - 1)) + PolarCordsOffsetInput.y * 2 + SlopeStartRadius) / 2;
                    SegmentPoints[i] = pointPosition;

                }

                if (SubtractRotationRate)
                {
                    PolarCordsModifier.x = (PolarCords.x - InitialPolarCords.x) / consistantSizeModifier;
                }

                if (SubtractRadius)
                {
                    PolarCordsModifier.y = PolarCords.y - InitialPolarCords.y;
                }

                PolarCordsOffsetOutput = PolarCordsOffsetInput + new Vector2(((PointCount - 1) * (ModifiedPolarCords.x) / (PointCount - 1)) / consistantSizeModifier, ModifiedPolarCords.y / 2);

            }

        }

        public void DrawSegmentGizmos(Transform transform, float size)
        {
            
                    Vector3 pos = transform.position;
            
            Gizmos.color = Color.red;

            if (size == 0)
            {
                size = 1;
            }

            float gizmoSize = 0;

#if UNITY_EDITOR


            if (SceneView.currentDrawingSceneView != null)
            {
                gizmoSize = SceneView.currentDrawingSceneView.size / (100 / size);

            }
#endif





            foreach (Vector3 point in SegmentPoints)
            {
                Gizmos.DrawSphere(pos + (Quaternion.AngleAxis(transform.rotation.eulerAngles.z, new Vector3(0, 0, 1)) * point), gizmoSize);
            }
        }
        
    }

    [ContextMenu("Delete Current Segment")]

    void DeleteCurrentSegment()
    {
        if (_CurrentSegment >= 0)
        {
            for (int i = _CurrentSegment; i < Segments.Length - 2; i++)
            {
                Segments[i] = Segments[i + 1];
            }

            _LastSegment = _CurrentSegment;

            DeleteLastSegment();
            Repair();
        }
    }



    [ContextMenu("Insert Segment")]

    void InsertSegment()
    {
        SegmentCount++;

        SpriteShapeSegment[] tempsegments = Segments;

        Segments = new SpriteShapeSegment[SegmentCount];

        int index = 0;
        for (int i = 0; i < Segments.Length; i++)
        {
           
            if (_CurrentSegment == index)
            {
                Segments[index] = new SpriteShapeSegment(_InitialRadius,_MinimumRadius  ,new Vector2(_RotationRate/60,_Radius));
               // Segments[index].SubtractRadius = true;
               // Segments[index].SubtractRotationRate = true;
               // Segments[index].InitialPolarCords = Vector2.zero;
                _LastSegment = -1;
                i++;
            }

            Segments[i] = tempsegments[index];
            index++;
        }
        Repair();
    }

    [ContextMenu("Apply Rotation Rate Modification")]
    void ApplyRotationRateModification()
    {
        if (_CurrentSegment < Segments.Length - 1 && Segments[_CurrentSegment].SubtractRotationRate)
        {
            Segments[_CurrentSegment + 1].PolarCords.x = Segments[_CurrentSegment + 1].PolarCords.x - Segments[_CurrentSegment].PolarCordsModifier.x;

            Segments[_CurrentSegment].PolarCordsModifier.x = 0;
            Segments[_CurrentSegment].SubtractRotationRate = false;
            _SubtractRotationRate = false;
            Repair();             
        }
    }

    [ContextMenu("Apply Radius Modification")]
    void ApplyRadiusModification()
    {
        if (_CurrentSegment < Segments.Length - 1 && Segments[_CurrentSegment].SubtractRadius)
        {
            Segments[_CurrentSegment + 1].PolarCords.y = Segments[_CurrentSegment + 1].PolarCords.y - Segments[_CurrentSegment].PolarCordsModifier.y;

            Segments[_CurrentSegment].PolarCordsModifier.y = 0;
            Segments[_CurrentSegment].SubtractRadius = false;
            _SubtractRadius = false;
            Repair();
        }
    }

    [ContextMenu("Delete Segments")]
    void DeleteAllSegments()
    {
        SegmentCount = 1;
        _CurrentSegment = 0;
        Segments = new SpriteShapeSegment[SegmentCount];
        Segments[0] = new SpriteShapeSegment(_InitialRadius, _MinimumRadius, new Vector2(_RotationRate, _Radius));
        Repair();

    }

    public void Clicked(Vector3 MousePosition)
    {
#if UNITY_EDITOR
        MousePosition.z = 0;
        MousePosition.y *= -1;
        MousePosition.y += SceneView.currentDrawingSceneView.cameraViewport.height;

        Repair();

       // Debug.Log("Clicked");
        //Debug.Log(SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(MousePosition));
        //Debug.Log(MousePosition);
        float checkDistance;
        float minDistance = float.MaxValue;
        int Currenminindex = 0;
        for (int i = 0; i < Segments.Length; i++)
        {
            for (int j = 0; j < Segments[i].SegmentPoints.Length; j++)
            {
                Vector3 position = transform.position;
                Vector3 pointPosition = Segments[i].SegmentPoints[j];



                Vector3 worldPointPosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(MousePosition);
                position.z = 0;
                pointPosition.z = 0;
                worldPointPosition.z = 0;



                checkDistance = Vector2.Distance(worldPointPosition, position + (Quaternion.AngleAxis(transform.rotation.eulerAngles.z, new Vector3(0, 0, 1)) * pointPosition));

                if (minDistance > checkDistance)
                {
                  //  Debug.Log("Clicked2");
                  //  Debug.Log(checkDistance);
                 //   Debug.Log(position + pointPosition);
                 //   Debug.Log(worldPointPosition);
                    Currenminindex = i;
                    minDistance = checkDistance;
                }
            }
        }
        _CurrentSegment = Currenminindex;
        Repair();
#endif
    }


    [ContextMenu("Add Segment")]

    void AddSegment()
    {
        SegmentCount++;

        SpriteShapeSegment[] tempsegments = Segments;

        Segments = new SpriteShapeSegment[SegmentCount];

        for (int i = 0; i < tempsegments.Length; i++)
        {
            Segments[i] = tempsegments[i];
        }

        Segments[Segments.Length -1 ] = new SpriteShapeSegment(_InitialRadius, _MinimumRadius, new Vector2(_RotationRate, _Radius));

        _CurrentSegment = SegmentCount - 1;

        Segments[Segments.Length - 1].PolarCords.x = Segments[Segments.Length - 2].PolarCords.x;
        Segments[Segments.Length - 1].PointCount = Segments[Segments.Length - 2].PointCount;
        Repair();


    }


    [ContextMenu("Delete Last Segment")]
    void DeleteLastSegment()
    {
        SegmentCount--;

        SpriteShapeSegment[] tempsegments = Segments;


        Segments = new SpriteShapeSegment[SegmentCount];

        for (int i = 0; i < tempsegments.Length - 1; i++)
        {
            Segments[i] = tempsegments[i];
        }

        _CurrentSegment = _CurrentSegment <= SegmentCount ? _CurrentSegment - 1 : _CurrentSegment;
        _LastSegment = _CurrentSegment + 1;

        // Segments[Segments.Length - 1] = new SpriteShapeSegment(_InitialRadius, _MinimumRadius, new Vector2(_RotationRate, _Radius));
        Repair();
        
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {

        }
        else
        {
            if (DrawAny)
            {
                if (DrawAll)
                {
                    foreach (var Segment in Segments)
                    {
                        Segment.DrawSegmentGizmos(transform, _GizmoSize);
                    }
                }
                else
                {

                    Segments[_CurrentSegment].DrawSegmentGizmos(transform, _GizmoSize);

                }
            }

        }
    }

    
    void RecalculateSpline()
    {
        MySpline.Clear();
        int count = 0;
        for (int i = 0; i < Segments.Length; i++)
        {
            for (int j = 0; j < Segments[i].SegmentPoints.Length; j++)
            {
                if (i+j == 1)
                {
                    //MySpline.InsertPointAt(count, Segments[0].SegmentPoints[0]);
                    //count++;
                }



                MySpline.InsertPointAt(count, Segments[i].SegmentPoints[j]);
                MySpline.SetSpriteIndex(count, Segments[i].SpriteIndex);
                


                if(Segments[i].SegmentPoints.Length - 1 == j)
                {
                    MySpline.SetSpriteIndex(count, Segments[i].EdgeSpriteIndex.y);
                    MySpline.SetCorner(count, true);
                }
                else if (j == 0)
                {
                    MySpline.SetSpriteIndex(count, Segments[i].EdgeSpriteIndex.x);
                    MySpline.SetCorner(count, true);
                }


                Vector3 rightTan = (Segments[i].SegmentPoints.Length - 1 < j) ? Segments[i].SegmentPoints[j - 1] - Segments[i].SegmentPoints[j] : (Segments.Length - 1 < i) ? Segments[i + 1].SegmentPoints[0] - Segments[i].SegmentPoints[j] : Vector3.zero;
                Vector3 leftTan = (0 < j) ? Segments[i].SegmentPoints[j - 1] - Segments[i].SegmentPoints[j] : (0 < i) ? Segments[i -1].SegmentPoints[Segments[i - 1].SegmentPoints.Length-1] - Segments[i].SegmentPoints[j]: Segments[Segments.Length-1].SegmentPoints[Segments[Segments.Length -1 ].SegmentPoints.Length - 1] - Segments[i].SegmentPoints[j];
                
                
                //Vector3 rightTan = (Segments[i].SegmentPoints.Length - 1 < j) ? Segments[i].SegmentPoints[j - 1] - Segments[i].SegmentPoints[j] : (Segments.Length - 1 < i) ? Segments[i + 1].SegmentPoints[0] - Segments[i].SegmentPoints[j] : Vector3.zero;

                //Vector3 leftTan = (0 < j) ? Segments[i].SegmentPoints[j - 1] - Segments[i].SegmentPoints[j] : (0 < i) ? Segments[i - 1].SegmentPoints[Segments[i - 1].SegmentPoints.Length - 1] - Segments[i].SegmentPoints[j] : Segments[Segments.Length - 1].SegmentPoints[Segments[Segments.Length - 1].SegmentPoints.Length - 1] - Segments[i].SegmentPoints[j];


                Vector3 tan = (rightTan.normalized + leftTan.normalized).normalized;
                tan = tan / 5;


                MySpline.SetTangentMode(count, (ShapeTangentMode)(_TanMode));

                MySpline.SetRightTangent(count , -tan);
                MySpline.SetLeftTangent(count, tan);
                count++;
            }
        }

        ShapeController.enabled = false;
        ShapeController.enabled = true;

    }

   
}





using UnityEngine;

using UnityEngine.Rendering.Universal;
using NUnit.Framework.Internal;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;

#endif

[ExecuteInEditMode]
public class LightsBakeable2D : MonoBehaviour
{
   
    public Vector2[] UVS = new Vector2[0];
    public bool ReplaceTexture; 
    public bool run;
    public bool UseCrt = false;
    public float Trim = 0.02f;
    public bool RealtimeOverride;
    public bool CastShadows = true;
    public CustomRenderTexture MyRenderTexture;
    [Range(1,360f)]public float MaxAngle = 360f;
    private Vector3 _Lastpos;
    public static bool UseBaked;

    public int LayerId;
    public int RenderQueue = 1000;

    public string Layer;
    public int SortingOrder = 1000;
    public float Intensity;
    [Range(0,1f)]public float FalloffPower;
    public int Points;
    public float Radius;
    public Color LightColor;
    public Mesh MyMesh;
    public Vector3[] Verts;
    public int[] Tris;
    public LayerMask Layers;
    public MeshRenderer MyMeshRenderer;
    public Shader StandardCustomLighting;
    public Material StandardCLMaterial;

    [Space]
    [Header("CRT Mode:")]
    [Space]

    public Shader CRTGenerationShader;
    public Material CRTGenerationMaterial;
    public Shader CRTDisplayShader;
    public Material CRTDisplayerMaterial;

    

    public Light2D MyLight;
    public delegate void ToggleLightState();
    public static event ToggleLightState Toggle;

    [ContextMenu("try update")]
    public void FixTexture()
    {
        if (UseCrt)
        {
            if (MyRenderTexture == null)
            {
                GenerateMeshCrt(false);
            }
            else
            {
                MyRenderTexture.Update();
            }

            MyRenderTexture.material = CRTGenerationMaterial;
            CRTDisplayerMaterial.SetTexture("_Tex", MyRenderTexture);
        }
       
    }
    private void Start()
    {
       // Debug.Log("Start");
        FixTexture();

    }
    public static void InvokeLightToggle()
    {
        Toggle.Invoke();
    }
    private void OnEnable()
    {
        if (MyRenderTexture != null)
        {
            MyRenderTexture.Update();
        }
        Toggle += ToggleLights;
    }
    private void OnDisable()
    {
        Toggle -= ToggleLights;

    }

    void ToggleLights()
    {
        if (TryGetComponent<Light2D>(out MyLight))
        {
            if (UseBaked)
            {
                MyLight.enabled = true;
                MyMeshRenderer.enabled = false;
            }
            else
            {
                MyLight.enabled = false;
                MyMeshRenderer.enabled = true;
            }
        }
    }

    void RefreshMaterials()
    {


#if UNITY_EDITOR

        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
          //  DestroyImmediate(material);
          //  material = new(shader);
          if (CRTGenerationMaterial != null)
            {
                EditorUtility.SetDirty(CRTGenerationMaterial);
                GenerateMesh(false);
            }
          
        }

#endif

    }

    private void Update()
    {
        if (RealtimeOverride)
        {
            GenerateMesh(false);

        }
    }

    [ContextMenu("Tex")]
    private void CreateTexture()
    {

        DestroyImmediate(MyRenderTexture);
        MyRenderTexture = new CustomRenderTexture(512, 512, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
        MyRenderTexture.material = CRTGenerationMaterial;
        if (MyRenderTexture.material != null)
        {
          // Debug.Log(MyRenderTexture.material.name);
        }
       // Debug.Log(1);
        MyRenderTexture.initializationMode = CustomRenderTextureUpdateMode.OnLoad;
        MyRenderTexture.updateMode = CustomRenderTextureUpdateMode.OnLoad;
        MyRenderTexture.name = "MyRenderTexture";
        MyRenderTexture.Update();
        //EditorUtility.SetDirty(MyRenderTexture);
       // PrefabUtility.RecordPrefabInstancePropertyModifications(MyRenderTexture);
    }

    public void GenerateMeshCrt(bool drawGizmos)
    {
        if (MyRenderTexture == null)
        {
            CreateTexture();
        }
       // Debug.Log("run");
        if (_Lastpos != transform.position && !RealtimeOverride)
        {
            _Lastpos = transform.position;
            RefreshMaterials();
        }

        if (CRTGenerationMaterial == null)
        {
            CRTGenerationMaterial = new(CRTGenerationShader);
        }

        CRTGenerationMaterial.SetFloat("_Falloff", Radius);
        CRTGenerationMaterial.SetFloat("_Pow", FalloffPower);
        CRTGenerationMaterial.SetFloat("_Intensity", Intensity);
        CRTGenerationMaterial.SetColor("_Color", LightColor);
        CRTGenerationMaterial.renderQueue = RenderQueue;

        MyMeshRenderer = GetComponent<MeshRenderer>();
        Layer = SortingLayer.IDToName(LayerId);
        MyMeshRenderer.sortingLayerName = Layer;
        MyMeshRenderer.rendererPriority = RenderQueue;
        MyMeshRenderer.sortingOrder = SortingOrder;

        if (CRTDisplayerMaterial == null)
        {
            CRTDisplayerMaterial = new(CRTDisplayShader);
            CRTDisplayerMaterial.name = "CRTMaterial";
        }

        if (ReplaceTexture)
        {
            CRTDisplayerMaterial.SetTexture("_Tex", MyRenderTexture);
           // Debug.Log(MyRenderTexture == null);
        }


        MyMeshRenderer.material = CRTDisplayerMaterial;

        if (!RealtimeOverride)
        {
            DestroyImmediate(MyMesh);
            //(MyMesh);
            MyMesh = new Mesh();
            MyMesh.name = "InstanceMesh";
        }
        else
        {
            if (MyMesh == null)
            {
                MyMesh = new Mesh();
                MyMesh.name = "InstanceMesh";
            }
            else
            {
                MyMesh.Clear();
            }
        }


        if (Points < 2)
        {
            Points = 2;
        }

     

        Verts = new Vector3[Points + 1];
        Verts[0] = Vector3.zero;
        Tris = new int[(Points + 1) * 3 ];


        float segmentArcLength = ((MaxAngle * Mathf.Deg2Rad) / (Points - 1));

        float startignrot = transform.eulerAngles.z * Mathf.Deg2Rad;
        startignrot = 0;
        for (int i = 0; i < Points; i++)
        {


            float arcLength = i * segmentArcLength;

            Vector3 direction = transform.TransformDirection((new Vector2(Mathf.Sin(arcLength + startignrot), Mathf.Cos(arcLength + startignrot)).normalized));
            Vector3 pointPosition = Vector3.zero;

            float transformScaledDistance = Vector2.Scale(direction * Radius, transform.lossyScale).magnitude;


            if (CastShadows)
            {
                RaycastHit2D ray = Logic.RaycastByType<ShadowCaster2D>(transform.position, direction, transformScaledDistance);
                //RaycastHit2D ray = Physics2D.Raycast(transform.position, direction, transformScaledDistance, Layers);

                direction = transform.InverseTransformDirection(direction);



                if (ray)
                {
                    pointPosition = (ray.distance - Trim) * direction;
                }
                else
                {

                    pointPosition = direction * (transformScaledDistance - Trim);


                }


                pointPosition = Vector2.Scale(pointPosition, Logic.Reciprocal(transform.lossyScale));

            }
            else
            {
                direction = transform.InverseTransformDirection(direction);

                pointPosition = direction * (Radius - Trim);
            }







            if (drawGizmos)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawSphere(transform.position + transform.TransformDirection(pointPosition), 1);
            }


            Verts[i + 1] = pointPosition;
        }

        int tricount = 0;

        for (int i = 0; i < Points + 1; i++)
        {
            Tris[tricount] = 0;
            Tris[tricount + 1] = i;

            if (i != Points - 1 + 1)
            {
                Tris[tricount + 2] = i + 1;
            }
            else
            {
                //   Debug.Log("end");

                Tris[tricount + 2] = 1;
            }
            tricount += 3;
        }


      

        MyMesh.vertices = Verts;
        MyMesh.triangles = Tris;

        MyMesh.RecalculateBounds();
        MyMesh.RecalculateNormals();
        MyMesh.RecalculateTangents();

        Vector3[] vertices = MyMesh.vertices;
        Bounds bounds = MyMesh.bounds;

        UVS = new Vector2[vertices.Length];
        float scaledRad = Radius * 2 * transform.lossyScale.magnitude; ;
        for (int i = 0; i < vertices.Length; i++)
        {
            //Debug.Log("Uvs");
            UVS[i] = new Vector2(vertices[i].x / scaledRad + 0.5f, vertices[i].y / scaledRad + 0.5f);
        }


        MyMesh.uv = UVS;
        MyMesh.RecalculateNormals();
        MyMesh.RecalculateTangents();
        MyMesh.RecalculateBounds();



        GetComponent<MeshFilter>().mesh = MyMesh;
    }






    public void GenerateMesh(bool drawGizmos)
    {
       // Debug.Log("run");
        if (_Lastpos != transform.position && !RealtimeOverride)
        {
            _Lastpos = transform.position;
            RefreshMaterials();
        }

        if (StandardCLMaterial == null)
        {
            StandardCLMaterial = new(StandardCustomLighting);
        }

        StandardCLMaterial.SetFloat("_Falloff", Radius);
        StandardCLMaterial.SetFloat("_Pow", FalloffPower);
        StandardCLMaterial.SetFloat("_Intensity", Intensity);
        StandardCLMaterial.SetColor("_Color", LightColor);
        // material.renderQueue = RenderQueue;

        StandardCLMaterial.renderQueue = RenderQueue;

        MyMeshRenderer = GetComponent<MeshRenderer>();
        Layer = SortingLayer.IDToName(LayerId);
        MyMeshRenderer.sortingLayerName = Layer;
        MyMeshRenderer.rendererPriority = RenderQueue;  
        MyMeshRenderer.sortingOrder = SortingOrder;


        MyMeshRenderer.material = StandardCLMaterial;

        if (!RealtimeOverride)
        {
            DestroyImmediate(MyMesh);
            //(MyMesh);
            MyMesh = new Mesh();

        }
        else
        {
            if (MyMesh == null)
            {
                MyMesh = new Mesh();

            }
            else
            {
                MyMesh.Clear();
            }
        }
                

        if (Points < 2)
        {
            Points = 2;
        }

     

        Verts = new Vector3[Points + 1];
        Verts[0] = Vector3.zero;
        Tris = new int[(Points) * 3];


       float segmentArcLength = ((MaxAngle * Mathf.Deg2Rad) / (Points - 1));

        float startignrot = transform.eulerAngles.z * Mathf.Deg2Rad;
        startignrot = 0;
        for (int i = 0; i < Points; i++)
        {


            float arcLength = i * segmentArcLength;

            Vector3 direction = transform.TransformDirection((new Vector2(Mathf.Sin(arcLength + startignrot), Mathf.Cos(arcLength + startignrot)).normalized));
            Vector3 pointPosition = Vector3.zero;

            float transformScaledDistance = Vector2.Scale(direction * Radius, transform.lossyScale).magnitude;

            if (CastShadows)
            {
                RaycastHit2D ray = Logic.RaycastByType<ShadowCaster2D>(transform.position, direction, transformScaledDistance);

                // RaycastHit2D ray = Physics2D.Raycast(transform.position, direction, transformScaledDistance, Layers);

                direction = transform.InverseTransformDirection(direction);

                if (ray)
                {
                    pointPosition = (ray.distance - Trim) * direction;
                }
                else
                {

                    pointPosition = direction * (transformScaledDistance - Trim);


                }
                //Debug.Log(Logic.Reciprocal(transform.lossyScale));
                pointPosition = Vector2.Scale(pointPosition, Logic.Reciprocal(transform.lossyScale));
                // pointPosition = Vector3.Scale(pointPosition, transform.lossyScale);
            }
            else
            {
                direction = transform.InverseTransformDirection(direction);

                pointPosition = direction * (Radius - Trim);
            }







            if (drawGizmos)
            {
                Gizmos.color = Color.red;

                Gizmos.DrawSphere(transform.position + transform.TransformDirection(pointPosition), 1);
            }
          

            Verts[i + 1] = pointPosition;
        }

        int tricount = 0;

        for (int i = 0; i < Points; i++)
        {
            Tris[tricount] = 0;
            Tris[tricount + 1] = i;

            if (i != Points - 1)
            {
                Tris[tricount + 2] = i + 1;
            }
            else
            {
             //   Debug.Log("end");

                Tris[tricount + 2] = 0;
            }

            tricount += 3;
        }



        MyMesh.vertices = Verts;
        MyMesh.triangles = Tris;


        MyMesh.RecalculateBounds();
        MyMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = MyMesh;
    }

    private void OnDrawGizmosSelected()
    {
        if (run)
        {
            GameObject parent = transform.parent.gameObject;
            if (parent != null)
            {
                Vector2 RunningPos = Vector2.zero;
                Vector2[] colliderPoints = new Vector2[0];

             //   Debug.Log("Parent:", parent);
                if (parent.TryGetComponent( out Collider2D parentColldier))
                {
                    if (parentColldier is EdgeCollider2D)
                    {
                       colliderPoints = (parentColldier as EdgeCollider2D).points;

                    }

                    if (parentColldier is PolygonCollider2D)
                    {
                        colliderPoints = (parentColldier as PolygonCollider2D).points;

                    }

                    if (colliderPoints.Length > 0)
                    {
                        for (int i = 0; i < colliderPoints.Length; i++)
                        {
                            RunningPos += colliderPoints[i];
                        }

                        RunningPos /= colliderPoints.Length;
                        transform.localPosition = RunningPos;
                    }

                }
            }

            if (UseCrt == true)
            {
                GenerateMeshCrt(true);

                if (MyRenderTexture != null)
                {

                    MyRenderTexture.Update();

                }
                else
                {
                    CreateTexture();
                }

            }
            else
            {
                GenerateMesh(true);

            }

            if (MyRenderTexture == null)
            {
                CreateTexture();

            }
            else if (ReplaceTexture)
            {
                MyRenderTexture.Update();

            }

            if (MyRenderTexture != null)
            {
              //  EditorUtility.SetDirty(MyRenderTexture);
             //   PrefabUtility.RecordPrefabInstancePropertyModifications(MyRenderTexture);
            }
        }
    
    }
}

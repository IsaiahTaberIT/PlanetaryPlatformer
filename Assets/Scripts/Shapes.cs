using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Shape : MonoBehaviour
{
    public ShapeType Type = 0;
    public Vector2[] UVS = new Vector2[0];
    [Range(1, 360f)] public float MaxAngle = 360f;
    public int SortingOrder = 10;
    public int SortingLayerID = 10;
    public string Layer;

    public SortingLayer layer;
    public int Points;
    public float Radius;
    public Mesh MyMesh;
    public Vector3[] Verts;
    public int[] Tris;
    public LayerMask Layers;
    public MeshRenderer MyMeshRenderer;
    public Material StandardCLMaterial;

    private void OnValidate()
    {
        SortingLayer[] allLayers = SortingLayer.layers;

        if (SortingLayerID < allLayers.Length && SortingLayerID >= 0)
        {
            Layer = allLayers[SortingLayerID].name;
        }
        else
        {
            SortingLayerID = Mathf.Clamp(SortingLayerID, 0, allLayers.Length - 1);
        }

        if (MyMeshRenderer == null)
        {
            MyMeshRenderer = GetComponent<MeshRenderer>();
        }


        if (MyMeshRenderer != null)
        {
            MyMeshRenderer.sortingOrder = SortingOrder;
            MyMeshRenderer.sortingLayerID = SortingLayer.GetLayerValueFromID(SortingLayerID);
        }

    }

    public enum ShapeType
    {
        Circle = 0,
    }

    private void OnDrawGizmosSelected()
    {
        GenerateMeshCircleMesh();
    }


    public void GenerateCirclePoints()
    {
      


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

            direction = transform.InverseTransformDirection(direction);

            pointPosition = direction * Radius;

            Verts[i + 1] = pointPosition;
        }
    }

    [ContextMenu("Generate circle mesh")]
    public void GenerateMeshCircleMesh()
    {


        if (MyMesh == null)
        {
            MyMesh = new Mesh();

        }
        else
        {
            MyMesh.Clear();
        }


        GenerateCirclePoints();


        if (MyMeshRenderer == null)
        {
            if (TryGetComponent(out MyMeshRenderer))
            {
                MyMeshRenderer.sortingOrder = SortingOrder;
                MyMeshRenderer.sortingLayerID = SortingLayer.GetLayerValueFromID(SortingLayerID);
            }
        }


        int tricount = 0;

        for (int i = 0; i < Points; i++)
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

        Vector3[] vertices = MyMesh.vertices;
        UVS = new Vector2[vertices.Length];
        Bounds bounds = MyMesh.bounds;
        for (int i = 0; i < vertices.Length; i++)
        {
            UVS[i] = new Vector2(vertices[i].x / bounds.size.x + 0.5f, vertices[i].y / bounds.size.y + 0.5f);
        }

        MyMesh.uv = UVS;
        MyMesh.RecalculateNormals();
        MyMesh.RecalculateTangents();
        MyMesh.RecalculateBounds();
        MyMesh.RecalculateNormals();

        if (TryGetComponent(out MeshFilter meshfilter))
        {
            meshfilter.mesh = MyMesh;
        }
        else
        {
            Debug.Log("You Must Add A MeshFilter To Apply The Mesh");
        }
       
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

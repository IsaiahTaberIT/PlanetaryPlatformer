using UnityEngine;

public class ShapeGenerator : MonoBehaviour
{
    public Shapes Shape = new(true);
    public Vector2[] UVS = new Vector2[0];
    public int[] Tris;
    public int SortingOrder = 10;
    public int SortingLayerID = 10;
    public string Layer;
    public SortingLayer layer;
    public Mesh MyMesh;
    public LayerMask Layers;
    public MeshRenderer MyMeshRenderer;
    public Material StandardCLMaterial;

    [System.Serializable]
    public class Shapes
    {
        public float AngleOffset = 0;
        [Range(1, 360f)] public float MaxAngle = 360f;
        public float Radius = 0.5f;
        [Min(3)] public int Points = 3;
        public ShapeType Type = 0;
        public Vector3[] Verts = new Vector3[] 
        {
            Vector3.zero, Vector3.zero,     
        };
        public bool Index0AtCenter;

        public Shapes()
        {
           
        }

        public Shapes(bool IndexAtcenter) 
        {
            Index0AtCenter = IndexAtcenter;
        }
        public enum ShapeType
        {
            RegularNGon = 0,
        }


        public void GenerateCirclePoints()
        {

            if (Points < 2)
            {
                Points = 2;
            }

            int initialIdnexOffset = 0;

            if (Index0AtCenter)
            {
                initialIdnexOffset = 1;
                Verts = new Vector3[Points + 1];
                Verts[0] = Vector3.zero;
            }
            else
            {
                Verts = new Vector3[Points];

            }

            float segmentArcLength = ((MaxAngle * Mathf.Deg2Rad) / (Points - initialIdnexOffset));
            float angleoffsetRadians = AngleOffset * Mathf.Deg2Rad;

            for (int i = 0; i < Points; i++)
            {
                float arcLength = i * segmentArcLength;

                Vector3 direction = (new Vector2(Mathf.Sin(arcLength + angleoffsetRadians), Mathf.Cos(arcLength + angleoffsetRadians)).normalized);
                Vector3 pointPosition = Vector3.zero;

                pointPosition = direction * Radius;

                Verts[i + initialIdnexOffset] = pointPosition;
            }
        }



    }


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
    private void OnDrawGizmosSelected()
    {
        GenerateMeshCircleMesh();
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


        Shape.GenerateCirclePoints();


        if (MyMeshRenderer == null)
        {
            if (TryGetComponent(out MyMeshRenderer))
            {
                MyMeshRenderer.sortingOrder = SortingOrder;
                MyMeshRenderer.sortingLayerID = SortingLayer.GetLayerValueFromID(SortingLayerID);
            }
        }
        Tris = new int[(Shape.Points) * 3];


        int tricount = 0;

        for (int i = 0; i < Shape.Points; i++)
        {
            Tris[tricount] = 0;
            Tris[tricount + 1] = i;

            if (i != Shape.Points - 1 + 1)
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

        MyMesh.vertices = Shape.Verts;
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
 
}

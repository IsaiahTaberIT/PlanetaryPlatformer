using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(MeshFilter))]

public class GenerateBetterFloorScript : MonoBehaviour
{
    public PolygonCollider2D Collider;
    public SpriteRenderer Sprite;
    public int Length = 20;

    Vector2[] Verticies;
    int[] Triangles;
    Vector2[] Uvs;
    Color[] Colors;
    public Gradient MyGradient;

    public Transform SelfTransform;
    private Mesh MyMesh;
  


    public Vector3 Pos;
    public Vector2Int GridSize = new Vector2Int(20, 20);
    public float offset1;
    public float offset2;







    // Start is called before the first frame update
    void Start()
    {
        SelfTransform = GetComponent<Transform>();
        Pos = SelfTransform.position;
        Pos = new Vector3(Pos.x / (GridSize.x), 0, Pos.z / (GridSize.y));
        MyMesh = new Mesh();


        GetComponent<PolygonCollider2D>().points = Verticies;
    }
    void Update()
    {
        Verticies = new Vector2[(Length * 2)];

        for (int i = 0; i < Length; i++)
        {
            Verticies[i] = new Vector3(1 * (i - (Length / 2)) - offset1, 0.5f, 0);

            Verticies[i + Length] = new Vector3(1 * (-i + (Length / 2)) - offset2, -0.5f, 0);

            Verticies[i].y -= Mathf.Pow(Mathf.Abs(1 * (i - (Length / 2)) - offset1),1.5f)/10;
            Verticies[i + Length].y -= Mathf.Pow(Mathf.Abs(1 * (-i + (Length / 2)) - offset2),1.5f)/10;

        }
        GetComponent<PolygonCollider2D>().points = Verticies;
    }
}

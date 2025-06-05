using UnityEngine;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]

public class SoftBodyScript : MonoBehaviour
{
    public float BobbingRate;
    public float BobbingStrength = 2;
    public float Movementbobbing;
    public AudioEvent SqueltchEvent;
    public AudioSource Squeltch;
    public GameObject SquishParticle;
    public Vector3[] PointVelocities;
    public float[,] TargetDistance;
    public Vector2 CurrentVertex;
    public List<Vector2> InititialVerts;
    public Vector2[] Verts;
    public Vector2[] StageOneVerts;
    public Vector2[] StageTwoVerts;
    public Vector2[] MeshVerts;
    public bool inspectTris;
    public int TriIndex;
    public int[] Tris;
    public MeshRenderer meshRenderer;
    public PolygonCollider2D Collider;
    public Mesh MyMesh;
    public int VertViewingIndex;
    private int _lastVertViewingIndex;
    public float RestoringForce;
    [Range(0f,1f)] public float Dampening;
    public Vector3 CurrentVelocity;
    public Vector3 LastVelocity;
    public Vector3 LastPosition;
    public Vector3 InitialCOM;
    public float WiggleFactor;
    public bool Vibrate;
    public List<Ray> Rays = new List<Ray>();
    public int NumberOfPoints;
    public float Radius;
    public float GravityPower = 1;
    public GameLogicScript MyGameLogicScript;
    public Vector3 CurrentCOM;
    public float SurfaceTensionModifier;
    public int[] SingleTris = new int[3];
    public Vector2 Acceleration;
    public float AccelSquishModifier;
    [Range(0f, 1f)] public float MinAlpha;
    [Range(0f, 1f)] public float MaxAlpha;
    [Range(1f, 100f)] public float VelocitySmoothingFactor;
    [Range(0f, 1f)] public float SquishProportion;
    public float MaxSquish;
    public Vector3 Gravity;
    public bool IsInitialized;



    void OnEnable ()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Collider = GetComponent<PolygonCollider2D>();
        GenerateInititalShape();
        LastPosition = transform.position;
        MyGameLogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();
        
    }

    [ContextMenu("Generate Shape")]

    void GenerateInititalShape()
    {
        PointVelocities = new Vector3[InititialVerts.Count];
        if (MyMesh != null)
        {
            MyMesh.Clear();

        }
        else
        {
            MyMesh = new Mesh();
        }
        if (InititialVerts.Count < 3)
        {
            Debug.LogWarning("Too Few Veticies To Form Mesh!");
            return;
        }
        Verts = new Vector2[InititialVerts.Count];
        MeshVerts = new Vector2[InititialVerts.Count * 2];
        Tris = new int[(InititialVerts.Count - 2) * 3 + InititialVerts.Count * 6];
        //Tris = new int[(InititialVerts.Count - 2) * 3];
        TargetDistance = new float[InititialVerts.Count,InititialVerts.Count];
        InitialCOM = Vector3.zero;

        for (int i = 0; i < InititialVerts.Count; i++)
        {
            InitialCOM += (Vector3)InititialVerts[i];

            if (i == InititialVerts.Count -1)
            {
                InitialCOM /= InititialVerts.Count;
            }
            
            for (int j = 0; j < InititialVerts.Count; j++)
            {
                if (i != j)
                {
                    TargetDistance[i, j] = Vector2.Distance(InititialVerts[i],InititialVerts[j]);
                }

            }
        }


        // StageOneVerts = InititialVerts.ToArray();


        //  .CopyTo(StageOneVerts, 0);
        Verts = InititialVerts.ToArray();

        AssignVerts();

        int tricount = 0;

        for (int i = 0; i < InititialVerts.Count -2; i ++)
        {
            Tris[tricount] = 0;
            Tris[tricount + 1] = i + 1;
            Tris[tricount + 2] = i + 2;
            tricount += 3;
        }

        int vc = InititialVerts.Count;
     


        for (int i = InititialVerts.Count; i < InititialVerts.Count * 2; i++)
        {


            Tris[tricount] = i;
            Tris[tricount+1] = vc + ((i + 1) % vc);
            Tris[tricount + 2] = ((i+1) % vc);


            Tris[tricount + 3] = i;
            Tris[tricount + 4] = (i + 1) % vc;
            Tris[tricount + 5] = (i % vc);

            tricount += 6;
        }
        


        UpdateShape();
    }
    
    public void Initialize()
    {
        LastPosition = transform.position;
        IsInitialized = true;


    }
    void AssignVerts()
    {
        StageOneVerts = new Vector2[InititialVerts.Count];
        StageTwoVerts = Verts;

        for (int i = 0; i < InititialVerts.Count; i++)
        {
            StageOneVerts[i] = Verts[i].normalized * 0.15f;
        }

        StageOneVerts.CopyTo(MeshVerts, 0);
        StageTwoVerts.CopyTo(MeshVerts, InititialVerts.Count);

    }

    private void FixedUpdate()
    {
        
        if (MyGameLogicScript == null)
        {
            MyGameLogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();

        }

        if (TargetDistance == null)
        {
            GenerateInititalShape();
            
        }

        transform.rotation = Quaternion.identity;
        Rays.Clear();
        if (Vibrate)
        {
            Wiggle(Mathf.Sqrt(WiggleFactor));
        }

        if (MyGameLogicScript.RunGame)
        {
            if (IsInitialized)
            {
                LastVelocity = CurrentVelocity;
                CurrentVelocity = LastPosition - transform.position;
                Acceleration = (LastVelocity - CurrentVelocity) * 10;
            }
            else
            {
                Initialize();
            }
            
            float accelmagnitude = Acceleration.magnitude;

            if (accelmagnitude > 4)
            {
                Gravity = GameLogicScript.GravityDirection(transform.position,MyGameLogicScript.NormalGravity * Vector3.zero).Gravity;

                GameObject particleobj = Instantiate(SquishParticle, transform.position - Gravity.normalized, Quaternion.identity);
                ParticleSystem particle = particleobj.GetComponent<ParticleSystem>();


                var main = particle.main;
                var speed = particle.main.startSpeed;
                var emission = particle.emission;
                emission.rateOverTime = Acceleration.sqrMagnitude;
                float min = Mathf.Min(accelmagnitude * 0.75f, 30);
                float max = min * 3;
                main.startSpeed = new ParticleSystem.MinMaxCurve(min, max);
                particle.Play();

                float volume = Mathf.InverseLerp(-2, 60, accelmagnitude);

                volume = UnityEngine.Random.Range(0.95f, 1f) * Mathf.Pow(volume,1.5f);
         
                SqueltchEvent.Play(Squeltch, volume);

         
            }

        }
        else
        {
            Acceleration = Vector2.zero;
        }

        LastPosition = transform.position;
        if (RestoringForce <= 0)
        {
            RestoringForce = 1;
        }

        UpdateShape();
    }
    void UpdateShape()
    {

        CurrentCOM = transform.parent.TransformDirection(Vector3.up) * (Mathf.Sin(Movementbobbing) * BobbingStrength);

        for (int i = 0; i < Verts.Length; i++)
        {
            CurrentCOM += (Vector3)Verts[i];
        }

        CurrentCOM /= Verts.Length;

        for (int i = 0; i < Verts.Length; i++)
        {
            Vector2 inwardDirection = ((Vector3)Verts[i] - CurrentCOM).normalized;
            float squishMagnitude = Mathf.Abs(Vector2.Dot(Acceleration.normalized,inwardDirection)) - SquishProportion;
            PointVelocities[i] += (Vector3)(Mathf.Clamp(Acceleration.magnitude * AccelSquishModifier * squishMagnitude,-MaxSquish, MaxSquish) * inwardDirection);
            PointVelocities[i] += (InitialCOM - CurrentCOM) * 50;

            for (int j = 0; j < Verts.Length; j++)
            {
                if (i != j)
                {
                    if (i == j+1 || i == j-1)
                    {
                        //surface tension
                        PointVelocities[i] += (TargetDistance[i, j] - Vector2.Distance(Verts[i], Verts[j])) * SurfaceTensionModifier * (Vector3)(Verts[i] - Verts[j]).normalized;
                    }
                    else
                    {
                        PointVelocities[i] += (Vector3)(Verts[i] - Verts[j]).normalized * (TargetDistance[i, j] - Vector2.Distance(Verts[i], Verts[j]));

                    }
                }

            }
        }


        for (int i = 0; i < Verts.Length; i++)
        {
            Verts[i] += (Vector2)PointVelocities[i] / (100 / RestoringForce);
            PointVelocities[i] *= (1 - Mathf.Pow(Dampening, 2));
        }

        Collider.points = Verts;

        AssignVerts();

        Vector3[] V = new Vector3[MeshVerts.Length];

        for (int i = 0; i < MeshVerts.Length; i++)
        {
            V[i] = MeshVerts[i];
        }
        
        MyMesh.vertices = V;
        Color[] colors = new Color[V.Length];

        Color baseColor = meshRenderer.sharedMaterial.color;

        baseColor.a = MinAlpha;

        Array.Fill<Color>(colors, baseColor);

        for (int i = 0; i < StageOneVerts.Length; i++)
        {
            colors[i].a = MaxAlpha;
        }
            
        MyMesh.colors = colors;
        SingleTris = new int[3];

        if (TriIndex > (Tris.Length/3) -1)
        {
            TriIndex = (Tris.Length / 3) -1;
        }

        if (TriIndex < 0)
        {
            TriIndex = 0;
        }

        if (inspectTris)
        {
            for (int i = 0; i < 3; i++)
            {
               SingleTris[i] = Tris[i + TriIndex * 3];
            }
           // SingleTris = new int[] { Tris[3], Tris[4], Tris[5]};


            Debug.Log(SingleTris[0] + " , " + SingleTris[1] + " , " + SingleTris[2]);
            MyMesh.triangles = SingleTris;
        }
        else
        {
            MyMesh.triangles = Tris;
        }


          


        MyMesh.RecalculateBounds();
        MyMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = MyMesh;
    }



    [ContextMenu("Wiggle")]

    void CallWiggle()
    {
        Wiggle(WiggleFactor);
    }

    void Wiggle(float wigFactor)
    {
        if (wigFactor < 1)
        {
            wigFactor = 1;
        }
        for (int i = 0; i < Verts.Length; i++)
        {
            Vector2 ranWiggle = new Vector2(UnityEngine.Random.Range(1 / wigFactor, wigFactor), UnityEngine.Random.Range(1 / wigFactor, wigFactor));



            Verts[i] = Vector2.Scale(Verts[i],ranWiggle);
        }

    }




    [ContextMenu("Add Vertex")]



    void AddVertex()
    {
        InititialVerts.Add(Vector2.zero);
    }


    [ContextMenu("Inspector Vertex Assignment")]

    void InspectorVertexAssignment()
    {
        if (VertViewingIndex >= InititialVerts.Count)
        {
            VertViewingIndex = InititialVerts.Count - 1;
        }

        if (VertViewingIndex < 0)
        {
            VertViewingIndex = 0;
        }

        if(VertViewingIndex != _lastVertViewingIndex)
        {
            CurrentVertex = InititialVerts[VertViewingIndex];
            _lastVertViewingIndex = VertViewingIndex;
        }
        else
        {
            InititialVerts[VertViewingIndex] = CurrentVertex;
        }
    }



    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision && collision.gameObject.layer == 6)
        {
            for (int i = 0; i < Verts.Length; i++)
            {
               // Vector3 Vertpos = Vector2.Scale(Verts[i], transform.lossyScale) + (Vector2)transform.position;

                Ray ray = new Ray();
                ray.origin = CurrentCOM + transform.position;
                ray.direction = (Vector3)Vector2.Scale(Verts[i],transform.lossyScale) - CurrentCOM;


                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 30, (1 << 6));

                if (hit && hit.distance <= Vector2.Distance(CurrentCOM, (Vector3)Vector2.Scale(Verts[i], transform.lossyScale)) - 0.01f)
                {
                    //  Debug.Log(hit.distance);

                    bool doContinue = true;

                    if (hit.collider.TryGetComponent(out PlatformEffector2D effector))
                    {
                        float raydirectionangle = Vector2.SignedAngle(ray.direction, Vector2.up);
                        raydirectionangle += 180f;

                        float rotationalOffset = effector.rotationalOffset;

                        if (Mathf.Abs(Mathf.DeltaAngle(rotationalOffset, raydirectionangle)) > 90)
                        {
                            doContinue = false;
                        }

                        if (hit.collider.OverlapPoint(transform.position))
                        {
                            doContinue = false;
                        }

                    }

                    if (doContinue)
                    {
                        Verts[i] = CurrentCOM + Vector3.Scale((ray.direction.normalized * (hit.distance)), Logic.Reciprocal(transform.lossyScale));
                        PointVelocities[i] = ray.direction.normalized * 0.1f;
                    }


                    //PointVelocities[i] = ray.direction.normalized * (Vector2.Distance(CurrentCOM, Verts[i]) - hit.distance) * RestoringForce * -2000;
                    ray.direction = ray.direction.normalized * hit.distance;
                    

                    Rays.Add(ray);

                }


            }
        }
    }


    [ContextMenu("Generate Circle")]

    void GenerateCircle()
    {
        InititialVerts.Clear();
        for (int i = 0; i < NumberOfPoints; i++)
        {
            InititialVerts.Add(new Vector2(Mathf.Cos((i * 2 * Mathf.PI)/ NumberOfPoints), Mathf.Sin((i * 2 * Mathf.PI) / NumberOfPoints)) * Radius) ;
        }
        GenerateInititalShape();
    }


    private void OnDrawGizmosSelected()
    {
        
        if (true)
        {
            Gizmos.color = Color.yellow;

            foreach (int item in SingleTris)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)MeshVerts[item], 0.2f);
            }

            Gizmos.color = Color.red;

            foreach (Ray ray in Rays)
            {
                Gizmos.DrawRay(ray.origin, ray.direction * 100);
            }



            foreach (Vector3 point in Verts)
            {
                //     Gizmos.DrawSphere(transform.position + point, 0.5f);
            }

            Gizmos.color = Color.blue;

            foreach (Vector3 point in InititialVerts)
            {
                Gizmos.DrawSphere(transform.position + point, 0.2f);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(InitialCOM + transform.position, 0.5f);
        }
           
    }

}

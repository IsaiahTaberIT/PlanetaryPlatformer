using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]

public class PlayerLightScript : MonoBehaviour
{
    [Min(1)]public float Radius = 1;
    [Min(4)] public int Verts = 10;
    public Light2D MyLight1;
    public Light2D MyLight2;

    [Min(0)] public float MaxIntensity = 1;
    [Min(0)] public float MinIntensity = 0;
    [Range(0,1f)] public float IntensityRatio = 0;

    public Vector3[] Points;
    private void OnValidate()
    {
        if (MyLight1 != null && MyLight2 != null)
        {
            float intensity = Mathf.Lerp(MinIntensity, MaxIntensity, IntensityRatio);
            MyLight1.intensity = intensity;
            MyLight2.intensity = MaxIntensity - intensity;
        }
     
    }

 
    void ControlPoints()
    {
        float step = (Mathf.PI * 2) / (Verts);
        Points = new Vector3[Verts];


        for (int i = 0; i < Verts; i++)
        {
            Points[i] = new Vector3(Mathf.Sin(i * step), Mathf.Cos(i * step)) * Radius;
        }

        MyLight1.SetShapePath(Points);
    }
}

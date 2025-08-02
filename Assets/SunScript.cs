
using UnityEngine;

public class SunScript : MonoBehaviour
{
    public Material SunShader;
   
   //public float[] Angles = new float[4];
   
   // public float[] AngleOffset = new float[4];
    public float NoiseOffset = 0;
    public float OffsetRate = 0;
    private float OffsetDirection = 1;

    //public Vector2[] Directions = new Vector2[4];

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        
     //   for (int i = 0; i < 4; i++)
     //   {
            
            
   //         AngleOffset[i] %= 2*Mathf.PI;
            
   //         Angles[i] = Mathf.Sin(AngleOffset[i]);
     //       AngleOffset[i] += Time.deltaTime * UnityEngine.Random.Range(0f, 3f);
            
    //        Directions[i] = Quaternion.AngleAxis(Angles[i], new Vector3(0, 0, 1)) * Directions[i];

    //    }

        SunShader.SetFloat("_NoiseOffset", NoiseOffset);
     //   SunShader.SetVector("_Octave1", Directions[0]);
     //   SunShader.SetVector("_Octave2", Directions[1]);
     //   SunShader.SetVector("_Octave3", Directions[2]);
       // SunShader.SetVector("_Octave4", Directions[3]);

        if(NoiseOffset > 1000)
        {
            OffsetDirection = -1;
        }
        else if(NoiseOffset < 0)
        {
            OffsetDirection = 1;
             
        }


        NoiseOffset += Time.deltaTime * UnityEngine.Random.Range(0f, 3f) * OffsetDirection * OffsetRate / 100;
        //NoiseOffset %= 2 * Mathf.PI;


    }
}

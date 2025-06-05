using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Test2[] Array = new Test2[2];
    public int[] dawdwa = new int[2];



    [ContextMenu("Add")]
    void GYHGYUIF()
    {
        dawdwa[0] = 1;
        dawdwa[1] = 2;




        Array[0] = new Test2();
        Array[1] = new Test2();
        Array[0].testfloat = 6;
        Array[1].testfloat = 2;
    }

    [ContextMenu("Add Segment")]
    void GYHGYU5IF()
    {
        Debug.Log(Array[0].testfloat + "," + Array[1].testfloat);
        Debug.Log(dawdwa[0] + "," + dawdwa[1]);

    }





    [System.Serializable]
    public class Test2
    {
        
        public float testfloat;
    }
        





}

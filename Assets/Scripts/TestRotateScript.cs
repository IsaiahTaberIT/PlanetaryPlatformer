using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotateScript : MonoBehaviour
{
    public Rigidbody2D Body;
    public float Torque;
    public float potato;
    // Start is called before the first frame update
    void Start()
    {

        Body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Body.AddTorque(Torque);
    }
}

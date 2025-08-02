using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour 
{
    public GameLogicScript GameLogicScript;
    public float PlanetGravity = 100000;    
    [SerializeField] private float _FalloffPower = 2;

    // Start is called before the first frame update 

    private void OnEnable()
    {
        GameLogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();
        GameLogicScript.Planets.Add(this.gameObject);

    }
    

    public Vector3 PlanetGravityVector(Vector3 ObjectPosition)
    {
        Vector3 output;

        //finding the direction of the gravity from the object checking to the specified planet

        Vector3 RelativeDirection = new Vector3(transform.position.x - ObjectPosition.x, transform.position.y - ObjectPosition.y).normalized;

        //applying a magnitude for the gravity in the direction of the gravity

        output = RelativeDirection * PlanetGravity / Mathf.Pow(Vector3.Distance(transform.position, ObjectPosition), _FalloffPower);


        return output;

    }
    private void OnDisable()
    {
        GameLogicScript.Planets.Remove(gameObject);

    }
    private void OnDestroy()
    {
        GameLogicScript.Planets.Remove(this.gameObject);
    }
}

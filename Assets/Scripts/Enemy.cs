using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using UnityEngine.U2D;

public class Enemy : MovementScript
{
    public Vector2 MovementDirection = Vector2.right;
    public bool CanSeePlayer;
    public GameObject TargetPlayer;
    public Vector3 DirectionToPlayer;
    public LayerMask LayersToIgnore;

    private void Awake()
    {
        TargetPlayer = GameObject.FindGameObjectWithTag("Player");
        Body = GetComponent<Rigidbody2D>();
        
    }

    public RaycastHit2D CheckLineOfSight()
    {
        RaycastHit2D LineOfSight = Physics2D.Raycast(transform.position, DirectionToPlayer, DirectionToPlayer.magnitude, LayersToIgnore);

        if (LineOfSight && LineOfSight.collider.gameObject == TargetPlayer)
        {
            CanSeePlayer = true;
        }
        else
        {
            CanSeePlayer = false;
        }
        return LineOfSight;
    }

   
}

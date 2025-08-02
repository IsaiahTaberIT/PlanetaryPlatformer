using UnityEngine;

public class SimpleEnemy : Enemy , Hurtful.IHurtful , Sticker.IStickerable
{
    public Sticker.StickerAbleInfo StickerInfo;
    public bool CanSee = true;
    private void OnTriggerStay2D(Collider2D collision)
    {
        bool right = true;
        if (MovementDirection.x < 0)
        {
            right = false;
        }

        Sticker.IStickerable.StickerCollision(collision, ref StickerInfo, !right, right, transform, GroundLayer, Body);
    }
    private void FixedUpdate()
    {
        
        if (StickerInfo._Stickered)
        {
            GravityDown = StickerInfo.StickerPull.normalized;
        }
        else
        {
            GravityDown = MyGamelogicScript.BasicGravityDirection(transform.position);

        }

        Velocity = Body.linearVelocity;
        Body.AddForce(GravityDown * Body.mass);

        Vector3 rot = transform.eulerAngles;
        LookRotation = TargetAngleBasedOnGravityDirection(GravityDown);
        rot.z = LookRotation;
        transform.eulerAngles = rot;
        DirectionToPlayer = TargetPlayer.transform.position - transform.position;

        if (CanSee)
        {
            RaycastHit2D playerCheckRay = CheckLineOfSight();

        }

        if (CanSeePlayer)
        {
            if (Vector2.SignedAngle(DirectionToPlayer, transform.TransformDirection(Vector2.up)) > 0)
            {
                Velocity = MoveRight(true, true, Body.linearVelocity);
                MovementDirection = Vector2.right;
            }
            else
            {
                MovementDirection = Vector2.left;
                Velocity = MoveLeft(true, true, Body.linearVelocity);
            }

        }
        else
        {
            if (MovementDirection == Vector2.right)
            {
                Velocity = MoveRight(true, true, Body.linearVelocity);

            }
            else
            {
                Velocity = MoveLeft(true, true, Body.linearVelocity);
            }

            RaycastHit2D WallCheck = Physics2D.Raycast(transform.position, transform.TransformDirection(MovementDirection), 2f, GroundLayer);

            if (WallCheck)
            {
                //  Debug.Log(MovementDirection);
                MovementDirection *= -1;
            }
        }

        IsGrounded = CheckGrounded(GravityDown, GroundCheckDistance, GroundLayer);

        Body.linearVelocity = Velocity;
    }
}

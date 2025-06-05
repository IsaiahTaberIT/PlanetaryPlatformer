using UnityEngine;

public class UpdateColliderWithScale : MonoBehaviour
{
    public Collider2D MyCollider;
    
    
    [OnEditorScaled]
    void FixCollider()
    {
        if (MyCollider == null)
        {
            MyCollider = GetComponent<Collider2D>();
        }

        if (MyCollider is BoxCollider2D)
        {
            (MyCollider as BoxCollider2D).size = Vector2.one - (MyCollider as BoxCollider2D).edgeRadius * Logic.Reciprocal((Vector2)transform.lossyScale / 2) ;
        }
    }

}

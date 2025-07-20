using JetBrains.Annotations;
using UnityEngine;

public class AutoFillScript : MonoBehaviour
{

    public SpriteRenderer Renderer;
    public float CheckLength = 5;
    public Transform MyTransform;
    public BoxCollider2D MyCollider;

    [OnEditorMoved]
    public void CenterAndFill()
    {
          CenterAndFillScale();
        
    }
  
    [ContextMenu("CenterAndFill")]

    void CenterAndFillScale()
    {
        if (MyTransform == null)
        {
            MyTransform = transform;
        }

        if (MyCollider != null || TryGetComponent(out MyCollider))
        {
            MyCollider.enabled = false;

            RaycastHit2D Right = Physics2D.Raycast(MyTransform.position, Vector2.right, CheckLength);
            RaycastHit2D left = Physics2D.Raycast(MyTransform.position, Vector2.left, CheckLength);

            Vector3 CurrentScale = MyTransform.localScale;
           
            if (Right.collider != null && left.collider != null)
            {
                Vector3 CurrentPos = MyTransform.position;
                CurrentPos.x += (Right.distance - left.distance) / 2;
                MyTransform.position = CurrentPos;

          
                CurrentScale.x = (Right.distance + left.distance);
                MyTransform.localScale = CurrentScale;

            }
            else
            {

                CurrentScale.x = CurrentScale.y;

                transform.localScale = CurrentScale;
            }

            MyCollider.enabled = true;
        }

    }
}

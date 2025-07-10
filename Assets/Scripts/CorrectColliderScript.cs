using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrectColliderScript : MonoBehaviour
{
    public BoxCollider2D Box;
    public CapsuleCollider2D Cap;
    public SpriteRenderer Sprite;
    // Start is called before the first frame update
    void Start()
    {
        Box = GetComponent<BoxCollider2D>();
        Cap = GetComponent<CapsuleCollider2D>();
        Sprite = GetComponent<SpriteRenderer>();

        
        ColliderSprite();

    }
    void Update()
    {
        if (Cap.size != Sprite.size)
        {
            ColliderSprite();
        }
    }

    public void ColliderSprite()
    {
        if (Sprite.size.x / Sprite.size.y >= 3 && (Sprite.size.y <= 2))
        {
            Cap.enabled = true;
            Box.enabled = false;
            Cap.size = Sprite.size;
        }
        else
        {
            Box.enabled = true;
            Cap.enabled = false;
            Box.size = Sprite.size;

        }
    }
    
}

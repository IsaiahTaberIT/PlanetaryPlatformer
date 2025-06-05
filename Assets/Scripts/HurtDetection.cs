using UnityEngine;

public class HurtDetection : MonoBehaviour
{
    private void Start()
    {
        MyGamelogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();
    }
    public GameLogicScript MyGamelogicScript;
    private void OnTriggerEnter2D(Collider2D collision)
    {
       
        if (collision.TryGetComponent(out Hurtful.IHurtful Hurtful))
        {
            if (!MyGamelogicScript.GodMode)
            {
                MyGamelogicScript.GameOver();

            }
            else
            {
                Debug.Log("Died");
            }
        }
    }
}

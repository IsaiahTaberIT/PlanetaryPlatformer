using UnityEngine;

public class SystemOfLinearEquationsTestScript : MonoBehaviour
{
    public Vector2 Offset1;
    public Vector2 Offset2;
    public Vector2 Direction1;
    public Vector2 Direction2;
    public float y;
    public Vector2 Intersection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = (Color.blue + Color.white) / 2;
        Gizmos.DrawRay(transform.position + (Vector3)Offset1 - (Vector3)(Direction1 * 500), Direction1 * 1000);
        Gizmos.color = (Color.red + Color.white) / 2;
        Gizmos.DrawRay(transform.position + (Vector3)Offset2 - (Vector3)(Direction2 * 500), Direction2 * 1000);

        Intersection = CalculateIntersection();

        Gizmos.color = (Color.green + Color.white) / 2;

        Gizmos.DrawSphere((Vector3)Intersection + transform.position, 2);



    }

    Vector2 CalculateIntersection()
    {
        Vector2 Intersection = Vector2.zero;
        float Slope1 = Direction1.y / Direction1.x;
        float Slope2 = (Direction2.y / Direction2.x);


        Intersection.x = 0;

        if (Direction1.x != 0 && Direction2.x != 0 && Slope1 != Slope2)
        {
            // original: Intersection.x = -1 / ((1 - 1 * (Slope1) / Slope2) / ((-Offset1.y + Offset2.y) / Slope2 - Offset2.x + (Offset1.x * Slope1 / Slope2)));

            // this equation is the result of an idiot perfoming algebra in notepad to solve a system of linear equations for "x"
            // said idiot was not used to doing algebra with 6 different variables in a situation that prevents the idiot from resolving terms to decimal values
            // idiot is also unable to re-organise/simplify this to make it more readable...
            // lastly idiot is too stubborn to paste this into chat-GPT to have it fix the algebra for them so, making the good assumption that it can be improved, be comforted in the knowledge that it wont
            Intersection.x = -1 / ((1 - Slope1 / Slope2) / ((-Offset1.y + Offset2.y) / Slope2 + (Offset1.x * Slope1 / Slope2) - Offset2.x));
            Intersection.y = (Direction1.y / Direction1.x) * (Intersection.x - Offset1.x) + Offset1.y;
        }
        else if (Direction1.x == 0)
        {
            Intersection.x = Offset1.x;
            Intersection.y = Slope2 * (Intersection.x - Offset2.x) + Offset2.y;


        }
        else if (Direction2.x == 0)
        {
            Intersection.x = Offset2.x;
            Intersection.y = Slope1 * (Intersection.x - Offset1.x) + Offset1.y;

        }
        else
        {
            Debug.LogWarning("Parralel Lines Never Meet");
        }





            // Intersection.y = y;





            return Intersection;
    }
    // Update is called once per frame
  
}

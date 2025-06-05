using UnityEngine;



public class ContactGetterScript : MonoBehaviour
{
    public Vector2 Normal;
    public int _NormalDivisor;
    public PlayerScript MyPlayerScript;
    public ContactPoint2D[] _Contacts = new ContactPoint2D[100];
    public int _ContactCount;
    public float Angle;
    public bool IsCloseToGround;
    private void OnTriggerStay2D(Collider2D collision)
    {
        //_NormalDivisor = 1;
        

        if (collision.gameObject.layer == 6)
        {
            IsCloseToGround = true;

            /*
            _Contacts = new ContactPoint2D[100];

            _Contacts = new ContactPoint2D[collision.GetContacts(_Contacts)];
            Debug.Log("Collider");
            collision.GetContacts(_Contacts);
            _ContactCount = _Contacts.Length;

            for (int i = 0; i < _ContactCount; i++)
            {

                float contactY = (Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * _Contacts[i].point).y;
                float PlayerY = ((Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * MyPlayerScript.transform.position).y - 0.5f);
               // Debug.Log(contactY);
              //  Debug.Log(PlayerY);

                if (Mathf.Abs(contactY - PlayerY) > 0.0001)
                {
                    Vector2 addNormal = (Vector2)(Quaternion.AngleAxis(-Angle, new Vector3(0, 0, 1)) * _Contacts[i].normal).normalized;
                   // Debug.Log(_Contacts[i].normal);
                   if (Mathf.Abs(addNormal.x) < 0.65f)
                    {
                        Normal -= addNormal;
                        _NormalDivisor++;

                    }


                }

            }


            */

        }

    }
    public void ResetValues()
    {
       IsCloseToGround = false;
       Normal = Vector2.zero;
    }

    public void PassAngle(float angle)
    {
        Angle = angle;
    }


    public void PassBackOutput( ref bool isClosetoGround)
    {
        isClosetoGround = IsCloseToGround;
      
    }
    public void PassBackOutput(ref int normalDivisor, ref Vector2 normal, ref bool isClosetoGround)
    {
        isClosetoGround = IsCloseToGround;
        normalDivisor = _NormalDivisor;
        normal = Normal;
    }
}


using UnityEngine;
using System.Collections.Generic;
public class Path : MonoBehaviour
{
    public bool LockParentPos = true;
    public List<Vector3> PathWorld=new(0);
    public List<Vector3> PathLocal=new(0);

    public bool Updated;

    public void OnDrawGizmosSelected()
    {
        if (LockParentPos)
        {
            transform.localPosition = Vector3.zero;
        }

        Vector3 lastpoint = Vector3.zero;
        bool atleast1 = false;

        foreach (Vector3 point in PathWorld)
        {
            Gizmos.color = (Color.blue * 2 + Color.white) / 3;
            Gizmos.DrawSphere(point, 1);
            if (atleast1)
            {
                Gizmos.color = (Color.blue + Color.white) / 2;
                Gizmos.DrawLine(lastpoint, point);

            }
            else
            {
                atleast1 = true;
            }

            lastpoint = point;
        }
    }
}

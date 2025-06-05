using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActivatedCheckpoints", menuName = "Scriptable Objects/ActivatedCheckpoints")]
public class ActivatedCheckpoints : ScriptableObject
{

    [SerializeField]
    public int HighestActiveCheckpointPriority = -1;

    [ContextMenu("Clear Active Checkpoints")]

    void ClearActiveCheckpoint()
    {
        HighestActiveCheckpointPriority = -1;
    }
   
   public void TryUpdatePriority(int newPriority)
   {
        if (HighestActiveCheckpointPriority < newPriority)
        {
            HighestActiveCheckpointPriority = newPriority;
        }
      
   }
   

}

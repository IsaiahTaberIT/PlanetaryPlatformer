using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Star Generator Data", menuName = "Star Gen Data")]
public class StarGenData : ScriptableObject
{
    public Vector2[,] GeneratorPositions = new Vector2[1,1];
}

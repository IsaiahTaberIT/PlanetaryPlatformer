using System.IO;
using UnityEngine;

public class PatrolingEnemy : Enemy , Hurtful.IHurtful
{
    public AlternatingTranslation translation;
    // Update is called once per frame
    void Update()
    {
        
        translation.StepTowardsNextTarget(transform);
    }
}

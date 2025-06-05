using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ForceUnlockAssemblies : MonoBehaviour
{
#if UNITY_EDITOR
    private Coroutine FuckingWorkYouCunt;
    void OnEnable()
    {
       FuckingWorkYouCunt = StartCoroutine(FixYourStupidBullshit());
    }

    void OnDisable()
    {
        StopCoroutine(FuckingWorkYouCunt);
    }

     

    IEnumerator FixYourStupidBullshit()
    {
        while (0 == 0)
        {
          //  Debug.Log("Fixed");
            EditorApplication.UnlockReloadAssemblies();
            yield return new WaitForSeconds(5f);
        }
      

    }

#endif
}

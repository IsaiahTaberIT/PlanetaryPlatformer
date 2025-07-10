using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DisableOnEnter : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ObjToDisable;
    public UnityEvent FixCheckpoint;
    public void DisablePlanet()
    {
        ObjToDisable.SetActive(false);
        
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (ObjToDisable.CompareTag("Planet"))
            {
                StartCoroutine(GravityFade(ObjToDisable.GetComponent<Gravity>().PlanetGravity, ObjToDisable.GetComponent<Gravity>()));
            }
            else
            {
                DisablePlanet();

            }
        }
    }


    IEnumerator GravityFade(float initialGravity, Gravity gravity)
    {        
        for (float i = 1f; i >= 0; i -= 0.05f)
        {
            gravity.PlanetGravity = Mathf.Lerp(0, initialGravity, i);
            yield return new WaitForSeconds(0.02f);
        }

        gravity.PlanetGravity = 0;
        FixCheckpoint.Invoke();
    }

}

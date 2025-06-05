using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class CheckpointScript : MonoBehaviour
{
   // public GameLogicScript MyGamelogicScript;
    public ActivatedCheckpoints MyActivatedCheckpoints;
    private GameLogicScript _MyGameLogicScript;
    public bool Triggered = false;
    public int Priority = 0;

    [SerializeField] private bool _InvokeOnStart;

    //public List<MethodAssignerScript> FunctionsOnActivate = new();
    public UnityEvent FunctionsOnActivate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _MyGameLogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();
        MyActivatedCheckpoints = _MyGameLogicScript.MyActivatedCheckpoints;
        UpdateColor();

        if (_InvokeOnStart)
        {
            InvokeFunctions();
        }
    }


    [ContextMenu("Invoke Functions")]
    public void InvokeFunctions()
    {
        FunctionsOnActivate.Invoke();
    }

    public void AddCheckpoint()
    {
        if (!Triggered)
        {
            Debug.Log("1");
            SaveData.SaveCheckpointData(Priority);
           // MyActivatedCheckpoints.TryUpdatePriority(Priority);
            _MyGameLogicScript.UpdateCheckpoints(Priority);
            UpdateColor();
            InvokeFunctions();
        }
        Triggered = true;
    }


    [ContextMenu("Add Checkpoint")]

    public void AddCheckpointWithoutInvoke()
    {
        if (!Triggered)
        {
            Debug.Log("2");

            SaveData.SaveCheckpointData(Priority);

            //MyActivatedCheckpoints.TryUpdatePriority(Priority);
            _MyGameLogicScript.UpdateCheckpoints(Priority);
            UpdateColor();
        }
        Triggered = true;
    }


    public void UpdateColor()
    {
        if (Triggered)
        {
            GetComponent<SpriteRenderer>().color = Color.green;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log(Priority);
            AddCheckpoint();
        }
    }

}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sprites For Buildings", menuName = "Sprites For Buildings")]

[ExecuteInEditMode]

public class BuildingSprites : ScriptableObject
{
    [SerializeField] private GameObject _ObjectAtIndex;

    [SerializeField]
    public List<GameObject> ViewableGameobjects = new List<GameObject>(3);
    [SerializeField]
    private  int _Index;
    [SerializeField]
    private int _OldIndex;
    [SerializeField]
    [ContextMenuItem(name: "AddLength", function: nameof(AddLength))]
    [ContextMenuItem(name: "RemoveLength", function: nameof(RemoveLength))]
    private int ChangeAmount;
    public static List<GameObject> BuildingList = new List<GameObject>(1);

    void AddLength()
    {
        for (int i = 0; i < ChangeAmount; i++)
        {
            BuildingList.Add(null);
        }
    }
    void RemoveLength()
    {
        for (int i = 0; i < BuildingList.Count - 1 - ChangeAmount; i++)
        {
            BuildingList.RemoveAt(BuildingList.Count - 1 - i);
        }
    }

    private void OnValidate()
    {
     
     
        int escapecount = 0;
        while (BuildingList.Count < ViewableGameobjects.Count && escapecount < 100)
        {
            BuildingList.Add(null);
            escapecount ++ ;
        }
        escapecount = 0;

        while (BuildingList.Count > ViewableGameobjects.Count && escapecount < 100)
        {
            BuildingList.RemoveAt(BuildingList.Count - 1);
            escapecount++;
        }

        _Index = (BuildingList.Count > _Index) ? (0 < _Index) ? _Index : 0 : BuildingList.Count - 1;
        _ObjectAtIndex = BuildingList[_Index];


        for (int i = 0; i < ViewableGameobjects.Count; i++)
        {
            BuildingList[i] = ViewableGameobjects[i];
        }

        _OldIndex = _Index;
    }

}

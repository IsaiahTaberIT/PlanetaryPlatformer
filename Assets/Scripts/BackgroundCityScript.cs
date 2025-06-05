using System.Collections.Generic;
using UnityEngine;

public class BackgroundCityScript : MonoBehaviour
{

    public List<Building> Buildings = new List<Building>(0);
    public List<GameObject> BuildingObjects = new List<GameObject>(0);
    [SerializeField] private int _MaxIndex = 1;


    [SerializeField] private int _BuildingCount = 1;

    [Range(0f,1f)]
    [SerializeField] private float _Cohesion = 1;
    [Range(0f, 1f)]
    [SerializeField] private float _GapCohesion = 1;

    [Range(0f, 1f)]
    [SerializeField] private float _GapOdds = 1;

    
    [SerializeField] private float _WrapRate = 1;

    public float Radius;

    [ExecuteInEditMode]

    



    [ContextMenu("WipeBuildings")]
    void WipeBuildings()
    {
        Buildings.Clear();

        foreach (GameObject item in BuildingObjects)
        {
            DestroyImmediate(item);
        }

        BuildingObjects.Clear();
    }

    [ContextMenu("GenerateBuildings")]

    void GenerateBuildings()
    {
        WipeBuildings();
        //Vector3 pos;
        /*
        pos.x = Mathf.PerlinNoise(0, i);
        pos.y = Mathf.PerlinNoise(10, i);
        pos.z = Mathf.PerlinNoise(20, i);

        pos -= new Vector3(0.5f, 0.5f, 0.5f);

        */
        int offset = 0;
        int escape = 0;
        for (int i = 0; i < _BuildingCount + offset && escape < _BuildingCount * 100;  i++)
        {
            escape++;


            if (Mathf.Clamp(Mathf.PerlinNoise((i + 1000) * (1 - _GapCohesion), i * (1 - _GapCohesion)), 0, 1) < _GapOdds)
            {
                offset++;
                
            }
            else
            {
                Vector3 pointPosition = new Vector3(Mathf.Sin((i) * _WrapRate / 60), Mathf.Cos((i) * _WrapRate / 60), 0) * Radius / 2;
                Buildings.Add(new Building(pointPosition + transform.position, Mathf.FloorToInt(Mathf.Clamp(Mathf.PerlinNoise(i * (1 - _Cohesion), i * (1 - _Cohesion)), 0, 1) * _MaxIndex)));
            }



            //Debug.Log(Mathf.PerlinNoise(i * 0.521f, i * 0.521f) * 7);
        }
        int temp = 0;
        foreach (Building item in Buildings)
        {

           
            GameObject tempobj = Instantiate(item.Objects[item.ObjectIndex], item.Position, Quaternion.Euler(0, 0, -Vector2.SignedAngle(transform.position - item.Position, Vector2.down)), transform);
            BuildingObjects.Add(tempobj);
            tempobj.GetComponent<SpriteRenderer>().sortingOrder = temp;
            temp--;
        }

    }




    public class Building
    {
        public List<GameObject> Objects; 
        public Vector3 Position;
        public int ObjectIndex;


        public Building(Vector3 position, int objectIndex)
        {
            Objects = BuildingSprites.BuildingList;
            Position = position;
            ObjectIndex = (objectIndex < Objects.Count) ? objectIndex : Objects.Count - 1;
        }




    }

}

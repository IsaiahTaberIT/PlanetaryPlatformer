using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarManagementScript : MonoBehaviour
{
    [ExecuteInEditMode]

    public GameObject StarGenerator;

    [SerializeField] private Vector3 _PlayerPosition = new Vector3(0, 0, 0);
    [SerializeField] private Transform _Player;
    [SerializeField] private GameObject[] _RuntimeGenerators = new GameObject[1];
    [SerializeField] private GameObject[,] _Generators = new GameObject[1,1];
    [SerializeField] private Vector3[] _GenPositions = new Vector3[1];
    [SerializeField] private List<GameObject> _GeneratorsList = new List<GameObject>();
    [SerializeField] private Vector2 _GenSpacing;
    [SerializeField] private Vector2Int _GenArrayDim;
    [SerializeField] private float _Density;
    [SerializeField] private float RenderDistance = 200;

    void OnEnable()
    {
        if (_Player == null)
        {
            try
            {
                _Player = GameObject.FindGameObjectWithTag("Player").transform;
            }
            catch
            {
                Debug.LogError("You do not need a StarManager if there is no player to move the camera");

            }
        }
        else
        {
            _PlayerPosition = _Player.position;
        }

        int index = 0;
        _GeneratorsList.Clear();

        AddDescendantsWithTag(transform, "SGen", _GeneratorsList);

        _RuntimeGenerators = new GameObject[_GeneratorsList.Count];
        _GenPositions = new Vector3[_GeneratorsList.Count];

        foreach (GameObject obj in _GeneratorsList)
        {
            _RuntimeGenerators[index] = obj;
            _GenPositions[index] = obj.transform.position;
           index++; 
        }

    }


    private void Update()
    {
        if (_Player == null)
        {
            try
            {
                _Player = GameObject.FindGameObjectWithTag("Player").transform;
                _PlayerPosition = _Player.position;
            }
            catch
            {
                Debug.LogWarning("You do not need a StarManager if there is no player to move the camera");
                _PlayerPosition = Vector3.zero;
            }
        }
        else
        {
            _PlayerPosition = _Player.position;
        }

        DecideIfRenderStars(_PlayerPosition);
    }






    [ContextMenu("Redux")]
    void Redux()
    {
        PlaceGenerators();
        ActivateGenerators();
    }

    [ContextMenu("Kill Generators")]
    void KillGenerators()
    {
      

        _GeneratorsList.Clear();

        AddDescendantsWithTag(transform, "SGen", _GeneratorsList);

        if (_GeneratorsList.Count > 0)
        {
            foreach (GameObject gen in _GeneratorsList)
            {
                DestroyImmediate(gen);
            }
        }

        _GeneratorsList.Clear();
    }

    [ContextMenu("Place Generators")]

    void PlaceGenerators()
    {
        KillGenerators();

        _Generators = new GameObject[_GenArrayDim.x * 2, _GenArrayDim.y * 2];

        for (int i = 0; i < _GenArrayDim.x *2; i++)
        {
            for (int j = 0; j < _GenArrayDim.y *2; j++)
            {
                Debug.Log(i + "," + j);


                Vector2 pos = new Vector2((i - _GenArrayDim.x) * _GenSpacing.x, (j - _GenArrayDim.y) * _GenSpacing.y);


                _Generators[i, j] = Instantiate(StarGenerator, new Vector3(pos.x, pos.y, 0), new Quaternion(0, 0, 0, 0), transform);
            }
        }

        _GeneratorsList.Clear();

        AddDescendantsWithTag(transform, "SGen", _GeneratorsList);

    }

    [ContextMenu("Activate Generators")]
    void ActivateGenerators()
    {
        for (int i = 0; i < _GenArrayDim.x * 2; i++)
        {
            for (int j = 0; j < _GenArrayDim.y * 2; j++)
            {
                StarGenerationScript starGenerationScript = _Generators[i,j].GetComponent<StarGenerationScript>();


                Debug.Log("k");

                starGenerationScript.Density = _Density;
                starGenerationScript.Range = new Vector3(_GenSpacing.x / 2, _GenSpacing.y / 2, starGenerationScript.Range.z);
                starGenerationScript.GenerateBackground();

                _Generators[i, j].SetActive(false);
            }
        }
            

            
        
    }

    void AddDescendantsWithTag(Transform parent, string tag, List<GameObject> list)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.tag == tag)
            {
                list.Add(child.gameObject);
            }
           // AddDescendantsWithTag(child, tag, list);
        }
    }



    public void DecideIfRenderStars(Vector3 pos)
    {
        for (int i = 0; i < _GenPositions.Length; i++)
        {
            if (Vector3.Distance(pos, _GenPositions[i]) > RenderDistance)
            {
                _RuntimeGenerators[i].SetActive(false);
            }
            else
            {
                _RuntimeGenerators[i].SetActive(true);
            }
        }
    }


}

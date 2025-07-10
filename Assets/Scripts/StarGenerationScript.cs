using System.Collections.Generic;
using UnityEngine;

public class StarGenerationScript : MonoBehaviour
{
    [ExecuteInEditMode]
    private bool _IsGalaxy = false;
    public int GalaxyOdds = 100;
    public GameObject Galaxy;
    [Range(0,1f)] public float Brightness = 1;
    public GameObject Star;
    public Vector3 Range;
    public float MinimumDepth;
    public float Density;
    public int MaxObjects;
    

    //private GameObject[] _Stars;
    private List<GameObject> Stars = new List<GameObject>();

    


    [ContextMenu("Kill Stars")]
    void KillStars()
    {
        //_Stars = GameObject.FindGameObjectsWithTag("Star");
        Stars.Clear();
        AddDescendantsWithTag(transform, "Star", Stars);
        
        if (Stars.Count > 0)
        {
            foreach (var star in Stars)
            {
                DestroyImmediate(star);
            }
        }
       
    }

    [ContextMenu("Generate Background")]
    public void GenerateBackground()
    {
        KillStars();

        int numberOfObjects = (int)(Range.x * Range.y * Density / 100);
        numberOfObjects = (numberOfObjects <= MaxObjects) ? numberOfObjects : MaxObjects;

        Vector3 randomPos;

        for (int i = 0; i < numberOfObjects; i++)
        {
            
            randomPos.z = Random.Range(0, Range.z) + MinimumDepth;

            float depthOffset = randomPos.z / 4f;

            randomPos.x = Random.Range(-(Range.x + depthOffset), Range.x + depthOffset);
            randomPos.y = Random.Range(-(Range.y + depthOffset), Range.y + depthOffset);

            GameObject temp = Instantiate(ChooseObject(GalaxyOdds), randomPos + transform.position, new Quaternion(0, 0, 0, 0), transform);

            
            //Vector3Int colors = new Vector3Int(200 + Random.Range(0, 55), 200 + Random.Range(0, 55), 200 + Random.Range(0, 55));

            Vector3Int colors = CreateStarColors(Random.Range(1, 10));

            colors = Redshift(colors, Range.z, randomPos.z, MinimumDepth);

            if (_IsGalaxy)
            {
                Debug.Log("h");
                temp.transform.position = new Vector3(temp.transform.position.x, temp.transform.position.y,Mathf.Lerp(randomPos.z, Range.z, 0.75f) + MinimumDepth);
            }


            temp.GetComponent<SpriteRenderer>().color = new Color32((byte)colors.x, (byte)colors.y, (byte)colors.z, 255);
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
            AddDescendantsWithTag(child, tag, list);
        }
    }

    Vector3Int CreateStarColors(int value)
    {
        Vector3Int colorValues = new Vector3Int(255,255,255);
        int ran = Random.Range(100, 255);

        switch (value)
        {
            case 1:

                //Blueness
                ran = Random.Range(130, 255);
                colorValues.x = ran;
                colorValues.y = ran;
                break;
            case 2:
                //Redness
                ran = Random.Range(90, 255);
                colorValues.z = ran;
                colorValues.y = ran;
                break;
            case 3:
                //Yellowness
                colorValues.z = ran;
                break;
            case 4:
                //orangeness
                colorValues.y = ran;
                colorValues.z = 100;
                break;
            default:
                break;
        }

        

        return ApplyBrightness(colorValues);

    }

    Vector3Int ApplyBrightness(Vector3Int colorValues)
    {
        Vector3 colorValuesF = new(colorValues.x, colorValues.y, colorValues.z);
        colorValuesF *= Brightness;

        for (int i = 0; i < 3; i++)
        {
            colorValuesF[i] = Mathf.RoundToInt(colorValuesF[i]);
        }

        return new Vector3Int((int)colorValuesF.x, (int)colorValuesF.y, (int)colorValuesF.z);
    }

    Vector3Int Redshift(Vector3Int inputColor, float maxRange, float zPosition, float minimumDepth)
    {
        Vector3Int colorValues = inputColor;

        colorValues.x = (int)Mathf.Lerp(inputColor.x, 80, Mathf.InverseLerp(minimumDepth, maxRange, zPosition));
        colorValues.y = (int)Mathf.Lerp(inputColor.y, 35, Mathf.InverseLerp(minimumDepth, maxRange, zPosition));
        colorValues.z = (int)Mathf.Lerp(inputColor.z, 35, Mathf.InverseLerp(minimumDepth, maxRange, zPosition));

        return colorValues;
    }


    GameObject ChooseObject(int odds)
    {
        _IsGalaxy = false;
        switch (Random.Range(1,odds))
        {
            case 1:
                _IsGalaxy = true;
                return Galaxy;
            default:
                return Star;
        }
    }
}

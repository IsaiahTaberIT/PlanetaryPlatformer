

using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

public class SaveTextureMaker : EditorWindow
{
    string myName = "";
    bool RegenerateImage = true;
    Vector2Int MyDimensions = Vector2Int.zero;
    Vector2Int InitialDimensions = Vector2Int.zero;

    TextureMaker target;


    //bool groupEnabled;
    // bool myBool = true;
    // float myFloat = 1.23f;





    [InitializeOnLoadMethod]
    static void Init()
    {

        TextureMaker.OnBringUpWindow = ShowWindow; // note: there is no () on this

     
    }

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Window/My Window")]
    public static void ShowWindow()
    {
      
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(SaveTextureMaker));
    }

    private void OnEnable()
    {
        var selected = Selection.activeGameObject;


        if (selected != null)
        {
            target = selected.GetComponent<TextureMaker>();

            if (target != null)
            {
                myName = target.Name;
                InitialDimensions = target.Dimensions;
                MyDimensions = InitialDimensions;
            }
        }
    }

    void OnGUI()
    {

        GUILayout.Label("Save Settings", EditorStyles.boldLabel);
        myName = EditorGUILayout.TextField("Name: ", myName);


        RegenerateImage = EditorGUILayout.BeginToggleGroup("Regenerate Image", RegenerateImage);
        MyDimensions = EditorGUILayout.Vector2IntField("Dimensions", MyDimensions);
        EditorGUILayout.EndToggleGroup();

        if (!RegenerateImage)
        {
            MyDimensions = InitialDimensions;
        }

        if (GUILayout.Button("Save"))
        {
            if (RegenerateImage)
            {
                target.Dimensions = MyDimensions;
               // Debug.Log(target.Dimensions);
                target.GenerateAndApply();
               // Debug.Log(target.Dimensions);
            }

            target.Name = myName;

            if (Save())
            {
                target.Dimensions = InitialDimensions;
                Close();
            }
            else
            {
                target.Dimensions = InitialDimensions;
            }

        }







    }

    public bool Save()
    {
        if (target.Name == null || target.Name == "")
        {
            bool doContinue = EditorUtility.DisplayDialog("No Name Selected", "Name Is required for saving", "Try Again");
           
           
                return false;
           
          
        }

        byte[] bytes = target.OutputTexture.EncodeToPNG();
        string folder = "Assets/TextureMakerSprites";
        string fileName = target.Name + ".png";
        Directory.CreateDirectory(folder);
        string filePath = folder + "/" + fileName;

        if (File.Exists(filePath))
        {

            bool doContinue = EditorUtility.DisplayDialog("Name already in use", "Would you like to overwrite the existing image?","Yes","Cancel");

            if (!doContinue)
            {
                return false;
            }
        }

        File.WriteAllBytes(filePath, bytes);


#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(filePath);
        Debug.Log($"Saved PNG at {filePath}", obj); 
        EditorUtility.FocusProjectWindow();  
        EditorGUIUtility.PingObject(obj);
#endif
        return true;
    }


}
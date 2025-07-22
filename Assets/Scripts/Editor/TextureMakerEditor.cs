using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using static TextureMaker.LayerManager;
using static TextureMaker;
using static TextureMaker.LayerManager.TextureLayer;


using System.Text;
using Codice.Client.Common;
using static UnityEditor.Experimental.GraphView.GraphView;
using System;

[CustomEditor(typeof(TextureMaker))]
public class TextureMakerEditor : Editor
{
    public TextureMaker maker;
    ReorderableList TextureLayerlist;
    private void OnEnable()
    {
        SerializedProperty managerProp = serializedObject.FindProperty("Manager");

        SerializedProperty layersProp = managerProp.FindPropertyRelative("TextureLayers");

        TextureLayerlist = CreateFromLayers(layersProp);

        maker = (TextureMaker)target;

    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        serializedObject.Update();
        List<SerializedProperty> properties = new List<SerializedProperty>();
        SerializedProperty property = serializedObject.GetIterator();
        property.NextVisible(true); // Skip m_Script

        while (property.NextVisible(false))
        {
            // Exclude CurrentSegment (we'll draw it manually with a slider)
            if (property.name != "Manager")
            {

                properties.Add(property.Copy());
            }
     
        }

        for (int i = 0; i < properties.Count; i++)
        {
         
            EditorGUILayout.PropertyField(properties[i], true);
     
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);
        TextureLayerlist.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            GameObject parent = maker.transform.GetRootParent();

            if (parent != null)
            {
                if (parent.TryGetComponent(out TextureMaker parentmaker))
                {
                    parentmaker.GenerateAndApply();
                }

            }
            else
            {
                maker.GenerateAndApply();
            }
        }

    }

    public class SerializedWrapper
    {
        public string type;
        public string json;
    }
    private void CopyElement(ReorderableList textureLayerList)
    {
        int index = textureLayerList.index;
        if (index >= 0)
        {
            SerializedProperty element = textureLayerList.serializedProperty.GetArrayElementAtIndex(index);
            object obj = element.managedReferenceValue;
            if (obj != null)
            {
                var wrapper = new SerializedWrapper
                {
                    type = obj.GetType().AssemblyQualifiedName,
                    json = JsonUtility.ToJson(obj)
                };

                EditorGUIUtility.systemCopyBuffer = JsonUtility.ToJson(wrapper);
            }
        }
    }

    private void PasteElement(ReorderableList textureLayerList)
    {
        string buffer = EditorGUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(buffer))
            return;

        SerializedWrapper wrapper;
        try
        {
            wrapper = JsonUtility.FromJson<SerializedWrapper>(buffer);
            var type = System.Type.GetType(wrapper.type);
            if (type != null)
            {
                object obj = JsonUtility.FromJson(wrapper.json, type);
                textureLayerList.serializedProperty.InsertArrayElementAtIndex(textureLayerList.count);
                var newElement = textureLayerList.serializedProperty.GetArrayElementAtIndex(textureLayerList.count - 1);
                newElement.managedReferenceValue = obj;
                serializedObject.ApplyModifiedProperties();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Paste failed: {e.Message}");
        }
    }

    private void ShowContextMenu(ReorderableList textureLayerList)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Copy"), false, () => CopyElement(textureLayerList));
        menu.AddItem(new GUIContent("Paste"), false, () => PasteElement(textureLayerList));

             
        var layer = maker.Manager.TextureLayers[textureLayerList.index];

        if (layer is IDistortionSubMaker disSM)
        {
            menu.AddItem(new GUIContent("Create Distortion Maker"), false, () => disSM.DistortionSM.CreateTextureMaker());
        }

        if (layer is IReplaceColorSubMaker rcSM)
        {
            menu.AddItem(new GUIContent("Create Secondary Maker"), false, () =>  rcSM.ReplaceColorSM.CreateTextureMaker());
        }

        if (layer is IMaskSubMaker maskSM)
        {
            menu.AddItem(new GUIContent("Create Mask Maker"), false, () => maskSM.MaskSM.CreateTextureMaker());
        }

        if (layer is ISecondarySubMaker secSM)
        {
            menu.AddItem(new GUIContent("Create Secondary Maker"), false, () => secSM.SecondarySM.CreateTextureMaker());
        }



        menu.ShowAsContext();
    }
    ReorderableList CreateFromLayers(SerializedProperty layersProp)
    {
        
        ReorderableList textureLayerList = new ReorderableList(serializedObject, layersProp, true, true, true, true);

        textureLayerList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Texture Layers");
        };

        textureLayerList.elementHeightCallback = (int index) =>
        {
            var element = textureLayerList.serializedProperty.GetArrayElementAtIndex(index);
            float height = EditorGUI.GetPropertyHeight(element, true) + 4;

            return height;
        };

        textureLayerList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            Event e = Event.current;
            if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
            {
                textureLayerList.index = index; // Ensure the correct element is selected
                ShowContextMenu(textureLayerList);
                e.Use(); // Mark the event as used
            }

            SerializedProperty element = textureLayerList.serializedProperty.GetArrayElementAtIndex(index);
            TextureMaker maker = (TextureMaker)target;
            var layer = maker.Manager.TextureLayers[index];
             
            float y = rect.y;
         
            float height = EditorGUI.GetPropertyHeight(element, true);

            string label = label = layer.Type.ToString().CapitalizeFirst();

            if (layer.Type == TextureLayer.TextureLayerType.filter)
            {
                string filterlabel = (layer as Filter).FilterType.ToString().CapitalizeFirst();
                filterlabel = ": " + filterlabel;
                label += filterlabel;
            }

            label.CapitalizeFirst();
            EditorGUI.PropertyField(new Rect(rect.x + 10, y, rect.width, height), element, new GUIContent(label), true);
            y += height + 4;
        };

        textureLayerList.onAddCallback = (list) =>
        {
            TextureMaker maker = (TextureMaker)target;
            maker.Manager.CreateNewLayer();
            EditorUtility.SetDirty(maker);
        };

        textureLayerList.onRemoveCallback = (list) =>
        {
            SerializedProperty element = textureLayerList.serializedProperty.GetArrayElementAtIndex(list.index);
            var obj = element.managedReferenceValue;

            if (obj is CompositeGpu comp)
            {
                if (comp.MakerObject.TryGetComponent(out TextureMaker _))
                {
                    GameObject.DestroyImmediate(comp.MakerObject);
                }
            }

            textureLayerList.serializedProperty.DeleteArrayElementAtIndex(list.index);
        };

        return textureLayerList;


    }

}

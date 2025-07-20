using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;
public class MainMenuLogicScript : MonoBehaviour
{
    public Camera MainCamera;
    public Slider LevelSelectSlider;
    public TextMeshProUGUI LevelSelectText;
    public UnityEngine.UI.Image image;
    public LevelGraphics levelGraphics;
    [Min(1)] public int CurrentSceneIndex;
    public List<int> levelIndicies = new();
    [Range(0f,1f)]public float CameraRangeScale = 0.1f;
    [Range(0f, 10f)] public float CameraLerpRate = 0.1f;
    public Vector3 WorldMousePos;
    public Image BackgroundImage;

    private void Update()
    {
        WorldMousePos = new Vector3(Input.mousePosition.x / Screen.width - 0.5f, Input.mousePosition.y / Screen.height - 0.5f) * CameraRangeScale * 100;
        WorldMousePos.z = MainCamera.transform.position.z;
        
        MainCamera.transform.position = Logic.LerpVector(MainCamera.transform.position, WorldMousePos, CameraLerpRate * Time.deltaTime);
    }
    public void UpdateLevelGraphic()
    {
        if (levelGraphics.Images.Count > CurrentSceneIndex -1)
        {
            if (levelGraphics.Images[CurrentSceneIndex - 1] != null)
            {
                image.sprite = levelGraphics.Images[CurrentSceneIndex - 1];
            }
            else
            {
                Debug.LogWarning("No Graphic Defined At Index");

            }
        }
        else
        {
            Debug.LogWarning("Graphic List Too Small");
        }
    }
    public void UpdateLevelSelectText()
    {
        LevelSelectText.SetText("Level: " + string.Format("{0:N0}", CurrentSceneIndex));
    }

    private void Start()
    {
        LevelSelectSlider.maxValue = levelIndicies.Count;
    }

    public void UpdateSceneIndex()
    {
        CurrentSceneIndex = (int)LevelSelectSlider.value;
    }

    public void ShiftSceneIndex(int shift)
    {
        CurrentSceneIndex += shift;
        CurrentSceneIndex = Mathf.Clamp(CurrentSceneIndex, 1, levelIndicies.Count);
        LevelSelectSlider.value = CurrentSceneIndex;

    }


    [ContextMenu("LoadLevelAtCurrentIndex")]
    public void LoadLevelAtCurrentIndex()
    {
        SceneManager.LoadScene(levelIndicies[CurrentSceneIndex-1]);
    }

    [ContextMenu("LoadLevelIndex")]
    public void LoadLevelByIndex(int Index)
    {
        SceneManager.LoadScene(Index);
    }
    [ContextMenu("LoadLevelName")]
    public void LoadLevelByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

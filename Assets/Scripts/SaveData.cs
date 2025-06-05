using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using static UnityEngine.Rendering.DebugUI;
public class SaveData : MonoBehaviour
{
    public static Dictionary<int, int> CheckPointsByLevel;

    public void ResetCheckpointsForLevel(int Index)
    {
        SetCheckpointData(-1, Index);
    }

    [ContextMenu("Reset Checkpoints For Current Level")]
    public void ResetCheckpointsForCurrentLevel()
    {
        SetCheckpointData(-1);
    }


    static public Dictionary<int, int> LoadCheckpointData()
    {
        string filepath = System.IO.Path.Combine(Application.persistentDataPath, "SavedCheckPoints.json");

        if (!File.Exists(filepath))
        {
            Debug.LogWarning("Checkpoint data file not found.");
            return new Dictionary<int, int>();
        }

        string data = File.ReadAllText(filepath);

       
        Debug.Log("Data Loaded From: " + filepath);

        ActiveCheckpoints points;
        points = JsonUtility.FromJson<ActiveCheckpoints>(data);
        return points.ConvertToDictionary();
    }

    public static void SetCheckpointData(int value, int index)
    {
        if (CheckPointsByLevel == null)
        {
            CheckPointsByLevel = new Dictionary<int, int>();
        }

        if (CheckPointsByLevel.ContainsKey(index))
        {
            CheckPointsByLevel[index] =  value;
        }
        else
        {
            CheckPointsByLevel.Add(index, value);
        }

        try
        {
            string checkpointdata = JsonUtility.ToJson(FromDictionary(CheckPointsByLevel));
            string filepath = System.IO.Path.Combine(Application.persistentDataPath, "SavedCheckPoints.json");
            System.IO.File.WriteAllText(filepath, checkpointdata);
            Debug.Log("Data Saved To: " + filepath);

        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save checkpoint data: {ex}");
        }


    }

    public static void SetCheckpointData(int value)
    {
        if (CheckPointsByLevel == null)
        {
            CheckPointsByLevel = new Dictionary<int, int>();
        }

        int index = SceneManager.GetActiveScene().buildIndex;

        if (CheckPointsByLevel.ContainsKey(index))
        {
            //Debug.Log(index + " , " + value);
            CheckPointsByLevel[index] = value;
        }
        else
        {
          //  Debug.Log(index + " , " + value);

            CheckPointsByLevel.Add(index, value);
        }

        try
        {
            string checkpointdata = JsonUtility.ToJson(FromDictionary(CheckPointsByLevel));
            string filepath = System.IO.Path.Combine(Application.persistentDataPath, "SavedCheckPoints.json");
            System.IO.File.WriteAllText(filepath, checkpointdata);
            Debug.Log("Data Saved To: " + filepath);

        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save checkpoint data: {ex}");
        }


    }
    public static void SaveCheckpointData(int value)
    {
        if (CheckPointsByLevel == null)
        {
            CheckPointsByLevel = new Dictionary<int, int>();
        }

        int index = SceneManager.GetActiveScene().buildIndex;

        if (CheckPointsByLevel.ContainsKey(index))
        {
            Debug.Log(index + " , " + value);
            CheckPointsByLevel[index] = Mathf.Max(CheckPointsByLevel[index], value);
        }
        else
        {
            Debug.Log(index + " , " + value);

            CheckPointsByLevel.Add(index, value);
        }

        try
        {
            string checkpointdata = JsonUtility.ToJson(FromDictionary(CheckPointsByLevel));
            string filepath = System.IO.Path.Combine(Application.persistentDataPath, "SavedCheckPoints.json");
            System.IO.File.WriteAllText(filepath, checkpointdata);
            Debug.Log("Data Saved To: " + filepath);

        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save checkpoint data: {ex}");
        }


    }

    [System.Serializable]
    public class LevelCheckpointEntry
    {
        public int sceneIndex;
        public int checkpointPriority;
    }

    public class ActiveCheckpoints
    {    
        public List<LevelCheckpointEntry> entries = new List<LevelCheckpointEntry>();
        public Dictionary<int, int> ConvertToDictionary()
        {
            return entries.ToDictionary(e => e.sceneIndex, e => e.checkpointPriority);
        }
    }


    public static ActiveCheckpoints FromDictionary(Dictionary<int, int> dict)
    {
        var data = new ActiveCheckpoints();

        foreach (var kv in dict)
        {
            data.entries.Add(new LevelCheckpointEntry
            {
                sceneIndex = kv.Key,
                checkpointPriority = kv.Value
            });
        }
        return data;
    }
}

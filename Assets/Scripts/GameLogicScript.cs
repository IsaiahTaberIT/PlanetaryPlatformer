using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.Rendering.Universal;
using static Logic;
public class GameLogicScript : MonoBehaviour
{
    public float MinBrightness = 10;
    public GameObject MyScreen;
    public Camera LightsCamera;
    public RenderTexture texture;
    public bool GodMode = false;
    public List<CheckpointScript> Checkpoints = new List<CheckpointScript>(1);
    public GameObject GlobalVolumeObj;
    public Volume GlobalVolume;

    public ActivatedCheckpoints MyActivatedCheckpoints;
    private PlayerScript PlayerScript;
    public bool RequireNewGame = false;
    public bool ActiveMenu = true;
    public GameObject MainCamera;
    public GameObject Canvas;
    public float NormalGravity = 0;
    static public List<GameObject> Planets = new List<GameObject>(0);
    static public GameObject[] PArray = new GameObject[3];
    static public Vector3[] VArray = new Vector3[3];
    //static public float RunningGravityMagnitudes;
    public bool IsGameOver = false;
    public bool IsLevelCompleted = false;
    public bool IsPaused = false;
    public bool CanMove;
    public bool RunGame;
    public Material shaderMaterial;
    public float testfloat;
    public TextMeshProUGUI EscapeCountdown;
    public GameObject EscapeUI;
    public LineRenderer MyLineRenderer;
    public Shader FireShader;
    public bool ShaderAnimations;
    public bool NormalLighting = true;

    [ContextMenu("Reset Baked Lights")]
    public void ResetAllBakeableLights()
    {
        LightsBakeable2D[] bakeable2Ds = FindObjectsByType<LightsBakeable2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < bakeable2Ds.Length; i++)
        {
            DestroyImmediate(bakeable2Ds[i].MyRenderTexture);

            if (bakeable2Ds[i].UseCrt)
            {
                bakeable2Ds[i].GenerateMeshCrt(false);
            }

        }
    }
  

    public struct GravityInfo
    {
        public float RunningGravityMagnitudes;
        public Vector3[] VectorArray;
        public GameObject[] PlanetArray;
        public Vector3 Gravity;

        public GravityInfo(Vector3 gravity, GameObject[] pArray, Vector3[] vArray, float runningGravityMagnitudes)
        {

            RunningGravityMagnitudes = runningGravityMagnitudes;
            VectorArray = vArray;
            PlanetArray = pArray;
            Gravity = gravity;
        }

    }





    public void SetCountDown(float value)
    {
        // EscapeCountdown.SetText("<size=50%><color=#ffff99>SLOW DOWN NOW!\nESCAPE IN:\n\n</size></color>" + string.Format("{0:N0}", value));
        EscapeCountdown.SetText("<size=50%><color=#ffff99>ESCAPE IN:\n\n</size></color>" + string.Format("{0:N0}", value));

    }


    void PlacePlayerOnCheckpoint()
    {
        if (Checkpoints.Count > 0)
        {
            CheckpointScript highestPriorityCheckpoint = Checkpoints[0];

            if (SaveData.CheckPointsByLevel == null)
            {
                return;
            }

            if (SaveData.CheckPointsByLevel.TryGetValue(SceneManager.GetActiveScene().buildIndex, out int Value))
            {
                int checkpointPriority = Value;

                foreach (CheckpointScript Checkpoint in Checkpoints)
                {
                    if (checkpointPriority >= Checkpoint.Priority)
                    {
                        Checkpoint.Triggered = true;
                    }

                    if (checkpointPriority == Checkpoint.Priority)
                    {
                        highestPriorityCheckpoint = Checkpoint;
                    }
                }

                if (checkpointPriority > -1)
                {
                    UpdateCheckpoints(checkpointPriority);

                    Vector2 checkpointPos = highestPriorityCheckpoint.transform.position;

                    PlayerScript.transform.position = checkpointPos;
                    PlayerScript.MainCamera.transform.position = new Vector3(checkpointPos.x, checkpointPos.y, PlayerScript.MainCamera.transform.position.z);
                }

            }
            else
            {
                return;
            }
        }

          

    }

    public void UpdateCheckpoints(int localPriority)
    {
        foreach (CheckpointScript Checkpoint in Checkpoints)
        {
            if (Checkpoint.Priority <= localPriority)
            {
                Checkpoint.Triggered = true;
                Checkpoint.UpdateColor();
                Checkpoint.InvokeFunctions();
            }
        }
    }
    void GatherCheckpoints()
    {
        Checkpoints.Clear();
        Checkpoints.AddRange(FindObjectsByType<CheckpointScript>(FindObjectsSortMode.None));
    }


    public void SetPause(bool pauseState)
    {
        //input !IsPaused to toggle

        if (pauseState)
        {
            IsPaused = true;
            CanMove = false;
            RunGame = false;

        }
        else
        {
            IsPaused = false;
            CanMove = true;
            RunGame = true;
        }
    }

    public void LevelComplete()
    {
        IsLevelCompleted = true;
        CanMove = false;
        RunGame = false;

    }

    [ContextMenu("GameOver")]

    public void GameOver()
    {
        if (!IsGameOver)
        {
            IsGameOver = true;
            CanMove = false;
            RunGame = false;

            StartCoroutine(PlayerScript.DeathAnimation());

        }

      


    }

    void ReInitialize()
    {
       // PlayerScript.SoftBody.GetComponent<SoftBodyScript>().LastPosition = PlayerScript.SoftBody.transform.position;

        IsGameOver = false;
        IsPaused = false;
        CanMove = true;
        SaveData.CheckPointsByLevel = SaveData.LoadCheckpointData();
        GatherCheckpoints();
        PlacePlayerOnCheckpoint();
    }   

    void Awake()
    {
        // HighScore = PlayerPrefs.GetFloat("HIghScore");



        GlobalVolume = GlobalVolumeObj.GetComponent<Volume>();
        MyScreen = GameObject.FindGameObjectWithTag("Screen");



        NormalLighting = IntToBool(PlayerPrefs.GetInt("NormalLighting"));

        ToggleLighting(!NormalLighting);

        


        Application.targetFrameRate = 60;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerScript = player.GetComponent<PlayerScript>();

        }
        ReInitialize();

    }

    void ToggleLighting(bool donormalLighting)
    {
        NormalLighting = !donormalLighting;

        PlayerPrefs.SetInt("NormalLighting", BoolToInt(NormalLighting));

        PlayerPrefs.Save();

        Light2D[] normalLights = FindObjectsByType<Light2D>(FindObjectsInactive.Include,FindObjectsSortMode.None);

        if (MyScreen == null)
        {
            MyScreen = GameObject.FindGameObjectWithTag("Screen");

        }
        if (MyScreen != null)
        {
            MyScreen.SetActive(!NormalLighting);
            SpriteRenderer renderer = MyScreen.GetComponent<SpriteRenderer>();
            Material mat = renderer.material;
            mat.SetFloat("_MinBrightness", MinBrightness);
        }


        for (int i = 0; i < normalLights.Length; i++)
        {
            if (normalLights[i].lightType == Light2D.LightType.Global)
            {
                
            }
            else
            {
                normalLights[i].enabled = NormalLighting;

            }
        }
    }


    // Update is called once per frame
    void Update()
    {



        if (Input.GetKeyDown(KeyCode.I))
        {
            LightsBakeable2D.UseBaked = !LightsBakeable2D.UseBaked;

            LightsBakeable2D.InvokeLightToggle();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleLighting(NormalLighting);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ShaderAnimations = !ShaderAnimations;
            if (ShaderAnimations)
            {
                Shader.SetGlobalFloat("_Animate", 1f);

            }
            else
            {
                Shader.SetGlobalFloat("_Animate", 0);

            }
        }




        if (Input.GetKeyDown(KeyCode.G))
        {
            GodMode = !GodMode;
        }

     //   shaderMaterial.SetFloat("_Smoothness", testfloat);

        if (Input.GetKeyDown(KeyCode.K))
        {
            Application.targetFrameRate += 10;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            Application.targetFrameRate -= 10;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayerPrefs.DeleteAll();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                Quit();
                
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }


   
    }
    public void Restart()
    {
        //PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //RequireNewGame = false;
    }

    public void Quit()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR

        EditorApplication.isPlaying = false;

#endif

        Application.Quit();

    }

    
  

    public Vector3 BasicGravityDirection(Vector3 objectPosition)
    {
        Vector3 localRunningPlanetGravitys = NormalGravity * Vector2.down;

        if (Planets == null)
        {
            return localRunningPlanetGravitys;
        }

        if (Planets.Count > 0)
        {
            foreach (GameObject G in Planets)
            {
                // Debug.Log("me", G.gameObject);
                Vector3 gravityVector = G.GetComponent<Gravity>().PlanetGravityVector(objectPosition);
                localRunningPlanetGravitys += gravityVector;
            }
        }

        return localRunningPlanetGravitys;
    }

    static public Vector3 BasicGravityDirection(Vector3 objectPosition,List<Gravity> planets, Vector3 normalGravity)
    {
        Vector3 localRunningPlanetGravitys = normalGravity;
       
        if (planets == null)
        {
            return localRunningPlanetGravitys;
        }

        if (planets.Count > 0)
        {
            foreach (Gravity G in planets)
            {
                // Debug.Log("me", G.gameObject);
                Vector3 gravityVector = G.PlanetGravityVector(objectPosition);
                localRunningPlanetGravitys += gravityVector;
            }
        }
      
            return localRunningPlanetGravitys;
    }
    static public GravityInfo GravityDirection(Vector3 objectPosition,Vector3 normalGravity)
    {
        Vector3 localRunningPlanetGravitys = normalGravity;
        float LocalRunningGravityMagnitudes = 0;
        LocalRunningGravityMagnitudes += normalGravity.magnitude;
        GameObject[] pArray = new GameObject[3];
        Vector3[] vArray = new Vector3[3];
        vArray[0] = normalGravity;

        
        if (Planets.Count > 0)
        {
            foreach (GameObject G in Planets)
            {
                Vector3 gravityVector = G.GetComponent<Gravity>().PlanetGravityVector(objectPosition);
                localRunningPlanetGravitys += gravityVector;
                LocalRunningGravityMagnitudes += gravityVector.magnitude;
                Transform gTra = G.transform;

                float sortvalue = gravityVector.magnitude;
                Vector3 linkedvector = gravityVector;
                GameObject linkedgameobject = G;

                bool sorted = false;
                int x = 0;
                while (x < 100 && sorted == false)
                {
                    for (int i = 0; i < pArray.Length; i++)
                    {
                        if (vArray[i].magnitude < sortvalue)
                        {
                            Vector3 temp = linkedvector;
                            GameObject tempobj = linkedgameobject;
                            linkedvector = vArray[i];
                            linkedgameobject = pArray[i];
                            vArray[i] = temp;
                            pArray[i] = tempobj;
                            sortvalue = vArray[i].magnitude;
                        }
                    }

                    sorted = true;

                    for (int i = 0; i < pArray.Length - 1; i++)
                    {
                        if (vArray[i].magnitude < vArray[i].magnitude)
                        {
                            sorted = false;
                        }
                    }
                    x++;
                    if (x == 100)
                    {
                        Debug.LogWarning("Sort Failed, Loop Exited");
                    }
                }

            }



        }
        else
        {
            localRunningPlanetGravitys = normalGravity;
            LocalRunningGravityMagnitudes = normalGravity.magnitude;
        }

        PArray = pArray;
        VArray = vArray;

        for (int i = 0; i < pArray.Length; i++)
        {
            VArray[i].z = new Vector2(VArray[i].x, VArray[i].y).magnitude;
        }

        return new GravityInfo(localRunningPlanetGravitys,pArray,vArray, LocalRunningGravityMagnitudes);

    }

    public static float ClampedAbs(float input, float max)
    {
        max = Mathf.Abs(max);
        return (Mathf.Abs(input) > max) ? max * Mathf.Sign(input) : input;
     }

    void ModifyValues(ref int v1, ref int v2)
    {
        v1++;
        v2 += 10;
    }
}





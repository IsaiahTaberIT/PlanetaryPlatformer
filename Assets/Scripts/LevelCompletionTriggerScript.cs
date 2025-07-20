using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LevelCompletionTriggerScript : MonoBehaviour
{
    public float MaxDistance = 5.5f;
    private GameLogicScript _GameLogicScript;
    private Coroutine _EndLevel;
    [SerializeField]
    private bool _Triggered = false;
    private Volume _Volume;
    public GameObject Filter;
    private SpriteRenderer _FilterRenderer;
    public AudioSource CompletionSound;
    public AudioSource CompletionSound2;
    void Start()
    {
        _GameLogicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameLogicScript>();
        _Volume = _GameLogicScript.GlobalVolume;
        _FilterRenderer = Filter.GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerScript>(out PlayerScript player))
        {
            if (!_Triggered)
            {
                _Triggered = true;
                _EndLevel = StartCoroutine(EndLevelRoutine(collision.gameObject));
                if (player.MyLight != null)
                {
                    player.MyLight.SetActive(false);

                }
            }
            _GameLogicScript.LevelComplete();
        }
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private IEnumerator EndLevelRoutine(GameObject Player)
    {
        float t = 0;
        float completeTime = 2;
        float offsetDist = 2f;
        Vector3 downOffset = transform.TransformDirection(Vector3.down * offsetDist);
        Vector2 direction = Player.transform.position - (transform.position + downOffset);
        float magnitude = direction.magnitude;
        float inititialMagnitude = magnitude;
        direction.Normalize();
        float rotspeed = -500;
        float fallRate = 2f;
        float bloomChangeRatio = 0.85f;
        
        _Volume.profile.TryGet<Bloom>(out Bloom bloom);
        float initialBloomIntensity = bloom.intensity.value;
        float initialScatter= bloom.scatter.value;
        float bloomlerp;
        float timeOffset = 0.5f;
        float offsetTime = timeOffset + completeTime;
        bool audio1played = false;
        bool audio2played = false;

        while (t < offsetTime)
        {
            if (magnitude > MaxDistance)
            {
                Player.transform.position = (Vector2)transform.position + (Vector2)downOffset + magnitude * direction;
                magnitude = Mathf.Lerp(magnitude, 0, fallRate * Time.deltaTime);
                inititialMagnitude = magnitude;
                yield return new WaitForSeconds(Time.deltaTime);
            }

            else
            {
                if (!audio1played)
                {
                    audio1played = true;
                    CompletionSound.Play();
                }

                if (offsetTime * bloomChangeRatio + t > offsetTime)
                {
                   

                    bloomlerp = Mathf.Pow((t - (offsetTime * (1 - bloomChangeRatio))) / (offsetTime * bloomChangeRatio), 10);

                    if (!audio2played && 0.6f < (t - (offsetTime * (1 - bloomChangeRatio))) / (offsetTime * bloomChangeRatio))
                    {
                        audio2played = true;
                        CompletionSound2.Play();
                    }
                    bloom.intensity.value = Mathf.Lerp(initialBloomIntensity, 2000, bloomlerp);
                    bloom.scatter.value = Mathf.Lerp(initialScatter, 0.75f, bloomlerp);
                    Color color = _FilterRenderer.color;

                    color.a = bloomlerp;
                    _FilterRenderer.color = color;
                    //Debug.Log(bloom.intensity.value);
                }

                direction = Quaternion.AngleAxis(rotspeed * Time.deltaTime, new Vector3(0, 0, 1)) * direction;
                magnitude = Mathf.Lerp(inititialMagnitude, 0, Mathf.Pow((t / completeTime), 2));
                Player.transform.position = (Vector2)transform.position + (Vector2)downOffset + magnitude * direction;
                Player.transform.localScale = Vector3.one * Mathf.Lerp(1, 0, Mathf.Pow((t / completeTime), 2));
                //Debug.Log(magnitude * direction);
                t += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }
                
        }
        CompletionSound.Stop();
    }
}

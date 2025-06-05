using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Audio Events/Simple")]
public class SimpleAudioEvent : AudioEvent
{
    public AudioClip[] clips;
    public float MinVolume;
    public float MaxVolume;
    public float MinPitch;
    public float MaxPitch;



    public override void Play(AudioSource source)
    {
        if (clips.Length == 0) return;

        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = Random.Range(MinVolume, MaxVolume);
        source.pitch = Random.Range(MinPitch, MaxPitch);
        source.Play();
    }
    public override void Play(AudioSource source, float volume)
    {
        if (clips.Length == 0) return;

        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = volume;
        source.pitch = Random.Range(MinPitch, MaxPitch);
        source.Play();
    }
}

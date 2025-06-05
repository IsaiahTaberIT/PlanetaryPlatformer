using UnityEngine;

public abstract class AudioEvent : ScriptableObject
{
    public abstract void Play(AudioSource source);

    public abstract void Play(AudioSource source, float volume);
}
using UnityEngine;
using System;

public class AudioHandler : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;

    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            if (s.playOnAwake) Play(s.name);
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            Debug.Log("No such audio clip");
        s.source.Play();        
    }
    
    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            Debug.Log("No such audio clip");
        s.source.Stop();        
    }

    public AudioSource GetAudioSource(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            Debug.Log("No such audio clip");
        return s.source;
    }
}

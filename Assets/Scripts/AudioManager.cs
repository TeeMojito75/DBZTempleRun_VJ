using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    // Start is called before the first frame update
    public string Sname;
    void Start()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
        Play(Sname);
    }

    public void Play(string name)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager1 : MonoBehaviour
{
    
    private static AudioManager1 ins;
    public static AudioManager1 Ins
    {
        get => ins;
    }
    
    [Serializable]
    public struct AudioInfo
    {
        public AudioSource Audio;
        public Sound[] Sounds;
    }

    [SerializeField] private AudioInfo sfx;
    [SerializeField] private AudioInfo music;

    private bool activeSfx;
    private bool activeMusic;

    private void Awake()
    {
        
        if (!ins) ins = this;

        if (music.Audio != null)
        {
            this.music.Audio.loop = true;
            if (music.Audio.enabled) activeMusic = true;
        }
        if (sfx.Audio != null)
        {
            this.sfx.Audio.loop = false;
            this.sfx.Audio.playOnAwake = false;
            if (sfx.Audio.enabled) activeSfx = true;
        }
    }

    public void PlaySfx(string name = null)
    {
        if(!activeSfx) return;
        if(sfx.Audio == null) return;
        if(sfx.Sounds == null || sfx.Sounds.Length == 0) return;
        
        Sound sound;
        if (string.IsNullOrEmpty(name))
        {
            sound = sfx.Sounds[0];
        }
        else
        {
            sound = Array.Find(sfx.Sounds, s => s.name == name);
        }
        if(sound == null) return;
        
        sfx.Audio.PlayOneShot(sound.clip);
        Debug.Log("Play Sfx");
    }
    
    public void PlayMusic(string name = null)
    {
        if(!activeMusic) return;
        if(music.Audio == null) return;
        if(music.Sounds == null || music.Sounds.Length == 0) return;
        
        Sound sound;
        if (string.IsNullOrEmpty(name))
        {
            //nếu name trống hoặc null thì sẽ lấy phần tử đầu tiên
            sound = music.Sounds[0];
        }
        else
        {
            //tìm phần tử trùng với name
            sound = Array.Find(music.Sounds, s => s.name == name);
        }
        if(sound == null) return;

        music.Audio.clip = sound.clip;
        music.Audio.Play();
        Debug.Log("Play Music");
    }

    public void ActiveSfx()
    {
        if(sfx.Audio == null) return;
        sfx.Audio.enabled = true;
        activeSfx = true;
    }

    public void DisableSfx()
    {
        if(sfx.Audio == null) return;
        sfx.Audio.enabled = false;
        activeSfx = false;
    }
    
    public void ActiveMusic()
    {
        if(music.Audio == null) return;
        music.Audio.enabled = true;
        activeMusic = true;
    }

    public void DisableMusic()
    {
        if(music.Audio == null) return;
        music.Audio.enabled = false;
        activeMusic = false;
    }
}

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}



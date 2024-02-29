using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    private static AudioManager ins;
    public static AudioManager Instance => ins;

    public AudioSource AudioMusic;
    public AudioSource AudioSFX;
    public Sound[] Sounds;

    private bool m_activeSfx;
    private bool m_activeMusic;

    private void Awake()
    {
        if (!ins)
        {
            ins = this;
        }

        if (AudioMusic != null)
        {
            if (AudioMusic.enabled)
            {
                m_activeMusic = true;
            }
        }

        if (AudioSFX != null)
        {
            if (AudioSFX.enabled)
            {
                m_activeSfx = true;
            }
        }
    }

    public void PlaySfx(KeySound key)
    {
        if (!m_activeSfx)
        {
            return;
        }

        if (AudioSFX == null || Sounds.Length == 0)
        {
            return;
        }

        Sound sound = Array.Find(Sounds, s => s.Key == key);
        if (sound == null || sound.SoundClip.Length == 0)
        {
            return;
        }

        int index = Random.Range(0, sound.SoundClip.Length);
        AudioSFX.PlayOneShot(sound.SoundClip[index], sound.Volume);
        Debug.Log("Play Sfx");
    }

    public void PlayMusic(KeySound key)
    {
        if (!m_activeMusic)
        {
            return;
        }

        if (AudioMusic == null || Sounds.Length == 0)
        {
            return;
        }

        Sound sound = Array.Find(Sounds, s => s.Key == key);
        if (sound == null || sound.SoundClip.Length == 0)
        {
            return;
        }

        int index = Random.Range(0, sound.SoundClip.Length);
        AudioMusic.clip = sound.SoundClip[index];
        AudioMusic.volume = sound.Volume;
        AudioMusic.Play();
        Debug.Log("Play Music");
    }

    public void ActiveSfx()
    {
        if (AudioSFX == null)
        {
            return;
        }

        AudioSFX.enabled = true;
        m_activeSfx = true;
    }

    public void DisableSfx()
    {
        if (AudioSFX == null)
        {
            return;
        }

        AudioSFX.enabled = false;
        m_activeSfx = false;
    }

    public void ActiveMusic()
    {
        if (AudioMusic == null)
        {
            return;
        }

        AudioMusic.enabled = true;
        m_activeMusic = true;
    }

    public void DisableMusic()
    {
        if (AudioMusic == null)
        {
            return;
        }

        AudioMusic.enabled = false;
        m_activeMusic = false;
    }
}

[Serializable]
public class Sound
{
    public KeySound Key;

    [Range(0.0f, 1.0f)]
    public float Volume;

    public AudioClip[] SoundClip;
}

[Serializable]
public enum KeySound
{
    WalkFootStepStone,
    RunFootStepStone,
    Landing
}
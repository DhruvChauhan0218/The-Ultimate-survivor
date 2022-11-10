using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundManager
{
    public enum Sound
    {
        PlayerMove,
        PlayerAttack,
        CoinCollect,
        PotionCollect,
        KeyCollect,
        EnemyDie,
        ButtonClick
    }

    private static Dictionary<Sound, float> soundTimerDictionary;
    private static GameObject oneShotObj;
    private static AudioSource audioSource;

    public static void Initialize()
    {
        soundTimerDictionary = new Dictionary<Sound, float>();
        soundTimerDictionary[Sound.PlayerMove] = 0f;
    }

    public static void PlaySound(Sound sound)
    {
        if(CanPlaySound(sound))
        {
            if(oneShotObj == null)
            {
                oneShotObj = new GameObject("Sound");
                audioSource = oneShotObj.AddComponent<AudioSource>();
            }
            if(audioSource.isPlaying) { audioSource.Stop(); }
            audioSource.clip = GetAudioClip(sound);
            audioSource.volume = 0.6f;
            audioSource.Play();
        }
    }

    private static bool CanPlaySound(Sound sound)
    {
        switch(sound)
        {
            default:
                return true;

            case Sound.PlayerMove:

                if(soundTimerDictionary.ContainsKey(sound))
                {
                    float lastTimePlayed = soundTimerDictionary[sound];
                    float playerMoveTimerMax = 0.75f;
                    if(lastTimePlayed + playerMoveTimerMax < Time.time)
                    {
                        soundTimerDictionary[sound] = Time.time;
                        return true;
                    }

                    else
                    {
                        return false;
                    }
                }

                else
                {
                    return true;
                }
        }
    }

    private static AudioClip GetAudioClip(Sound sound)
    {
        foreach(GameManager.SoundAudioClip soundAudioClip in GameManager.Instance.soundAudioClips)
        {
            if(soundAudioClip.sound == sound)
            {
                return soundAudioClip.audioClip;
            }
        }
        Debug.LogError("Sound " + sound + " not Found...");
        return null;
    }

}

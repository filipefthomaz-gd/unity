using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    //AudioClips that will be playing on start of this scene;
    // AudioClip[0] -> BackgroundMusic;
    // AudioClip[1-3] -> SFX;
    public AudioClip[] audioClips;
    public float[] audioVolumes;

    //Music and SFX from the previous scene will carry over to the new scene;
    //FadeOut time is time for previous to fade out, and fadeIn to bring new sounds on;
    public float fadeInTime = 5;
    public float fadeOutTime = 3;

    //This AudioManager can be used for its functions and changing the audio midway through a scene, and not in the beginning of a scene
    public bool changeOnStart = true;

    //Some moments of the game have audio pitch variation;
    //If resetPitch = true, independently of how the previous audio was, it will force pitch to 1;
    public bool resetPitch = true;

    private AudioSource[] audioSources;

    private float[] realAudioVolumes;
    private AudioSource[] allAudioSources;

    // Start is called before the first frame update
    void Start()
    {
        realAudioVolumes = new float[4];
        if (changeOnStart)
        {
            changeMusic("BackgroundMusic");
            changeMusic("SoundEffects1");
            changeMusic("SoundEffects2");
            changeMusic("SoundEffects3");
            allAudioSources = FindObjectsOfType<AudioSource>();
            ///////

            //There are three slider settings for audio control: Dialogue, Music and SFX
            //SFX slider controls all AudioSource components besides BackgroundMusic and DialogueManager
            foreach (AudioSource audioSource in allAudioSources)
            {
                if (audioSource.name == "BackgroundMusic" || audioSource.name == "Dialogue Manager")
                    continue;
                else if (audioSource.name == "SoundEffects1" || audioSource.name == "SoundEffects2" || audioSource.name == "SoundEffects3")
                    continue;
                else
                    audioSource.volume = audioSource.volume * (AudioLevels.sfxVolumeFraction + 0.0001f);
            }
        }


    }

    private int listIndex;

    //Changes the Audio on the GameObject named 'gameObjectName'
    public void changeMusic(string gameObjectName)
    {
        //Determine index list for GameObject (0 is for BgMusic, and 1 to 3 are SFX)
        listIndex = stringToNumber(gameObjectName);

        //Loads both AudioSources in Audio GameObject
        audioSources = GameObject.Find(gameObjectName).GetComponents<AudioSource>();

        //Backgroundmusic is always looped
        if (listIndex == 0)
        {
            if (resetPitch)
            {
                audioSources[0].pitch = 1;
                audioSources[1].pitch = 1;
            }
            audioSources[0].loop = true;
            audioSources[1].loop = true;
        }

        //Next music and SFX to be loaded will be to a fraction of its original (max) volume
        //Fraction stored in a DontDestroyOnLoad AudioLevels class static variable changed via slider (see "adjust" functions in the end of this class);
        realAudioVolumes[0] = audioVolumes[0] * (AudioLevels.musicVolumeFraction+0.0001f);

        for (int i = 1; i <= 3; i++)
            realAudioVolumes[i] = audioVolumes[i] * (AudioLevels.sfxVolumeFraction+0.0001f);

        //If clip is the same (it may be due to debug purposes) don't do anything;
        if ((audioSources[0].clip == audioClips[listIndex] && audioSources[0].clip != null) ||
            (audioSources[1].clip == audioClips[listIndex] && audioSources[1].clip != null))
            return;


        //Swap AudioSources (Fades one out while fading the other in);
        if (audioSources[0].isPlaying)
            SwapMusic(audioSources[0], audioSources[1]);

        else if (audioSources[1].isPlaying)
            SwapMusic(audioSources[1], audioSources[0]);

        else
            SwapMusic(audioSources[1], audioSources[0]);

    }


    //Fades out the first argument audioSource and fades in the second argument to the clip and volume set publicly
    private void SwapMusic(AudioSource fadeOutAudio, AudioSource fadeInAudio)
    {
        StartCoroutine(FadeOut(fadeOutAudio, fadeOutTime)); //Fades Out the playing audioSource;
        if(audioClips[listIndex] != null)
        {
            fadeInAudio.clip = audioClips[listIndex];
            fadeInAudio.Play();
            StartCoroutine(FadeIn(fadeInAudio, 0.01f, fadeInTime, realAudioVolumes[listIndex]));
        }
    }

    private int number;

    public int stringToNumber(string gameObjectName)
    {
        if (gameObjectName == "BackgroundMusic")
            number = 0;
        else if (gameObjectName == "SoundEffects1")
            number = 1;
        else if (gameObjectName == "SoundEffects2")
            number = 2;
        else if (gameObjectName == "SoundEffects3")
            number = 3;

        return number;
    }


    //Audio Fade In and Fade Out functions
    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }
        audioSource.clip = null;
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float startFadeVolume, float FadeTime, float finalVolume)
    {
        if (finalVolume > startFadeVolume)
            audioSource.volume = startFadeVolume;
        else
            audioSource.volume = 0;

        while (audioSource.volume < finalVolume)
        {
            audioSource.volume += finalVolume * Time.deltaTime / FadeTime;

            yield return null;
        }
    }


    //AUDIO ADJUSTMENT VIA MAIN MENU

    //AudioLevels has the overall music and SFX fraction volumes as static variables (is a DontDestroyOnLoad)


    //Adjusts Music Volume by Slider
    public void adjustMusicVolume(float volumeFraction)
    {
        //Changes the current background music volume
        audioSources = GameObject.Find("BackgroundMusic").GetComponents<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
            audioSource.volume = audioSource.volume / (AudioLevels.musicVolumeFraction+0.0001f) * (volumeFraction+0.0001f);

        //Sets the volume via the slider and stores in a static variable
        AudioLevels.musicVolumeFraction = volumeFraction;
        MenuFunctions.saveSettings();
    }

    public void adjustSFXVolume(float volumeFraction)
    {
        //Changes the current SFX tracks volumes and all other AudioSourcers not BGmusic and Dialogue
        audioSources = FindObjectsOfType<AudioSource>();
        ///////
        ///
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.name == "BackgroundMusic" || audioSource.name == "Dialogue Manager")
                continue;

            else
                audioSource.volume = audioSource.volume / (AudioLevels.sfxVolumeFraction + 0.0001f) * (volumeFraction + 0.0001f);
        }

        AudioLevels.sfxVolumeFraction = volumeFraction; //Sets the volume via the slider and stores in a static variable
        MenuFunctions.saveSettings();
    }

    public void adjustDialogueVolume(float volumeFraction)
    {
        AudioLevels.dialogueVolumeFraction = volumeFraction; //Sets the volume via the slider and stores in a static variable
        MenuFunctions.saveSettings();
        try
        {
            GameObject.Find("Dialogue Manager").GetComponent<AudioSource>().volume = volumeFraction;
        }
        catch(Exception e)
        {
            return;
        }

    }

}

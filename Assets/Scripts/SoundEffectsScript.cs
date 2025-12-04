using UnityEngine;

public class SoundEffectsScript : MonoBehaviour
{
    public AudioClip[] soundEffects;
    public AudioSource audioSource;

    public void Hover()
    {
        PlaySFX(soundEffects[0]);
    }

    public void Click()
    {
        PlaySFX(soundEffects[1]);
    }

    public void OnDice()
    {
        audioSource.loop = true;
        audioSource.clip = soundEffects[2];
        audioSource.volume = SettingsMenu.SFXVolume;
        audioSource.Play();
    }

    public void CancelButton()
    {
        PlaySFX(soundEffects[3]);
    }

    public void PlayButton()
    {
        PlaySFX(soundEffects[4]);
    }

    public void NameField()
    {
        PlaySFX(soundEffects[5]);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, SettingsMenu.SFXVolume);
        }
    }
}
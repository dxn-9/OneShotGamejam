using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioClip[] soundEffects;
    [SerializeField] AudioClip winSFX;
    [SerializeField] AudioClip looseSFX;
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayPlacingSound()
    {
        var i = Random.Range(0, soundEffects.Length);
        audioSource.PlayOneShot(soundEffects[i]);
    }

    public void PlayWinSound()
    {
        audioSource.PlayOneShot(winSFX);
    }

    public void PlayLooseSound()
    {
        audioSource.PlayOneShot(looseSFX);
    }
}
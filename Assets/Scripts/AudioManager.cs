using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Play music (loops by default)
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Stop music
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // Play a one-shot SFX
    public void PlaySFX(AudioClip clip, float volume = 1f, float duration = -1f)
    {
        sfxSource.PlayOneShot(clip, volume);

        if (duration > 0f)
            StartCoroutine(StopSFXAfter(duration));
    }

    private IEnumerator StopSFXAfter(float time)
    {
        yield return new WaitForSeconds(time);

        // Only stop if it's still playing
        if (sfxSource.isPlaying)
            sfxSource.Stop();
    }
}

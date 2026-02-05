using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_BGM
{
    TUTORIAL,
}
public enum E_SFX
{
    WALK,
    PLACE,
    REMOVE,
    BUTTON_CLICK,
    ITEM_PICKUP,
    USE_ENERGY,
    //MISSION_CLEAR,
}

public class FYPSoundManager : MonoBehaviour
{
    public static FYPSoundManager i;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] sfxClips;

    [Header("Object Pool Settings")]
    [SerializeField] private int poolSize = 10;

    private Dictionary<E_BGM, AudioClip> bgmDict;
    private Dictionary<E_SFX, AudioClip> sfxDict;
    private Queue<AudioSource> audioSourcePool;

    private AudioSource bgmPlayer;
    
    private void Awake()
    {
        i = this;
        SetDictionary();
    }

    private void SetDictionary()
    {
        bgmDict = new Dictionary<E_BGM, AudioClip>();
        for (int i = 0; i < bgmClips.Length; i++)
        {
            bgmDict[(E_BGM)i] = bgmClips[i];
        }

        sfxDict = new Dictionary<E_SFX, AudioClip>();
        for (int i = 0; i < sfxClips.Length; i++)
        {
            sfxDict[(E_SFX)i] = sfxClips[i];
        }

        bgmPlayer = gameObject.AddComponent<AudioSource>();
        bgmPlayer.loop = true;
    
        SetPool();
    }

    private void SetPool()
    {
        audioSourcePool = new Queue<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.enabled = false;
            audioSourcePool.Enqueue(source);
        }
    }
    public void PlaySFX(E_SFX sfxType)
    {
        if (sfxDict.TryGetValue(sfxType, out var clip))
        {
            if (audioSourcePool.Count == 0) return;
            
            AudioSource source = audioSourcePool.Dequeue();
            source.clip = clip;
            source.enabled = true;
            source.Play();

            StartCoroutine(ReturnToPool(source, clip.length));

        }
        else
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.clip = clip;
            newSource.playOnAwake = false;
            newSource.enabled = true;
            newSource.Play();

            StartCoroutine(ReturnToPool(newSource, clip.length));
        }
    }

    public void PlayBGM(E_BGM bgmType)
    {
        if (bgmDict.TryGetValue(bgmType, out var clip))
        {
            if (bgmPlayer.clip != clip)
            {
                bgmPlayer.clip = clip;
                bgmPlayer.Play();
            }
        }
        else
        {
            Debug.LogWarning("BGM not found");
        }
    }
    

    private IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        source.enabled = false;
        audioSourcePool.Enqueue(source);
    }
}

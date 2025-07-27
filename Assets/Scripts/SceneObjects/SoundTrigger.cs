using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundTrigger : MonoBehaviour
{
    [SerializeField]
    AudioClip audioClip;

    AudioSource audioSource;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = audioClip;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class AudioInteractive : MonoBehaviour
{
    [Header("Audio Radio")]
    public AudioSource audioSource;
    public AudioClip radioClip;
    public bool loop = true;

    [Header("Interaction")]
    public Text promptText;
    public string interactKey = "E";
    public string promptOn = "Tắt radio (E)";
    public string promptOff = "Bật radio (E)";

    bool playerNearby;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = loop;
            if (radioClip != null)
                audioSource.clip = radioClip;
        }
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ToggleRadio();
        }
    }

    void ToggleRadio()
    {
        if (audioSource == null)
            return;

        if (audioSource.isPlaying)
            audioSource.Stop();
        else
            audioSource.Play();

        UpdatePrompt();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            UpdatePrompt();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (promptText != null)
                promptText.text = string.Empty;
        }
    }

    void UpdatePrompt()
    {
        if (promptText == null)
            return;

        if (playerNearby)
        {
            if (audioSource != null && audioSource.isPlaying)
                promptText.text = promptOn;
            else
                promptText.text = promptOff;
        }
    }
}

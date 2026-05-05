using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;

[Summary("Tương tác cơ bản để bật/tắt nguồn phát âm thanh (như Radio).")]
public class AudioInteractive : MonoBehaviour
{
    [Header("Audio Radio")]
    [Tooltip("Nguồn phát âm thanh.")]
    public AudioSource audioSource;
    [Tooltip("File âm thanh sẽ được phát.")]
    public AudioClip radioClip;
    [Tooltip("Có phát lặp lại (loop) hay không?")]
    public bool loop = true;

    [Header("Interaction")]
    [Tooltip("UI Text hiển thị phím bấm tương tác.")]
    public Text promptText;
    [Tooltip("Phím dùng để tương tác.")]
    public string interactKey = "E";
    [Tooltip("Dòng chữ hiển thị khi đang bật (tùy chọn tắt).")]
    public string promptOn = "Tắt radio (E)";
    [Tooltip("Dòng chữ hiển thị khi đang tắt (tùy chọn bật).")]
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

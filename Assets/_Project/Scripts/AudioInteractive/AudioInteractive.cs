using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AudioInteractive : MonoBehaviour
{
    [Header("Audio Radio")]
    public AudioSource audioSource;
    public AudioClip radioClip;
    public AudioClip buttonPressClip; // Âm thanh nhấn nút
    public bool loop = true;

    [Header("Interaction (Raycast)")]
    public float interactDistance = 3f; // Khoảng cách tối đa để tương tác
    public Text promptText;
    public string interactKey = "E";
    public string promptOn = "Tắt radio (E)";
    public string promptOff = "Bật radio (E)";

    private Transform playerCamera;
    private bool isLooking; // Thay thế cho playerNearby cũ

    void Start()
    {
        Debug.Log("[AudioInteractive] Start script");
        
        // Tự động tìm Camera chính của người chơi
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("[AudioInteractive] Không tìm thấy Camera.main! Hãy đảm bảo Camera của bạn có tag 'MainCamera'.");
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

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
        if (playerCamera == null) return;

        // 1. Bắn tia Raycast từ giữa màn hình (từ Camera bắn thẳng về phía trước)
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        bool hitThisRadio = false;

        // Kiểm tra xem tia Ray có trúng vật thể nào trong phạm vi interactDistance không
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // 2. Nếu vật thể bị tia Ray chiếu trúng CHÍNH LÀ cái Radio này
            if (hit.collider.gameObject == gameObject)
            {
                hitThisRadio = true;
            }
        }

        // 3. Xử lý logic hiển thị và bấm phím
        if (hitThisRadio)
        {
            if (!isLooking) // Vừa mới nhìn vào
            {
                isLooking = true;
                UpdatePrompt();
            }

            // Lắng nghe bấm phím E
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                ToggleRadio();
            }
        }
        else
        {
            if (isLooking) // Vừa quay mặt đi chỗ khác
            {
                isLooking = false;
                if (promptText != null) promptText.text = string.Empty;
            }
        }
    }

    void ToggleRadio()
    {
        // Phát âm thanh nhấn nút
        if (buttonPressClip != null)
        {
            AudioSource.PlayClipAtPoint(buttonPressClip, transform.position);
        }

        if (audioSource == null) return;

        if (audioSource.isPlaying)
            audioSource.Stop();
        else
            audioSource.Play();

        UpdatePrompt();
    }

    void UpdatePrompt()
    {
        if (promptText == null) return;

        if (isLooking)
        {
            if (audioSource != null && audioSource.isPlaying)
                promptText.text = promptOn;
            else
                promptText.text = promptOff;
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class InteractionSystem : MonoBehaviour
{
    public float interactRange = 2f;

    private PlayerInputHandler input;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        input.OnInteract += HandleInteract;
    }

    private void OnDisable()
    {
        input.OnInteract -= HandleInteract;
    }

    void HandleInteract()
    {
        // simple raycast forward
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactRange))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
            }
        }
    }
}

public interface IInteractable
{
    void Interact(InteractionSystem source);
}
using System.Collections.Generic;
using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Rigidbody Parenter")]
    [HelpBox("Attach this component to a movable object to which interactable objects should be parented. Allowed objects are only DraggableItem or InteractableItem.")]
    public class RigidbodyParenter : MonoBehaviour
    {
        [Space]
        public List<GameObject> ParentedObjects = new();
        public bool FlipUP = false;

        private void OnCollisionStay(Collision collision)
        {
            TryParentObject(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (ParentedObjects.Contains(collision.gameObject))
            {
                if (collision.transform.parent == transform)
                {
                    ParentedObjects.Remove(collision.gameObject);
                    collision.transform.SetParent(null);
                }
            }
        }

        private void TryParentObject(Collision collision)
        {
            if (collision.transform.parent == transform)
                return;

            if (collision.gameObject.TryGetComponent<DraggableItem>(out _)
                || collision.gameObject.TryGetComponent<InteractableItem>(out _))
            {
                // check if the collision object is on top
                foreach (ContactPoint contact in collision.contacts)
                {
                    float dot = Vector3.Dot(contact.normal, Vector3.up);
                    if (FlipUP ? (dot > 0.9f) : (dot < -0.9f))
                    {
                        if (!ParentedObjects.Contains(collision.gameObject))
                            ParentedObjects.Add(collision.gameObject);

                        collision.transform.SetParent(transform);
                        break;
                    }
                }
            }
        }
    }
}
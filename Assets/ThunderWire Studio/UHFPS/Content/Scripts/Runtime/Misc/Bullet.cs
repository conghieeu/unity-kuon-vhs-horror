using UnityEngine;

namespace UHFPS.Runtime
{
    public class Bullet : MonoBehaviour
    {
        public LayerMask CheckMask;
        public bool DestroyAfterTime = true;
        public float TimeAlive = 2f;

        private float force;
        private Vector3 direction;
        private Vector3 lastPosition;
        private bool forceSet;

        private void Awake()
        {
            lastPosition = transform.position;
            if(DestroyAfterTime) Destroy(gameObject, TimeAlive);
        }

        private void Update()
        {
            if (forceSet) transform.Translate(direction * force * Time.deltaTime);
            if (Physics.Linecast(lastPosition, transform.position, CheckMask))
                Destroy(gameObject);

            lastPosition = transform.position;
        }

        public void SetDirection(Vector3 direction, float force)
        {
            this.direction = direction;
            this.force = force;
            forceSet = true;
        }
    }
}
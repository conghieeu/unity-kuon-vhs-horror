using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý vật thể đạn (Bullet) và va chạm của đạn.")]
    public class Bullet : MonoBehaviour
    {
        [Tooltip("LayerMask để kiểm tra va chạm của đạn với môi trường.")]
        public LayerMask CheckMask;
        [Tooltip("Có tự động hủy viên đạn sau một khoảng thời gian không?")]
        public bool DestroyAfterTime = true;
        [Tooltip("Thời gian tồn tại của viên đạn trước khi bị hủy.")]
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
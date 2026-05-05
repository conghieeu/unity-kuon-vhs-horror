using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UHFPS.Scriptable;
using ThunderWire.Attributes;
using static UHFPS.Scriptable.SurfaceDefinitionSet;

namespace UHFPS.Runtime
{
    [Summary("Điều khiển vật phẩm Dao găm (Phiên bản cũ), quản lý đòn chém và đâm.")]
    public class KnifeItemOld : PlayerItemBehaviour
    {
        [System.Serializable]
        public struct SlashType
        {
            [Tooltip("Chỉ số của đòn đánh (truyền vào Animator).")]
            public ushort AttackIndex;
            [Tooltip("Góc độ chém.")]
            public float AttackAngle;
            [Tooltip("Hiển thị Gizmos góc chém.")]
            public bool Visualize;
        }

        [Header("Surface Settings")]
        [Tooltip("Tập hợp định nghĩa bề mặt khi bị dao chém trúng.")]
        public SurfaceDefinitionSet SurfaceDefinitionSet;
        [Tooltip("Phương pháp phát hiện bề mặt.")]
        public SurfaceDetection SurfaceDetection;
        [Tooltip("Thẻ (Tag) nhận diện bề mặt da thịt.")]
        public Tag FleshTag;

        [Header("Attack Setup")]
        [Tooltip("Lớp mạng (LayerMask) có thể bị chém trúng.")]
        public LayerMask RaycastMask;
        [Tooltip("Khoảng cách tối đa để chém trúng mục tiêu.")]
        public float AttackDistance;
        [Tooltip("Sát thương gây ra (ngẫu nhiên trong khoảng).")]
        public MinMaxInt AttackDamage;
        [Tooltip("Thời gian chờ giữa các lần tấn công.")]
        public float AttackWait;

        [Header("Animations")]
        [Tooltip("Trạng thái rút dao.")]
        public string KnifeDrawState = "KnifeDraw";
        [Tooltip("Trạng thái cất dao.")]
        public string KnifeHideState = "KnifeHide";
        [Tooltip("Trạng thái nghỉ.")]
        public string KnifeIdleState = "KnifeIdle";

        [Header("Triggers")]
        [Tooltip("Tham số Trigger cất dao.")]
        public string HideTrigger = "Hide";
        [Tooltip("Tham số Trigger tấn công.")]
        public string AttackTrigger = "Attack";
        [Tooltip("Tham số Integer loại hình tấn công (Chém/Đâm).")]
        public string AttackTypeTrigger = "AttackType";

        [Header("Attack Types")]
        [Tooltip("Các loại đòn chém (Chém ngang, chém dọc, ...).")]
        public SlashType[] SlashTypes;
        [Tooltip("Chỉ số của đòn đâm (Stab).")]
        public ushort StabIndex = 2;

        [Tooltip("Hiệu ứng máu bắn ra khi chém trúng da thịt.")]
        public GameObject FleshImpact;

        [Header("Sounds")]
        [Tooltip("Âm thanh vung dao khi chém.")]
        public SoundClip SlashWhoosh;
        [Tooltip("Âm thanh vung dao khi đâm.")]
        public SoundClip StabWhoosh;

        [Tooltip("Danh sách âm thanh khi chém trúng da thịt.")]
        public AudioClip[] FleshSlash;
        [Tooltip("Danh sách âm thanh khi đâm trúng da thịt.")]
        public AudioClip[] FleshStab;

        [Range(0f, 1f)]
        [Tooltip("Âm lượng mặc định khi chém.")]
        public float DefaultSlashVolume = 1f;
        [Range(0f, 1f)]
        [Tooltip("Âm lượng mặc định khi đâm.")]
        public float DefaultStabVolume = 1f;

        [Range(0f, 1f)]
        [Tooltip("Âm lượng khi chém trúng da thịt.")]
        public float FleshSlashVolume = 1f;
        [Range(0f, 1f)]
        [Tooltip("Âm lượng khi đâm trúng da thịt.")]
        public float FleshStabVolume = 1f;

        private AudioSource audioSource;
        private bool isEquipped;
        private bool isBusy;
        private bool isStab;

        private float attackTime;
        private float attackAngle;

        public override string Name => "Knife";

        public override bool IsBusy() => !isEquipped || isBusy;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public override void OnUpdate()
        {
            if (isEquipped && CanInteract)
            {
                if (attackTime > 0f) 
                    attackTime -= Time.deltaTime;

                if (InputManager.ReadButton(Controls.FIRE) && attackTime <= 0f)
                {
                    int attackType = Random.Range(0, SlashTypes.Length);
                    SlashType slashType = SlashTypes[attackType];
                    attackType = slashType.AttackIndex;
                    attackAngle = slashType.AttackAngle;

                    audioSource.PlayOneShotSoundClip(SlashWhoosh);

                    Animator.SetInteger(AttackTypeTrigger, attackType);
                    Animator.SetTrigger(AttackTrigger);
                    attackTime = AttackWait;
                    isStab = false;
                }
                else if (InputManager.ReadButton(Controls.ADS) && attackTime <= 0f)
                {
                    audioSource.PlayOneShotSoundClip(StabWhoosh);

                    Animator.SetInteger(AttackTypeTrigger, StabIndex);
                    Animator.SetTrigger(AttackTrigger);
                    attackTime = AttackWait;
                    attackAngle = 0f;
                    isStab = true;
                }
            }
        }

        public override void OnItemSelect()
        {
            ItemObject.SetActive(true);
            StartCoroutine(ShowKnife());
            isEquipped = false;
        }

        IEnumerator ShowKnife()
        {
            yield return new WaitForAnimatorClip(Animator, KnifeDrawState);
            isEquipped = true;
        }

        public override void OnItemDeselect()
        {
            StopAllCoroutines();
            StartCoroutine(HideKnife());
            Animator.SetTrigger(HideTrigger);
            isBusy = true;
        }

        IEnumerator HideKnife()
        {
            yield return new WaitForAnimatorClip(Animator, KnifeHideState);
            ItemObject.SetActive(false);
            isEquipped = false;
            isBusy = false;
        }

        public override void OnItemActivate()
        {
            StopAllCoroutines();
            ItemObject.SetActive(true);
            Animator.Play(KnifeIdleState);
            isBusy = false;
            isEquipped = true;
        }

        public override void OnItemDeactivate()
        {
            StopAllCoroutines();
            ItemObject.SetActive(false);
            isBusy = false;
            isEquipped = false;
        }

        public void OnAttack()
        {
            Ray ray = new Ray(PlayerItems.transform.position, PlayerItems.transform.forward);
            if(Physics.Raycast(ray, out RaycastHit hit, AttackDistance, RaycastMask))
            {
                if(hit.collider.TryGetComponent(out IDamagable damagable))
                {
                    int damage = AttackDamage.Random();
                    damagable.OnApplyDamage(damage, PlayerManager.transform);
                }

                if (damagable is NPCBodyPart or IHealthEntity)
                {
                    if(FleshImpact != null) Instantiate(FleshImpact, hit.point, Quaternion.identity);
                    AudioClip impactSound = isStab ? FleshStab.Random() : FleshSlash.Random();
                    float impactVolume = isStab ? FleshStabVolume : FleshSlashVolume;

                    if(impactSound != null)
                        AudioSource.PlayClipAtPoint(impactSound, hit.point, impactVolume);
                }
                else
                {
                    var surfaceDefinition = SurfaceDefinitionSet.GetTagSurface(hit.collider.gameObject);
                    if (surfaceDefinition != null)
                    {
                        GameObject hitmarkPrefab = surfaceDefinition.SurfaceMeleemarks.Random();
                        if (hitmarkPrefab != null)
                        {
                            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                            GameObject hitmark = Instantiate(hitmarkPrefab, hit.point, rotation, hit.collider.transform);

                            Vector3 camPos = PlayerManager.MainCamera.transform.position;
                            Vector3 relative = hitmark.transform.InverseTransformPoint(camPos);
                            int angle = Mathf.RoundToInt(Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg);
                            hitmark.transform.RotateAround(hit.point, hit.normal, angle);
                        }
                    }
                }
            }
        }

        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Vector3 forward = PlayerItems.transform.forward;
            Vector3 origin = PlayerItems.transform.position + forward;
            Vector3 previewDir = Quaternion.Euler(0f, 90f, 0f) * forward;

            float length = 0.5f;
            previewDir = previewDir.normalized * length;

            Gizmos.color = Color.green;
            foreach (var slashType in SlashTypes)
            {
                if (slashType.Visualize)
                {
                    Vector3 slashDir = Quaternion.Euler(0f, 0f, slashType.AttackAngle) * previewDir;
                    origin -= slashDir / 2f;
                    Gizmos.DrawRay(origin, slashDir);
                }
            }
        }
    }
}
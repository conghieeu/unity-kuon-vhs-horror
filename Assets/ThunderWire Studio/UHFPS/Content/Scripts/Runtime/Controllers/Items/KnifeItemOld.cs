using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UHFPS.Scriptable;
using static UHFPS.Scriptable.SurfaceDefinitionSet;

namespace UHFPS.Runtime
{
    public class KnifeItemOld : PlayerItemBehaviour
    {
        [System.Serializable]
        public struct SlashType
        {
            public ushort AttackIndex;
            public float AttackAngle;
            public bool Visualize;
        }

        public SurfaceDefinitionSet SurfaceDefinitionSet;
        public SurfaceDetection SurfaceDetection;
        public Tag FleshTag;

        public LayerMask RaycastMask;
        public float AttackDistance;
        public MinMaxInt AttackDamage;
        public float AttackWait;

        public string KnifeDrawState = "KnifeDraw";
        public string KnifeHideState = "KnifeHide";
        public string KnifeIdleState = "KnifeIdle";

        public string HideTrigger = "Hide";
        public string AttackTrigger = "Attack";
        public string AttackTypeTrigger = "AttackType";

        public SlashType[] SlashTypes;
        public ushort StabIndex = 2;

        public GameObject FleshImpact;

        public SoundClip SlashWhoosh;
        public SoundClip StabWhoosh;

        public AudioClip[] FleshSlash;
        public AudioClip[] FleshStab;

        [Range(0f, 1f)] public float DefaultSlashVolume = 1f;
        [Range(0f, 1f)] public float DefaultStabVolume = 1f;

        [Range(0f, 1f)] public float FleshSlashVolume = 1f;
        [Range(0f, 1f)] public float FleshStabVolume = 1f;

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
using System;
using System.Collections;
using System.Reactive.Disposables;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UHFPS.Scriptable;
using Newtonsoft.Json.Linq;
using static UHFPS.Scriptable.SurfaceDefinitionSet;

namespace UHFPS.Runtime
{
    public class GunItem : PlayerItemBehaviour
    {
        public enum WeaponTypeEnum { Semi, Auto }

        [Serializable]
        public sealed class BaseSettings
        {
            public int BaseDamage = 20;
            public float DropoffDistance = 500f;
            public float RangeModifier = 1f;
            public float ShootRange = 250.0f;

            [Header("Other")]
            public float RoundsPerMinute = 600f;
            public float Hitforce = 20.0f;
            public float HeadModifier = 1f;

            [Header("Debug")]
            public bool VisualizeDistance;
            [Range(3, 10)]
            public uint DropoffSections = 4;
        }

        [Serializable]
        public sealed class RecoilSettings
        {
            public float BaseRecoil = 0.1f;

            [Header("Recoil Multipliers")]
            public float ADSMultiplier = 0.5f;
            public float WalkMultiplier = 2f;
            public float RunMultiplier = 3f;
            public float CrouchMultiplier = 0.75f;
            public float CrouchWalkMultiplier = 0.5f;
            public float JumpMultiplier = 5f;

            [Header("Settings")]
            public float RecoilChangeSpeed = 0.1f;
            public float CrouchWalkVel = 0.1f;
            public float SteadyVelocity = 0.1f;
        }

        [Serializable]
        public sealed class GunProperties
        {
            public bool EnableAiming = true;
            public bool EnableAdsPos = true;
            public bool EnableAdsFov = true;
            public bool KeepReloadMagBullets = true;
        }

        [Serializable]
        public sealed class AimingSettings
        {
            public Transform AimingTransform;
            public Vector3 AimPosition;
            [Range(0f, 1f)]
            public float AimMotionsWeight = 0.5f;

            [Header("Weapon Model Aiming")]
            public float AimSpeed = 20f;
            public float BackAimSpeed = 30f;

            [Header("ADS Aiming")]
            public float ADSCameraFOV = 40f;
            public float ADSAimSpeed = 20f;
            public float ADSBackAimSpeed = 30f;

            [Header("Other")]
            public string FovOption = "fov";
        }

        [Serializable]
        public sealed class AttachmentSettings
        {
            public GameObject FlashlightAttachment;
            public Light FlashlightLight;
            public SoundClip SwitchSound;
        }

        [Serializable]
        public sealed class BulletSettings
        {
            public string AmmoTextFormat = "<size=30><color=#4d828f>{0}</color> / </size>{1}";
            public int BulletsPerMag = 0;
            public int BulletsPerShot = 1;
        }

        [Serializable]
        public sealed class BulletAndMuzzleFlash
        {
            public Transform BarrelEnd;
            public GameObject MuzzleFlash;

            [Header("Bullet Settings")]
            public GameObject BulletPrefab;
            public Color TrailColor = Color.white;
            public float BulletForce;

            [Header("Muzzle Flash Settings")]
            public Axis MuzzleAxis;
            public float MuzzleFlashDelayTime = 0.01f;
            public float MuzzleFlashShowTime = 0.01f;
        }

        [Serializable]
        public sealed class AnimationSettings
        {
            public string GunDrawState = "Draw";
            public string GunHideState = "Hide";
            public string GunIdleState = "Idle";
            public string GunShootState = "Shoot";
            public string GunReloadState = "Reload";

            [Header("Triggers")]
            public string HideTrigger = "Hide";
            public string ShootTrigger = "Shoot";
            public string ReloadTrigger = "Reload";
        }

        [Serializable]
        public sealed class GunSounds
        {
            public SoundClip DrawSound;
            public SoundClip HideSound;
            public SoundClip ShootSound;
        }

        public string GunName = "Pistol";
        public WeaponTypeEnum WeaponType;
        public LayerMask RaycastMask;
        
        public SurfaceDefinitionSet SurfaceDefinitionSet;
        public SurfaceDetection SurfaceDetection;
        public Tag FleshTag;

        public ItemGuid GunInventoryItem;
        public ItemGuid AmmoInventoryItem;
        public ItemGuid FlashlightAttachmentItem;

        public BaseSettings baseSettings;
        public RecoilSettings recoilSettings;
        public GunProperties gunProperties;
        public AimingSettings aimingSettings;
        public AttachmentSettings attachmentSettings;
        public BulletSettings bulletSettings;
        public BulletAndMuzzleFlash bulletAndMuzzleFlash;
        public AnimationSettings animationSettings;
        public GunSounds gunSounds;

        [SerializeField] private float newRecoil;
        [SerializeField] private int carryingBullets;
        [SerializeField] private int bulletsInMag;

        private GameManager gameManager;
        private Inventory inventory;
        private readonly CompositeDisposable disposables = new();

        private AudioSource audioSource;
        private MeshRenderer muzzleRenderer;

        private CanvasGroup ammoPanel;
        private TMPro.TMP_Text ammoText;

        private Vector3 aimVelocity;
        private Vector3 defaultAim;
        private float defaultFov;

        private float currentRecoil;
        private float currentFov;

        private float fireRate;
        private float fireTime;
        private float muzzleDelayTime;
        private float muzzleShowTime;

        private bool isEquipped;
        private bool isBusy;
        private bool isReloading;

        private bool flashlightAttached;
        private bool flashlightEnabled;

        public override string Name => GunName;

        public override bool IsBusy() => !isEquipped || isBusy;


        private void Awake()
        {
            gameManager = GameManager.Instance;
            inventory = Inventory.Instance;

            audioSource = GetComponent<AudioSource>();
            muzzleRenderer = bulletAndMuzzleFlash.MuzzleFlash.GetComponentInChildren<MeshRenderer>();

            var behaviours = gameManager.GraphicReferences.Value["Gun"];
            ammoPanel = (CanvasGroup)behaviours[0];
            ammoText = (TMPro.TMP_Text)behaviours[1];

            if(aimingSettings.AimingTransform != null)
                defaultAim = aimingSettings.AimingTransform.localPosition;

            defaultFov = PlayerManager.MainVirtualCamera.Lens.FieldOfView;
            currentFov = defaultFov;
            currentRecoil = recoilSettings.BaseRecoil;

            disposables.Add(inventory.OnInventoryChanged.Subscribe(item =>
            {
                if (item.guid == AmmoInventoryItem)
                {
                    carryingBullets = inventory.GetAllItemsQuantity(AmmoInventoryItem);
                    UpdateAmmoText();
                }
            }));
        }

        public override void Start()
        {
            base.Start();

            // observe fov change
            OptionsManager.ObserveOption(aimingSettings.FovOption, (value) =>
            {
                defaultFov = (int)value;
                currentFov = defaultFov;
                PlayerManager.MainVirtualCamera.Lens.FieldOfView = defaultFov;
            });
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            disposables.Dispose();
        }

        public override void OnUpdate()
        {
            if (!isEquipped) 
                return;

            fireRate = 60 / baseSettings.RoundsPerMinute;

            if (fireTime > 0) fireTime -= Time.deltaTime;
            fireTime = Mathf.Clamp(fireTime, 0, Mathf.Infinity);

            if (bulletAndMuzzleFlash.MuzzleFlash != null)
            {
                if(muzzleDelayTime > 0) muzzleDelayTime -= Time.deltaTime;
                else if(muzzleShowTime > 0)
                {
                    bulletAndMuzzleFlash.MuzzleFlash.SetActive(true);
                    muzzleShowTime -= Time.deltaTime;
                }
                else
                {
                    bulletAndMuzzleFlash.MuzzleFlash.SetActive(false);
                }

                muzzleShowTime = Mathf.Clamp(muzzleShowTime, 0, Mathf.Infinity);
                muzzleDelayTime = Mathf.Clamp(muzzleDelayTime, 0, Mathf.Infinity);
            }

            if (CanInteract)
            {
                if (WeaponType == WeaponTypeEnum.Semi)
                {
                    if (InputManager.ReadButtonOnce("Fire", Controls.FIRE) && !isReloading && fireTime <= 0)
                    {
                        FireOneBullet();
                        fireTime = fireRate;
                    }
                }
                else if (WeaponType == WeaponTypeEnum.Auto)
                {
                    if (InputManager.ReadButton(Controls.FIRE) && !isReloading && fireTime <= 0)
                    {
                        FireOneBullet();
                        fireTime = fireRate;
                    }
                }

                if (!isReloading)
                {
                    if (carryingBullets > 0 && bulletsInMag != bulletSettings.BulletsPerMag)
                    {
                        if (InputManager.ReadButtonOnce(this, Controls.RELOAD))
                        {
                            StartCoroutine(ReloadGun());
                            isReloading = true;
                        }
                    }
                }
                else
                {
                    fireTime = 0f;
                }

                if (flashlightAttached)
                {
                    if (InputManager.ReadButtonOnce(this, Controls.FLASHLIGHT) && !isReloading)
                    {
                        flashlightEnabled = !flashlightEnabled;
                        attachmentSettings.FlashlightLight.enabled = flashlightEnabled;
                        audioSource.PlayOneShotSoundClip(attachmentSettings.SwitchSound);
                    }
                }
            }

            if (gunProperties.EnableAiming)
            {
                if (CanInteract && !isReloading && InputManager.ReadButton(Controls.ADS))
                {
                    if(gunProperties.EnableAdsPos && aimingSettings.AimingTransform != null)
                        aimingSettings.AimingTransform.localPosition = Vector3.SmoothDamp(aimingSettings.AimingTransform.localPosition, aimingSettings.AimPosition, ref aimVelocity, aimingSettings.AimSpeed);
                    
                    if (gunProperties.EnableAdsFov)
                    {
                        currentFov = Mathf.Lerp(currentFov, aimingSettings.ADSCameraFOV, Time.deltaTime * aimingSettings.ADSAimSpeed);
                        PlayerManager.MainVirtualCamera.Lens.FieldOfView = currentFov;
                    }

                    MotionBlender.Weight = aimingSettings.AimMotionsWeight;
                }
                else
                {
                    if (gunProperties.EnableAdsPos && aimingSettings.AimingTransform != null)
                        aimingSettings.AimingTransform.localPosition = Vector3.SmoothDamp(aimingSettings.AimingTransform.localPosition, defaultAim, ref aimVelocity, aimingSettings.BackAimSpeed);
                    
                    if (gunProperties.EnableAdsFov)
                    {
                        currentFov = Mathf.Lerp(currentFov, defaultFov, Time.deltaTime * aimingSettings.ADSBackAimSpeed);
                        PlayerManager.MainVirtualCamera.Lens.FieldOfView = currentFov;
                    }

                    MotionBlender.Weight = 1f;
                }   
            }
        }

        private void FireOneBullet()
        {
            if (bulletsInMag <= 0)
            {
                bulletsInMag = 0;
                return;
            }

            Animator.SetTrigger(animationSettings.ShootTrigger);
            audioSource.PlayOneShotSoundClip(gunSounds.ShootSound);

            if (bulletAndMuzzleFlash.MuzzleFlash)
            {
                bulletAndMuzzleFlash.MuzzleFlash.SetActive(true);

                if (muzzleRenderer != null)
                {
                    Vector3 muzzleRot = muzzleRenderer.transform.localEulerAngles;
                    muzzleRot += 360 * UnityEngine.Random.value * bulletAndMuzzleFlash.MuzzleAxis.Convert();
                    muzzleRenderer.transform.localEulerAngles = muzzleRot;
                }

                muzzleDelayTime = bulletAndMuzzleFlash.MuzzleFlashDelayTime;
                muzzleShowTime = bulletAndMuzzleFlash.MuzzleFlashShowTime;
            }

            for (int i = 0; i < bulletSettings.BulletsPerShot; i++)
            {
                float width = UnityEngine.Random.Range(-1f, 1f) * currentRecoil;
                float height = UnityEngine.Random.Range(-1f, 1f) * currentRecoil;

                Transform mainCamera = PlayerManager.MainCamera.transform;
                Vector3 spray = mainCamera.forward + mainCamera.transform.right * width + mainCamera.up * height;
                Ray aim = new(mainCamera.position, spray.normalized);

                if (Physics.Raycast(aim, out RaycastHit hit, baseSettings.ShootRange, RaycastMask))
                    ShowBulletMark(hit, aim.direction);

                if (bulletAndMuzzleFlash.BulletPrefab != null)
                {
                    Vector3 direction = hit.collider != null ? hit.point - bulletAndMuzzleFlash.BarrelEnd.position : aim.direction;
                    direction.Normalize();

                    if ((direction.z > 0 && aim.direction.z > 0) || (direction.z < 0 && aim.direction.z < 0))
                    {
                        ShowWeaponTrace(direction);
                    }
                }
            }

            bulletsInMag--;
            ApplyEffect("Shoot");

            inventory.SetItemQuantity(GunInventoryItem, (ushort)bulletsInMag, false);
            UpdateAmmoText();
        }

        private void ShowBulletMark(RaycastHit hit, Vector3 dir)
        {
            bool isFlesh = false;
            if (hit.collider.TryGetComponent(out IDamagable damagable))
            {
                float baseDamage = baseSettings.BaseDamage;
                float rangeModifier = baseSettings.RangeModifier;
                float maxDistance = baseSettings.DropoffDistance;
                int damage = Mathf.RoundToInt(baseDamage * Mathf.Pow(rangeModifier, hit.distance / maxDistance));
                damagable.OnApplyDamage(damage, PlayerManager.transform);
                isFlesh = damagable is NPCBodyPart or IHealthEntity;
            }

            if (hit.rigidbody != null) hit.rigidbody.AddForceAtPosition(dir * baseSettings.Hitforce, hit.point);

            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            GameObject hitObject = hit.collider.gameObject;
            Vector3 hitPoint = hit.point;

            SurfaceDefinition surfaceDefinition = isFlesh
                ? SurfaceDefinitionSet.GetSurface(FleshTag)
                : SurfaceDefinitionSet.GetSurface(hitObject, hitPoint, SurfaceDetection);

            if (surfaceDefinition != null)
            {
                if (surfaceDefinition.SurfaceBulletmarks.Length > 0)
                {
                    GameObject hitPrefab = surfaceDefinition.SurfaceBulletmarks.Random();
                    GameObject bulletmark = Instantiate(hitPrefab, hitPoint, hitRotation);
                    bulletmark.transform.SetParent(hit.transform);
                }
            }
        }

        private void ShowWeaponTrace(Vector3 localAimDirection)
        {
            GameObject bullet = Instantiate(bulletAndMuzzleFlash.BulletPrefab, bulletAndMuzzleFlash.BarrelEnd.position, Quaternion.identity);
            bullet.GetComponent<Bullet>().SetDirection(localAimDirection, bulletAndMuzzleFlash.BulletForce * 10);

            if (bullet.TryGetComponent(out TrailRenderer trail))
                trail.material.SetColor("_BaseColor", bulletAndMuzzleFlash.TrailColor);
        }

        IEnumerator ReloadGun()
        {
            int bulletsToFullMag = gunProperties.KeepReloadMagBullets 
                ? bulletSettings.BulletsPerMag - bulletsInMag 
                : bulletSettings.BulletsPerMag;

            Animator.SetTrigger(animationSettings.ReloadTrigger);
            yield return new WaitForAnimatorStateExit(Animator, animationSettings.GunReloadState);

            if (carryingBullets >= bulletsToFullMag)
            {
                bulletsInMag = bulletSettings.BulletsPerMag;
            }
            else
            {
                bulletsInMag = gunProperties.KeepReloadMagBullets 
                    ? bulletsInMag += carryingBullets 
                    : carryingBullets;
            }

            carryingBullets -= bulletsToFullMag;
            carryingBullets = (int)Mathf.Clamp(carryingBullets, 0, Mathf.Infinity);

            inventory.SetItemQuantity(GunInventoryItem, (ushort)bulletsInMag, false);
            inventory.RemoveItemQuantityMany(AmmoInventoryItem, (ushort)bulletsToFullMag);
            UpdateAmmoText();

            isReloading = false;
            fireTime = 0;
        }

        private void UpdateAmmoText()
        {
            ammoText.text = string.Format(bulletSettings.AmmoTextFormat, bulletsInMag, carryingBullets);
        }

        public override bool CanCombine() => !flashlightAttached;

        public override void OnItemCombine(InventoryItem combineItem)
        {
            if (combineItem.ItemGuid != FlashlightAttachmentItem)
                return;

            inventory.RemoveItem(combineItem);
            attachmentSettings.FlashlightAttachment.SetActive(true);
            flashlightAttached = true;
        }

        public override void OnItemSelect()
        {
            CanvasGroupFader.StartFadeInstance(ammoPanel, true, 5f);

            bulletsInMag = inventory.GetItemQuantity(GunInventoryItem);
            carryingBullets = inventory.GetAllItemsQuantity(AmmoInventoryItem);
            UpdateAmmoText();

            ItemObject.SetActive(true);
            StartCoroutine(ShowGun());
            isEquipped = false;
        }

        IEnumerator ShowGun()
        {
            yield return new WaitForAnimatorClip(Animator, animationSettings.GunDrawState);
            isEquipped = true;
        }

        public override void OnItemDeselect()
        {
            CanvasGroupFader.StartFadeInstance(ammoPanel, false, 5f,
                () => ammoPanel.gameObject.SetActive(false));

            StopAllCoroutines();
            StartCoroutine(HideGun());
            Animator.SetTrigger(animationSettings.HideTrigger);

            if (flashlightAttached && attachmentSettings.FlashlightLight.enabled)
            {
                attachmentSettings.FlashlightLight.enabled = false;
                audioSource.PlayOneShotSoundClip(attachmentSettings.SwitchSound);
            }

            flashlightEnabled = false;
            isBusy = true;
        }

        IEnumerator HideGun()
        {
            yield return new WaitForAnimatorClip(Animator, animationSettings.GunHideState);
            ItemObject.SetActive(false);
            isEquipped = false;
            isBusy = false;
        }

        public override void OnItemActivate()
        {
            ammoPanel.alpha = 1f;
            ammoPanel.gameObject.SetActive(true);

            StopAllCoroutines();
            ItemObject.SetActive(true);
            Animator.Play(animationSettings.GunIdleState);

            bulletsInMag = inventory.GetItemQuantity(GunInventoryItem);
            carryingBullets = inventory.GetAllItemsQuantity(AmmoInventoryItem);

            isBusy = false;
            isEquipped = true;
        }

        public override void OnItemDeactivate()
        {
            ammoPanel.alpha = 0f;
            ammoPanel.gameObject.SetActive(false);

            StopAllCoroutines();
            ItemObject.SetActive(false);

            if (flashlightAttached && attachmentSettings.FlashlightLight.enabled)
                attachmentSettings.FlashlightLight.enabled = false;

            isBusy = false;
            isEquipped = false;
        }

        public override StorableCollection OnCustomSave()
        {
            return new StorableCollection()
            {
                { nameof(flashlightAttached), flashlightAttached }
            };
        }

        public override void OnCustomLoad(JToken data)
        {
            flashlightAttached = (bool)data[nameof(flashlightAttached)];
            attachmentSettings.FlashlightAttachment.SetActive(flashlightAttached);
        }

        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (baseSettings.VisualizeDistance)
            {
                Transform cam = PlayerManager.MainCamera.transform;

                Gizmos.color = Color.white;
                Gizmos.DrawRay(cam.position, cam.forward * baseSettings.DropoffDistance);

                float distanceIte = baseSettings.DropoffDistance / (baseSettings.DropoffSections - 1);
                for (int i = 0; i < baseSettings.DropoffSections; i++)
                {
                    float realDistance = distanceIte * i;
                    float damage = baseSettings.BaseDamage * Mathf.Pow(baseSettings.RangeModifier, realDistance / baseSettings.DropoffDistance);
                    Vector3 damageDistance = cam.position + cam.forward * realDistance;

                    Gizmos.color = Color.red.Alpha(0.75f);
                    Gizmos.DrawSphere(damageDistance, 0.2f);

#if UNITY_EDITOR
                    UnityEditor.Handles.Label(damageDistance, Mathf.RoundToInt(damage).ToString());
#endif
                }
            }
        }
    }
}
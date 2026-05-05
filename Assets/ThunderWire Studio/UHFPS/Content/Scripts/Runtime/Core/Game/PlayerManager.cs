using UnityEngine;
using Unity.Cinemachine;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Player Manager", space = false)]
    public class PlayerManager : MonoBehaviour, ISaveableCustom
    {
        [Header("Player References")]
        public Transform CameraHolder;
        public Camera MainCamera;
        public CinemachineCamera MainVirtualCamera;

        [Header("Load Options")]
        public bool LoadSelectedItem;

        /// <summary>
        /// Reference to a PlayerManager.
        /// </summary>
        public static PlayerManager Instance =>
            PlayerPresenceManager.Instance.PlayerManager;

        private CharacterController m_playerCollider;
        public CharacterController PlayerCollider
        {
            get
            {
                if (m_playerCollider == null)
                    m_playerCollider = GetComponent<CharacterController>();

                return m_playerCollider;
            }
        }

        private PlayerStateMachine m_playerStateMachine;
        public PlayerStateMachine PlayerStateMachine
        {
            get
            {
                if (m_playerStateMachine == null)
                    m_playerStateMachine = GetComponent<PlayerStateMachine>();

                return m_playerStateMachine;
            }
        }

        private PlayerHealth m_PlayerHealth;
        public PlayerHealth PlayerHealth
        {
            get
            {
                if (m_PlayerHealth == null)
                    m_PlayerHealth = GetComponent<PlayerHealth>();

                return m_PlayerHealth;
            }
        }

        private InteractController m_interactController;
        public InteractController InteractController
        {
            get
            {
                if (m_interactController == null)
                    m_interactController = GetComponentInChildren<InteractController>();

                return m_interactController;
            }
        }

        private LookController m_lookController;
        public LookController LookController
        {
            get
            {
                if (m_lookController == null)
                    m_lookController = GetComponentInChildren<LookController>();

                return m_lookController;
            }
        }

        private ExamineController m_examineController;
        public ExamineController ExamineController
        {
            get
            {
                if (m_examineController == null)
                    m_examineController = GetComponentInChildren<ExamineController>();

                return m_examineController;
            }
        }

        private PlayerItemsManager m_playerItems;
        public PlayerItemsManager PlayerItems
        {
            get
            {
                if (m_playerItems == null)
                    m_playerItems = GetComponentInChildren<PlayerItemsManager>();

                return m_playerItems;
            }
        }

        private MotionController m_motionController;
        public MotionController MotionController
        {
            get
            {
                if (m_motionController == null)
                    m_motionController = GetComponentInChildren<MotionController>();

                return m_motionController;
            }
        }

        private void Start()
        {
            if (!SaveGameManager.GameWillLoad || !SaveGameManager.GameStateExist)
            {
                // transfer player rotation to look rotation
                Vector3 rotation = transform.eulerAngles;
                if(LookController.PlayerForward == LookController.ForwardStyle.LookForward)
                    transform.rotation = Quaternion.identity;
                LookController.LookRotation.x = rotation.y;
            }
        }

        private void Update()
        {
            // keep the player rotation unchanged when PlayerForward is set to LookForward
            if (LookController.PlayerForward == LookController.ForwardStyle.LookForward)
                transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Parent player to object.
        /// </summary>
        public void ParentToObject(Transform parent, bool autoSync = true)
        {
            Physics.autoSyncTransforms = autoSync;
            LookController.ParentToObject(parent);
            transform.SetParent(parent);
        }

        /// <summary>
        /// Unparent player from object.
        /// </summary>
        public void UnparentFromObject()
        {
            Physics.autoSyncTransforms = false;
            LookController.UnparentFromObject();
            transform.SetParent(null);
        }

        /// <summary>
        /// This function is used to collect all local player data to be saved.
        /// </summary>
        public StorableCollection OnCustomSave()
        {
            StorableCollection data = new StorableCollection();
            data.Add("health", PlayerHealth.EntityHealth);

            StorableCollection playerItemsData = new StorableCollection();
            for (int i = 0; i < PlayerItems.PlayerItems.Count; i++)
            {
                var playerItem = PlayerItems.PlayerItems[i];
                var itemData = (playerItem as ISaveableCustom).OnCustomSave();
                playerItemsData.Add("playerItem_" + i, itemData);
            }

            if(LoadSelectedItem) data.Add("selectedItem", PlayerItems.CurrentItemIndex);
            data.Add("playerItems", playerItemsData);
            return data;
        }

        /// <summary>
        /// This function is used to load all stored local player data.
        /// </summary>
        public void OnCustomLoad(JToken data)
        {
            PlayerHealth.StartHealth = data["health"].ToObject<uint>();
            PlayerHealth.InitHealth();

            for (int i = 0; i < PlayerItems.PlayerItems.Count; i++)
            {
                var playerItem = PlayerItems.PlayerItems[i];
                var itemData = data["playerItems"]["playerItem_" + i];
                (playerItem as ISaveableCustom).OnCustomLoad(itemData);
            }

            if (LoadSelectedItem)
            {
                int itemIndex = (int)data["selectedItem"];
                if(itemIndex != -1) PlayerItems.ActivateItem(itemIndex);
            }
        }
    }
}
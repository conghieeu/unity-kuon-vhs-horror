using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Jumpscare Direct")]
    [Docs("https://docs.twgamesdev.com/uhfps/guides/jumpscares#direct-jumpscare")]
    [Summary("Component quản lý các mô hình Jumpscare 3D trực tiếp gắn trước Camera (Ví dụ: Ma nhảy ra trước mặt).")]
    public class JumpscareDirect : MonoBehaviour
    {
        [Serializable]
        public struct DirectModel
        {
            public string ModelID;
            public GameObject ModelObject;
        }

        [Tooltip("Danh sách các mô hình Jumpscare 3D được gắn trước Camera. ID dùng để gọi từ Jumpscare Trigger.")]
        public DirectModel[] JumpscareDirectModels;

        private GameObject directModel;
        private float directDuration;

        public void ShowDirectJumpscare(string modelID, float duration)
        {
            foreach (var direct in JumpscareDirectModels)
            {
                if (direct.ModelID == modelID)
                {
                    direct.ModelObject.SetActive(true);
                    directModel = direct.ModelObject;
                    break;
                }
            }

            if(directModel != null) directDuration = duration;
        }

        private void Update()
        {
            if(directDuration > 0f)
            {
                directDuration -= Time.deltaTime;
            }
            else if(directModel != null)
            {
                directModel.SetActive(false);
                directModel = null;
                directDuration = 0f;
            }
        }
    }
}
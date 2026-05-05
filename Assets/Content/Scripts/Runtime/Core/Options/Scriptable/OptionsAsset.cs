using System.Collections.Generic;
using System;
using UnityEngine;
using UHFPS.Runtime;

using ThunderWire.Attributes;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "Options", menuName = "UHFPS/Game/Options Asset")]
    [Summary("File ScriptableObject chứa toàn bộ thiết lập Options của game, được chia thành các chuyên mục (Section).")]
    public class OptionsAsset : ScriptableObject
    {
        [Serializable]
        public sealed class Section
        {
            public string Name;
            public string GUID;
        }

        [Serializable]
        public sealed class OptionsSection
        {
            public Section Section;
            [SerializeReference]
            public List<OptionModule> Items;
        }

        [Tooltip("Danh sách các danh mục Settings chính (Ví dụ: Video, Audio, Controls...).")]
        public List<OptionsSection> Sections = new();

        [Tooltip("Danh sách các UI Prefab (Slider, Toggle, Dropdown...) dùng để vẽ các tuỳ chọn lên màn hình.")]
        public List<OptionBehaviour> OptionPrefabs = new();

        public OptionsSection GetSection(string guid)
        {
            foreach (var section in Sections)
            {
                if (section.Section.GUID == guid)
                    return section;
            }

            return null;
        }
    }
}
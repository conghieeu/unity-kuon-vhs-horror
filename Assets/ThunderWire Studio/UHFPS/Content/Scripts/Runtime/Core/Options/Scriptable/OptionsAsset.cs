using System.Collections.Generic;
using System;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "Options", menuName = "UHFPS/Game/Options Asset")]
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

        public List<OptionsSection> Sections = new();
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
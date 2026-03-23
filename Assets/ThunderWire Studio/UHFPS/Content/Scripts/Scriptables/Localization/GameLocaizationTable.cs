using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "GameLocalizationTable", menuName = "UHFPS/Localization/Locaization Table")]
    public class GameLocaizationTable : ScriptableObject
    {
        [Serializable]
        public struct SheetItem
        {
            public int Id;
            public string Key;

            public SheetItem(string key, int id)
            {
                Id = id;
                Key = key;
            }
        }

        [Serializable]
        public struct TableData
        {
            public int Id;
            public string SectionName;
            public List<SheetItem> SectionSheet;

            public TableData(string section, int id)
            {
                Id = id;
                SectionName = section;
                SectionSheet = new List<SheetItem>();
            }
        }

        public List<LocalizationLanguage> Languages = new();
        public List<TableData> TableSheet = new();
    }
}
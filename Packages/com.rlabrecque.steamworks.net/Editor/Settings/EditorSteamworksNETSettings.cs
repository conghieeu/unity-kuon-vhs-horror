using System.IO;
using UnityEditor.Compilation;
using UnityEngine;

public sealed class EditorSteamworksNETSettings : ScriptableObject
{
    private const string FilePath = "ProjectSettings/SteamworksNETSettings.json";

    [Tooltip("Khi được bật, gói Steamworks.NET sẽ thêm các ký hiệu định nghĩa cần thiết vào dự án của bạn.")]
    [SerializeField]
    private bool canManageDefineSymbols = true;

    public bool CanManageDefineSymbols
    {
        get => canManageDefineSymbols;

        set
        {
            if (canManageDefineSymbols == value)
            {
                return;
            }

            canManageDefineSymbols = value;
            Save();

            if (canManageDefineSymbols)
            {
                // Reload domain to ensure that define symbols are applied correctly.
                CompilationPipeline.RequestScriptCompilation();
            }
        }
    }

    /// <summary>
    /// The instance of the SteamworksNETSettings class.
    /// </summary>
    public static EditorSteamworksNETSettings Instance
    {
        get
        {
            var json = File.Exists(FilePath) ? File.ReadAllText(FilePath) : "{}";
            var settings = CreateInstance<EditorSteamworksNETSettings>();
            JsonUtility.FromJsonOverwrite(json, settings);
            return settings;
        }
    }

    private void Save()
    {
        var jsonToSave = JsonUtility.ToJson(this, true);
        File.WriteAllText(FilePath, jsonToSave);
    }
}
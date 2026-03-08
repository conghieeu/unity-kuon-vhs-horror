using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

public class SetupInteractHelper
{
#if UNITY_EDITOR
    [MenuItem("Tools/Setup InteractObject")]
    public static void Setup()
    {
        GameObject interactObj = GameObject.Find("InteractObject");
        if (interactObj == null)
        {
            Debug.LogError("Could not find InteractObject");
            return;
        }

        // Add or get ActivationScript
        ActivationScript actScript = interactObj.GetComponent<ActivationScript>();
        if (actScript == null)
        {
            actScript = interactObj.AddComponent<ActivationScript>();
        }

        actScript.activationType = ActivationScript.ActivationType.Interact;

        // Ensure we have a script to log, because UnityEvent binding to static UnityLogger.Log(object) with argument via code is tricky
        // Let's create a local component to handle the logging
        InteractLogHandler logger = interactObj.GetComponent<InteractLogHandler>();
        if (logger == null)
        {
            logger = interactObj.AddComponent<InteractLogHandler>();
            logger.message = "Hello from InteractObject!";
        }

        // Clear existing events
        UnityEventBase eventBase = actScript.InteractEvent;
        for (int i = eventBase.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(actScript.InteractEvent, i);
        }

        // Add the listener
        UnityAction action = new UnityAction(logger.LogMessage);
        UnityEventTools.AddPersistentListener(actScript.InteractEvent, action);
        
        // Also ensure layer is hit by Player's InteractManager
        // InteractManager cullLayers check
        InteractManager im = Object.FindObjectOfType<InteractManager>();
        if (im != null)
        {
            // Set interact object to a layer that the cullLayers mask includes, or just change cullLayers to Everything
            im.cullLayers = -1; // Everything
            EditorUtility.SetDirty(im);
        }

        EditorUtility.SetDirty(actScript);
        EditorUtility.SetDirty(interactObj);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(interactObj.scene);
        
        Debug.Log("InteractObject setup successfully!");
    }
#endif
}

public class InteractLogHandler : MonoBehaviour
{
    public string message = "Interact Object Log Message";
    public void LogMessage()
    {
        UnityLogger.Log(message);
    }
}

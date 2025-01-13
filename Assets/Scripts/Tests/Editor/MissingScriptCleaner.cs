using UnityEditor;
using UnityEngine;

public class MissingScriptCleaner : MonoBehaviour
{
    [MenuItem("Tools/Clean Missing Scripts")]
    private static void CleanMissingScripts()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int removedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            var components = obj.GetComponents<Component>();
            SerializedObject serializedObject = new SerializedObject(obj);
            SerializedProperty prop = serializedObject.FindProperty("m_Component");

            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (components[i] == null)
                {
                    Debug.Log($"Removed missing script from: {obj.name}", obj);
                    prop.DeleteArrayElementAtIndex(i);
                    removedCount++;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        Debug.Log($"Removed {removedCount} missing scripts.");
    }
}

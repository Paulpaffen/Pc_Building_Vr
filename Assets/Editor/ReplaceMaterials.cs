using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ReplaceMaterials : EditorWindow
{
    [MenuItem("Tools/Replace Materials with URP")]
    public static void Replace()
    {
        string urpPath = "Assets/MaterialsURP"; // 📂 tu carpeta donde guardaste los URP
        var urpMats = AssetDatabase.LoadAllAssetsAtPath(urpPath);

        Material[] allUrpMats = Resources.FindObjectsOfTypeAll<Material>();

        foreach (var renderer in GameObject.FindObjectsOfType<MeshRenderer>())
        {
            var mats = renderer.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material oldMat = mats[i];
                if (oldMat == null) continue;

                // Busca un URP con el mismo nombre
                Material newMat = FindUrpMaterial(oldMat.name, allUrpMats);
                if (newMat != null && oldMat != newMat)
                {
                    mats[i] = newMat;
                    Debug.Log($"Reemplazado {oldMat.name} → {newMat.name} en {renderer.name}");
                }
            }
            renderer.sharedMaterials = mats;
        }
    }

    private static Material FindUrpMaterial(string name, Material[] all)
    {
        foreach (var m in all)
        {
            if (m != null && m.name == name)
                return m;
        }
        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReplacePrefabMaterialsEditor
{
    [MenuItem("Tools/Replace Materials in Prefabs (by name)")]
    public static void ReplaceMaterialsInAllPrefabs()
    {
        // 1) Cargar todos los materiales del proyecto en un diccionario por nombre
        string[] matGuids = AssetDatabase.FindAssets("t:Material");
        var matsByName = new Dictionary<string, Material>();
        foreach (var g in matGuids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            Material m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m == null) continue;
            string key = m.name;
            if (!matsByName.ContainsKey(key))
                matsByName.Add(key, m);
        }

        // 2) Buscar todos los prefabs del proyecto
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int prefabsProcessed = 0;
        int replacements = 0;

        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            bool changed = false;

            // Recorre Renderers (MeshRenderer, SkinnedMeshRenderer) dentro del prefab
            var renderers = prefabRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var rend in renderers)
            {
                var mats = rend.sharedMaterials;
                if (mats == null || mats.Length == 0) continue;

                bool localChange = false;
                for (int j = 0; j < mats.Length; j++)
                {
                    Material oldMat = mats[j];
                    if (oldMat == null) continue;

                    // normaliza el nombre por si tiene "(Instance)"
                    string oldName = oldMat.name;
                    if (oldName.EndsWith(" (Instance)"))
                        oldName = oldName.Replace(" (Instance)", "");

                    if (matsByName.TryGetValue(oldName, out Material newMat))
                    {
                        if (newMat != oldMat)
                        {
                            mats[j] = newMat;
                            localChange = true;
                            replacements++;
                        }
                    }
                }

                if (localChange)
                {
                    rend.sharedMaterials = mats; // aplica cambios a la instancia del prefab en memoria
                    changed = true;
                }
            }

            if (changed)
            {
                // Guarda el prefab modificado de vuelta al asset
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                prefabsProcessed++;
                Debug.Log($"[ReplacePrefabMaterials] Prefab actualizado: {prefabPath}");
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ReplacePrefabMaterials] Hecho. Prefabs procesados: {prefabsProcessed}. Reemplazos totales: {replacements}.");
    }
}
#endif

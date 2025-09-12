using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class ConvertGltfToURP
{
    const string backupFolder = "Assets/MaterialBackups";

    [MenuItem("Tools/Convert GLTF to URP (create backups)")]
    public static void ConvertAllGltfMaterials()
    {
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader == null)
        {
            Debug.LogError("[ConvertGLTF] Shader 'Universal Render Pipeline/Lit' no encontrado. Instala/activa URP.");
            return;
        }

        // Crear carpeta de backup si no existe
        if (!AssetDatabase.IsValidFolder(backupFolder))
        {
            AssetDatabase.CreateFolder("Assets", "MaterialBackups");
        }

        string[] guids = AssetDatabase.FindAssets("t:Material");
        int converted = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            string shaderName = mat.shader != null ? mat.shader.name.ToLower() : "";
            // condición de heurística para detectar materiales glTF/PBR
            bool looksLikeGltf = shaderName.Contains("gltf") || shaderName.Contains("pbr") || shaderName.Contains("pbrmetallic") || shaderName.Contains("metallicroughness");

            if (!looksLikeGltf) continue;

            // Backup (si no existe ya)
            string backupPath = Path.Combine(backupFolder, Path.GetFileName(path));
            if (!File.Exists(backupPath))
            {
                AssetDatabase.CopyAsset(path, backupPath);
            }

            // Hacemos la conversión IN-PLACE: cambiamos el shader al URP Lit
            mat.shader = urpShader;

            // 1) Albedo / Base Map
            Texture baseTex = null;
            if (mat.HasProperty("_BaseColor") && mat.HasProperty("_BaseMap"))
            {
                // Some GLTF importers use _BaseMap/_BaseColor
                baseTex = mat.GetTexture("_BaseMap");
                Color baseColor = mat.GetColor("_BaseColor");
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", baseColor);
                if (baseTex != null && mat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", baseTex);
            }
            else if (mat.HasProperty("_MainTex"))
            {
                baseTex = mat.GetTexture("_MainTex");
                Color col = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", baseTex);
                // also keep the legacy properties if present
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", col);
            }
            else
            {
                // try common alternative names
                string[] albedoProps = { "_BaseMap", "_MainTex", "_Albedo", "_BaseColorMap" };
                foreach (var p in albedoProps)
                {
                    if (mat.HasProperty(p))
                    {
                        baseTex = mat.GetTexture(p);
                        if (baseTex != null && mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", baseTex);
                        break;
                    }
                }
            }

            // 2) Metallic / Roughness factors (map to URP Metallic & Smoothness)
            float metallic = 0f;
            float roughness = 1f;
            if (mat.HasProperty("_Metallic")) metallic = mat.GetFloat("_Metallic");
            if (mat.HasProperty("_MetallicFactor")) metallic = mat.GetFloat("_MetallicFactor");
            if (mat.HasProperty("_Roughness")) roughness = mat.GetFloat("_Roughness");
            if (mat.HasProperty("_RoughnessFactor")) roughness = mat.GetFloat("_RoughnessFactor");

            // URP expects smoothness: smoothness = 1 - roughness
            float smoothness = 1f - Mathf.Clamp01(roughness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            // In some URP versions property names differ: try both
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            // 3) Normal map
            Texture normal = null;
            string[] normalProps = { "_NormalMap", "_BumpMap", "_NormalTexture" };
            foreach (var p in normalProps)
            {
                if (mat.HasProperty(p))
                {
                    normal = mat.GetTexture(p);
                    break;
                }
            }
            if (normal != null && mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", normal);
            if (normal != null && mat.HasProperty("_NormalMap")) mat.SetTexture("_NormalMap", normal);

            // 4) Emission
            if (mat.HasProperty("_EmissiveColor") || mat.HasProperty("_EmissionColor"))
            {
                Color e = mat.HasProperty("_EmissiveColor") ? mat.GetColor("_EmissiveColor") : (mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black);
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", e);
                if (mat.HasProperty("_EmissionMap") && mat.HasProperty("_EmissiveTexture"))
                {
                    Texture eTex = mat.GetTexture("_EmissiveTexture");
                    if (eTex != null) mat.SetTexture("_EmissionMap", eTex);
                }
                // habilitar keyword
                mat.EnableKeyword("_EMISSION");
            }

            // 5) Occlusion / AO (si hay)
            if (mat.HasProperty("_OcclusionMap") && mat.HasProperty("_OcclusionTexture"))
            {
                var ao = mat.GetTexture("_OcclusionTexture");
                if (ao != null) mat.SetTexture("_OcclusionMap", ao);
            }

            EditorUtility.SetDirty(mat);
            converted++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ConvertGLTF] Conversión terminada. Materiales procesados: {converted}. Backups en: {backupFolder}");
    }
}
#endif


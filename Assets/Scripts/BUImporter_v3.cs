using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public class BUImporter : AssetPostprocessor
{
    private List<Material> extractedMaterials = new List<Material>();
    private string shaderType;

    private Dictionary<string, List<string>> materialInfo;

    void OnPostprocessGameObjectWithUserProperties(GameObject obj, string[] propNames, object[] values)
    {
        for (int i = 0; i < propNames.Length; i++)
        {
            if (propNames[i] == "shader_type")
            {
                shaderType = values[i].ToString();
            }
            else if (propNames[i] == "material_info")
            {
                string materialInfoJson = values[i].ToString(); // Log the received JSON string
                // Deserialize the JSON string using Newtonsoft.Json
                materialInfo = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(materialInfoJson);
            }
        }

        // foreach (var kvp in materialInfo)
        // {
        //     string materialName = kvp.Key;
        //     List<string> textureNames = kvp.Value;

        //     Debug.Log("Material: " + materialName);
        //     foreach (string textureName in textureNames)
        //     {
        //         Debug.Log("Texture: " + textureName);
        //     }
        // }
    }

    void OnPostprocessModel(GameObject obj)
    {
        // Store the name of the object
        string objName = obj.name;
        if (objName.Contains("_LOD"))
        {
            // Split the name into parts using '_LOD' as the separator
            string[] parts = objName.Split(new string[] { "_LOD" }, StringSplitOptions.None);

            // Use the first part of the name (before '_LOD')
            objName = parts[0];
        }

        // Apply collider component to the model
        ApplyCollider(obj.transform);

        ModelImporter modelImporter = assetImporter as ModelImporter;
        modelImporter.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName, ModelImporterMaterialSearch.Everywhere);

        // Force re-import of the asset to refresh the model
        AssetDatabase.WriteImportSettingsIfDirty(assetPath);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        // Use the stored object name
        string materialDirectory = Path.Combine("Assets/Materials");
        string textureDirectory = Path.Combine("Assets/Textures");

        // Check if the directory exists, if not, create it
        if (!Directory.Exists(materialDirectory))
        {
            Directory.CreateDirectory(materialDirectory);
        }
        if (!Directory.Exists(textureDirectory))
        {
            Directory.CreateDirectory(textureDirectory);
        }

        // Create an instance of ModelImporter and call ExtractTextures
        ExtractTextures(assetPath, textureDirectory);

        // Delay the creation of the asset until after the import process has finished
        EditorApplication.delayCall += () =>
        {

            // Extract materials from the model
            ExtractMaterials(assetPath, materialDirectory);

            // Refresh the AssetDatabase to make sure the new material appears in the editor
            AssetDatabase.Refresh();

            // Assign textures to the extracted materials
            AssignTexturesToMaterials();

        };
    }

    void ApplyCollider(Transform obj)
    {
        if (obj.name.ToLower().Contains("collider"))
            obj.gameObject.AddComponent<MeshCollider>();

        foreach (Transform child in obj)
        {
            if (child.name.ToLower().Contains("collider"))
            {
                child.GetComponent<MeshRenderer>().enabled = false;
                ApplyCollider(child);
            }
            else
            {
                ApplyCollider(child);
            }
        }
    }

    void ExtractTextures(string assetPath, string textureDirectory)
    {
        ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (modelImporter != null && !string.IsNullOrEmpty(textureDirectory))
        {
            modelImporter.ExtractTextures(textureDirectory);
        }
    }

    void ExtractMaterials(string assetPath, string destinationPath)
    {
        HashSet<string> hashSet = new HashSet<string>();
        IEnumerable<UnityEngine.Object> enumerable = from x in AssetDatabase.LoadAllAssetsAtPath(assetPath)
                                                     where x.GetType() == typeof(Material)
                                                     select x;
        foreach (UnityEngine.Object item in enumerable)
        {
            string materialPath = Path.Combine(destinationPath, item.name) + ".mat";
            if (File.Exists(materialPath))
            {
                // Material already exists, skip extraction
                // Debug.Log("Material already exists: " + materialPath);
                Material existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (existingMaterial != null)
                {
                    extractedMaterials.Add(existingMaterial);
                }
                continue;
            }

            // Generate a unique asset path
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(materialPath);
            string value = AssetDatabase.ExtractAsset(item, uniquePath);
            if (string.IsNullOrEmpty(value))
            {
                hashSet.Add(assetPath);
                Material extractedMaterial = AssetDatabase.LoadAssetAtPath<Material>(uniquePath);
                if (extractedMaterial != null)
                {
                    extractedMaterials.Add(extractedMaterial);
                }
            }
        }

        foreach (string item2 in hashSet)
        {
            AssetDatabase.WriteImportSettingsIfDirty(item2);
            AssetDatabase.ImportAsset(item2, ImportAssetOptions.ForceUpdate);
        }
    }

    void AssignTexturesToMaterials()
    {
        // Loop through the materialInfo dictionary
        if (materialInfo != null)
        {
            foreach (var kvp in materialInfo)
            {
                string materialName = kvp.Key;
                List<string> textureNamesList = kvp.Value;
                // Convert List<string> to string[]
                string[] textureNames = textureNamesList.ToArray();

                // Get the material from the Materials folder
                string[] guids = AssetDatabase.FindAssets(materialName + " t:Material", new[] { "Assets/Materials" });
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

                    if (material != null)
                    {
                        if (shaderType == "STANDARD")
                        {
                            material.shader = Shader.Find("Standard");
                            StandardShader(material, textureNames);
                        }
                        else if (shaderType == "SPECULAR")
                        {
                            material.shader = Shader.Find("Standard (Specular setup)");
                            SpecularShader(material, textureNames);
                        }
                        else if (shaderType == "AUTODESK")
                        {
                            material.shader = Shader.Find("Autodesk Interactive");
                            AutodeskInteractiveShader(material, textureNames);
                        }
                        else if (shaderType == "URP_AUTODESK")
                        {
                            material.shader = Shader.Find("Universal Render Pipeline/Autodesk Interactive/AutodeskInteractive");
                            URPAutodeskInteractiveShader(material, textureNames);
                        }
                        else if (shaderType == "URP_LIT")
                        {
                            material.shader = Shader.Find("Universal Render Pipeline/Lit");
                            URPLitShader(material, textureNames);
                        }
                        else if (shaderType == "HDRP_AUTODESK")
                        {
                            material.shader = Shader.Find("HDRP/Autodesk Interactive/AutodeskInteractive");
                            HDRPAutodeskInteractiveShader(material, textureNames);
                        }
                        else if (shaderType == "HDRP_LIT")
                        {
                            material.shader = Shader.Find("HDRP/Lit");
                            HDRPLitShader(material, textureNames);
                        }
                        else
                        {
                            material.shader = Shader.Find("Standard");
                            StandardShader(material, textureNames);
                        }
                    }
                }
            }
        }
    }

    void StandardShader(Material material, string[] textureNames)
    {
        // Define a list of possible values for each texture type
        var albedoKeywords = new List<string> { "albedo", "color", "basecolor", "diffuse", "base" };
        var metallicKeywords = new List<string> { "metallic", "metalness", "metal", "glossiness" };
        var normalKeywords = new List<string> { "normal", "normalgl", "tangent" };
        var heightKeywords = new List<string> { "height", "displacement", "parallax", "bump" };
        var occlusionKeywords = new List<string> { "occlusion", "ambientocclusion", "ao", "ambient" };
        var emissionKeywords = new List<string> { "emission", "emissive", "glow" };

        foreach (string textureName in textureNames)
        {
            // Find the texture asset by name in the Textures folder
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture", new[] { "Assets/Textures" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (texture != null)
                {
                    // Split the texture name into parts
                    string[] textureParts = textureName.ToLower().Split('_');

                    // Check if the base name of the texture matches the base name of the material
                    if (textureParts.Length > 0)
                    {
                        // Get the type of the texture
                        string type = textureParts[textureParts.Length - 1];

                        // Check if the type contains any of the keywords
                        if (albedoKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_MainTex", texture);
                            // Debug.Log("Texture assigned: Albedo/Color/BaseColor");
                        }
                        else if (metallicKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_MetallicGlossMap", texture);
                            // Debug.Log("Texture assigned: Metallic");
                        }
                        else if (normalKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_BumpMap", texture);
                            // Debug.Log("Texture assigned: Normal");
                        }
                        else if (heightKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_ParallaxMap", texture);
                            // Debug.Log("Texture assigned: Height");
                        }
                        else if (occlusionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_OcclusionMap", texture);
                            // Debug.Log("Texture assigned: Occlusion");
                        }
                        else if (emissionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.EnableKeyword("_EMISSION");
                            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                            material.SetTexture("_EmissionMap", texture);
                            // Debug.Log("Texture assigned: Emission");
                        }
                    }
                }
            }
        }
    }

    void SpecularShader(Material material, string[] textureNames)
    {
        // Define a list of possible values for each texture type
        var albedoKeywords = new List<string> { "albedo", "color", "basecolor", "diffuse", "base" };
        var specularKeywords = new List<string> { "specular", "specularcolor", "specularmap", "glossiness", "smoothness" };
        var normalKeywords = new List<string> { "normal", "normalgl", "tangent" };
        var heightKeywords = new List<string> { "height", "displacement", "parallax", "bump" };
        var occlusionKeywords = new List<string> { "occlusion", "ambientocclusion", "ao", "ambient" };
        var emissionKeywords = new List<string> { "emission", "emissive", "glow" };

        foreach (string textureName in textureNames)
        {
            // Find the texture asset by name in the Textures folder
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture", new[] { "Assets/Textures" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (texture != null)
                {
                    // Split the texture name into parts
                    string[] textureParts = textureName.ToLower().Split('_');

                    // Check if the base name of the texture matches the base name of the material
                    if (textureParts.Length > 0)
                    {
                        // Get the type of the texture
                        string type = textureParts[textureParts.Length - 1];

                        // Check if the type contains any of the keywords
                        if (albedoKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_MainTex", texture);
                            // Debug.Log("Texture assigned: Albedo/Color/BaseColor");
                        }
                        else if (specularKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_MetallicGlossMap", texture);
                            // Debug.Log("Texture assigned: Metallic");
                        }
                        else if (normalKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_BumpMap", texture);
                            // Debug.Log("Texture assigned: Normal");
                        }
                        else if (heightKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_ParallaxMap", texture);
                            // Debug.Log("Texture assigned: Height");
                        }
                        else if (occlusionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_OcclusionMap", texture);
                            // Debug.Log("Texture assigned: Occlusion");
                        }
                        else if (emissionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.EnableKeyword("_EMISSION");
                            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                            material.SetTexture("_EmissionMap", texture);
                            // Debug.Log("Texture assigned: Emission");
                        }
                    }
                }
            }
        }
    }

    void AutodeskInteractiveShader(Material material, string[] textureNames)
    {
        // Define a list of possible values for each texture type
        var albedoKeywords = new List<string> { "albedo", "color", "basecolor", "diffuse", "base" };
        var metallicKeywords = new List<string> { "metallic", "metalness", "metal", };
        var roughnessKeywords = new List<string> { "roughness", "glossiness" };
        var normalKeywords = new List<string> { "normal", "normalgl", "tangent" };
        var heightKeywords = new List<string> { "height", "displacement", "parallax", "bump" };
        var occlusionKeywords = new List<string> { "occlusion", "ambientocclusion", "ao", "ambient" };
        var emissionKeywords = new List<string> { "emission", "emissive", "glow" };

        foreach (string textureName in textureNames)
        {
            // Find the texture asset by name in the Textures folder
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture", new[] { "Assets/Textures" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (texture != null)
                {
                    // Split the texture name into parts
                    string[] textureParts = textureName.ToLower().Split('_');

                    // Check if the base name of the texture matches the base name of the material
                    if (textureParts.Length > 0)
                    {
                        // Get the type of the texture
                        string type = textureParts[textureParts.Length - 1];

                        // Check if the type contains any of the keywords
                        if (albedoKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_MainTex", texture);
                            // Debug.Log("Texture assigned: Albedo/Color/BaseColor");
                        }
                        else if (metallicKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_MetallicGlossMap", texture);
                            // Debug.Log("Texture assigned: Metallic");
                        }
                        else if (roughnessKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_SpecGlossMap", texture);
                            // Debug.Log("Texture assigned: Roughness");
                        }
                        else if (normalKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_BumpMap", texture);
                            // Debug.Log("Texture assigned: Normal");
                        }
                        else if (heightKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_ParallaxMap", texture);
                            // Debug.Log("Texture assigned: Height");
                        }
                        else if (occlusionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_OcclusionMap", texture);
                            // Debug.Log("Texture assigned: Occlusion");
                        }
                        else if (emissionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.EnableKeyword("_EMISSION");
                            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                            material.SetTexture("_EmissionMap", texture);
                            // Debug.Log("Texture assigned: Emission");
                        }
                    }
                }
            }
        }
    }

    void URPAutodeskInteractiveShader(Material material, string[] textureNames)
    {
        // Define a list of possible values for each texture type
        var baseColorKeywords = new List<string> { "albedo", "color", "basecolor", "diffuse", "base" };
        var normalKeywords = new List<string> { "normal", "normalgl", "tangent" };
        var metallicKeywords = new List<string> { "metallic", "metalness", "metal", };
        var roughnessKeywords = new List<string> { "roughness", "glossiness" };
        var emissionKeywords = new List<string> { "emission", "emissive", "glow" };
        var occlusionKeywords = new List<string> { "occlusion", "ambientocclusion", "ao", "ambient" };

        foreach (string textureName in textureNames)
        {
            // Find the texture asset by name in the Textures folder
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture", new[] { "Assets/Textures" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (texture != null)
                {
                    // Split the texture name into parts
                    string[] textureParts = textureName.ToLower().Split('_');

                    // Check if the base name of the texture matches the base name of the material
                    if (textureParts.Length > 0)
                    {
                        // Get the type of the texture
                        string type = textureParts[textureParts.Length - 1];

                        // Check if the type contains any of the keywords
                        if (baseColorKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseColorMap", 1);
                            material.SetTexture("_MainTex", texture);
                            // Debug.Log("Texture assigned: Albedo/Color/BaseColor");
                        }
                        else if (normalKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseNormalMap", 1);
                            material.SetTexture("_BumpMap", texture);
                            // Debug.Log("Texture assigned: Normal");
                        }
                        else if (metallicKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseMetallicMap", 1);
                            material.SetTexture("_MetallicGlossMap", texture);
                            // Debug.Log("Texture assigned: Metallic");
                        }
                        else if (roughnessKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseRoughnessMap", 1);
                            material.SetTexture("_SpecGlossMap", texture);
                            // Debug.Log("Texture assigned: Roughness");
                        }
                        else if (emissionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseEmissiveMap", 1);
                            material.SetTexture("_EmissionMap", texture);
                            // Debug.Log("Texture assigned: Emission");
                        }
                        else if (occlusionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseAoMap", 1);
                            material.SetTexture("_OcclusionMap", texture);
                            // Debug.Log("Texture assigned: Occlusion");
                        }
                    }
                }
            }
        }
    }

    void URPLitShader(Material material, string[] textureNames)
    {
        // Define a list of possible values for each texture type
        var albedoKeywords = new List<string> { "albedo", "color", "basecolor", "diffuse", "base" };
        var metallicKeywords = new List<string> { "metallic", "metalness", "metal", "glossiness" };
        var normalKeywords = new List<string> { "normal", "normalgl", "tangent" };
        var heightKeywords = new List<string> { "height", "displacement", "parallax", "bump" };
        var occlusionKeywords = new List<string> { "occlusion", "ambientocclusion", "ao", "ambient" };
        var emissionKeywords = new List<string> { "emission", "emissive", "glow" };

        foreach (string textureName in textureNames)
        {
            // Find the texture asset by name in the Textures folder
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture", new[] { "Assets/Textures" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (texture != null)
                {
                    // Split the texture name into parts
                    string[] textureParts = textureName.ToLower().Split('_');

                    // Check if the base name of the texture matches the base name of the material
                    if (textureParts.Length > 0)
                    {
                        // Get the type of the texture
                        string type = textureParts[textureParts.Length - 1];

                        // Check if the type contains any of the keywords
                        if (albedoKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_BaseMap", texture);
                            // Debug.Log("Texture assigned: Albedo/Color/BaseColor");
                        }
                        else if (metallicKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_MetallicGlossMap", texture);
                            // Debug.Log("Texture assigned: Metallic");
                        }
                        else if (normalKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_BumpMap", texture);
                            // Debug.Log("Texture assigned: Normal");
                        }
                        else if (heightKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_ParallaxMap", texture);
                            // Debug.Log("Texture assigned: Height");
                        }
                        else if (occlusionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_OcclusionMap", texture);
                            // Debug.Log("Texture assigned: Occlusion");
                        }
                        else if (emissionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.EnableKeyword("_EMISSION");
                            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                            material.SetTexture("_EmissionMap", texture);
                            // Debug.Log("Texture assigned: Emission");
                        }
                    }
                }
            }
        }
    }


    void HDRPAutodeskInteractiveShader(Material material, string[] textureNames)
    {
        // Define a list of possible values for each texture type
        var baseColorKeywords = new List<string> { "albedo", "color", "basecolor", "diffuse", "base" };
        var normalKeywords = new List<string> { "normal", "normalgl", "tangent" };
        var metallicKeywords = new List<string> { "metallic", "metalness", "metal", };
        var roughnessKeywords = new List<string> { "roughness", "glossiness" };
        var emissionKeywords = new List<string> { "emission", "emissive", "glow" };
        var occlusionKeywords = new List<string> { "occlusion", "ambientocclusion", "ao", "ambient" };

        foreach (string textureName in textureNames)
        {
            // Find the texture asset by name in the Textures folder
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture", new[] { "Assets/Textures" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (texture != null)
                {
                    // Split the texture name into parts
                    string[] textureParts = textureName.ToLower().Split('_');

                    // Check if the base name of the texture matches the base name of the material
                    if (textureParts.Length > 0)
                    {
                        // Get the type of the texture
                        string type = textureParts[textureParts.Length - 1];

                        // Check if the type contains any of the keywords
                        if (baseColorKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseColorMap", 1);
                            material.SetTexture("_MainTex", texture);
                            // Debug.Log("Texture assigned: Albedo/Color/BaseColor");
                        }
                        else if (normalKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseNormalMap", 1);
                            material.SetTexture("_BumpMap", texture);
                            // Debug.Log("Texture assigned: Normal");
                        }
                        else if (metallicKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseMetallicMap", 1);
                            material.SetTexture("_MetallicGlossMap", texture);
                            // Debug.Log("Texture assigned: Metallic");
                        }
                        else if (roughnessKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseRoughnessMap", 1);
                            material.SetTexture("_SpecGlossMap", texture);
                            // Debug.Log("Texture assigned: Roughness");
                        }
                        else if (emissionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseEmissiveMap", 1);
                            material.SetTexture("_EmissionMap", texture);
                            // Debug.Log("Texture assigned: Emission");
                        }
                        else if (occlusionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetInt("_UseAoMap", 1);
                            material.SetTexture("_OcclusionMap", texture);
                            // Debug.Log("Texture assigned: Occlusion");
                        }
                    }
                }
            }
        }
    }

    void HDRPLitShader(Material material, string[] textureNames)
    {
        // Define a list of possible values for each texture type
        var baseKeywords = new List<string> { "albedo", "color", "basecolor", "diffuse", "base" };
        var maskKeywords = new List<string> { "mask" };
        var normalKeywords = new List<string> { "normal", "normalgl", "tangent" };
        var bentNormalKeywords = new List<string> { "bent_normal", "bent_normalgl", "bent_tangent" };
        var coatKeywords = new List<string> { "coat" };
        var detailKeywords = new List<string> { "detail" };
        var emissionKeywords = new List<string> { "emission", "emissive", "glow" };

        foreach (string textureName in textureNames)
        {
            // Find the texture asset by name in the Textures folder
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture", new[] { "Assets/Textures" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (texture != null)
                {
                    // Split the texture name into parts
                    string[] textureParts = textureName.ToLower().Split('_');

                    // Check if the base name of the texture matches the base name of the material
                    if (textureParts.Length > 0)
                    {
                        // Get the type of the texture
                        string type = textureParts[textureParts.Length - 1];

                        // Check if the type contains any of the keywords
                        if (baseKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_BaseColorMap", texture);
                            // Debug.Log("Texture assigned: Base");
                        }
                        else if (maskKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_MaskMap", texture);
                            // Debug.Log("Texture assigned: Mask");
                        }
                        else if (normalKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_NormalMap", texture);
                            // Debug.Log("Texture assigned: Normal");
                        }
                        else if (bentNormalKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_BentNormalMap", texture);
                            // Debug.Log("Texture assigned: Bent Normal");
                        }
                        else if (coatKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_CoatMaskMap", texture);
                            // Debug.Log("Texture assigned: Coat");
                        }
                        else if (detailKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.SetTexture("_DetailMap", texture);
                            // Debug.Log("Texture assigned: Detail");
                        }
                        else if (emissionKeywords.Any(keyword => type.Contains(keyword)))
                        {
                            material.EnableKeyword("_UseEmissiveIntensity");
                            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                            material.SetTexture("_EmissiveColorMap", texture);
                            // Debug.Log("Texture assigned: Emission");
                        }
                    }
                }
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshCombinerPostprocessor : AssetPostprocessor {
    void OnPostprocessModel(GameObject importedModel) {
        var allMeshFilters = importedModel.GetComponentsInChildren<MeshFilter>(true);
        var combineInstances = new List<CombineInstance>();
        foreach (var mf in allMeshFilters) {
            if (mf.sharedMesh == null) continue;
            combineInstances.Add(new CombineInstance {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            });
        }
        if (combineInstances.Count == 0) return;
        var rootMF = importedModel.GetComponent<MeshFilter>() ?? 
                     importedModel.AddComponent<MeshFilter>();
        var rootMR = importedModel.GetComponent<MeshRenderer>() ?? 
                     importedModel.AddComponent<MeshRenderer>();
        var combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances.ToArray());
        rootMF.sharedMesh = combinedMesh;
        foreach (var mf in allMeshFilters) {
            if (mf != rootMF) mf.gameObject.SetActive(false);
        }
    }
}

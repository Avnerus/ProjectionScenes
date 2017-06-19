using UnityEngine;
[ExecuteInEditMode]
public class ProjectionScenes : MonoBehaviour
{
    public string unfoldingDataFileName = "data.json";
    [Range(0.0f, 180.0f)]
    public float sceneThreshold = 0.0F;


    /*

    void OnRenderObject() {
        if (_scenesMesh) {
            Debug.Log("Drawing scenes mesh");
            Graphics.DrawMeshNow(_scenesMesh, Vector3.zero, Quaternion.identity);
        }
    }*/
    
}

using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[CustomEditor( typeof( ProjectionScenes ) )]
public class ProjectionScenesEditor : Editor
{
    private PreviewRenderUtility _previewRenderUtility;
    private GameObject _previewObject;
    private Mesh _unfoldingMesh;
    private Material _vertexColorMaterial;

    private Vector2 _rotation = Vector2.zero;

    void OnEnable () {
        _previewRenderUtility = new PreviewRenderUtility (false);
        _previewRenderUtility.m_CameraFieldOfView = 30f;

        _previewRenderUtility.m_Camera.farClipPlane = 1000;
        _previewRenderUtility.m_Camera.nearClipPlane = 0.3f;

        ProjectionScenes t = target as ProjectionScenes;

        _previewObject = t.gameObject;

        Shader vertexColorShader = Shader.Find("Custom/VertexColorShader");

        _vertexColorMaterial = new Material(vertexColorShader);

        Debug.Log("Projection Scenes Editor - Target object set to " + _previewObject);
        LoadUnfoldingData();
    }
     void OnDisable ()
    {
        _previewRenderUtility.Cleanup ();
        _previewRenderUtility = null;
        _previewObject = null;
    }
    public override bool HasPreviewGUI () {
        return true;
    }
    public override void OnInteractivePreviewGUI (Rect r, GUIStyle background) {

        _previewRenderUtility.BeginPreview (r, background);

        Camera previewCamera = _previewRenderUtility.m_Camera;

        previewCamera.backgroundColor = Color.black;
        previewCamera.clearFlags = CameraClearFlags.Color;
        previewCamera.transform.position =
                                Vector3.zero + Vector3.forward * -50;

        previewCamera.transform.LookAt (Vector3.zero);

        Vector2 drag = Vector2.zero;


        bool dirty = false;

        if (Event.current.type == EventType.MouseDrag) {
            drag = Event.current.delta;
            dirty = true;
        }
        
        _previewRenderUtility.DrawMesh(_unfoldingMesh, Vector3.zero, RotatePreviewObject(drag), _vertexColorMaterial , 0);
        previewCamera.Render ();

        if (dirty) {
            Repaint ();
            dirty = false;
        }

        _previewRenderUtility.EndAndDrawPreview (r);


    }

    private Quaternion RotatePreviewObject (Vector2 drag)
    {

        _rotation.x -= drag.x;
        _rotation.y -= drag.y;

        Quaternion aroundY = Quaternion.Euler(_rotation.y  , 0, 0);
        Quaternion aroundX = Quaternion.Euler(0, _rotation.x , 0);

        return aroundX * aroundY;

    }

    void LoadUnfoldingData() {
        ProjectionScenes t = target as ProjectionScenes;
        string filePath = Path.Combine(Application.dataPath, t.unfoldingDataFileName);
        Debug.Log(filePath);
        if(File.Exists(filePath)) {
            List<Vector3> vertices = new List<Vector3>();;
            List<int> indices = new List<int>();
            List<Color> colors = new List<Color>();
            //Vector3[] normals;

            int index = 0;

            _unfoldingMesh = new Mesh();
            string dataAsJson = File.ReadAllText(filePath);
            JObject root = JObject.Parse(dataAsJson);
            foreach (JObject face in root["faces"].Children<JObject>()) {
                foreach (JObject vertex in face["vtx"].Children<JObject>()) {
                    Vector3 vector = vertex.ToObject<Vector3>();
                    vertices.Add(vector);
                    indices.Add(index);
                    colors.Add(Color.white);
                    index++;
                }
            }

            Debug.Log(index + " Vertices");

            _unfoldingMesh.SetVertices(vertices);
            _unfoldingMesh.SetColors(colors);
            _unfoldingMesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0); 

            Debug.Log("unfolding mesh ready");
        }
        else {　
            Debug.LogError("Cannot load unfolding data!");
        }


    }

    /*
    void OnSceneGUI( ) {
        if (!wasDrawn) {
            Debug.Log("OnSceneGUI!");
            wasDrawn = true;

            ProjectionScenes t = target as ProjectionScenes;

        }
    }*/

}

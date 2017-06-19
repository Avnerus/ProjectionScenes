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

    private float _sceneThreshold;

    void OnEnable () {
        _previewRenderUtility = new PreviewRenderUtility (false);
        _previewRenderUtility.m_CameraFieldOfView = 30f;

        _previewRenderUtility.m_Camera.farClipPlane = 1000;
        _previewRenderUtility.m_Camera.nearClipPlane = 0.3f;

        ProjectionScenes t = target as ProjectionScenes;

        _previewObject = t.gameObject;
        _sceneThreshold = t.sceneThreshold;

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
            List<Vector3> normals = new List<Vector3>();

            int vertexIndex = 0;

            _unfoldingMesh = new Mesh();
            string dataAsJson = File.ReadAllText(filePath);

            JObject root = JObject.Parse(dataAsJson);
            // Arrange the faces in a dictionary
            //
            Dictionary<int, JObject> faces = new Dictionary<int,JObject>();
            Dictionary<int, bool> processed = new Dictionary<int,bool>();

            foreach (JObject face in root["faces"].Children<JObject>()) {
                int faceId = face["id"].ToObject<int>();
                faces[faceId] = face;
                processed[faceId] = false;
            }

            // Just start with 0
            ProcessFace(faces[0], faces, processed, ref vertexIndex, Random.ColorHSV(), vertices, indices, colors, normals );
            /*
                float colorFactor = (40 * (faceId + 1)) / 255.0f;
                Color faceColor = new Color(colorFactor, colorFactor, colorFactor, 1.0f);
            }*/

            Debug.Log(vertexIndex + " Vertices");

            _unfoldingMesh.SetVertices(vertices);
            _unfoldingMesh.SetColors(colors);
            _unfoldingMesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0); 
            _unfoldingMesh.SetNormals(normals);

            Debug.Log("unfolding mesh ready");
        }
        else {　
            Debug.LogError("Cannot load unfolding data!");
        }


    }

    void ProcessFace(
            JObject face, 
            Dictionary<int, JObject> faces, 
            Dictionary<int, bool> processed, 
            ref int vertexIndex,
            Color currentColor,
            List<Vector3> vertices, 
            List<int> indices, 
            List<Color> colors, 
            List<Vector3> normals
        ) {
        int faceId = face["id"].ToObject<int>();
        if (!processed[faceId]) {
            Debug.Log("Processing face " + faceId);
            processed[faceId] = true;
            // Process vertices
            Vector3 normal = face["normal"].ToObject<Vector3>();
            foreach (JObject vertex in face["vtx"].Children<JObject>()) {
                Vector3 vector = vertex.ToObject<Vector3>();
                vertices.Add(vector);
                indices.Add(vertexIndex);
                colors.Add(currentColor);
                normals.Add(normal);
                vertexIndex++;
            }
            foreach (JObject neighbor in face["neighbors"].Children<JObject>()) {
                int neighborId = neighbor["id"].ToObject<int>();
                JObject neighborFace = faces[neighborId];
                // Is it a new scene?
                Vector3 neighborNormal = neighborFace["normal"].ToObject<Vector3>();
                float angle = Vector3.Angle(normal, neighborNormal);
                Debug.Log("Neighbor angle: " + angle);
                if (angle > _sceneThreshold) {
                    currentColor = Random.ColorHSV();
                }

                ProcessFace(neighborFace, faces, processed, ref vertexIndex, currentColor, vertices, indices, colors, normals);
            }
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

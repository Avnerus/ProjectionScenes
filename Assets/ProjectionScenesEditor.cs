using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[CustomEditor( typeof( ProjectionScenes ) )]
public class ProjectionScenesEditor : Editor
{
    private bool wasDrawn = false;

    void OnSceneGUI( ) {
        if (!wasDrawn) {
            Debug.Log("OnSceneGUI!");
            wasDrawn = true;

            ProjectionScenes t = target as ProjectionScenes;
            string filePath = Path.Combine(Application.dataPath, t.unfoldingDataFileName);
            Debug.Log(filePath);
            if(File.Exists(filePath)) {
                List<Vector3> vertices = new List<Vector3>();;
                List<int> indices = new List<int>();
                List<Color> colors = new List<Color>();
                //Vector3[] normals;

                int index = 0;

                Mesh sceneMesh = new Mesh();
                string dataAsJson = File.ReadAllText(filePath);
                JObject root = JObject.Parse(dataAsJson);
                foreach (JObject face in root["faces"].Children<JObject>()) {
                    foreach (JObject vertex in face["vtx"].Children<JObject>()) {
                        Vector3 vector = vertex.ToObject<Vector3>();
                        vertices.Add(vector);
                        indices.Add(index);
                        colors.Add(Color.black);
                        index++;
                    }
                }

                sceneMesh.SetVertices(vertices);
                sceneMesh.SetColors(colors);
                sceneMesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);
                Graphics.DrawMeshNow(sceneMesh, Vector3.zero, Quaternion.identity);
            }
            else {　
                Debug.LogError("Cannot load unfolding data!");
            }


        }
    }
}

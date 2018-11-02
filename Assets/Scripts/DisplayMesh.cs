using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DisplayMesh : MonoBehaviour {

    [Range(4,64)]
    public int cellSize = 32;
    [Range(-3, 3)]
    public int lod = 1;

    public Vector3 offset = new Vector3(0, 0, 0);

    public Material m_material;
    public bool autoUpdate;


    private MarchingCubes marchingCubes = new MarchingCubes();
    LinkedList<GameObject> meshes = new LinkedList<GameObject>();
    Mesh mesh;

    // Use this for initialization
    void Start ()
    {
        MakeMesh();

    }

    public void MakeMesh()
    {
        
        mesh = marchingCubes.GenerateCubes(32, cellSize, (float)Mathf.Pow(2,lod));

        GameObject go = new GameObject("Mesh");
        go.transform.parent = transform;
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<Renderer>().material = m_material;
        go.GetComponent<MeshFilter>().mesh = mesh;

        meshes.AddLast(go);
    }

    // Update is called once per frame
    void Update () {
		
	}
}

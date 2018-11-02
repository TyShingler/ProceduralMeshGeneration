using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoiseTest;
using MarchingCubesProject;
using System;

public class TestMarchingCube : MonoBehaviour {

    public Material m_material;

    List<GameObject> meshes = new List<GameObject>();

    //public MeshFilter meshFilter;
    //public MeshRenderer meshRenderer;

    // Use this for initialization
    void Start()
    {
        Debug.Log("---Marching Cubes Testing Begins---");
        Group root = new Group("Root");
        root.AddSubGroup(TestSimplex());
        root.AddSubGroup(TestMarchingCubes());
        root.Review();



    }
    static bool displayMessage = false;
    private static bool Assert<T>(string message, ref LinkedList<bool> batch , T expected,T given) where T : System.IComparable<T>
    {
        bool result = expected.CompareTo(given) == 0;
        string didPass = result ? "Pass" : "Fail";
        string ifFail = result ? "" :"\nExpected : " + expected + "\tResult : " + given;
        if (displayMessage) Debug.Log(didPass + " :: " + message + ifFail);
        batch.AddLast(result);
        return result;
    }

    private static bool AssertNotNull<T>(string message, ref LinkedList<bool> batch, T given)
    {
        bool result = given != null;
        string didPass = result ? "Pass" : "Fail";
        string ifFail = result ? "" : "\nExpected : Not Null" + "\tResult : Null";
        if (displayMessage) Debug.Log(didPass + " :: " + message + ifFail);
        batch.AddLast(result);
        return result;
    }

    private class Group
    {
        private string name;
        private LinkedList<bool> batch;
        private LinkedList<Group> subGroups = new LinkedList<Group>();

        public Group(string name)
        {
            this.name = name;
        }

        public Group(string name, LinkedList<bool> batch)
        {
            this.name = name;
            this.batch = batch;
        }

        public void AddSubGroup(Group subGroup)
        {
            this.subGroups.AddLast(subGroup);
        }

        public void AddBatch(LinkedList<bool> batch)
        {
            this.batch = batch;
        }

        public void Review()
        {
            int numberOfTests = 0;
            int numberOfTestsPassed = 0;
            if (batch != null)
            {
                LinkedListNode<bool> temp = batch.First;


                while (temp != null)
                {
                    numberOfTests++;
                    if (temp.Value == true) numberOfTestsPassed++;
                    temp = temp.Next;
                }
            }

            string passing = "\n ";
            if (numberOfTests > 0)
            {
                passing = "\nPass : " + numberOfTestsPassed + "/" + numberOfTests;
            }

            if (subGroups.Count > 0) passing = "\nNumber of Groups : " + subGroups.Count;
            Debug.Log("Group : " + this.name + passing);

            foreach(Group g in subGroups)
            {
                g.Review();
            }

        }
    }

    private Group AssertGroup(string name)
    {
        Group a = new Group(name);

        return a;
    }

    private bool CheckBatch(LinkedList<bool> batch)
    {
        foreach (bool b in batch) if (!b) return false;
        return true;
    }

    private Group TestMarchingCubes()
    {
        ProceduralVoxel a = new ProceduralVoxel();
        float[,,] cell = TestCellGeneraion(a);

        Group group = new Group("MarchingCubes");

        group.AddSubGroup(TestCubeToCaseNumber(cell));
        group.AddSubGroup(TestGetFloatValueWithCubeOffset(cell));
        group.AddSubGroup(TestEdgeToVector(cell));
        group.AddSubGroup(TestGenerateCubes(cell));


        return group;
    }

    private Group TestGenerateCubes(float[,,] cell)
    {
        LinkedList<bool> batch = new LinkedList<bool>();
        ProceduralVoxel gen = new ProceduralVoxel();
        batch = MyRender(batch, gen.generateCell(32));
        ThereRender();

        return new Group("GenerateCubes", batch);
    }

    private void ThereRender()
    {
        //Set the mode used to create the mesh.
        //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
        Marching marching = new MarchingCubesProject.MarchingCubes();

        //Surface is the value that represents the surface of mesh
        //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
        //The target value does not have to be the mid point it can be any value with in the range.
        marching.Surface = 0.0f;

        //The size of voxel array.
        int width = 32;
        int height = 32;
        int length = 32;

        float[] voxels = new float[width * height * length];

        OpenSimplexNoise noise = new OpenSimplexNoise(1111);

        //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    float fx = x / (width - 1.0f);
                    float fy = y / (height - 1.0f);
                    float fz = z / (length - 1.0f);

                    int idx = x + y * width + z * width * height;

                    voxels[idx] = (float)noise.Evaluate(fx, fy, fz);
                }
            }
        }

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels, width, height, length, verts, indices);

        //A mesh in unity can only be made up of 65000 verts.
        //Need to split the verts between multiple meshes.

        int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
        int numMeshes = verts.Count / maxVertsPerMesh + 1;

        for (int i = 0; i < numMeshes; i++)
        {

            List<Vector3> splitVerts = new List<Vector3>();
            List<int> splitIndices = new List<int>();

            for (int j = 0; j < maxVertsPerMesh; j++)
            {
                int idx = i * maxVertsPerMesh + j;

                if (idx < verts.Count)
                {
                    splitVerts.Add(verts[idx]);
                    splitIndices.Add(j);
                }
            }

            if (splitVerts.Count == 0) continue;

            Mesh mesh = new Mesh();
            mesh.SetVertices(splitVerts);
            mesh.SetTriangles(splitIndices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = m_material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = new Vector3((32), 0, 0);

            meshes.Add(go);
        }
    }

    private LinkedList<bool> MyRender(LinkedList<bool> batch, float[,,] cell)
    {
        MarchingCubes marchingCubes = new MarchingCubes();
        Mesh mesh = marchingCubes.GenerateCubes(32, 32,4f);

        AssertNotNull<Mesh>("A mesh was created!", ref batch, mesh);

        GameObject go = new GameObject("Mesh");
        go.transform.parent = transform;
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<Renderer>().material = m_material;
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.transform.localPosition = new Vector3(0, 0, 0);
        return batch;
    }

    private Group TestEdgeToVector(float[,,] cell)
    {
        LinkedList<bool> batch = new LinkedList<bool>();
        MarchingCubes marchingCubes = new MarchingCubes();
        marchingCubes.SetVolume(cell);

        MarchingCubes.Positon pos = new MarchingCubes.Positon();

        pos.x = 1;
        pos.y = 0;
        pos.z = 0;

        float expected = Mathf.Lerp(1f, 2f, Mathf.Abs(1.1f) / Mathf.Abs(-1f - 1.1f));
        expected = Mathf.Round(expected * 100000)/ 100000;

        float actual = marchingCubes.EdgeToVector(3, pos)[0];
        actual = Mathf.Round(actual * 100000)/ 100000;

        Assert<float>("Edge To Vector Test & LerpVectors 1", ref batch, expected, actual);
        return new Group("EdgeToVector", batch);
    }

    private Group TestGetFloatValueWithCubeOffset(float[,,] cell)
    {
        LinkedList<bool> batch = new LinkedList<bool>();
        

        MarchingCubes.Positon pos = new MarchingCubes.Positon();
        pos.x = 0;
        pos.y = 0;
        pos.z = 0;

        SetCellStates(cell);
        MarchingCubes marchingCubes = new MarchingCubes();
        marchingCubes.SetVolume(cell);

        Assert<float>("GetFloat Test {0,0,0} = -1", ref batch, -1f, marchingCubes.GetFloatValueWithCubeOffset(0, pos));

        pos.x = 2;

        Assert<float>("GetFloat Test {2,0,0} = 1.1", ref batch, 1.1f, marchingCubes.GetFloatValueWithCubeOffset(0, pos));

        Assert<float>("GetFloat Test {2,1,0} = 1.2", ref batch, 1.2f, marchingCubes.GetFloatValueWithCubeOffset(1, pos));

        Assert<float>("GetFloat Test {2,0,1} = 1.3", ref batch, 1.3f, marchingCubes.GetFloatValueWithCubeOffset(4, pos));

        Assert<float>("GetFloat Test {2,1,1} = 1.4", ref batch, 1.4f, marchingCubes.GetFloatValueWithCubeOffset(5, pos));
        return new Group("GetFloatValueWithCubeOffset", batch);
    }

    private Group TestCubeToCaseNumber(float[,,] cell)
    {

        LinkedList<bool> batch = new LinkedList<bool>();

        SetCellStates(cell);

        // Testing Case 0u
        MarchingCubes marchingCubes = new MarchingCubes();
        marchingCubes.SetVolume(cell); 
        MarchingCubes.Positon pos = new MarchingCubes.Positon();
        pos.x = 0;
        pos.y = 0;
        pos.z = 0;

        Assert<int>("Case Test {0,0,0} = 0", ref batch, 0, marchingCubes.CubeToCaseNumber(pos));

        // Testing Case 1
        pos.x = 1;
        Assert<int>("Case Test {1,0,0} = 204", ref batch, 204, marchingCubes.CubeToCaseNumber(pos));

        //Testing Case 2
        pos.x = 2;
        Assert<int>("Case Test {2,0,0} = 51", ref batch, 51, marchingCubes.CubeToCaseNumber(pos));

        return new Group("CubeToCaseNumber", batch);
    }

    private void SetCellStates(float[,,] cell)
    {
        // Set up cell to test CubeToCaseNumber
        // Pos{ 0, 0, 0 } = State 0
        //   x, y, z
        cell[0, 0, 0] = -1f; // 0
        cell[0, 1, 0] = -1f; // 1
        cell[0, 0, 1] = -1f; // 4
        cell[0, 1, 1] = -1f; // 5

        cell[1, 0, 0] = -1f; // 3
        cell[1, 1, 0] = -1f; // 2
        cell[1, 0, 1] = -1f; // 7
        cell[1, 1, 1] = -1f; // 6

        // 76543210
        // 11001100 = 204
        //Pos{ 1, 0, 0 } = State 204
        //   x, y, z
        cell[2, 0, 0] = 1.1f;
        cell[2, 1, 0] = 1.2f;
        cell[2, 0, 1] = 1.3f;
        cell[2, 1, 1] = 1.4f;

        // 76543210
        // 00110011 = 51
        //Pos{ 2, 0, 0 } = State 51
        //   x, y, z
        cell[3, 0, 0] = -1f; // 3
        cell[3, 1, 0] = -1f; // 2
        cell[3, 0, 1] = -1f; // 7
        cell[3, 1, 1] = -1f; // 6
    }

    private float[,,] TestCellGeneraion(ProceduralVoxel a)
    {

        LinkedList<bool> batch = new LinkedList<bool>();

        float[,,] cell = a.generateCell(32);

        Assert<int>("Cell length = 32", ref batch, 32, cell.GetLength(0));

        return cell;
    }

    private Group TestSimplex()
    {

        LinkedList<bool> batch = new LinkedList<bool>();
        
        int seed = 1111;

        OpenSimplexNoise noise = new OpenSimplexNoise(seed);
        double sample = noise.Evaluate(0, 0, 0);
        Assert<double>("Getting a sample", ref batch, sample, noise.Evaluate(0, 0, 0));

        noise = new OpenSimplexNoise(seed);

        Assert<double>("Seeding Works", ref batch, sample, noise.Evaluate(0, 0, 0));

        double max, min;
        max = 0;
        min = 0;
        for (int i = 0; i < 1000000; i++)
        {
            sample = noise.Evaluate(0, 0, i / 32);
            if (sample > max) max = sample;
            if (sample < min) min = sample;
        }

        Debug.Log("Min = " + min + "\nMax = " + max);

        return new Group("Testing Simplex", batch);
    }

    // Update is called once per frame
    void Update () {
		
	}
}

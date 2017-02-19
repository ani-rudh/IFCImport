using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using Assimp.Configs;

public class Import_Check : MonoBehaviour
{

    public string filetoImport;
    public UnityEngine.Material standardmat;
    Node MasterNode;
    Node[] children;
    List<Node> allnodes = new List<Node>();
    
    int[] meshindeces;
    AssimpImporter importer;
    Scene model;

    Assimp.Mesh[] meshesinmodel;
    Assimp.Face[] facesinmesh;
    Assimp.Vector3D[] verticesinmesh;
    uint[] faceindices;
    

    [ContextMenu("mesh test")]
    public void importmodel()
    {
        //string fileName = "F:/UNITY stuff/FlexHub/FlexHub_parts/FBX-Parts_FlexHub/Part_72.fbx";

        allnodes.Clear();
        //Create a new importer
        importer = new AssimpImporter(); // create an importer instance
        //importer.SetConfig (new con)
        model = importer.ImportFile(filetoImport, //import the specified file
            //PostProcessSteps.JoinIdenticalVertices |
            PostProcessSteps.Triangulate| //very important
            PostProcessSteps.FindInvalidData |
            PostProcessSteps.RemoveRedundantMaterials             
            );

        Debug.Log(model.RootNode.Name + "\n" + "-----------------------------"); // get the root node name

        MasterNode = model.RootNode; // get the master node of the scene

        //meshesinmodel = model.Meshes;
       
        Debug.Log("No.Of Meshes in the model : " +model.MeshCount + "\n" + "-----------------------------");

        //foreach (Assimp.Mesh m in meshesinmodel)
        //{
        //    Debug.Log("Polygon type: " +m.PrimitiveType);
        //    /*************************************************************************************/
        //    Debug.Log("No. Vertices in this mesh: " +m.VertexCount);
           
        //    verticesinmesh = m.Vertices;
        //    for (int y = 0; y < verticesinmesh.Length; ++y)
        //    {
        //        Debug.Log ("vertex " + (y+1) + " is " + verticesinmesh[y]);
        //    }
        //    Debug.Log("No.Vertices detected in this mesh: " +verticesinmesh.Length);

        //    /*************************************************************************************/
        //    Debug.Log("Faces in this mesh: " + m.FaceCount);
        //    facesinmesh = m.Faces;
        //    foreach (Assimp.Face f in facesinmesh)
        //    {
        //        faceindices = f.Indices;
        //        for (int x = 0; x < faceindices.Length; ++x)
        //        {
        //            Debug.Log(faceindices[x]);
        //        }
        //        Debug.Log("Length of the face indeces array: "+faceindices.Length);
        //    }
            
        //    Debug.Log("No.Faces detected in this mesh: " + facesinmesh.Length);

        //}
        GameObject gobject = convertNodes(MasterNode); // call the function and pass the resulting GO to a new GO


        importer.Dispose(); // diacard the importer when importing is done
    }

    public GameObject convertNodes(Node node) // function to replicate the hierarchy
    {
        GameObject gameObj = new GameObject(); // create a GO for each node passed to the function

        gameObj.name = node.Name; // rename it as the node passed

        settransforms(node, gameObj); // function to set the positions of the GO in the scene

        foreach (Node child in node.Children) // for each child object under the node passed
        {
            GameObject childObj; // create an empty child gameobject

            if (child.HasChildren) // if this child has further children node 
            {
                childObj = convertNodes(child); // if there is further children, pass this child back to the function
            }
            else
            {
                childObj = new GameObject(); // if not, create an aempty GO and name it as the current node
                childObj.name = child.Name;
                settransforms(child, childObj);
            }

            if (child.HasMeshes) // if the node has meshes under it
            {

                for (int i = 0; i < child.MeshCount; i++) // iterate thru each mesh
                {
                    GameObject mGo = new GameObject(); // create a GO for each mesh

                    //mGo.name = "MESH " + i.ToString();
                    mGo.name = child.Name;

                    mGo.transform.parent = childObj.transform; // add the mesh GO under the parent node

                    //settransforms(child, mGo); // set the position of this GO relative to the parent
                    /*******************************************************/

                    UnityEngine.Mesh mesh = new UnityEngine.Mesh();
                    //mesh.name = "MESH " + i.ToString();

                    mGo.AddComponent<MeshRenderer>();
                    mGo.GetComponent<MeshRenderer>().sharedMaterial = standardmat;
                    mGo.AddComponent<MeshFilter>();

                    drawmesh(child, mesh, model);

                    mGo.GetComponent<MeshFilter>().mesh = mesh;

                }
            }

            childObj.transform.parent = gameObj.transform; // for each child found, assign it as a child of the parent it was derived from
        }

        return gameObj; // return the main node passed as GO
    }

    public void settransforms(Node refnode, GameObject g_object)
    {
        Assimp.Vector3D position, scaling;
        Assimp.Quaternion rotation;

        refnode.Transform.Decompose(out scaling, out rotation, out position);

        g_object.transform.position = new Vector3(position.X, position.Y, position.Z);
        g_object.transform.rotation = new UnityEngine.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
        g_object.transform.localScale = new Vector3(scaling.X, scaling.Y, scaling.Z);
    }

    public void drawmesh(Node refnode_m, UnityEngine.Mesh themesh, Scene importedmodel)
    {
        List<Vector3> tempvertices = new List<Vector3>();
        
        for (int a = 0; a < refnode_m.MeshCount; a++) // for each mesh
        {
            Vector3 verticevec_temp = new Vector3();
            tempvertices.Clear();
            //List<Vector3> tempvertices = new List<Vector3>();
            for (int b = 0; b < importedmodel.Meshes[refnode_m.MeshIndices[a]].VertexCount; b++) // get the vertices of the mesh
            {
                verticevec_temp = new Vector3(importedmodel.Meshes[refnode_m.MeshIndices[a]].Vertices[b].X,
                                                importedmodel.Meshes[refnode_m.MeshIndices[a]].Vertices[b].Z,
                                                importedmodel.Meshes[refnode_m.MeshIndices[a]].Vertices[b].Y);
                tempvertices.Add(verticevec_temp);

            }

            Debug.Log(  "-----------------------------" +"\n"+ "Vertices under MESH " + refnode_m.MeshIndices[a].ToString() + " of " + refnode_m.Name + " are: " + importedmodel.Meshes[refnode_m.MeshIndices[a]].VertexCount);
            Debug.Log("Faces under MESH " + refnode_m.MeshIndices[a].ToString() + " of " + refnode_m.Name + " are: " + importedmodel.Meshes[refnode_m.MeshIndices[a]].FaceCount + "\n" + "-----------------------------");

            themesh.SetVertices(tempvertices);
            themesh.triangles = importedmodel.Meshes[refnode_m.MeshIndices[a]].GetIntIndices();

            //themesh.triangles = importedmodel.Meshes[a].GetIntIndices();
        }
        
    }

}
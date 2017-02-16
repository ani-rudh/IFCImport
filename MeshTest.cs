using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using Assimp.Configs;

public class MeshTest : MonoBehaviour
{

    public string filetoImport;
    public UnityEngine.Material standardmat;
    Node MasterNode;
    Node[] children;
    List<Node> allnodes = new List<Node>();
    Assimp.Mesh[] meshesinmodel;
    int[] meshindeces;
    AssimpImporter importer;
    Scene model;

    [ContextMenu("mesh test")]
    public void importmodel()
    {
        //string fileName = "F:/UNITY stuff/FlexHub/FlexHub_parts/FBX-Parts_FlexHub/Part_72.fbx";

        allnodes.Clear();
        //Create a new importer
        importer = new AssimpImporter(); // create an importer instance
        //importer.SetConfig (new con)
        model = importer.ImportFile(filetoImport, //import the specified file
            PostProcessSteps.JoinIdenticalVertices |
            PostProcessSteps.FindInvalidData |
            PostProcessSteps.RemoveRedundantMaterials |
            PostProcessSteps.FixInFacingNormals
            );

        Debug.Log(model.RootNode.Name + "\n" + "-----------------------------"); // get the root node name

        MasterNode = model.RootNode; // get the master node of the scene

        meshesinmodel = model.Meshes;

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

                /*****************************************************************************************************************************/
                
                //Debug.Log("Meshcount:" + child.MeshCount);
                //Debug.Log("Length of Mesh indeces: " + child.MeshIndices.Length + "\n" + "------------------------------------");
                //for (int i = 0; i < child.MeshCount; i++)
                //{
                //    Debug.Log("index: " + child.MeshIndices[i]);
                //}
                //Debug.Log("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                /*****************************************************************************************************************************/

                for (int i = 0; i < child.MeshCount; i++) // iterate thru each mesh
                {
                    GameObject mGo = new GameObject(); // create a GO for each mesh

                    mGo.name = "MESH " + i.ToString();
                    
                    mGo.transform.parent = childObj.transform; // add the mesh GO under the parent node

                    //settransforms(child, mGo); // set the position of this GO relative to the parent
                    /*******************************************************/

                    UnityEngine.Mesh mesh = new UnityEngine.Mesh();
                    mesh.name = "MESH " + i.ToString();

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

        g_object.transform.localPosition = new Vector3(position.X, position.Y, position.Z);
        g_object.transform.localRotation = new UnityEngine.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
        g_object.transform.localScale = new Vector3(scaling.X, scaling.Y, scaling.Z);
    }

    public void drawmesh(Node refnode_m, UnityEngine.Mesh themesh, Scene importedmodel)
    {
        for (int a = 0; a < refnode_m.MeshCount; a++) // for each mesh
        {
            Vector3 verticevec_temp = new Vector3();
            //Vector3[] tempvertices = new Vector3[importedmodel.Meshes[refnode_m.MeshIndices[a]].VertexCount];
            List<Vector3> tempvertices = new List<Vector3>();
            for (int b = 0; b < importedmodel.Meshes[refnode_m.MeshIndices[a]].VertexCount; b++) // get the vertices of the mesh
            {
                verticevec_temp = new Vector3(importedmodel.Meshes[refnode_m.MeshIndices[a]].Vertices[b].X,
                                                importedmodel.Meshes[refnode_m.MeshIndices[a]].Vertices[b].Z,
                                                importedmodel.Meshes[refnode_m.MeshIndices[a]].Vertices[b].Y);
                tempvertices.Add (verticevec_temp);
                
            }

            Debug.Log("Vertices under MESH " + refnode_m.MeshIndices[a].ToString() + " of " + refnode_m.Name + " are: " + importedmodel.Meshes[refnode_m.MeshIndices[a]].VertexCount);
            
            themesh.SetVertices(tempvertices);
            themesh.triangles = importedmodel.Meshes[refnode_m.MeshIndices[a]].GetIntIndices();
            
            //themesh.triangles = importedmodel.Meshes[a].GetIntIndices();

        }
    }

}
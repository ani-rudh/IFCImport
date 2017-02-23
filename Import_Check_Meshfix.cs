using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using Assimp.Configs;

public class Import_Check_Meshfix : MonoBehaviour
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

    List<Vector3> tempvertices;

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
            PostProcessSteps.Triangulate | //very important
            PostProcessSteps.FindInvalidData |
            PostProcessSteps.RemoveRedundantMaterials
            );

        Debug.Log(model.RootNode.Name + "\n" + "-----------------------------"); // get the root node name

        MasterNode = model.RootNode; // get the master node of the scene

        //meshesinmodel = model.Meshes;

        Debug.Log("No.Of Meshes in the model : " + model.MeshCount + "\n" + "-----------------------------");


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
                List<UnityEngine.Mesh> meshes = drawmesh(child, model);

                foreach (UnityEngine.Mesh mesh in meshes)
                {
                    GameObject mGo = new GameObject();
                    mGo.name = "MESH " + meshes.IndexOf(mesh);

                    MeshFilter filter = mGo.AddComponent<MeshFilter>();
                    MeshRenderer renderer = mGo.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = standardmat;
                    filter.mesh = mesh;

                    mGo.transform.parent = childObj.transform; // add the mesh GO under the parent node
                    mGo.transform.localPosition = Vector3.zero;
                    mGo.transform.localRotation = new UnityEngine.Quaternion(0, 0, 0, 0);
                }

                //mGo.AddComponent<MeshRenderer>();

                //mGo.GetComponent<MeshRenderer>().sharedMaterial = standardmat;
                //mGo.AddComponent<MeshFilter>();

                ////                    drawmesh(child, mesh, model);


                //mGo.GetComponent<MeshFilter>().mesh = drawmesh(child,model)[0];
                //childObj.transform.parent = childObj.transform; // add the mesh GO under the parent node
                //}
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

    public List<UnityEngine.Mesh> drawmesh(Node refnode_m, Scene importedmodel)
    {
        List<UnityEngine.Mesh> meshesArray = new List<UnityEngine.Mesh>();

        for (int a = 0; a < refnode_m.MeshCount; a++) // for each mesh
        {
            tempvertices = new List<Vector3>();
            List<int> meshesIndices = new List<int>();
            
            
            int indice = refnode_m.MeshIndices[a];
            Debug.Log("node mesh indices " + refnode_m.MeshIndices[a].ToString());

            foreach (Assimp.Mesh mesh in importedmodel.Meshes)
            {
                Debug.Log("indices " + mesh.GetIntIndices().Length);
                
                foreach (int i in mesh.GetIntIndices())
                {
                    foreach(int ind in refnode_m.MeshIndices)
                    {
                        if (ind == i)
                        {
                            Debug.Log("indice found " + ind);
                            meshesIndices.AddRange(mesh.GetIntIndices());

                            foreach (Vector3D vertice in mesh.Vertices)
                            {
                                Vector3 verticevec_temp = new Vector3(vertice.X,
                                                            vertice.Y,
                                                            vertice.Z);
                                tempvertices.Add(verticevec_temp);
                            }
                        }
                    }
                    
                }


                //for (int b = 0; b < importedmodel.Meshes[refnode_m.MeshIndices[a]].VertexCount; b++) // get the vertices of the mesh
                //{
                //    Vector3 verticevec_temp = new Vector3(importedmodel.Meshes[refnode_m.MeshIndices[a]].Vertices[b].X,
                //                                    importedmodel.Meshes[refnode_m.MeshIndices[a]].Vertices[b].Y,
                //                                    importedmodel.Meshes[refnode_m.MeshIndices[a]].Vertices[b].Z);
                //    tempvertices.Add(verticevec_temp);
                //}

                //Debug.Log("-----------------------------" + "\n" + "Vertices under MESH " + refnode_m.MeshIndices[a].ToString() + " of " + refnode_m.Name + " are: " + importedmodel.Meshes[refnode_m.MeshIndices[a]].VertexCount);
                //Debug.Log("Faces under MESH " + refnode_m.MeshIndices[a].ToString() + " of " + refnode_m.Name + " are: " + importedmodel.Meshes[refnode_m.MeshIndices[a]].FaceCount + "\n" + "-----------------------------");

                UnityEngine.Mesh newMesh = new UnityEngine.Mesh();
                newMesh.SetVertices(tempvertices);
                //newMesh.triangles = importedmodel.Meshes[refnode_m.MeshIndices[a]].GetIntIndices();
                newMesh.triangles = meshesIndices.ToArray();

                meshesArray.Add(newMesh);
            }
        }



        return meshesArray;
    }
    
   
}


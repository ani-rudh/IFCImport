using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using Assimp.Configs;




public class IFCImporter_3 : MonoBehaviour
{

    public string filetoImport;
    Node MasterNode;
    Node[] children;
    List<Node> allnodes = new List<Node>();
    IEnumerable<Node> parents;
    IEnumerable<Node> allnodesundertheparent;

    [ContextMenu("importmodel_using_Assimp")]
    public void importmodel()
    {
        //string fileName = "F:/UNITY stuff/FlexHub/FlexHub_parts/FBX-Parts_FlexHub/Part_72.fbx";

        allnodes.Clear();
        //Create a new importer
        AssimpImporter importer = new AssimpImporter(); // create an importer instance
        //importer.SetConfig (new con)
        Scene model = importer.ImportFile(filetoImport, //import hte specified file
            PostProcessSteps.JoinIdenticalVertices |
            PostProcessSteps.FindInvalidData |
            PostProcessSteps.RemoveRedundantMaterials |
            PostProcessSteps.FixInFacingNormals
            );  

        Debug.Log(model.RootNode.Name + "\n" + "-----------------------------"); // get the root node name

        MasterNode = model.RootNode; // get the master node of the scene

        GameObject gobject = convertNodes(MasterNode); // call the function and pass the resulting GO to a new GO

        //int count = gobject.transform.childCount;
        //for(int i = 0; i < count; i++)
        //{
        //    Transform obj = gobject.transform.GetChild(i);
        //    Debug.Log(obj.name);
        //}


        importer.Dispose(); // diacard the importer when importing is done
    }

    public GameObject convertNodes(Node node) // function to replicate the hierarchy
    {
        GameObject gameObj = new GameObject(); // create a GO for each node passed to the function

        gameObj.name = node.Name; // rename it as the node passed

        settransforms(node, gameObj);

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
            if (child.HasMeshes && !child.HasChildren)
            {
                //GameObject meshObj;
                int mcount = child.MeshCount;
                for (int i = 0; i < mcount; i++)
                {
                    GameObject mGo = new GameObject();
                    mGo.name = "MESH " + i.ToString() ;

                    settransforms(child, mGo);

                    mGo.transform.parent = childObj.transform;
                }
            }


            childObj.transform.parent = gameObj.transform; // for each child found, assign it as a child of the parent it was derived from
        }

        return gameObj; // return the main node passed as GO
    }

    public void settransforms (Node refnode, GameObject g_object)
    {
        Assimp.Vector3D position, scaling;
        Assimp.Quaternion rotation;

        refnode.Transform.Decompose(out scaling, out rotation, out position);

        g_object.transform.localPosition = new Vector3(position.X, position.Y, position.Z);
        g_object.transform.localRotation = new UnityEngine.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
        g_object.transform.localScale = new Vector3(scaling.X, scaling.Y, scaling.Z);
    }


}






using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

using System;

namespace kondo
{
[Serializable]
public class InputJson
{
    public JsonObject[] features;
}

[Serializable]
public class JsonObject
{
    public int id;
    public string bloom_date;
    public float[] coordinates;
}


public class LocationScore : MonoBehaviour
{
    //racastの最大距離
    public float maxDistance = 100;
    public int jsonLength = 10;
    public float treeHight = 6;

    public Color32 firstColor = new Color32((byte)255, (byte)255, (byte)255, (byte)255);
    public Color32 secondColor = new Color32((byte)32, (byte)32, (byte)32, (byte)32);
    public Color32 thirdColor = new Color32((byte)32, (byte)32, (byte)32, (byte)32);
    public Color32 forthColor = new Color32((byte)32, (byte)32, (byte)32, (byte)32);
    //public Color32 fifthColor = new Color32((byte)255, (byte)0, (byte)0, (byte)0);

    // Start is called before the first frame update
    void Start()
    {
        //建物と樹種の基本情報取得
        Vector3[] BuildingsCenter = getCenter("building");
        //int[] BuildingsID = getID("building");

        //Vector3[] SakuraCenter = getCenter("sakura");
        Vector3[] SakuraCenter = new Vector3[jsonLength];
        //int[] SakuraID = getID("sakura");

        Debug.Log("BuildingsCenter length = " + BuildingsCenter.Length);
        //Debug.Log("BuildingID  length = " + BuildingsID.Length);
        Debug.Log("SakuraCenter length =" + SakuraCenter.Length);
        //Debug.Log("SakuraID length = " + SakuraID.Length);


        string inputString = Resources.Load<TextAsset>("Sakuma/input").ToString();
        InputJson inputJson = JsonUtility.FromJson<InputJson>(inputString);
        int jsonRoopConuter = 0;
        for (int i = 0; i < 10; i++)
        {
            float x = inputJson.features[i].coordinates[0];
            float z = inputJson.features[i].coordinates[1];

            Vector3 sakuraCenterPos = new Vector3(x, treeHight, z);
            SakuraCenter[jsonRoopConuter] = sakuraCenterPos;
            jsonRoopConuter += 1;
        }


        //スコアテーブル
        GameObject[] objs = GameObject.FindGameObjectsWithTag("building");
        int[] BuildingScore = new int[objs.Length];

        Debug.Log("BuildingScoreMax = " + BuildingScore.Max());

        //raycasttest
        //BuildingScore = visibleJudges(BuildingScore, SakuraCenter[0], BuildingsCenter, maxDistance);

        //木からの可視判定
        foreach (Vector3 SakuraOrigin in SakuraCenter)
        {
            BuildingScore = visibleJudges(BuildingScore, SakuraOrigin, BuildingsCenter, maxDistance, "building");
            Debug.Log("score sum " + BuildingScore.Sum());
        }

        Debug.Log("buildingsocreSum" + BuildingScore.Sum());

        //色変更
        changeColor(BuildingScore, "building");

    }

    // Update is called once per frame
    void Update()
    {

    }

    Vector3[] getCenter(string tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        Vector3[] objectscenter = new Vector3[objs.Length];

        int counter = 0;
        foreach (GameObject obj in objs)
        {
                if (obj.GetComponent<MeshFilter>() == null)
                    continue;
            Mesh objectmesh = obj.GetComponent<MeshFilter>().mesh;
                if (objectmesh.isReadable == false)
                    continue;
                if (objectmesh.vertices.Length == 0)
                    continue;
            Vector3[] vertices = objectmesh.vertices;

            Vector3 SumVertices = new Vector3(0f, 0f, 0f);
            foreach (Vector3 vertex in vertices)
            {
                SumVertices += vertex;
            }

            SumVertices = SumVertices / vertices.Length;

            //最終的に出力する物
            Vector3 center = SumVertices;

            objectscenter[counter] = center;
            counter += 1;

        }
        return objectscenter;
    }

    int[] getID(string tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        int[] objIDs = new int[objs.Length];

        int counter = 0;
        foreach (GameObject obj in objs)
        {
            int objID = obj.GetInstanceID();

            objIDs[counter] = objID;
            counter += 1;
        }
        return objIDs;
    }

    bool visibleJudge(Vector3 originObj, Vector3 targetObj, float maxDistance)
    {
        Vector3 dirTemp = targetObj - originObj;
        Vector3 dir = dirTemp.normalized;

        bool judge = Physics.Raycast(originObj, dir, maxDistance);
        return judge;
    }

    int[] visibleJudges(int[] scoreTable, Vector3 originObj, Vector3[] targetObjs, float maxDistance, string tag)
    {
        //建物のを一時的にraycast ignoreするための設定
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);

        int Counter = 0;
        foreach (Vector3 targetObj in targetObjs)
        {
            Vector3 dirTemp = targetObj - originObj;
            Vector3 dir = dirTemp.normalized;



            //視線が通る＝raycastがfalse

            RaycastHit hit;

            if (Physics.Raycast(originObj, dir, out hit, maxDistance))
            {
                int hitID = hit.collider.gameObject.GetInstanceID();
                int targetID = objs[Counter].GetInstanceID();
                if (hitID == targetID)
                {
                    scoreTable[Counter] += 1;
                }
            }



            objs[Counter].layer = LayerMask.NameToLayer("Default");

            //rayの可視化
            //Debug.DrawRay(originObj, dir * maxDistance, Color.red, 300);

            Counter += 1;
        }

        return scoreTable;
    }

    void changeColor(int[] scoreTable, string tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);

        int scoreMax = scoreTable.Max();
        Debug.Log("scoremax =" + scoreMax);

        int Counter = 0;
        foreach (GameObject obj in objs)
        {
                if (obj.GetComponent<Renderer>() == null)
                    continue;
            //すべのオブジェクトの色を最低評価にする。
            obj.GetComponent<Renderer>().material.color = forthColor;

            if (scoreTable[Counter] > 0)
            {


                if (scoreTable[Counter] < scoreMax * 2 / 4)
                {
                    obj.GetComponent<Renderer>().material.color = thirdColor;
                    Debug.Log("third " + scoreTable[Counter]);

                }
                else if (scoreTable[Counter] < scoreMax * 3 / 4)
                {
                    obj.GetComponent<Renderer>().material.color = secondColor;
                    Debug.Log("second " + scoreTable[Counter]);
                }
                else
                {
                    obj.GetComponent<Renderer>().material.color = firstColor;
                    Debug.Log("first " + scoreTable[Counter]);
                }

                //float cc = 255 - (255 * scoreTable[Counter] / scoreMax);
                //byte c = (byte)cc;
                //obj.GetComponent<Renderer>().material.color = firstColor;
                //Debug.Log("color "+c);
            }

            Counter += 1;
        }
    }

}


}
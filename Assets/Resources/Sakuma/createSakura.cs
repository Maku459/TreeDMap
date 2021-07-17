using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 入力されるJSONに合わせてクラスを作成
[Serializable]
public class InputJson
{
    public JsonObject[] features;
}

[Serializable]
public class JsonObject
{
    public string tree_type;

    public string geometry_type;
    public float[] coordinates;
}

public class createSakura : MonoBehaviour
{
    /*
    public Mesh noodle1;
    public Mesh noodle2;
    public Mesh noodle3;
    public Mesh noodle4;
    */

    private GameObject[] _sakura;
    // Start is called before the first frame update
    void Start()
    {
        // input.jsonをテキストファイルとして読み取り、string型で受け取る
        string inputString = Resources.Load<TextAsset>("Sakuma/input").ToString();
        // 上で作成したクラスへデシリアライズ
        InputJson inputJson = JsonUtility.FromJson<InputJson>(inputString);
        Debug.Log(inputJson.features[0].tree_type);
        /*
        if (!inputJson.leaf_date){
            
        }
        
        Instantiate(originObject, new Vector3( -1.0f, 0.0f, 0.0f), Quaternion.identity);
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

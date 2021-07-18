using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

// 入力されるJSONに合わせてクラスを作成
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
}

public class createSakura : MonoBehaviour
{
    [SerializeField] public GameObject sakura_lowpoly_1;
    [SerializeField] public GameObject sakura_lowpoly_2;
    [SerializeField] public GameObject sakura_lowpoly_3;
    [SerializeField] public GameObject sakura_lowpoly_4;
    [SerializeField] public GameObject sakura_lowpoly_5;
    [SerializeField] public GameObject sakura_lowpoly_6;

    public GameObject sakura;
    
    // Start is called before the first frame update
    void Start()
    {
        // input.jsonをテキストファイルとして読み取り、string型で受け取る
        string inputString = Resources.Load<TextAsset>("Sakuma/input").ToString();
        // 上で作成したクラスへデシリアライズ
        InputJson inputJson = JsonUtility.FromJson<InputJson>(inputString);
        var dt = new DateTime( 2021, 3, 10);

        for (int i = 0; i < 2; i++)
        {
            DateTime start_date = DateTime.Parse(inputJson.features[i].bloom_date);

            if (start_date.AddDays(10) < dt)
            {
                sakura = sakura_lowpoly_6;
            }
            else if (start_date.AddDays(8) < dt)
            {
                sakura = sakura_lowpoly_5;
            }
            else if (start_date.AddDays(6) < dt)
            {
                sakura = sakura_lowpoly_4;
            }
            else if (start_date.AddDays(4) < dt)
            {
                sakura = sakura_lowpoly_3;
            }
            else if (start_date.AddDays(2) < dt)
            {
                sakura = sakura_lowpoly_2;
            }
            else
            {
                sakura = sakura_lowpoly_1;
            }

            GameObject go = Instantiate(sakura, new Vector3(-1.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class timeSlider : MonoBehaviour
{
    Slider Slider;
 
    // Use this for initialization
    void Start()
    {

        Slider = GetComponent<Slider>();
 
        int maxSlide = 10;
        int nowSlide = 0;

        //スライダーの最大値の設定
        Slider.maxValue = maxSlide;
 
        //スライダーの現在値の設定
        Slider.value = nowSlide;
 
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

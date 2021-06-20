using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using epoching.easy_debug_on_the_phone;
using UnityEngine.UI;
using epoching.easy_gui;

public class DisplayCard : MonoBehaviour
{
    public GameObject ImageA;
    // Start is called before the first frame update
    void Start()
    {
        ImageA.SetActive(true);
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Login : MonoBehaviour
{
    private DialogPlugin dialogPlugin;
    // Start is called before the first frame update
    void Start()
    {
        dialogPlugin = GameObject.FindGameObjectWithTag("GameController").GetComponent<DialogPlugin>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClickPlayBtn() {
        dialogPlugin.Dialog();
    }
}

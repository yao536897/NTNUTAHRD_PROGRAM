using UnityEngine;
using UnityEngine.UI;
using GlobalSetting;

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

    public void switchLanguage(Button btn)
    {
        switch (Language.currentLanguage)
        {
            case Language.languageType.Chinese:
                btn.GetComponentInChildren<Text>().text = "中文";
                Language.currentLanguage = Language.languageType.English;
                break;
            case Language.languageType.English:
                btn.GetComponentInChildren<Text>().text = "英文";
                Language.currentLanguage = Language.languageType.Chinese;
                break;
        }
    }

    public void onClickPlayBtn()
    {
        dialogPlugin.Dialog();
    }
}

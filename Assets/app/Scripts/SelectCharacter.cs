using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using GlobalSetting;

public class SelectCharacter : MonoBehaviour
{
    [System.Serializable]
    public struct SpriteLanguage
    {
        public Sprite ch;
        public Sprite en;
    }

    public SpriteLanguage title;
    public SpriteLanguage john;
    public SpriteLanguage jacky;
    public SpriteLanguage aries;
    public SpriteLanguage teresa;
    public Image titleNode;
    public Image johnNode;
    public Image jackyNode;
    public Image ariesNode;
    public Image teresaNode;
    public UnityEvent onSelectOver;

    // Start is called before the first frame update
    void Start()
    {
        GameController.gameStatus = Game_Status.SelectCharacter;
        foreach (string key in GameController.player.Keys)
        {
            GameObject.FindGameObjectWithTag(key).GetComponent<Button>().interactable = false;
        }
        switch (Language.currentLanguage)
        {
            case Language.languageType.Chinese:
                titleNode.sprite = title.ch;
                johnNode.sprite = john.ch;
                jackyNode.sprite = jacky.ch;
                ariesNode.sprite = aries.ch;
                teresaNode.sprite = teresa.ch;
                break;
            case Language.languageType.English:
                titleNode.sprite = title.en;
                johnNode.sprite = john.en;
                jackyNode.sprite = jacky.en;
                ariesNode.sprite = aries.en;
                teresaNode.sprite = teresa.en;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onSelectCharacter(Button btn)
    {
        btn.interactable = false;
        DialogPlugin dialogPlugin = btn.GetComponent<DialogPlugin>();
        dialogPlugin.Dialog();
        Sprite avatar = btn.GetComponent<Image>().sprite;
        Sprite nameCard = btn.transform.GetChild(0).GetComponent<Image>().sprite;
        string oriName = btn.tag;
        setCurrentCharacter(oriName);
        GameController.playerName.SetValue(oriName, GameController.selectedIndex);
        GameController.playerNameMap.Add(oriName, Language.currentLanguage == Language.languageType.Chinese ?
        GameController.characterNameCh[((int)dialogPlugin.dialogItems[0].characterName)] : oriName);
        GameController.playerCard.Add(oriName, nameCard);
        GameController.player.Add(oriName, avatar);

        GameController.selectedIndex++;
        SceneManager.LoadScene("qrcode_scanner");
    }

    public void SelectCharacterOver()
    {
        if (GameController.selectedIndex == GameController.playerName.Length)
        {
            GameController.useOriginalName = false;
            onSelectOver.Invoke();
        }


    }

    private void setCurrentCharacter(string name)
    {
        switch (name)
        {
            case "John":
                GameController.currentCharacter = "John";
                break;
            case "Jacky":
                GameController.currentCharacter = "Jackie";
                break;
            case "Teresa":
                GameController.currentCharacter = "Teresa";
                break;
            case "Aries":
                GameController.currentCharacter = "Aries";
                break;
        }
    }
}

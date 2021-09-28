using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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

    private int selectedIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
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

        string name = "";
        switch (Language.currentLanguage)
        {
            case Language.languageType.Chinese:
                name = GameController.characterNameCh[((int)dialogPlugin.dialogItems[0].characterName)];
                break;
            case Language.languageType.English:
                name = GameController.characterNameEn[((int)dialogPlugin.dialogItems[0].characterName)];
                break;
        }
        Sprite avatar = btn.GetComponent<Image>().sprite;
        Sprite nameCard = btn.transform.GetChild(0).GetComponent<Image>().sprite;
        GameController.playerName.SetValue(name, selectedIndex);
        GameController.playerCard.Add(name, nameCard);
        GameController.player.Add(name, avatar);

        selectedIndex++;
    }

    public void SelectCharacterOver()
    {
        if (selectedIndex == GameController.playerName.Length)
        {
            onSelectOver.Invoke();
        }
    }
}

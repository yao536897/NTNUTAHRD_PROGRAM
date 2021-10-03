using UnityEngine;
using UnityEngine.UI;
using GlobalSetting;

public class SetNickName : MonoBehaviour
{
    public Image avatar;
    public Image nickName;
    public InputField inputNickName;

    private int currentCharacterIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        SetCharacter();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onClickConfirmNickNameBtn()
    {
        string oldName = GameController.playerName[currentCharacterIndex];
        string newName = inputNickName.text;
        GameController.playerNameMap.Remove(oldName);
        GameController.playerNameMap.Add(oldName, newName);
        currentCharacterIndex++;
        if (currentCharacterIndex == GameController.playerName.Length)
        {
            DialogPlugin dialogPlugin = this.GetComponent<DialogPlugin>();
            dialogPlugin.Dialog();
        }
        else
        {
            SetCharacter();
        }
    }

    void SetCharacter()
    {
        string playerName = GameController.playerName[currentCharacterIndex];
        inputNickName.text = GameController.playerNameMap[playerName];
        avatar.sprite = GameController.player[playerName];
        nickName.sprite = GameController.playerCard[playerName];
    }
}

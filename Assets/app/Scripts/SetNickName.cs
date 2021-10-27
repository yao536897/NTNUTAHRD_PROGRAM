using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GlobalSetting;

public class SetNickName : MonoBehaviour
{
    public GameObject character;
    public Image avatar;
    public Image nickName;
    public InputField inputNickName;
    public GameObject round;
    public InputField inputMaxRound;

    private int currentCharacterIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        character.SetActive(true);
        currentCharacterIndex = GameController.player.Count - 1;
        SetCharacter();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onClickConfirmNickNameBtn()
    {
        if (character.activeSelf)
        {
            string oldName = GameController.playerName[currentCharacterIndex];
            string newName = inputNickName.text;
            GameController.playerNameMap.Remove(oldName);
            GameController.playerNameMap.Add(oldName, newName);
            currentCharacterIndex++;
            if (currentCharacterIndex == GameController.playerName.Length)
            {
                GameController.gameStatus = Game_Status.Gaming;
                DialogPlugin dialogPlugin = this.GetComponent<DialogPlugin>();
                dialogPlugin.Dialog();
            }
            else
            {
                SceneManager.LoadScene("select_char");
            }
        }
        else if (round.activeSelf)
        {
            GameController.maxRound = System.Int16.Parse(inputMaxRound.text);
            SceneManager.LoadScene("Tutorial");
        }
    }

    public void setMaxRound()
    {
        character.SetActive(false);
        round.SetActive(true);
    }

    void SetCharacter()
    {
        string playerName = GameController.playerName[currentCharacterIndex];
        inputNickName.text = GameController.playerNameMap[playerName];
        avatar.sprite = GameController.player[playerName];
        nickName.sprite = GameController.playerCard[playerName];
    }
}

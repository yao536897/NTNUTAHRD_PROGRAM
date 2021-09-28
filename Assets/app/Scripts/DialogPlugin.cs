
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using GlobalSetting;

[System.Serializable]
public struct DialogItem
{
    public Sprite avatar;
    public GlobalSetting.Character characterName;
    public bool setImageLeft;
    public bool clear;
    public Sprite showImage;
    [TextArea]
    public string chTxt;
    [TextArea]
    public string enTxt;
}

public class DialogPlugin : MonoBehaviour
{

    public GameObject targetNode = null;
    public GameObject dialogPrefab = null;
    public bool playOnLoad = false;
    public Sprite dialogBg = null;

    public UnityEvent dialogOverCallback;
    public DialogItem[] dialogItems = null;

    private int currentDialogIndex = 0;
    private bool isListening = false;
    private Sprite currentShowImage = null;

    // Start is called before the first frame update
    void Start()
    {
        targetNode = targetNode ? targetNode : this.gameObject;
        if (GameObject.FindGameObjectWithTag("Dialog")) GameObject.Destroy(GameObject.FindGameObjectWithTag("Dialog"));
    }

    // Update is called once per frame
    void Update()
    {
        if ((isListening && Input.anyKeyDown) || (!isListening && playOnLoad))
        {
            Dialog();
        }
    }

    public void Dialog()
    {
        if (GameObject.FindGameObjectWithTag("Dialog")) GameObject.Destroy(GameObject.FindGameObjectWithTag("Dialog"));
        if (currentDialogIndex >= dialogItems.Length)
        {
            dialogOverCallback.Invoke();
            return;
        }
        isListening = true;
        GameObject dialogObject = Instantiate(dialogPrefab);
        dialogObject.transform.SetParent(targetNode.transform);

        DialogItem dialogData = dialogItems[currentDialogIndex];
        GameObject avatar = dialogObject.transform.GetChild(0).gameObject;
        GameObject txt = dialogObject.transform.GetChild(1).gameObject;
        GameObject characterName = dialogObject.transform.GetChild(2).gameObject;
        GameObject logo = dialogObject.transform.GetChild(3).gameObject;
        GameObject showImage = dialogObject.transform.GetChild(4).gameObject;

        dialogObject.GetComponent<Image>().sprite = dialogBg;
        avatar.GetComponent<Image>().sprite = dialogData.avatar;
        if (!dialogData.avatar)
        {
            avatar.SetActive(false);
        }
        if (currentDialogIndex == 0 || dialogData.clear)
        {
            showImage.SetActive(false);
        }
        if ((dialogData.showImage || currentShowImage) && !dialogData.clear)
        {
            currentShowImage = dialogData.showImage ? dialogData.showImage : currentShowImage;
            showImage.SetActive(true);
            showImage.GetComponent<Image>().sprite = currentShowImage;
            showImage.GetComponent<RectTransform>().sizeDelta = currentShowImage.rect.size;
        }
        else
        {
            currentShowImage = null;
            showImage.SetActive(false);
        }
        switch (Language.currentLanguage)
        {
            case Language.languageType.Chinese:
                txt.GetComponent<Text>().text = dialogData.chTxt;
                characterName.GetComponent<Text>().text = GameController.useOriginalName || !GameController.playerNameMap.ContainsKey(dialogData.characterName.ToString()) ?
                GameController.characterNameCh[(int)dialogData.characterName] : GameController.playerNameMap[dialogData.characterName.ToString()];
                break;
            case Language.languageType.English:
                txt.GetComponent<Text>().text = dialogData.enTxt;
                characterName.GetComponent<Text>().text = GameController.useOriginalName || !GameController.playerNameMap.ContainsKey(dialogData.characterName.ToString()) ?
                GameController.characterNameEn[(int)dialogData.characterName] : GameController.playerNameMap[dialogData.characterName.ToString()];
                break;
        }

        RectTransform rt = dialogObject.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(0, 300);
        rt.anchoredPosition = new Vector2(0, 0);
        rt.localScale = new Vector3(1, 1, 1);

        RectTransform avatarRt = avatar.GetComponent<RectTransform>();
        avatarRt.anchorMin = dialogData.setImageLeft ? new Vector2(0, 0) : new Vector2(1, 0);
        avatarRt.anchorMax = dialogData.setImageLeft ? new Vector2(0, 0) : new Vector2(1, 0);
        avatarRt.pivot = dialogData.setImageLeft ? new Vector2(0, 0) : new Vector2(1, 0);
        avatarRt.sizeDelta = new Vector2(294, 358);
        avatarRt.anchoredPosition = dialogData.setImageLeft ? new Vector3(30, 30, 0) : new Vector3(-30, 30, 0);
        avatarRt.localScale = new Vector3(1, 1, 1);

        RectTransform textRt = txt.GetComponent<RectTransform>();
        textRt.anchorMin = dialogData.setImageLeft ? new Vector2(1, 0) : new Vector2(0, 0);
        textRt.anchorMax = dialogData.setImageLeft ? new Vector2(1, 0) : new Vector2(0, 0);
        textRt.pivot = dialogData.setImageLeft ? new Vector2(1, 0) : new Vector2(0, 0);
        textRt.anchoredPosition = dialogData.setImageLeft ? new Vector3(-30, 40, 0) : new Vector3(30, 40, 0);
        textRt.sizeDelta = dialogData.avatar ? new Vector2(700, 190) : new Vector2(1000, 190);
        textRt.localScale = new Vector3(1, 1, 1);

        RectTransform nameTxt = characterName.GetComponent<RectTransform>();
        nameTxt.anchorMin = dialogData.setImageLeft ? new Vector2(1, 1) : new Vector2(0, 1);
        nameTxt.anchorMax = dialogData.setImageLeft ? new Vector2(1, 1) : new Vector2(0, 1);
        nameTxt.pivot = dialogData.setImageLeft ? new Vector2(1, 1) : new Vector2(0, 1);
        nameTxt.anchoredPosition = dialogData.setImageLeft ? new Vector3(-570, -30, 0) : new Vector3(30, -30, 0);
        nameTxt.sizeDelta = new Vector2(160, 36);
        nameTxt.localScale = new Vector3(1, 1, 1);

        RectTransform logoRt = logo.GetComponent<RectTransform>();
        logoRt.anchorMin = dialogData.setImageLeft ? new Vector2(0, 0) : new Vector2(1, 0);
        logoRt.anchorMax = dialogData.setImageLeft ? new Vector2(0, 0) : new Vector2(1, 0);
        logoRt.pivot = dialogData.setImageLeft ? new Vector2(0, 0) : new Vector2(1, 0);
        logoRt.sizeDelta = new Vector2(100, 100);
        logoRt.anchoredPosition = dialogData.setImageLeft ? new Vector3(30, 0, 0) : new Vector3(-30, 0, 0);
        logoRt.localScale = new Vector3(1, 1, 1);

        currentDialogIndex++;
    }

    public void turnDialog() {
        this.playOnLoad = true;
    }

}

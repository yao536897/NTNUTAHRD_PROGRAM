using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ZXing;
using UnityEngine.UI;
using epoching.easy_gui;
using GlobalSetting;


namespace epoching.easy_qr_code
{
    public class Read_qr_code : MonoBehaviour
    {
        static readonly string[] questionCard = { "Color2", "Color3", "Color4", "Color5" };
        static readonly string[] functionCard = { "Auto", "Sleep", "Quiz", "Ending" };
        static readonly string eventCard = "Color6";

        [Header("raw_image_video")]
        public RawImage raw_image_video;

        [Header("audio source")]
        public AudioSource audio_source;

        //public ReadingResult res;

        //camera texture
        private WebCamTexture cam_texture;

        //is reading qr_code
        private bool is_reading = false;

        public UnityEvent eventCall;

        // Start is called before the first frame update

        void OnEnable()
        {
            StartCoroutine(this.start_webcam());
        }

        private IEnumerator start_webcam()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            yield return new WaitForSeconds(0.11f);

            //init camera texture
            this.cam_texture = new WebCamTexture();

            //this.cam_texture.requestedWidth = 720;
            //this.cam_texture.requestedHeight = 1280;

            this.cam_texture.requestedWidth = 540;
            this.cam_texture.requestedHeight = 720;


            this.cam_texture.Play();

            if (Application.platform == RuntimePlatform.Android)
            {
                this.raw_image_video.rectTransform.sizeDelta = new Vector2(Screen.width * cam_texture.width / (float)this.cam_texture.height, Screen.width);
                this.raw_image_video.rectTransform.rotation = Quaternion.Euler(0, 0, -90);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                this.raw_image_video.rectTransform.sizeDelta = new Vector2(1080, 1080 * this.cam_texture.width / (float)this.cam_texture.height);
                this.raw_image_video.rectTransform.localScale = new Vector3(-1, 1, 1);
                this.raw_image_video.rectTransform.rotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
                this.raw_image_video.rectTransform.sizeDelta = new Vector2(Camera.main.pixelWidth, Camera.main.pixelWidth * this.cam_texture.height / (float)this.cam_texture.width);
                this.raw_image_video.rectTransform.localScale = new Vector3(-1, 1, 1);
            }

            this.raw_image_video.texture = cam_texture;

            this.is_reading = true;

            yield return null;
        }


        void OnDisable()
        {
            if (this.cam_texture != null)
            {
                this.cam_texture.Stop();
            }
        }

        private float interval_time = 0.1f;
        private float time_stamp = 0;
        void Update()
        {
            if (this.is_reading)
            {
                this.time_stamp += Time.deltaTime;

                if (this.time_stamp > this.interval_time)
                {
                    this.time_stamp = 0;

                    try
                    {
                        Debug.Log("reading");

                        IBarcodeReader barcodeReader = new BarcodeReader();
                        // decode the current frame
                        var result = barcodeReader.Decode(this.cam_texture.GetPixels32(), this.cam_texture.width, this.cam_texture.height);
                        if (result != null)
                        {
                            Canvas_confirm_box.card_num = result.Text;

                            switch (GameController.gameStatus)
                            {
                                case Game_Status.SelectCharacter:
                                    if (GameController.currentCharacter == result.Text)
                                    {
                                        scanCharacter(result.Text);
                                    }
                                    else
                                    {
                                        Canvas_confirm_box.confirm_box
                                        (
                                           "錯誤角色卡",
                                           "Invalid",
                                           "",
                                           "返回",
                                           true,
                                            delegate () { },
                                            delegate ()
                                            {
                                                SceneManager.LoadScene("qrcode_scanner");
                                                this.is_reading = true;
                                            }
                                       );
                                        this.is_reading = false;

                                        this.audio_source.Play();
                                    }
                                    break;
                                case Game_Status.Gaming:
                                    switch (cardType(result.Text))
                                    {
                                        case "Question":
                                            // Canvas_confirm_box.confirm_box
                                            // (
                                            //    result.Text,
                                            //    "Invalid",
                                            //    "",
                                            //    "返回",
                                            //     delegate () { },
                                            //     delegate ()
                                            //     {
                                            //         int questionType = Array.IndexOf(questionCard, result.Text);
                                            //         CustomQuestion[] targetQuestions = Array.FindAll(GameController.questions, e => e.type == questionType);
                                            //         int targetIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, targetQuestions.Length));
                                            //         GameController.currentQuestion = targetQuestions[targetIndex];

                                            //         this.is_reading = true;
                                            //         SceneManager.LoadScene("CS_GameHotseat");
                                            //     }
                                            setQuestion(result.Text);
                                            SceneManager.LoadScene("CS_GameHotseat");
                                            //    );
                                            break;
                                        case "Function":

                                            string cardName = getCardName(result.Text);
                                            setQuestion();

                                            Canvas_confirm_box.confirm_box
                                            (
                                               cardName,
                                               result.Text,
                                               "取消",
                                               "使用",
                                                delegate ()
                                                {
                                                    SceneManager.LoadScene("qrcode_scanner");
                                                    this.is_reading = true;
                                                },
                                                delegate ()
                                                {
                                                    Debug.Log("使用" + cardName);
                                                    switch (result.Text)
                                                    {
                                                        case "Auto":
                                                            int nowScore = PlayerPrefs.GetInt("ps" + GameController.currentPlayer.ToString());
                                                            nowScore += 100;
                                                            PlayerPrefs.SetInt("ps" + GameController.currentPlayer, nowScore);
                                                            GameController.currentPlayer++;
                                                            SceneManager.LoadScene("qrcode_scanner");
                                                            break;
                                                        case "Sleep":
                                                            GameController.gameStatus = Game_Status.Card;
                                                            // sleeping = -2時，表示要等待掃描效果施加對象
                                                            CardBuffer.sleeping = -2;
                                                            SceneManager.LoadScene("qrcode_scanner");
                                                            break;
                                                        case "Quiz":
                                                            CardBuffer.extraQuestion = true;
                                                            SceneManager.LoadScene("CS_GameHotseat");
                                                            break;
                                                        case "Ending":
                                                            CardBuffer.forceGameOver = true;
                                                            SceneManager.LoadScene("CS_GameHotseat");
                                                            break;
                                                    }
                                                    // SceneManager.LoadScene("SetNickName");
                                                    this.is_reading = true;
                                                }
                                           );
                                            Debug.Log("讀取道具卡【自動作答】 " + result.Text);

                                            this.is_reading = false;

                                            this.audio_source.Play();
                                            break;
                                        case "Event":
                                            eventCall.Invoke();
                                            break;
                                        case "Unknown":
                                            Canvas_confirm_box.confirm_box
                                            (
                                               "未知卡片",
                                               "Invalid",
                                               "",
                                               "返回",
                                                true,
                                                delegate () { },
                                                delegate ()
                                                {
                                                    SceneManager.LoadScene("qrcode_scanner");
                                                    this.is_reading = true;
                                                }
                                           );
                                            this.is_reading = false;

                                            this.audio_source.Play();
                                            break;
                                    }
                                    break;
                                case Game_Status.Card:
                                    scanCharacter(result.Text);
                                    break;
                                default:
                                    Canvas_confirm_box.confirm_box
                                    (
                                       "未知的遊戲階段",
                                       "Invalid",
                                       "",
                                       "返回",
                                        true,
                                        delegate () { },
                                        delegate ()
                                        {
                                            SceneManager.LoadScene("qrcode_scanner");
                                            this.is_reading = true;
                                        }
                                   );
                                    this.is_reading = false;

                                    this.audio_source.Play();
                                    break;
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex.Message);
                        Canvas_confirm_box.confirm_box
                        (
                            "讀取失敗",
                            "error>>>" + ex.Message,
                            "取消",
                            "確認",
                            true,
                            delegate () { },
                            delegate ()
                            {
                                SceneManager.LoadScene("qrcode_scanner");
                            }
                        );

                        this.is_reading = false;
                    }
                }
            }
        }

        string cardType(string text)
        {
            if (Array.Exists(questionCard, e => e == text)) return "Question";
            if (Array.Exists(functionCard, e => e == text)) return "Function";
            if (text == eventCard) return "Event";
            return "Unknown";
        }

        string getCardName(string text)
        {
            switch (text)
            {
                case "Auto": return "使用「自動作答」？";
                case "Sleep": return "使用「搖籃曲」？";
                case "Quiz": return "使用「隨堂小考」？";
                case "Ending": return "使用「遊戲結束」？";
                default: return "無法辨識的卡片";
            }
        }

        void setQuestion(string text)
        {

            int questionType = Array.IndexOf(questionCard, text);
            CustomQuestion[] targetQuestions = Array.FindAll(GameController.questions, e => e.type == questionType);
            int targetIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, targetQuestions.Length));
            if (targetQuestions.Length > 0)
            {
                GameController.currentQuestion = targetQuestions[targetIndex];
            }
            else
            {
                GameController.currentQuestion = GameController.questions[0];
            }

            this.is_reading = true;
        }

        void setQuestion()
        {
            int targetIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, GameController.questions.Length));
            GameController.currentQuestion = GameController.questions[targetIndex];

            this.is_reading = true;
        }

        void setPlayerSleep(Character name)
        {
            int targetIndex = Array.FindIndex(GameController.playerName, e => e == name.ToString());
            CardBuffer.sleeping = targetIndex;
            GameController.gameStatus = Game_Status.Gaming;
            SceneManager.LoadScene("qrcode_scanner");
        }

        void scanCharacter(string text)
        {
            string characterTitle = "";
            switch (text)
            {
                case "John":
                    characterTitle = CardBuffer.sleeping == -2 ? "目標玩家_" + GameController.playerNameMap[Character.John.ToString()] : "讀取角色卡_" + GameController.playerNameMap[Character.John.ToString()];
                    Canvas_confirm_box.confirm_box
                    (
                       characterTitle,
                       text,
                       "取消",
                       "選定角色[" + GameController.playerNameMap[Character.John.ToString()] + "]",
                        delegate ()
                        {
                            SceneManager.LoadScene("qrcode_scanner");
                            this.is_reading = true;
                        },
                        delegate ()
                        {
                            if (CardBuffer.sleeping == -2)
                            {
                                setPlayerSleep(Character.John);
                            }
                            else
                            {
                                Debug.Log("選定角色:約翰");
                                SceneManager.LoadScene("SetNickName");
                                this.is_reading = true;
                            }
                        }
                   );
                    Debug.Log("角色資料[約翰]: " + text);

                    this.is_reading = false;

                    this.audio_source.Play();

                    break;

                case "Jackie":
                    characterTitle = CardBuffer.sleeping == -2 ? "目標玩家_" + GameController.playerNameMap[Character.Jacky.ToString()] : "讀取角色卡_" + GameController.playerNameMap[Character.Jacky.ToString()];
                    Canvas_confirm_box.confirm_box
                    (
                       characterTitle,
                       text,
                       "取消",
                       "選定角色[" + GameController.playerNameMap[Character.Jacky.ToString()] + "]",
                        delegate ()
                        {
                            SceneManager.LoadScene("qrcode_scanner");
                            this.is_reading = true;
                        },
                        delegate ()
                        {
                            if (CardBuffer.sleeping == -2)
                            {
                                setPlayerSleep(Character.Jacky);
                            }
                            else
                            {
                                Debug.Log("選定角色:杰奇");
                                SceneManager.LoadScene("SetNickName");
                                this.is_reading = true;
                            }
                        }
                   );
                    Debug.Log("角色資料[杰奇]: " + text);

                    this.is_reading = false;

                    this.audio_source.Play();

                    break;
                case "Teresa":
                    characterTitle = CardBuffer.sleeping == -2 ? "目標玩家_" + GameController.playerNameMap[Character.Teresa.ToString()] : "讀取角色卡_" + GameController.playerNameMap[Character.Teresa.ToString()];
                    Canvas_confirm_box.confirm_box
                    (
                       characterTitle,
                       text,
                       "取消",
                       "選定角色[" + GameController.playerNameMap[Character.Teresa.ToString()] + "]",
                        delegate ()
                        {
                            SceneManager.LoadScene("qrcode_scanner");
                            this.is_reading = true;
                        },
                        delegate ()
                        {
                            if (CardBuffer.sleeping == -2)
                            {
                                setPlayerSleep(Character.Teresa);
                            }
                            else
                            {
                                Debug.Log("選定角色:泰瑞莎");
                                SceneManager.LoadScene("SetNickName");
                                this.is_reading = true;
                            }
                        }
                   );
                    Debug.Log("角色資料[泰瑞莎]: " + text);

                    this.is_reading = false;

                    this.audio_source.Play();

                    break;
                case "Aries":
                    characterTitle = CardBuffer.sleeping == -2 ? "目標玩家_" + GameController.playerNameMap[Character.Aries.ToString()] : "讀取角色卡_" + GameController.playerNameMap[Character.Aries.ToString()];
                    Canvas_confirm_box.confirm_box
                    (
                       characterTitle,
                       text,
                       "取消",
                       "選定角色[" + GameController.playerNameMap[Character.Aries.ToString()] + "]",
                        delegate ()
                        {
                            SceneManager.LoadScene("qrcode_scanner");
                            this.is_reading = true;
                        },
                        delegate ()
                        {
                            if (CardBuffer.sleeping == -2)
                            {
                                setPlayerSleep(Character.Aries);
                            }
                            else
                            {
                                Debug.Log("選定角色:艾瑞絲");
                                SceneManager.LoadScene("SetNickName");
                                this.is_reading = true;
                            }
                        }
                   );
                    Debug.Log("角色資料[艾瑞絲]: " + text);

                    this.is_reading = false;

                    this.audio_source.Play();

                    break;
            }
        }

    }
}

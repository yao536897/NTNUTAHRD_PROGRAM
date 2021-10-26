using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using epoching.easy_gui;
using GlobalSetting;

namespace epoching.easy_qr_code
{
    public class Demo_control : MonoBehaviour
    {
        static readonly string[] questionCard = { "Color1", "Color2", "Color3", "Color4" };
        static readonly string[] functionCard = { "Inherit", "Grab", "Auto", "Assign", "Cake", "Booster" };
        public TextAsset questionJson = null;






        [Header("audio source")]
        public AudioSource audio_source;

        //singleton
        public static Demo_control instance;

        //public WebCamTexture cam_texture;

        private Demo_statu demo_statu;  //the statu of the demo

        [Header("main gameobject, read_qr_code gameobject,generate_qr_code gameobject")]
        public GameObject game_obj_main;
        public GameObject game_obj_read_qr_code;
        public GameObject game_obj_generate_qr_code;
        //public GameObject ImageA;

        public DialogPlugin[] dialogPlugins = new DialogPlugin[0];

        void Awake()
        {
            Demo_control.instance = this;

            this.change_to_main();
        }

        void Start()
        {

            // new WebCamTexture(WebCamTexture.devices[0].name);
            // Screen.orientation = ScreenOrientation.Portrait;

            // QuestionContainer questionContainer = JsonUtility.FromJson<QuestionContainer>(questionJson.text);
            // GameController.questions = questionContainer.data;

            //WebCamTexture cam_texture = new WebCamTexture();
            //cam_texture.Play();
        }

        public void change_to_main()
        {
            StartCoroutine(Canvas_grounp_fade.hide(this.game_obj_read_qr_code));
            StartCoroutine(Canvas_grounp_fade.hide(this.game_obj_generate_qr_code));

            StartCoroutine(Canvas_grounp_fade.show(this.game_obj_main));


            this.demo_statu = Demo_statu.main;
        }

        public void change_to_read_qr_code()
        {
            StartCoroutine(Canvas_grounp_fade.hide(this.game_obj_main));
            StartCoroutine(Canvas_grounp_fade.hide(this.game_obj_generate_qr_code));

            StartCoroutine(Canvas_grounp_fade.show(this.game_obj_read_qr_code));


            this.demo_statu = Demo_statu.read_qr_code;
        }

        public void change_to_generate_qr_code()
        {
            StartCoroutine(Canvas_grounp_fade.hide(this.game_obj_read_qr_code));
            StartCoroutine(Canvas_grounp_fade.hide(this.game_obj_main));

            StartCoroutine(Canvas_grounp_fade.show(this.game_obj_generate_qr_code));


            this.demo_statu = Demo_statu.generate_qr_code;
        }
        string cardType(string text)
        {
            if (Array.Exists(questionCard, e => e == text)) return "Question";
            if (Array.Exists(functionCard, e => e == text)) return "Function";
            return "Unknown";
        }
        //event
        #region
        public void on_read_qr_code_btn()
        {
            // switch (GameController.gameStatus)
            // {
            //     case Game_Status.SelectCharacter:
            //         SceneManager.LoadScene("SetNickName");
            //         break;
            //     case Game_Status.Gaming:
            //         // this.change_to_read_qr_code();
            //         // int questionType = Array.IndexOf(questionCard, "Color2");
            //         // CustomQuestion[] targetQuestions = Array.FindAll(GameController.questions, e => e.type == questionType);
            //         // int targetIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, targetQuestions.Length));
            //         // GameController.currentQuestion = targetQuestions[targetIndex];

            //         // SceneManager.LoadScene("CS_GameHotseat");

            //         randomEvent();
            //         break;
            // }
            this.change_to_read_qr_code();
            this.audio_source.Play();
        }

        public void randomEvent()
        {
            StartCoroutine(Canvas_grounp_fade.hide(this.game_obj_main));
            StartCoroutine(Canvas_grounp_fade.hide(this.game_obj_read_qr_code));
            StartCoroutine(Canvas_grounp_fade.hide(this.game_obj_generate_qr_code));


            int dialogIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, dialogPlugins.Length));
            dialogPlugins[dialogIndex].Dialog();
        }

        public void on_generate_qr_code_btn()
        {
            this.change_to_generate_qr_code();
            this.audio_source.Play();
        }

        public void on_back_btn()
        {
            this.change_to_main();
            this.audio_source.Play();
        }
        #endregion
    }

    public enum Demo_statu
    {
        main,
        read_qr_code,
        generate_qr_code
    }

}

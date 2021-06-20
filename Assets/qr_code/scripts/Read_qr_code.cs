using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using epoching.easy_debug_on_the_phone;
using UnityEngine.UI;
using epoching.easy_gui;


namespace epoching.easy_qr_code
{
    public class Read_qr_code : MonoBehaviour
    {
        [Header("raw_image_video")]
        public RawImage raw_image_video;

        [Header("audio source")]
        public AudioSource audio_source;

        //public ReadingResult res;

        //camera texture
        private WebCamTexture cam_texture;

        //is reading qr_code
        private bool is_reading = false;


        // Start is called before the first frame update

        void OnEnable()
        {
            StartCoroutine(this.start_webcam());
        }

        private IEnumerator start_webcam()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
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
                            Canvas_confirm_box.card_num=result.Text;
                            switch (result.Text)
                            {
                                case "A":
                                    Canvas_confirm_box.confirm_box
                                    (
                                       "讀取卡片A",
                                       result.Text,
                                       "取消",
                                       "選定角色A",
                                        delegate ()
                                        {
                                            this.is_reading = true;
                                        },
                                        delegate ()
                                        {
                                            Debug.Log("選定角色A");
                                            Application.OpenURL(result.Text);
                                            this.is_reading = true;
                                        }
                                   );
                                    Debug.Log("角色資料A: " + result.Text);

                                    this.is_reading = false;

                                    this.audio_source.Play();

                                    break;

                                case "B":
                                    Canvas_confirm_box.confirm_box
                                    (
                                       "讀取卡片B",
                                       result.Text,
                                       "取消",
                                       "選定角色B",
                                        delegate ()
                                        {
                                            this.is_reading = true;
                                        },
                                        delegate ()
                                        {
                                            Debug.Log("選定角色B");
                                            Application.OpenURL(result.Text);
                                            this.is_reading = true;
                                        }
                                   );
                                   Debug.Log("角色資料B: " + result.Text);

                                   this.is_reading = false;

                                   this.audio_source.Play();
                                   
                                   break;

                                default:
                                    Canvas_confirm_box.confirm_box
                                    (
                                       "讀取道具卡【自動作答】",
                                       result.Text,
                                       "取消",
                                       "使用道具卡",
                                        delegate ()
                                        {
                                            this.is_reading = true;
                                        },
                                        delegate ()
                                        {
                                            Debug.Log("讀取道具卡【自動作答】");
                                            Application.OpenURL(result.Text);
                                            this.is_reading = true;
                                        }
                                   );
                                    Debug.Log("讀取道具卡【自動作答】 " + result.Text);

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
                            delegate () { }
                        );

                        this.is_reading = false;
                    }
                }
            }
        }
    }
}


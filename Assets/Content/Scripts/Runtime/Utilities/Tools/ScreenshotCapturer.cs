using System;
using System.IO;
using ThunderWire.Attributes;
using System.Collections;
using UnityEngine;
using UHFPS.Input;

namespace UHFPS.Tools
{
    [Summary("Thành phần hỗ trợ chụp ảnh màn hình bằng phím tắt và lưu vào thư mục định trước.")]
    public class ScreenshotCapturer : MonoBehaviour
    {
        [Tooltip("Hành động Input để chụp ảnh.")]
        public string ScreenshotAction = "input.action.screenshot";
        [Tooltip("Thư mục lưu ảnh (tính từ đường dẫn Application.dataPath).")]
        public string ScreenshotDirectory = "/Screenshots/";
        [Tooltip("Hệ số nhân độ phân giải ảnh chụp.")]
        public int ResolutionMultiplier = 1;

        private int screenshotCounter = 0;

        void Start()
        {
            ScreenshotDirectory = Application.dataPath + ScreenshotDirectory;
            Directory.CreateDirectory(ScreenshotDirectory);
        }

        void Update()
        {
            if(InputManager.ReadButtonOnce(this, ScreenshotAction))
            {
                StartCoroutine(CaptureScreenshot());
            }
        }

        IEnumerator CaptureScreenshot()
        {
            // Let the frame render completely before taking a screenshot
            yield return new WaitForEndOfFrame();

            // Construct the screenshot file path
            string screenshotName = "Screenshot_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + "_" + screenshotCounter + ".png";
            string screenshotFilePath = ScreenshotDirectory + screenshotName;

            // Take the screenshot
            ScreenCapture.CaptureScreenshot(screenshotFilePath, ResolutionMultiplier);
            Debug.Log("Screenshot Saved to: " + screenshotFilePath);

            // Increase the screenshot counter
            screenshotCounter++;
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Background Fader")]
    public class BackgroundFader : MonoBehaviour
    {
        public Image Background;

        /// <summary>
        /// Start background fade.
        /// </summary>
        public IEnumerator StartBackgroundFade(bool fadeOut, float waitTime = 0, float fadeSpeed = 3)
        {
            yield return new WaitForEndOfFrame();

            if (fadeOut)
            {
                Background.enabled = true;

                yield return new WaitForSecondsRealtime(waitTime);
                Color color = Background.color;

                while (color.a > 0)
                {
                    color.a = Mathf.MoveTowards(color.a, 0, Time.deltaTime * fadeSpeed);
                    Background.color = color;
                    yield return null;
                }

                Background.enabled = false;
            }
            else
            {
                Color color = Background.color;
                color.a = 0;

                Background.color = color;
                Background.enabled = true;

                while (color.a < 1)
                {
                    color.a = Mathf.MoveTowards(color.a, 1, Time.deltaTime * fadeSpeed);
                    Background.color = color;
                    yield return null;
                }

                yield return new WaitForSecondsRealtime(waitTime);
            }
        }
    }
}
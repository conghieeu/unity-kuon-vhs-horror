using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Tools;
using TMPro;
using static UHFPS.Scriptable.DialogueAsset;

namespace UHFPS.Runtime
{
    public class DialogueSystem : Singleton<DialogueSystem>
    {
        public enum DialogueBinderType { Start, Subtitle, Finish, End }

        public sealed class DialogueData : List<Dialogue> { }

        public AudioSource AudioSource;
        public CanvasGroup DialoguePanel;
        public TMP_Text DialogueText;

        public bool ShowNarrator;
        public bool UseNarratorColors;
        public float SequenceWait;
        public float FadeTime;

        private string dialogueBinderName;
        private DialogueTrigger currentTrigger;
        private AudioSource currentAudio;
        private DialogueData currentData;
        private int dialogueIndex = 0;

        private bool fadeDialoguePanel;
        private bool forceHidePanel;
        private bool isSequenceType;
        private bool dialoguePlaying;
        private bool nextDialogueTrigger;

        private DialogueBinder[] dialogueBinders;

        /// <summary>
        /// Status, whether the dialogue is playing.
        /// </summary>
        public bool IsPlaying => dialoguePlaying;

        public Subject<Unit> OnDialogueStart = new();
        public Subject<Unit> OnDialogueEnd = new();

        private void Awake()
        {
            dialogueBinders = FindObjectsByType<DialogueBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        /// <summary>
        /// Play the dialogue specified in the Dialogue Trigger component.
        /// </summary>
        /// <returns>State if the dialogue is being played.</returns>
        public bool PlayDialogue(DialogueTrigger trigger)
        {
            if (dialoguePlaying) 
                return false;

            currentTrigger = trigger;
            dialogueBinderName = trigger.BinderName;
            currentData = trigger.DialogueData;
            var dialogue = currentData[dialogueIndex];

            if(trigger.DialogueType == DialogueTrigger.DialogueTypeEnum.Global)
            {
                currentAudio = AudioSource;
            }
            else
            {
                currentAudio = trigger.DialogueAudio;
            }

            dialoguePlaying = true;
            isSequenceType = trigger.DialogueContinue == DialogueTrigger.DialogueContinueEnum.Sequence;

            OnDialogueStart.OnNext(Unit.Default);
            SendBinderEvent(DialogueBinderType.Start);
            StartCoroutine(HandleDialogue(dialogue));
            return true;
        }

        /// <summary>
        /// Stop the currently running dialogue.
        /// </summary>
        public void StopDialogue()
        {
            if (currentAudio == null || currentData == null || !dialoguePlaying)
                return;

            SendBinderEvent(DialogueBinderType.End);
            StopAllCoroutines();
            ResetDialogue();

            nextDialogueTrigger = false;
            fadeDialoguePanel = false;
            dialoguePlaying = false;
        }

        /// <summary>
        /// Start the next dialogue if the dialogue type is not a sequence.
        /// </summary>
        public void NextDialogue()
        {
            if (currentAudio == null || currentData == null || !dialoguePlaying)
                return;

            nextDialogueTrigger = true;
        }

        /// <summary>
        /// Force show the dialogue panel.
        /// </summary>
        public void ForceShow(bool show)
        {
            if (show)
            {
                forceHidePanel = false;
            }
            else
            {
                forceHidePanel = true;
                DialoguePanel.alpha = 0f;
            }
        }

        private IEnumerator HandleDialogue(Dialogue dialogue)
        {
            yield return HandleSubtitles(dialogue);
            currentTrigger.IsCompleted = true;

            SendBinderEvent(DialogueBinderType.End);
            ResetDialogue();

            nextDialogueTrigger = false;
            fadeDialoguePanel = false;
            dialoguePlaying = false;
        }

        private IEnumerator HandleSubtitles(Dialogue dialogue)
        {
            // set dialogue audio and play
            currentAudio.clip = dialogue.DialogueAudio;
            currentAudio.Play();

            // handle single dialogue subtitle
            if (dialogue.SubtitleType == SubtitleTypeEnum.Single)
            {
                var subtitle = dialogue.SingleSubtitle;
                yield return new WaitForSeconds(subtitle.Time);

                ShowDialogueText(subtitle);
                fadeDialoguePanel = true;

                // send subtitle event to binder
                SendBinderEvent(DialogueBinderType.Subtitle, new object[] { dialogue.DialogueAudio, (string)subtitle.Text });

                yield return new WaitUntil(() => !currentAudio.isPlaying);
            }
            else
            {
                // handle multiple dialogue subtitles
                int subtitleIndex = -1;
                List<SubtitleEntry> sortedSubtitles = dialogue.Subtitles.OrderBy(x => x.Time).ToList();

                while (currentAudio.isPlaying)
                {
                    float time = currentAudio.time;
                    for (int i = subtitleIndex + 1; i < sortedSubtitles.Count; i++)
                    {
                        var subtitle = sortedSubtitles[i];

                        // wait until next subtitle time position
                        if (time > subtitle.Time)
                        {
                            if(subtitle is DialogueSubtitle sub)
                            {
                                fadeDialoguePanel = true;
                                ShowDialogueText(sub);

                                if (subtitleIndex != i)
                                {
                                    // send subtitle event to binder
                                    SendBinderEvent(DialogueBinderType.Subtitle, new object[] { dialogue.DialogueAudio, (string)sub.Text });
                                    subtitleIndex = i;
                                }
                            }
                            else if(subtitle is SubtitleBreak)
                            {
                                fadeDialoguePanel = false;
                                subtitleIndex = i;
                            }

                            break;
                        }
                    }

                    yield return null;
                }
            }

            // stop audio source just in case
            currentAudio.Stop();

            // handle other dialogues
            if(dialogueIndex < currentData.Count - 1)
            {
                var nextDialogue = currentData[++dialogueIndex];

                // send dialogue finish event
                SendBinderEvent(DialogueBinderType.Finish);

                if (isSequenceType)
                {
                    // sequence time wait
                    yield return new WaitForSeconds(SequenceWait);
                }
                else
                {
                    // wait for next dialogue trigger 
                    yield return new WaitUntil(() => nextDialogueTrigger);
                    nextDialogueTrigger = false;
                }

                // handle next dialogue
                yield return HandleSubtitles(nextDialogue);
            }

            OnDialogueEnd.OnNext(Unit.Default);
        }

        private void ShowDialogueText(DialogueSubtitle subtitle)
        {
            if (UseNarratorColors)
            {
                string color = ColorUtility.ToHtmlStringRGB(subtitle.NarratorColor);
                string text = ShowNarrator && !subtitle.Narrator.IsEmpty()
                    ? $"<b><color=#{color}>{subtitle.Narrator}: </color></b> {subtitle.Text}"
                    : $"{subtitle.Text}";

                DialogueText.text = text;
            }
            else
            {
                string text = ShowNarrator && !subtitle.Narrator.IsEmpty()
                    ? $"<b>{subtitle.Narrator}: </b> {subtitle.Text}"
                    : $"{subtitle.Text}";

                DialogueText.text = text;
            }
        }

        private void SendBinderEvent(DialogueBinderType binderType, object[] parameters = null)
        {
            foreach (var binder in dialogueBinders)
            {
                switch (binderType)
                {
                    case DialogueBinderType.Start:
                        binder.OnDialogueStart?.Invoke(currentAudio, dialogueBinderName);
                        break;
                    case DialogueBinderType.Subtitle:
                        AudioClip subtitleClip = (AudioClip)parameters[0];
                        string subtitleText = (string)parameters[1];
                        binder.OnSubtitle?.Invoke(subtitleClip, subtitleText);
                        break;
                    case DialogueBinderType.Finish:
                        binder.OnSubtitleFinish?.Invoke();
                        break;
                    case DialogueBinderType.End:
                        binder.OnDialogueEnd?.Invoke();
                        break;
                }
            }
        }

        private void ResetDialogue()
        {
            if (currentAudio != null)
            {
                currentAudio.Stop();
                currentAudio.clip = null;
                currentAudio = null;
            }

            dialogueBinderName = string.Empty;
            currentTrigger = null;
            currentData = null;
            dialogueIndex = 0;
        }

        private void Update()
        {
            if (forceHidePanel)
            {
                DialoguePanel.alpha = 0f;
                return;
            }

            if (fadeDialoguePanel)
            {
                if (!Mathf.Approximately(DialoguePanel.alpha, 1f))
                {
                    DialoguePanel.alpha = Mathf.MoveTowards(DialoguePanel.alpha, 1f, Time.deltaTime * FadeTime);
                }
                else
                {
                    DialoguePanel.alpha = 1f;
                }
            }
            else
            {
                if (!Mathf.Approximately(DialoguePanel.alpha, 0f))
                {
                    DialoguePanel.alpha = Mathf.MoveTowards(DialoguePanel.alpha, 0f, Time.deltaTime * FadeTime);
                }
                else
                {
                    DialoguePanel.alpha = 0f;
                }
            }
        }
    }
}
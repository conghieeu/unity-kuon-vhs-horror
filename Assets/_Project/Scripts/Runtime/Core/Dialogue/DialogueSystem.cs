using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UHFPS.Scriptable;
using UHFPS.Tools;
using TMPro;
using static UHFPS.Scriptable.DialogueAsset;

namespace UHFPS.Runtime
{
    public class DialogueSystem : Singleton<DialogueSystem>
    {
        public enum DialogueBinderType { Start, Subtitle, Finish, End, Options }

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

        private bool optionSelected;
        private int selectedOptionIndex;

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
        /// Select an option by its jump index.
        /// </summary>
        public void SelectOption(int jumpIndex)
        {
            if (currentData == null || !dialoguePlaying)
                return;

            selectedOptionIndex = jumpIndex;
            optionSelected = true;
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

            // Reset trigger so it can be re-triggered on next interact
            currentTrigger.ResetTrigger();

            ResetDialogue();

            nextDialogueTrigger = false;
            fadeDialoguePanel = false;
            dialoguePlaying = false;
        }

        private IEnumerator HandleSubtitles(Dialogue dialogue)
        {
            // set dialogue audio and play
            bool hasAudio = dialogue.DialogueAudio != null;
            if (hasAudio)
            {
                currentAudio.clip = dialogue.DialogueAudio;
                currentAudio.Play();
            }

            bool wasSkipped = false;
            float timer = 0f;

            // handle single dialogue subtitle
            if (dialogue.SubtitleType == SubtitleTypeEnum.Single)
            {
                var subtitle = dialogue.SingleSubtitle;
                
                // delay before showing
                while (timer < subtitle.Time)
                {
                    if (dialogue.CanSkip && IsSkipInputPressed())
                    {
                        wasSkipped = true;
                        break;
                    }
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (!wasSkipped)
                {
                    ShowDialogueText(subtitle);
                    fadeDialoguePanel = true;
                    SendBinderEvent(DialogueBinderType.Subtitle, new object[] { dialogue.DialogueAudio, (string)subtitle.Text });

                    float displayTimer = 0f;
                    while (hasAudio ? currentAudio.isPlaying : displayTimer < 3f)
                    {
                        if (dialogue.CanSkip && IsSkipInputPressed())
                        {
                            wasSkipped = true;
                            break;
                        }
                        displayTimer += Time.deltaTime;
                        yield return null;
                    }
                }
            }
            else
            {
                // handle multiple dialogue subtitles
                int subtitleIndex = -1;
                List<SubtitleEntry> sortedSubtitles = dialogue.Subtitles.OrderBy(x => x.Time).ToList();
                float maxTime = sortedSubtitles.Count > 0 ? sortedSubtitles.Last().Time + 2f : 2f;

                while (hasAudio ? currentAudio.isPlaying : timer < maxTime)
                {
                    if (dialogue.CanSkip && IsSkipInputPressed())
                    {
                        wasSkipped = true;
                        break;
                    }

                    float time = hasAudio ? currentAudio.time : timer;
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

                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            // stop audio source just in case
            currentAudio.Stop();

            // handle dialogue end types
            if (dialogue.EndType == DialogueEndType.End)
            {
                // do nothing, let it fall through
            }
            else if (dialogue.EndType == DialogueEndType.Options)
            {
                optionSelected = false;
                selectedOptionIndex = -1;

                // Ensure options UI is visible
                fadeDialoguePanel = true;

                // send dialogue finish event
                SendBinderEvent(DialogueBinderType.Finish);
                SendBinderEvent(DialogueBinderType.Options, new object[] { dialogue.Options });
                
                // Show options UI directly
                var ui = DialoguePanel.GetComponent<DialogueOptionsUI>();
                if(ui != null) ui.OnShowOptions(dialogue.Options);

                yield return new WaitUntil(() => optionSelected);

                // wait 1 frame to clear input state so it doesn't double-skip the next dialogue
                yield return null;

                if (selectedOptionIndex >= 0 && selectedOptionIndex < currentData.Count)
                {
                    dialogueIndex = selectedOptionIndex;
                    var nextDialogue = currentData[dialogueIndex];

                    yield return HandleSubtitles(nextDialogue);
                }
            }
            else
            {
                int nextIndex = dialogue.EndType == DialogueEndType.JumpToIndex ? dialogue.JumpIndex : dialogueIndex + 1;

                if (nextIndex >= 0 && nextIndex < currentData.Count)
                {
                    dialogueIndex = nextIndex;
                    var nextDialogue = currentData[dialogueIndex];

                    // send dialogue finish event
                    SendBinderEvent(DialogueBinderType.Finish);

                    if (!wasSkipped)
                    {
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

                            // wait 1 frame to clear the input state so we don't double-skip
                            yield return null;
                        }
                    }
                    else
                    {
                        // wait 1 frame to clear the input state so we don't double-skip
                        yield return null;
                        nextDialogueTrigger = false;
                    }

                    // handle next dialogue
                    yield return HandleSubtitles(nextDialogue);
                }
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
                    case DialogueBinderType.Options:
                        List<DialogueOption> options = (List<DialogueOption>)parameters[0];
                        binder.OnShowOptions?.Invoke(options);
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

            var ui = DialoguePanel.GetComponent<DialogueOptionsUI>();
            if(ui != null) ui.OnDialogueEnd();
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

        private bool IsSkipInputPressed()
        {
            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                return true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;
            return false;
        }
    }
}
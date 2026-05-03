using System;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Runtime;

namespace UHFPS.Scriptable
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "UHFPS/Dialogue/Dialogue Asset")]
    public class DialogueAsset : ScriptableObject
    {
        public enum SubtitleTypeEnum { Single, Multiple }
        public enum DialogueEndType { NextSequence, JumpToIndex, Options, End }

        [Serializable]
        public class DialogueOption
        {
            public GString OptionText;
            public int JumpToIndex;
        }

        [Serializable]
        public abstract class SubtitleEntry 
        {
            public float Time;
        }

        [Serializable]
        public sealed class SubtitleBreak : SubtitleEntry { }

        [Serializable]
        public sealed class DialogueSubtitle : SubtitleEntry
        {
            public string Narrator;
            public Color NarratorColor = Color.white;
            public GString Text;
        }

        [Serializable]
        public sealed class Dialogue
        {
            public AudioClip DialogueAudio;
            public SubtitleTypeEnum SubtitleType;

            public DialogueSubtitle SingleSubtitle = new();

            [SerializeReference]
            public List<SubtitleEntry> Subtitles = new();

            public DialogueEndType EndType = DialogueEndType.NextSequence;
            public int JumpIndex;
            public List<DialogueOption> Options = new();

            public Dialogue()
            {
                DialogueAudio = null;
                Subtitles = new();
                Options = new();
            }

            public Dialogue Copy()
            {
                Dialogue copy = new()
                {
                    DialogueAudio = DialogueAudio,
                    SubtitleType = SubtitleType,
                    Subtitles = new(),
                    EndType = EndType,
                    JumpIndex = JumpIndex,
                    Options = new(),

                    SingleSubtitle = new()
                    {
                        Time = SingleSubtitle.Time,
                        Narrator = SingleSubtitle.Narrator,
                        NarratorColor = SingleSubtitle.NarratorColor,
                        Text = new(SingleSubtitle.Text),
                    }
                };

                foreach (var subtitle in Subtitles)
                {
                    if(subtitle is SubtitleBreak subBreak)
                    {
                        copy.Subtitles.Add(new SubtitleBreak()
                        {
                            Time = subBreak.Time
                        });
                    }
                    else if(subtitle is DialogueSubtitle dialogue)
                    {
                        copy.Subtitles.Add(new DialogueSubtitle()
                        {
                            Time = dialogue.Time,
                            Narrator = dialogue.Narrator,
                            NarratorColor = dialogue.NarratorColor,
                            Text = new(dialogue.Text)
                        });
                    }
                }

                foreach (var option in Options)
                {
                    copy.Options.Add(new DialogueOption()
                    {
                        OptionText = new(option.OptionText),
                        JumpToIndex = option.JumpToIndex
                    });
                }

                return copy;
            }
        }

        public List<Dialogue> Dialogues = new();
    }
}
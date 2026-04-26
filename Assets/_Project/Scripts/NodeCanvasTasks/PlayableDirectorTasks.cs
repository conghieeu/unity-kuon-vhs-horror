using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.Playables;

namespace NodeCanvas.Tasks.Cutscene
{
    [Name("Check Playable Director Finished")]
    [Category("Extensions/Cutscene")]
    [Description("Trả về true khi PlayableDirector đã hoàn thành phát (trạng thái không phải là Playing).")]
    public class CheckPlayableDirectorFinished : ConditionTask<PlayableDirector>
    {
        protected override string info
        {
            get { return "Timeline Finished"; }
        }

        protected override bool OnCheck()
        {
            return agent.state != PlayState.Playing;
        }
    }

    [Name("Pause Playable Director")]
    [Category("Extensions/Cutscene")]
    [Description("Tạm dừng PlayableDirector đang phát (Timeline).")]
    public class PausePlayableDirector : ActionTask<PlayableDirector>
    {
        protected override string info
        {
            get { return "Pause Timeline"; }
        }

        protected override void OnExecute()
        {
            if (agent == null)
            {
                EndAction(false);
                return;
            }

            agent.Pause();
            EndAction(true);
        }
    }

    [Name("Play Playable Director")]
    [Category("Extensions/Cutscene")]
    [Description("Phát PlayableDirector (Timeline). Có thể tùy chọn chờ đến khi timeline kết thúc trước khi kết thúc hành động.")]
    public class PlayPlayableDirector : ActionTask<PlayableDirector>
    {
        [Name("Wait Until Finish")]
        public bool waitUntilFinish = true;

        [Name("Restart If Playing")]
        [Tooltip("Nếu true, timeline sẽ khởi động lại từ đầu ngay cả khi đang phát.")]
        public bool restartIfPlaying = true;

        [Name("Initial Time")]
        [Tooltip("Thời gian (tính bằng giây) để bắt đầu phát từ. 0 = bắt đầu.")]
        public BBParameter<double> initialTime = 0;

        private bool _timelineStopped;

        protected override string info
        {
            get { return (waitUntilFinish ? "Play Timeline & Wait" : "Play Timeline"); }
        }

        protected override void OnExecute()
        {
            if (agent == null)
            {
                EndAction(false);
                return;
            }

            _timelineStopped = false;

            // Subscribe to the stopped event — this is the most reliable signal
            // that the PlayableDirector has finished (fires for WrapMode.None).
            agent.stopped += OnTimelineStopped;

            if (restartIfPlaying || agent.state != PlayState.Playing)
            {
                agent.time = initialTime.value;
                agent.Play();
            }

            if (!waitUntilFinish)
            {
                agent.stopped -= OnTimelineStopped;
                EndAction(true);
            }
        }

        protected override void OnUpdate()
        {
            if (agent == null)
            {
                EndAction(false);
                return;
            }

            // 1. The stopped callback fired (WrapMode.None triggers this).
            if (_timelineStopped)
            {
                EndAction(true);
                return;
            }

            // 2. Director is no longer playing (Paused/stopped externally).
            if (agent.state != PlayState.Playing)
            {
                EndAction(true);
                return;
            }

            // 3. WrapMode.Hold keeps state == Playing at the end,
            //    so check if time has reached (or nearly reached) the duration.
            //    Use a small tolerance to avoid floating-point precision issues.
            if (agent.duration > 0 && agent.time >= agent.duration - 0.05f)
            {
                EndAction(true);
                return;
            }
        }

        private void OnTimelineStopped(PlayableDirector director)
        {
            _timelineStopped = true;
        }

        protected override void OnStop()
        {
            // Unsubscribe to prevent memory leaks / double-calls.
            if (agent != null)
            {
                agent.stopped -= OnTimelineStopped;
            }
        }
    }

    [Name("Resume Playable Director")]
    [Category("Extensions/Cutscene")]
    [Description("Tiếp tục PlayableDirector đã tạm dừng (Timeline). Có thể tùy chọn chờ đến khi timeline kết thúc.")]
    public class ResumePlayableDirector : ActionTask<PlayableDirector>
    {
        [Name("Wait Until Finish")]
        public bool waitUntilFinish = true;

        protected override string info
        {
            get { return (waitUntilFinish ? "Resume Timeline & Wait" : "Resume Timeline"); }
        }

        protected override void OnExecute()
        {
            if (agent == null)
            {
                EndAction(false);
                return;
            }

            agent.Resume();

            if (!waitUntilFinish)
            {
                EndAction(true);
            }
        }

        protected override void OnUpdate()
        {
            if (agent.state != PlayState.Playing)
            {
                EndAction(true);
            }
        }
    }

    [Name("Stop Playable Director")]
    [Category("Extensions/Cutscene")]
    [Description("Dừng PlayableDirector đang phát (Timeline).")]
    public class StopPlayableDirector : ActionTask<PlayableDirector>
    {
        protected override string info
        {
            get { return "Stop Timeline"; }
        }

        protected override void OnExecute()
        {
            if (agent == null)
            {
                EndAction(false);
                return;
            }

            agent.Stop();
            EndAction(true);
        }
    }
}
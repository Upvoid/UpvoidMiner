using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EfficientUI;

namespace UpvoidMiner.UI
{
    public class TutorialUI : UIProxy
    {
        public TutorialUI()
            : base("Tutorial")
        {
            UIProxyManager.AddProxy(this);
        }

        public class TutorialMsg
        {
            public string Type;
            public string Tutorial;
        }

        public Dictionary<string, MsgUI> Msgs = new Dictionary<string, MsgUI>();

        [UICollection("TutorialMessage")]
        public IEnumerable<MsgUI> TutorialMsgs { get { return Msgs.Values; } }

        public class MsgUI : UIProxy
        {
            [UIObject]
            public string Name { get; set; }

            [UIObject]
            public string Progress { get; set; }

            public MsgUI(string name)
            {
                Name = name;
            }
        }
    }

    public static class Tutorials
    {

        public class TutorialMessage
        {
            private readonly Func<float, float, string> progFunc;
            public bool Visible = false;
            public bool Cleared = false;

            public string Name;

            public List<string> EnableMsgs = new List<string>();

            public float Target;
            public float Current;

            public void Report(float amount)
            {
                if(!Visible)
                    return;

                Current += amount;
                progress = progFunc(Current, Target);
                TutorialUI.Msgs[Name].Progress = progress;
                if (Current >= Target)
                    Clear();
            }

            private string progress;

            public TutorialMessage(Func<float, float, string> progFunc, float target, params string[] enableMsgs)
            {
                Target = target;
                progress = progFunc(Current, Target);
                this.progFunc = progFunc;
                EnableMsgs.AddRange(enableMsgs);
            }

            /// <summary>
            /// If this message is visible, clears this tutorial
            /// </summary>
            public void Clear()
            {
                if (!Visible) return;

                Cleared = true;
                Visible = false;
                TutorialUI.Msgs.Remove(Name);
                foreach (var msg in EnableMsgs)
                    AllMessages[msg].TriggerShow();
            }
            public void TriggerShow()
            {
                if (Cleared || Visible) return;

                Visible = true;
                TutorialUI.Msgs.Add(Name, new TutorialUI.MsgUI(Name) { Progress = progress });
            }

            public void Reset()
            {
                Current = 0;
                Visible = false;
                Cleared = false;
            }
        }

        /// <summary>
        /// Tutorial UI
        /// </summary>
        public static readonly TutorialUI TutorialUI = new TutorialUI();

        #region Messages
        public readonly static TutorialMessage MsgIntro = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 20, "MsgQuickMove");
        public readonly static TutorialMessage MsgQuickMove = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 20, "MsgJump");
        public readonly static TutorialMessage MsgJump = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 5);
        #endregion

        public readonly static Dictionary<string, TutorialMessage> AllMessages = new Dictionary<string, TutorialMessage>();

        /// <summary>
        /// Loads tutorials from file
        /// </summary>
        public static void Init()
        {
            foreach (var fieldInfo in typeof(Tutorials).GetFields())
            {
                if (fieldInfo.FieldType == typeof(TutorialMessage))
                {
                    var msg = fieldInfo.GetValue(null) as TutorialMessage;
                    msg.Name = fieldInfo.Name;
                    AllMessages.Add(msg.Name, msg);
                }
            }

            // show intro
            MsgIntro.TriggerShow();
        }

        public static void ResetTutorials()
        {
            foreach (var message in AllMessages)
            {
                message.Value.Reset();
            }
        }
    }
}

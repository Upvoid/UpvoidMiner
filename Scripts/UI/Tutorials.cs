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

            [UIObject]
            public string ProgressPercentage { get; set; }

            public MsgUI(string name)
            {
                Name = name;
            }
        }
    }

    public static class Tutorials
    {
        public enum Modes
        {
            Both,
            Adventure,
            God
        }

        public class TutorialMessage
        {
            private readonly Func<float, float, string> progFunc;
            public bool Visible = false;
            public bool Cleared = false;
            public bool Skipped = false;

            public string Name;

            public List<string> EnableMsgs = new List<string>();

            public float Target;
            private readonly Modes mode;
            public float Current;

            public void Report(float amount)
            {
                if (!Visible)
                    return;

                Current += amount;
                progress = progFunc(Current, Target);
                TutorialUI.Msgs[Name].Progress = progress;
                TutorialUI.Msgs[Name].ProgressPercentage = (Current / Target * 100).ToString("0") + "%";
                if (Current >= Target)
                    Clear();
            }

            private string progress;

            public TutorialMessage(Func<float, float, string> progFunc, float target, string followMsg, Modes mode = Modes.Both)
            {
                Target = target;
                this.mode = mode;
                progress = progFunc(Current, Target);
                this.progFunc = progFunc;
                if (!String.IsNullOrEmpty(followMsg))
                    EnableMsgs.Add(followMsg);
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

                if ((mode == Modes.Adventure && godMode)
                    || (mode == Modes.God && !godMode))
                {
                    Skipped = true;
                    foreach (var msg in EnableMsgs)
                        AllMessages[msg].TriggerShow();
                    return;
                }

                Visible = true;
                TutorialUI.Msgs.Add(Name, new TutorialUI.MsgUI(Name) { Progress = progress, ProgressPercentage = "0%" });
            }

            public void Reset()
            {
                Current = 0;
                Visible = false;
                Cleared = false;
                Skipped = false;
            }
        }

        /// <summary>
        /// Tutorial UI
        /// </summary>
        public static readonly TutorialUI TutorialUI = new TutorialUI();

        private static bool godMode = false;

        #region Tutorials
        public readonly static TutorialMessage MsgIntro = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 20, "MsgMovementSprint");

        public readonly static TutorialMessage MsgMovementSprint = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 20, "MsgMovementJump");
        public readonly static TutorialMessage MsgMovementJump = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 5, "MsgBasicDiggingDirt", Modes.Adventure);

        public readonly static TutorialMessage MsgBasicDiggingDirt = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 50, "MsgBasicDiggingStone", Modes.Adventure);
        public readonly static TutorialMessage MsgBasicDiggingStone = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 30, "MsgBasicDiggingGod", Modes.Adventure);
        public readonly static TutorialMessage MsgBasicDiggingGod = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 50, "MsgBasicBuildingDirt", Modes.God);

        public readonly static TutorialMessage MsgBasicBuildingDirt = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 10, "MsgBasicBuildingStone");
        public readonly static TutorialMessage MsgBasicBuildingStone = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 10, "MsgBasicCraftingDirtCube");

        public readonly static TutorialMessage MsgBasicCraftingDirtCube = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 2, "MsgBasicCraftingDirtCubePlace");
        public readonly static TutorialMessage MsgBasicCraftingDirtCubePlace = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 2, "MsgBasicCraftingStoneNonCube");
        public readonly static TutorialMessage MsgBasicCraftingStoneNonCube = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 2, null);

        #endregion

        public readonly static Dictionary<string, TutorialMessage> AllMessages = new Dictionary<string, TutorialMessage>();

        /// <summary>
        /// Loads tutorials from file
        /// </summary>
        /// <param name="isGodMode"></param>
        public static void Init(bool isGodMode)
        {
            godMode = isGodMode;
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

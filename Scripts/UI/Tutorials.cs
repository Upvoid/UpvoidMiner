using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EfficientUI;
using Newtonsoft.Json;

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

            [UICallback]
            public void FinishMe()
            {
                if (Tutorials.AllMessages.ContainsKey(Name))
                    Tutorials.AllMessages[Name].Report(Tutorials.AllMessages[Name].Target + 1);
            }

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
            public readonly Modes Mode;
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
                this.Mode = mode;
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

                ShowNextNonClearedOnDemand();
            }
            public void TriggerShow()
            {
                if (Cleared || Visible) return;

                if ((Mode == Modes.Adventure && godMode)
                    || (Mode == Modes.God && !godMode))
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
        public readonly static TutorialMessage MsgMovementJump = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 3, "MsgBasicDiggingDirt", Modes.Adventure);

        public readonly static TutorialMessage MsgBasicDiggingDirt = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 50, "MsgBasicDiggingStone", Modes.Adventure);
        public readonly static TutorialMessage MsgBasicDiggingStone = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 30, "MsgBasicDiggingGod", Modes.Adventure);
        public readonly static TutorialMessage MsgBasicDiggingGod = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 50, "MsgBasicBuildingDirt", Modes.God);

        public readonly static TutorialMessage MsgBasicBuildingDirt = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 10, "MsgBasicBuildingStone");
        public readonly static TutorialMessage MsgBasicBuildingStone = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 10, "MsgBasicCraftingDirtCube");

        public readonly static TutorialMessage MsgBasicCraftingDirtCube = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 2, "MsgBasicCraftingDirtCubePlace");
        public readonly static TutorialMessage MsgBasicCraftingDirtCubePlace = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 2, "MsgBasicCraftingStoneNonCube");
        public readonly static TutorialMessage MsgBasicCraftingStoneNonCube = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 2, "MsgBasicCraftingCollect");
        public readonly static TutorialMessage MsgBasicCraftingCollect = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 2, "MsgBasicChoppingTree");

        public readonly static TutorialMessage MsgBasicChoppingTree = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 3, "MsgBasicChoppingCollect");
        public readonly static TutorialMessage MsgBasicChoppingCollect = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 5, "MsgAdvancedDiggingSmall");

        public readonly static TutorialMessage MsgAdvancedDiggingSmall = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 5, "MsgAdvancedDiggingNonSphere");
        public readonly static TutorialMessage MsgAdvancedDiggingNonSphere = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 50, "MsgAdvancedDiggingBottom");
        public readonly static TutorialMessage MsgAdvancedDiggingBottom = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 30, "MsgAdvancedDiggingView");
        public readonly static TutorialMessage MsgAdvancedDiggingView = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 50, "MsgAdvancedDiggingAngle");
        public readonly static TutorialMessage MsgAdvancedDiggingAngle = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 50, "MsgAdvancedBuildingTerrainAligned");

        public readonly static TutorialMessage MsgAdvancedBuildingTerrainAligned = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 10, "MsgAdvancedBuildingReplaceMaterial");
        public readonly static TutorialMessage MsgAdvancedBuildingReplaceMaterial = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 10, "MsgAdvancedBuildingReplaceAll");
        public readonly static TutorialMessage MsgAdvancedBuildingReplaceAll = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 10, "MsgAdvancedBuildingPipette");
        public readonly static TutorialMessage MsgAdvancedBuildingPipette = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 1, "MsgAdvancedBuildingPlaceDrone", Modes.God);
        public readonly static TutorialMessage MsgAdvancedBuildingPlaceDrone = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 3, "MsgAdvancedBuildingPlaceConstrained");
        public readonly static TutorialMessage MsgAdvancedBuildingPlaceConstrained = new TutorialMessage((c, t) => c.ToString("0.0") + "/" + t, 10, "MsgAdvancedBuildingCollectDrone");
        public readonly static TutorialMessage MsgAdvancedBuildingCollectDrone = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 3, "MsgAdvancedCraftingThrowQ");

        public readonly static TutorialMessage MsgAdvancedCraftingThrowQ = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 3, "MsgAdvancedCraftingThrowUse");
        public readonly static TutorialMessage MsgAdvancedCraftingThrowUse = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 3, "MsgAdvancedCraftingStaticUse");
        public readonly static TutorialMessage MsgAdvancedCraftingStaticUse = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 3, "MsgAdvancedCraftingCollectAllDynamic");
        public readonly static TutorialMessage MsgAdvancedCraftingCollectAllDynamic = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 1, "MsgAdvancedCraftingCollectAllStatic");
        public readonly static TutorialMessage MsgAdvancedCraftingCollectAllStatic = new TutorialMessage((c, t) => c.ToString("0") + "/" + t, 1, null);

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

            bool restartTut = true;
            try
            {
                if (File.Exists(UpvoidMiner.SavePathTutorial))
                {
                    var save = JsonConvert.DeserializeObject<List<TutorialSave>>(File.ReadAllText(UpvoidMiner.SavePathTutorial));

                    if (save != null)
                    {
                        foreach (var s in save)
                        {
                            if (!AllMessages.ContainsKey(s.Name))
                                continue;

                            var msg = AllMessages[s.Name];
                            msg.Cleared = s.Cleared;
                            msg.Current = s.Current;
                            if (s.Visible)
                            {
                                msg.TriggerShow();
                                msg.Report(0f);
                            }
                        }
                        restartTut = false;
                    }
                }
            }
            catch (Exception)
            {
                restartTut = true;
            }

            // show intro
            if (restartTut)
                MsgIntro.TriggerShow();
            ShowNextNonClearedOnDemand();
        }

        private static void ShowNextNonClearedOnDemand()
        {
            if (AllMessages.Values.Any(m => m.Visible && !m.Cleared))
                return;

            var msg = AllMessages.Values.FirstOrDefault(m => !m.Visible && !m.Cleared && (m.Mode == Modes.Both || (m.Mode == Modes.God) == godMode));
            if (msg != null)
                msg.TriggerShow();
        }

        public static void SaveState()
        {
            var save = AllMessages.Select(message => new TutorialSave(message.Value)).ToList();

            Directory.CreateDirectory(new FileInfo(UpvoidMiner.SavePathTutorial).Directory.FullName);
            File.WriteAllText(UpvoidMiner.SavePathTutorial, JsonConvert.SerializeObject(save, Formatting.Indented));
        }

        public static void ResetTutorials()
        {
            foreach (var message in AllMessages)
                message.Value.Reset();
            TutorialUI.Msgs.Clear();

            // show intro
            MsgIntro.TriggerShow();
            ShowNextNonClearedOnDemand();
        }

        public class TutorialSave
        {
            public string Name;
            public bool Cleared;
            public bool Visible;
            public float Current;

            public TutorialSave() { }
            public TutorialSave(TutorialMessage msg)
            {
                Name = msg.Name;
                Cleared = msg.Cleared;
                Visible = msg.Visible;
                Current = msg.Current;
            }
        }
    }
}

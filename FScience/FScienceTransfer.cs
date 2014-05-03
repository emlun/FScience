using System.Collections.Generic;
using UnityEngine;

namespace FScience {
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FScienceTransfer : MonoBehaviour {

        List<ModuleScienceContainer> containers;
        List<ModuleScienceExperiment> experiments;

        public void OnLevelWasLoaded() {
            LateSetup();
        }
        private void LateSetup() {
            SceneCheck();
        }

        private Vector2 containersScrollViewer = Vector2.zero;
        private Vector2 experimentsScrollViewer = Vector2.zero;

        private Vector3 partTargetScrollViewer = Vector2.zero;
        private PartModule _selectedPart;
        public PartModule SelectedPart {
            get {
                if(_selectedPart != null && !FlightGlobals.ActiveVessel.Parts.Contains(_selectedPart.part)) {
                    _selectedPart = null;
                }
                return _selectedPart;
            }
            set {
                if(_selectedPart != null) {
                    ClearHighlight(_selectedPart);
                }
                _selectedPart = value;
                SetPartHighlight(_selectedPart.part, Color.yellow);
            }
        }

        private ModuleScienceContainer _selectedPartTarget;
        private ModuleScienceContainer SelectedPartTarget {
            get {
                if(_selectedPartTarget != null && !FlightGlobals.ActiveVessel.Parts.Contains(_selectedPartTarget.part)) {
                    _selectedPartTarget = null;
                }
                return _selectedPartTarget;
            }
            set {
                if(_selectedPartTarget != null) {
                    ClearHighlight(_selectedPartTarget);
                }
                _selectedPartTarget = value;
                SetPartHighlight(_selectedPartTarget.part, Color.red);
            }
        }

        Rect windowRect = new Rect((Screen.width/2) - (Resources.DefaultWindowRect.width/2),
                                    (Screen.height/2) - (Resources.DefaultWindowRect.height/2),
                                    Resources.DefaultWindowRect.width,
                                    Resources.DefaultWindowRect.height);
        Rect warningWindowRect = new Rect(20, 20, 320, 120);

        bool guiMaximized;
        Rect ui_icon_pos = new Rect((Screen.width / 2) - 270, Screen.height - 22, 40, 20);
        bool hideMainButton = false;

        public void OnGUI() {
            if(!SceneCheck()) {
                return;
            }
            Resources.SetupGUI();
            GUI.skin = HighLogic.Skin;
            if(!hideMainButton && GUI.Button(ui_icon_pos, "ScT", GUI.skin.button)) {
                ToggleGUI();
            }
            if(guiMaximized) {
                windowRect = GUI.Window(0, windowRect, MainWindowShow, "Science Transfer");
            } else {
                ClearHighlight(SelectedPart);
                ClearHighlight(SelectedPartTarget);
            }
        }

        public void ToggleGUI() {
            guiMaximized = !guiMaximized;
        }

        public void MainWindowShow(int id) {
            FindScienceContainers();
            TransferGUI();
        }

        public void TransferGUI() {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();

            // FROM (experiments) scroll view
            GUILayout.Label("From Experiment:");
            experimentsScrollViewer = GUILayout.BeginScrollView(experimentsScrollViewer, GUILayout.Width(300));
            GUILayout.BeginVertical();
            foreach(ModuleScienceExperiment sc in experiments) {
                var style = sc == SelectedPart ? Resources.ButtonToggledStyle : Resources.ButtonStyle;
                if(GUILayout.Button(formatExperiment(sc), style, GUILayout.Width(265))) {
                    SelectedPart = sc;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            // FROM (containers) scroll view
            GUILayout.Label("From Container:");
            containersScrollViewer = GUILayout.BeginScrollView(containersScrollViewer, GUILayout.Width(300));
            GUILayout.BeginVertical();
            foreach(ModuleScienceContainer sc in containers) {
                var style = sc == SelectedPart ? Resources.ButtonToggledStyle : Resources.ButtonStyle;
                if(GUILayout.Button(formatContainer(sc), style, GUILayout.Width(265))) {
                    SelectedPart = sc;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            // TO scroll view
            GUILayout.BeginVertical();
            GUILayout.Label("To Container:");
            partTargetScrollViewer = GUILayout.BeginScrollView(partTargetScrollViewer, GUILayout.Width(300));
            GUILayout.BeginVertical();
            foreach(ModuleScienceContainer sc in containers) {
                var style = sc == SelectedPartTarget ? Resources.ButtonToggledRedStyle : Resources.ButtonStyle;
                if(GUILayout.Button(formatContainer(sc), style, GUILayout.Width(265))) {
                    SelectedPartTarget = sc;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            // FROM Selection text
            if(SelectedPart is ModuleScienceContainer) {
                GUILayout.Label("From: " + formatContainer((ModuleScienceContainer)SelectedPart), GUILayout.Width(300));
            } else if(SelectedPart is ModuleScienceExperiment) {
                GUILayout.Label("From: " + formatExperiment((ModuleScienceExperiment)SelectedPart), GUILayout.Width(300));
            } else {
                GUILayout.Label("No Part Selected");
            }

            // TO Selection text
            if(SelectedPartTarget != null) {
                GUILayout.Label("To: " + formatContainer(SelectedPartTarget), GUILayout.Width(300));
            } else {
                GUILayout.Label("No Part Selected");
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if(SelectedPartTarget != null && GUILayout.Button("Transfer from all experiments")) {
                TransferAllExperiments();
            }
            if(SelectedPartTarget != null && GUILayout.Button("Transfer from all containers")) {
                TransferAllContainers();
            }
            GUILayout.EndHorizontal();

            if(SelectedPart != null && SelectedPartTarget != null && SelectedPart.part != SelectedPartTarget.part && GUILayout.Button("Transfer")) {
                TransferScience((IScienceDataContainer)SelectedPart, SelectedPartTarget);
            }

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        public void TransferAllContainers() {
            foreach(ModuleScienceContainer container in containers) {
                TransferScience(container, SelectedPartTarget);
            }
        }

        public void TransferAllExperiments() {
            foreach(ModuleScienceExperiment experiment in experiments) {
                TransferScience(experiment, SelectedPartTarget);
            }
        }

        private string formatContainer(ModuleScienceContainer container) {
            return string.Format("{0} - {1} data", container.part.partInfo.title, container.GetScienceCount());
        }

        private string formatExperiment(ModuleScienceExperiment experiment) {
            return string.Format("{0} ({1})", experiment.part.partInfo.title, experiment.GetScienceCount() == 0 ? "empty" : experiment.GetScienceCount() + " data");
        }

        private void FindScienceContainers() {
            Vessel v = FlightGlobals.ActiveVessel;

            containers = new List<ModuleScienceContainer>();
            experiments = new List<ModuleScienceExperiment>();

            foreach(Part pd in v.parts) {
                foreach(PartModule pm in pd.Modules) {
                    if(pm is ModuleScienceContainer) {
                        containers.Add((ModuleScienceContainer)pm);
                    }
                    if(pm is ModuleScienceExperiment) {
                        experiments.Add((ModuleScienceExperiment)pm);
                    }
                }
            }
        }

        private void TransferScience(IScienceDataContainer source, ModuleScienceContainer target) {
            if(source == null || target == null) {
                return;
            }

            ScienceData[] sd = source.GetData();
            if(sd == null || sd.Length == 0) {
                Debug.Log("No data ");
                return;
            }

            if(source is ModuleScienceContainer) {
                foreach(ScienceData data in sd) {
                    if(TargetAcceptsData(target, data)) {
                        if(target.AddData(data)) {
                            ((ModuleScienceContainer)source).RemoveData(data);
                        } else {
                            Debug.Log("Transfer fail");
                        }
                    }
                }
            } else if(source is ModuleScienceExperiment) {
                if(TargetAcceptsData(target, sd[0])) {
                    if(target.AddData(sd[0])) {
                        ((ModuleScienceExperiment)source).DumpData(sd[0]);
                    } else {
                        Debug.Log("Transfer fail");
                    }
                }
            }
        }

        private bool TargetAcceptsData(ModuleScienceContainer target, ScienceData data) {
            if(target.allowRepeatedSubjects) {
                Debug.Log(string.Format("Target {0} allows repeated subjects", target.part.partInfo.title));
                return true;
            }
            if(target.HasData(data)) {
                Debug.Log(string.Format("Target {0} already has data {1} and does not allow repeated subjects", target.part.partInfo.title, data.title));
                return false;
            }
            return true;
        }

        private void ClearHighlight(PartModule partModule) {
            if(partModule != null) {
                partModule.part.SetHighlightDefault();
                partModule.part.SetHighlight(false);
            }
        }

        private void SetPartHighlight(Part part, Color color) {
            if(part != null) {
                part.SetHighlightColor(color);
                part.SetHighlight(true);
            }
        }

        public bool SceneCheck() {
            if(HighLogic.LoadedScene != GameScenes.FLIGHT) {
                return false;
            } else {
                return true;
            }
        }
    }
}


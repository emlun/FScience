using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Net;
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
                if(_selectedPart != null && !FlightGlobals.ActiveVessel.Parts.Contains(((PartModule)_selectedPart).part)) {
                    _selectedPart = null;
                }
                return _selectedPart;
            }
            set {
                if(_selectedPart != null) {
                    ClearHighlight(((PartModule)_selectedPart).part);
                }
                _selectedPart = value;
                SetPartHighlight(((PartModule)_selectedPart).part, Color.yellow);
            }
        }

        private PartModule _selectedPartTarget;
        private PartModule SelectedPartTarget {
            get {
                if(_selectedPartTarget != null && !FlightGlobals.ActiveVessel.Parts.Contains(((PartModule)_selectedPartTarget).part)) {
                    _selectedPartTarget = null;
                }
                return _selectedPartTarget;
            }
            set {
                if(_selectedPartTarget != null) {
                    ClearHighlight(((PartModule)_selectedPartTarget).part);
                }
                _selectedPartTarget = value;
                SetPartHighlight(((PartModule)_selectedPartTarget).part, Color.red);
            }
        }

        Rect windowRect  = new Rect(20, 20, 640, 360);
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
            if(!hideMainButton) {
                if(GUI.Button(ui_icon_pos, "ScT", GUI.skin.button)) {
                    guiMaximized = !guiMaximized;
                    windowRect.x = (Screen.width/2) - (windowRect.width/2);
                    windowRect.y = (Screen.height/2) - (windowRect.height/2);
                }
            }
            if(guiMaximized) {
                windowRect = GUI.Window(0, windowRect, MainWindowShow, "Science Transfer");
            }
        }

        public void MainWindowShow(int id) {
            FindScienceContainers();
            TransferGUI();
        }

        public void TransferGUI() {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();

            GUILayout.Label("From : ");

            // FROM (experiments) scroll view
            GUILayout.Label("Experiments : ");
            experimentsScrollViewer = GUILayout.BeginScrollView(experimentsScrollViewer, GUILayout.Width(300));
            GUILayout.BeginVertical();
            foreach(ModuleScienceExperiment sc in experiments) {
                var style = sc == SelectedPart ? Resources.ButtonToggledStyle : Resources.ButtonStyle;
                if(GUILayout.Button(string.Format("{0} {1} /{2}", sc.part.partInfo.title, sc.GetScienceCount(), "1"), style, GUILayout.Width(265))) {
                    SelectedPart = sc;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            // FROM (containers) scroll view
            GUILayout.Label("Containers : ");
            containersScrollViewer = GUILayout.BeginScrollView(containersScrollViewer, GUILayout.Width(300));
            GUILayout.BeginVertical();
            foreach(ModuleScienceContainer sc in containers) {
                var style = sc == SelectedPart ? Resources.ButtonToggledStyle : Resources.ButtonStyle;
                if(GUILayout.Button(string.Format("{0} {1} /{2}", sc.part.partInfo.title, sc.GetScienceCount(), sc.capacity), style, GUILayout.Width(265))) {
                    SelectedPart = sc;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            // TO scroll view
            GUILayout.BeginVertical();
            GUILayout.Label("TO : ");
            partTargetScrollViewer = GUILayout.BeginScrollView(partTargetScrollViewer, GUILayout.Width(300));
            GUILayout.BeginVertical();
            foreach(ModuleScienceContainer sc in containers) {
                var style = sc == SelectedPartTarget ? Resources.ButtonToggledRedStyle : Resources.ButtonStyle;
                if(GUILayout.Button(string.Format("{0} {1} /{2}", sc.part.partInfo.title, sc.GetScienceCount(), sc.capacity), style, GUILayout.Width(265))) {
                    SelectedPartTarget = sc;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            // FROM Selection text
            if(SelectedPart is ModuleScienceContainer) {
                GUILayout.Label(SelectedPart != null ? string.Format("FROM: {0} {1} /{2}", SelectedPart.part.partInfo.title, ((ModuleScienceContainer)SelectedPart).GetScienceCount(), ((ModuleScienceContainer)SelectedPart).capacity) : "No Part Selected", GUILayout.Width(300));
            } else if(SelectedPart is ModuleScienceExperiment) {
                GUILayout.Label(SelectedPart != null ? string.Format("FROM: {0} {1} /{2}", SelectedPart.part.partInfo.title, ((ModuleScienceExperiment)SelectedPart).GetScienceCount(), "1") : "No Part Selected", GUILayout.Width(300));
            } else {
                GUILayout.Label("No Part Selected");
            }

            // TO Selection text
            if(SelectedPartTarget is ModuleScienceContainer) {
                GUILayout.Label(SelectedPartTarget != null ? string.Format("TO: {0} {1} /{2}", SelectedPartTarget.part.partInfo.title, ((ModuleScienceContainer)SelectedPartTarget).GetScienceCount(), ((ModuleScienceContainer)SelectedPartTarget).capacity) : "No Part Selected", GUILayout.Width(300));
            } else {
                GUILayout.Label("No Part Selected");
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if(SelectedPart != null && SelectedPartTarget != null && SelectedPart.part != SelectedPartTarget.part && GUILayout.Button("Transfer")) {
                TransferScience(SelectedPart, SelectedPartTarget);
            }

            GUILayout.EndVertical();

            GUI.DragWindow();
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

        private void TransferScience(PartModule source, PartModule target) {
            ScienceData[] sd;
            int i;
            if(source is ModuleScienceContainer) {
                sd = ((ModuleScienceContainer)source).GetData();
                if(sd == null || sd.Length == 0) {
                    Debug.Log("No data ");
                    return;
                }
                for(i = 0; i < sd.Length; i++) {
                    if(target is ModuleScienceContainer) {
                        if(((ModuleScienceContainer)target).AddData(sd[i])) {
                            ((ModuleScienceContainer)source).RemoveData(sd[i]);
                        } else {
                            Debug.Log("Transfer fail");
                        }
                    }
                }
            } else if(source is ModuleScienceExperiment) {
                sd = ((ModuleScienceExperiment)source).GetData();
                if(sd == null || sd.Length == 0) {
                    Debug.Log("No data ");
                    return;
                }
                if(target is ModuleScienceContainer) {
                    if(((ModuleScienceContainer)target).AddData(sd[0])) {
                        ((ModuleScienceExperiment)source).DumpData(sd[0]);
                    } else {
                        Debug.Log("Transfer fail");
                    }
                }
            }
        }

        private void ClearHighlight(Part part) {
            if(part != null) {
                part.SetHighlightDefault();
                part.SetHighlight(false);
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


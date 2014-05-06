using FScience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbar;
using UnityEngine;

namespace FScienceToolbar {
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class FScienceToolbar : MonoBehaviour {

        public const string NAMESPACE = "FScience";
        public const string BUTTON_ID_TOGGLE_MAIN_WINDOW = "toggleMainWindow";
        public const string BUTTON_ID_TRANSFER_EXPERIMENTS = "transferExperiments";

        private IButton toggleButton;
        private IButton transferExperimentsButton;

        public void Start() {
            if(!FScienceTransfer.SceneCheck()) {
                return;
            }

            if(AddToggleMainWindowButton()) {
                FScienceTransfer.hideMainButton = true;
            }

            AddTransferExperimentsButton();
        }

        private bool AddToggleMainWindowButton() {
            toggleButton = ToolbarManager.Instance.add(NAMESPACE, BUTTON_ID_TOGGLE_MAIN_WINDOW);
            if(toggleButton == null) {
                return false;
            }

            toggleButton.Text = Resources.toggleMainWindowButtonText;
            toggleButton.ToolTip = Resources.toggleMainWindowButtonTooltip;
            toggleButton.TexturePath = Resources.toggleMainWindowButtonTexturePath;

            toggleButton.OnClick += (e) => {
                FScienceTransfer.ToggleGUI();
            };

            return true;
        }

        private bool AddTransferExperimentsButton() {
            transferExperimentsButton = ToolbarManager.Instance.add(NAMESPACE, BUTTON_ID_TRANSFER_EXPERIMENTS);
            transferExperimentsButton.Text = Resources.transferExperimentsButtonText;
            transferExperimentsButton.ToolTip = Resources.transferExperimentsButtonTooltip;
            transferExperimentsButton.TexturePath = Resources.transferExperimentsButtonTexturePath;

            transferExperimentsButton.OnClick += (e) => {
                FScienceTransfer.TransferAllExperiments();
            };

            return true;
        }
    }
}

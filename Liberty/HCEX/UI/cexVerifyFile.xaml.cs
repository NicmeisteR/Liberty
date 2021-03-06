﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Liberty.HCEX.UI
{
    /// <summary>
    /// Interaction logic for cexVerifyFile.xaml
    /// </summary>
    public partial class cexVerifyFile : UserControl, StepUI.IStep
	{
        private Util.SaveManager<HCEX.CampaignSave> _saveManager;

        public cexVerifyFile(Util.SaveManager<HCEX.CampaignSave> saveManager)
        {
            InitializeComponent();
            _saveManager = saveManager;
        }

        public string Gamertag { get; set; }

        public void Load()
        {
            HCEX.CampaignSave saveData = _saveManager.SaveData;
            lblGamertag.Content = Gamertag;
            lblMapName.Text = Util.EditorSupport.GetMissionName(saveData) + " - " + saveData.Map;
            lblDifficulty.Content = saveData.CFGData.Difficulty;

            // Try to load the mission image
            try
            {
                string mapName = saveData.Map;
                mapName = mapName.Substring(mapName.LastIndexOf('\\') + 1);
                var source = new Uri(@"/Liberty;component/Images/hcexMaps/" + mapName + ".png", UriKind.Relative);
                imgMapImage.Source = new BitmapImage(source);

                int diff = saveData.CFGData.DifficultyID + 1;
                source = new Uri(@"/Liberty;component/Images/Difficulty/Blam_Default/" + diff.ToString() + ".png", UriKind.Relative);
                imgDifficulty.Source = new BitmapImage(source);
            }
            catch { }
        }

        public bool Save()
        {
            return true;
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Hidden;
        }
    }
}

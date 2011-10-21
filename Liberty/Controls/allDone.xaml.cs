﻿using System;
using System.Collections.Generic;
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

namespace Liberty.Controls
{
	/// <summary>
	/// Interaction logic for step5.xaml
	/// </summary>
	public partial class allDone : UserControl, StepUI.IStep
	{		
		public allDone()
		{
			this.InitializeComponent();
		}

        public void Load(Util.SaveEditor saveEditor)
        {
            //if (classInfo.storage.fileInfoStorage.saveIsLocal)
            {
                subBaseHeader_2_1.Content = "Liberty and then just transfer the save back to your hard drive, USB, or memory unit and enjoy some";
                subBaseHeader_2_2.Content = "more Halo: Reach.";
            }
            /*else
            {
                subBaseHeader_2_1.Content = "Liberty and enjoy some more Halo: Reach.";
                subBaseHeader_2_2.Content = "";
            }*/
        }

        public bool Save(Util.SaveEditor saveEditor)
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
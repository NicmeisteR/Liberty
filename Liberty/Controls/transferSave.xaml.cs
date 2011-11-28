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

namespace Liberty
{
	/// <summary>
	/// Interaction logic for transferSave.xaml
	/// </summary>
	public partial class transferSave : UserControl, StepUI.IWorkStep
	{
		public transferSave(Util.ISaveTransferrer saveTransferrer)
		{
			this.InitializeComponent();

            _saveTransferrer = saveTransferrer;
            _saveTransferrer.NextFile += new EventHandler<Util.TransferFileEventArgs>(saveTransferrer_NextFile);
            _saveTransferrer.ProgressChanged += new EventHandler<Util.TransferProgressEventArgs>(saveTransferrer_ProgressChanged);
            _saveTransferrer.Done += new EventHandler(saveTransferrer_Done);
		}

        public string Gamertag
        {
            get { return lblGamertag.Text; }
            set { lblGamertag.Text = value; }
        }

        public string GameName
        {
            get { return lblGameName.Text; }
            set { lblGameName.Text = value; }
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
            _saveTransferrer.Start();
        }

        public void Hide()
        {
            Visibility = Visibility.Hidden;
        }

        public void Load()
        {
        }

        public bool Save()
        {
            return true;
        }

        private void UpdateProgress(int transferred, int total)
        {
            // Progress bar
            progressBar.Value = transferred;
            progressBar.Maximum = total;

            // Progress label
            double mbTransferred = Math.Round(transferred / 1000000f, 2);
            double mbSize = Math.Round(total / 1000000f, 2);
            lblProgress.Text = mbTransferred + " / " + mbSize + " MB transferred";

            // ProgressChanged event
            if (ProgressChanged != null)
                ProgressChanged(this, (double)transferred / total);
        }

        private void saveTransferrer_NextFile(object sender, Util.TransferFileEventArgs e)
        {
            lblDeviceName.Text = e.DeviceName;
            lblFileName.Text = e.FileName;
            UpdateProgress(0, e.SizeInBytes);
        }

        private void saveTransferrer_ProgressChanged(object sender, Util.TransferProgressEventArgs e)
        {
            UpdateProgress(e.BytesTransferred, e.BytesTotal);
        }

        private void saveTransferrer_Done(object sender, EventArgs e)
        {
            if (Complete != null)
                Complete(this);
        }

        public event StepUI.WorkStepProgressEvent ProgressChanged;
        public event StepUI.WorkStepCompletedEvent Complete;

        private Util.ISaveTransferrer _saveTransferrer;
    }
}
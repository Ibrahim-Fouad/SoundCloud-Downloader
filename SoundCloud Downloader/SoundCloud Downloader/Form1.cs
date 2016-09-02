using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
namespace SoundCloud_Downloader
{
    public partial class Form1 : Form
    {
        public const string AppID = "3d9f15301a25e269942f939e2de89b98";
        public string DownloadLink { get; set; }
        WebClient wc;
        public Form1()
        {
            InitializeComponent();
            wc = new WebClient();
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += wc_DownloadFileCompleted;
        }

        void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Track downloaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtTrackLink.Enabled = true;
            button1.Enabled = true;
            lblStatus.Text = "-- Downloading Completed!";
            progressBar1.Value = 0;
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            lblStatus.Text = string.Format("-- {0}( {2}% ) / {1} ", ConvertBytes(e.BytesReceived), ConvertBytes(e.TotalBytesToReceive), e.ProgressPercentage);
        }
        void GetDirectLink(string Link)
        {
            string ApiLink = string.Format("http://api.soundcloud.com/resolve?url={0}&client_id={1}",Link,AppID);
            dynamic TrackInfo = JObject.Parse(wc.DownloadString(ApiLink));
            ApiLink = string.Format("{0}?client_id={1}",TrackInfo.stream_url.ToString(),AppID);
            DownloadLink = ApiLink;
            this.Invoke(() =>
            {
                lblTrackName.Text = string.Format("Track Name: {0}", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(TrackInfo.title.ToString())));
                lblStatus.Text = "-- Success, Click Button to start download...";
                button1.Enabled = true;
                btnGO.Enabled = true;
            });
        }
        string ConvertBytes(long bytes)
        {
              string[] SizeSuffixes = 
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
              if (bytes < 0) { return "-" + ConvertBytes(-bytes); }
              if (bytes == 0) { return "0.0 bytes"; }

              int mag = (int)Math.Log(bytes, 1024);
              decimal adjustedSize = (decimal)bytes / (1L << (mag * 10));
              return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Audio Files |*.mp3";
            sfd.Title = "Save Audio File";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                wc.DownloadFileAsync(new Uri(DownloadLink), sfd.FileName);
                txtTrackLink.Enabled = false;
                button1.Enabled = false;
                btnGO.Enabled = false;
            }
        }

        private void btnGO_Click(object sender, EventArgs e)
        {
            btnGO.Enabled = false;
            lblStatus.Text = "-- Getting track info...";
            System.Threading.Thread thr = new System.Threading.Thread(() => { GetDirectLink(txtTrackLink.Text.Trim()); });
            thr.IsBackground = true;
            thr.Start();
        }
    }
}

//For Safe Thread
public static partial class ControlExtensions
{
    public static void Invoke(this Control control,Action action)
    {
        if (control.InvokeRequired)
        {
            control.Invoke(new MethodInvoker(action), null);
        }
        else
        {
            control.Invoke(action);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;

using System.Text;
using System.Windows.Forms;

namespace PingUrl
{
    public partial class Form1 : Form
    {
        
        static int MaxRequests = 5;
        static bool IsChecking = false;
        private List<Url> Urls;
        private List<Url> UncheckedUrls;
        private int CheckingCount = 0;
        private int Next = 0;
        private int[] RandomOrder;
        private string UrlsListPath = @"UrlsList.txt";
        private string BadUrlsListPath = @"BadUrlsList.txt";
        private string StartText = @"Start";
        private string StopText = @"Stop";

        public Form1()
        {
            InitializeComponent();
            trackBar1.Value = MaxRequests;
            label1.Text = MaxRequests.ToString();
            UncheckedUrls = new List<Url>();
        }

        private static int[] Shuffle(int n)
        {
            var random = new Random();
            var result = new int[n];
            for (var i = 0; i < n; i++)
            {
                var j = random.Next(0, i + 1);
                if (i != j)
                {
                    result[i] = result[j];
                }
                result[j] = i;
            }
            return result;
        }

        public void RunToStop()
        {
            IsChecking = false;
        }

        public void ReadUrls()
        {
            string[] lines = File.ReadAllLines(UrlsListPath);
            Urls = new List<Url>();
            UncheckedUrls = new List<Url>();

            Console.WriteLine(lines.Length.ToString());

            foreach (string line in lines)
            {
                Urls.Add(new Url(line));
                UncheckedUrls.Add(new Url(line));
            }
        }

        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            if (this.button1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.button1.Text = text;
            }
        }

        private void Done(Url url)
        {
            CheckingCount--;
            Console.WriteLine("Done {0}", url.Path);
            if (url.IsNotOK())
            {
                using (StreamWriter sw = File.AppendText(BadUrlsListPath))
                {
                    sw.WriteLine(url.Path);
                }
            }
            if (UncheckedUrls.Count <= 0)
            {
                SetText(StartText);
            }
            RunChecks();
        }

        private void RunChecks()
        {
            while (CheckingCount < MaxRequests)
            {
                if (Next >= Urls.Count) break;
                Next++;
                CheckingCount++;
                Url url = Urls[RandomOrder[Next - 1]];
                url.Check(new DoneCallback(Done));
                UncheckedUrls = UncheckedUrls.Where(x => x.Path != url.Path).ToList();
                File.WriteAllLines(UrlsListPath, UncheckedUrls.Select(x => x.Path).ToArray(), Encoding.UTF8);
                
            }
        }

        public void CheckUrls()
        {
            RunChecks();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (IsChecking)
            { }
            else
            {
                if (UncheckedUrls.Count <= 0) Next = 0;
                button1.Text = StopText;
                ReadUrls();
                RandomOrder = Shuffle(Urls.Count);
                CheckUrls();
            }

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            MaxRequests = trackBar1.Value;
            label1.Text = MaxRequests.ToString();
        }
        
    }
}

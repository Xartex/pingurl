using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace PingUrl
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    enum UrlStatus { NotChecked, OK, NotOK };

    public delegate void DoneCallback(Url url);

    public class Url
    {
        private UrlStatus Status;
        private HttpWebRequest Request;
        private HttpWebResponse Response;
        private DoneCallback Done;
        public string Path;

        public Url()
        {
            Status = UrlStatus.NotChecked;
            Path = "";
        }

        public Url(string path)
        {
            Status = UrlStatus.NotChecked;
            Path = path;
        }

        public void Check(DoneCallback done)
        {
            Done = done;
            Request = (HttpWebRequest)WebRequest.Create(Path);
            Request.Timeout = 60 * 1000;
            Request.UserAgent = "Desktop Generation Bot";
            Request.BeginGetResponse(new AsyncCallback(FinishWebRequest), null);
            Console.WriteLine("Starting {0}", Path);
            
        }

        private void FinishWebRequest(IAsyncResult result)
        {
            try
            {
                Response = (HttpWebResponse)Request.EndGetResponse(result);
                if (((HttpWebResponse)Response).StatusCode == HttpStatusCode.OK) this.Status = UrlStatus.OK;
                else this.Status = UrlStatus.NotOK;
                Response.Close();
            }
            catch (WebException e)
            {
                this.Status = UrlStatus.NotOK;
            }
            Done(this);
        }

        public bool IsOK()
        {
            return Status == UrlStatus.OK;
        }

        public bool IsNotOK()
        {
            return Status == UrlStatus.NotOK;
        }

        public bool IsNotChecked()
        {
            return Status == UrlStatus.NotChecked;
        }
    }
}

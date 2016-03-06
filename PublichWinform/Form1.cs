using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BaiduApi;
using System.Threading;
using System.Diagnostics;

namespace PublichWinform
{
    public partial class Form1 : Form
    {
        int iii = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Baidu1 baidu1 = new Baidu1("670232921@qq.com", "ccc1459260");

            //List<BaiDuFile> list = baidu1.GetFileDir();

            //string path = "/data/32.mkv";
            //string localPath = System.Environment.CurrentDirectory + "/data/data_" + DateTime.Now.ToString("MMddHHmmss") + ".mkv";
            //FileOper fo = baidu1.DownFile(path, localPath, callback);

            //path = "/data/00.jpg";
            //string fileToUpload = System.Environment.CurrentDirectory + "/data/00.jpg";
            //fo = baidu1.UpLoadFile(path, fileToUpload);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(taskfun));
            t.Start();
        }
        private void taskfun()
        {
            Baidu1 baidu1 = new Baidu1("670232921@qq.com", "ccc1459260", 10);

            List<BaiDuFile> list = baidu1.GetFileDir();

            string path = "/data/32.mkv";
            string localPath = System.Environment.CurrentDirectory + "/data/data_" + DateTime.Now.ToString("MMddHHmmss") + ".mkv";
            iii = Environment.TickCount;
            FileOper fo = baidu1.DownFile(path, localPath, callback);

            path = "/data/00.jpg";
            string fileToUpload = System.Environment.CurrentDirectory + "/data/00.jpg";
            fo = baidu1.UpLoadFile(path, fileToUpload);
        }

        /// <param name="callback"> size now, szie totle, time now s, speed</param>
        private void callback(double a, double b, double c, double d)
        {

            double[] args = new double[]{ c, d, d / 1024,
                 a, a / 1024, a /1024 /1024,
                 b, b / 1024, b / 1024 / 1024};
                List<string> ss = new List<string>();
            foreach(var v in args)
            {
                ss.Add(Convert.ToString(v));
            }
            //string s = String.Format("time: {0} s\nspeed: {1} byte/s; {2} kb/s\n" +
            //    "size now: {3} byte\t\t{4} kb\t\t{5} mb\n" +
            //    "size totle: {6} byte\t\t{7} kb\t\t{8} mb",ss.ToArray()
            //     );
            string s = "time: " + Convert.ToString((Environment.TickCount - iii)/1000) + " s\nspeed: " + ss[1] + " byte/s; " + ss[2] + " kb/s\n" +
                "size now: " + ss[3] + " byte\t\t" + ss[4] + " kb\t\t" + ss[5] + " mb\n" +
                "size totle: " + ss[6] + " byte\t\t" + ss[7] + " kb\t\t" + ss[8] + " mb";
            Debug.WriteLine(s);
            this.Invoke(new global::System.Action(() => { label1.Text = s; }));
            //label1.Text = s;
        }
    }
}

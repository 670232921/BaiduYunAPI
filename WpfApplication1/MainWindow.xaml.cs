using BaiduApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Threading;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window//, INotifyPropertyChanged
    {
        private Baidu1 baidu1;
        //private List<Entry> _files = new List<Entry>();
        private Stack<Entry> _his = new Stack<Entry>();
        //private ProgressManager _proManager = new ProgressManager();
        public MainWindow()
        {
            InitializeComponent();
            ProManager = new ProgressManager();
            DataContext = ProManager;
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
        }

        //public List<Entry> Files
        //{
        //    get { return _files; }
        //    set
        //    {
        //        if (_files != value)
        //        {
        //            _files = value;
        //            if (PropertyChanged != null)
        //            {
        //                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Files"));
        //            }
        //        }
        //    }
        //}

        public ProgressManager ProManager
        {
            get; set;
            //get
            //{
            //    return _proManager;
            //}
            //set
            //{
            //    if (_proManager != value)
            //    {
            //        _proManager = value;

            //        if (PropertyChanged != null)
            //        {
            //            PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ProManager"));
            //        }
            //    }
            //}
        }

        //public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            baidu1 = new Baidu1(UserName.Text, Password.Text);
            _his.Push(null);
        ProManager.Files = baidu1.List(null);
        }

        private void FileListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var entry = FileListBox.SelectedItem as Entry;
            if (entry != null && entry.isdir == 1)
            {
                _his.Push(entry);
            ProManager.Files = baidu1.List(entry);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_his.Count > 1)
            {
                _his.Pop();
                var entry = _his.Peek();
            ProManager.Files = baidu1.List(entry);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var entry = FileListBox.SelectedItem as Entry;
            if (entry != null)
            {
                string dest = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), entry.server_filename);
                var callback = ProManager.AddDownload(entry.server_filename);
                //new Thread(() => baidu1.DownFileWithProcess(entry, dest, callback)).Start();
                new Thread(() => baidu1.DownPiceFileWithProgress(entry, dest, callback)).Start();
                //baidu1.DownFileWithProcess(entry.path, dest, null);
            }
        }
    }

}

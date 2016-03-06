using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows;
using System.Collections.ObjectModel;
using BaiduApi;

namespace WpfApplication1
{
    public class NotifierBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notifier(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
            }
            else
            {
                Debug.WriteLine("PropertyChanged null");
            }
        }
    }

    public class ProgressManager : NotifierBase
    {
        public ProgressManager()
        {
            //_Children.Add(new ProgressChild());
        }

        public Action<long, long> AddDownload(string name)
        {
            ProgressChild child = new ProgressChild(name, UpdateTotal);
            Children.Add(child);
            //AddWithNotifier(child);
            return child.Report;
        }

        //private void AddWithNotifier(ProgressChild child)
        //{
        //    List<ProgressChild> temp = new List<ProgressChild>(Children.ToArray());
        //    temp.Add(child);
        //    Children = temp;
        //}

        private void UpdateTotal()
        {
            TotalProgressCount = Children.Count;
            TotalProgressSpd = 0;
            TotalProgressCur = 0;
            TotalProgressSize = 0;
            foreach (var item in Children)
            {
                TotalProgressSpd += item.Speed;
                TotalProgressCur += item.Current;
                TotalProgressSize += item.Size;
            }
        }


        private long _TotalProgressCount;
        public long TotalProgressCount
        {
            get { return _TotalProgressCount; }
            set
            {
                if (_TotalProgressCount != value)
                {
                    _TotalProgressCount = value;
                    Notifier("TotalProgressCount");
                }
            }
        }

        private long _TotalProgressSpd;
        public long TotalProgressSpd
        {
            get { return _TotalProgressSpd; }
            set
            {
                if (_TotalProgressSpd != value)
                {
                    _TotalProgressSpd = value;
                    Notifier("TotalProgressSpd");
                }
            }
        }

        private long _TotalProgressCur;
        public long TotalProgressCur
        {
            get { return _TotalProgressCur; }
            set
            {
                if (_TotalProgressCur != value)
                {
                    _TotalProgressCur = value;
                    Notifier("TotalProgressCur");
                }
            }
        }

        private long _TotalProgressSize;
        public long TotalProgressSize
        {
            get { return _TotalProgressSize; }
            set
            {
                if (_TotalProgressSize != value)
                {
                    _TotalProgressSize = value;
                    Notifier("TotalProgressSize");
                }
            }
        }

        private ObservableCollection<ProgressChild> _Children = new ObservableCollection<ProgressChild>();
        public ObservableCollection<ProgressChild> Children
        {
            get { return _Children; }
            set
            {
                if (_Children != value)
                {
                    _Children = value;
                    Notifier("Children");
                }
            }
        }

        private List<Entry> _files = new List<Entry>();
        public List<Entry> Files
        {
            get { return _files; }
            set
            {
                if (_files != value)
                {
                    _files = value;
                    Notifier("Files");
                }
            }
        }
    }

    public class ProgressChild : NotifierBase
    {
        private Action UpdateParent;
        private DateTime lastUpdate = DateTime.MinValue;
        public ProgressChild(string name, Action updateParent)
        {
            Name = name;
            UpdateParent = updateParent;
        }

        public void Report(long cur, long total)
        {
            if ((DateTime.Now - lastUpdate).TotalMilliseconds < 500)
            {
                return;
            }
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate { ReportIn(cur, total); });
        }
        private void ReportIn(long cur, long total)
        {
            if ((DateTime.Now - lastUpdate).TotalMilliseconds == 0)
            {
                return;
            }

            if (lastUpdate != DateTime.MinValue)
            {
                var ss = (DateTime.Now - lastUpdate).TotalMilliseconds;
                Speed = Convert.ToInt64((cur - Current) * 1000 / ss);
            }
            lastUpdate = DateTime.Now;
            Current = cur;
            Size = total;

            if (UpdateParent != null)
            {
                UpdateParent();
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    Notifier("Name");
                }
            }
        }

        private long _Speed;
        public long Speed
        {
            get { return _Speed; }
            set
            {
                if (_Speed != value)
                {
                    _Speed = value;
                    Notifier("Speed");
                }
            }
        }

        private long _Current;
        public long Current
        {
            get { return _Current; }
            set
            {
                if (_Current != value)
                {
                    _Current = value;
                    Notifier("Current");
                }
            }
        }

        private long _Size;
        public long Size
        {
            get { return _Size; }
            set
            {
                if (_Size != value)
                {
                    _Size = value;
                    Notifier("Size");
                }
            }
        }
    }

    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return SizeSuffix(System.Convert.ToInt64(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        static readonly string[] SizeSuffixes =
                  { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        static string SizeSuffix(Int64 value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }

            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue / 1024) >= 1)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n1} {1}", dValue, SizeSuffixes[i]);
        }
    }
    public class SpeedConverter : IValueConverter
    {
        private SizeConverter sc = new SizeConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)sc.Convert(value, targetType, parameter, culture) + "/s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
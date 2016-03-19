using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BaiduApi
{
    static class StreamExtersion
    {
        static public Stream GetStream(this Baidu1 b, Entry e)
        {
            return new RAStream(b, e);
        }
    }
    class RAStream : System.IO.Stream
    {
        enum Status
        {
            Wait,Downloadling, Finish
        }
        Entry _entry;
        Baidu1 _baidu1;

        bool _released = false;
        long _pos;
        long _downloadPos;
        long BufferSize = 10 * 1024 * 1024;
        long UnitSize = 512 * 1024;
        Dictionary<PiceData, Status> _dict = new Dictionary<PiceData, Status>();

        internal RAStream(Baidu1 baidu1, Entry entry)
        {
            _baidu1 = baidu1;
            _entry = entry;
            for (int i = 0; i < 10; i++)
            {
                Start();
            }
        }

        private void Start()
        {
            Thread th = new Thread(ThreadStart);
            th.IsBackground = true;
            th.Start();
        }

        private void FinishCallBack(PiceData data, bool ok)
        {
            lock (_dict)
            {
                _dict[data] = Status.Finish;
            }
        }

        private PiceData GetPiceData()
        {
            while (!_released)
            {
                lock (_dict)
                {
                    foreach (var pair in _dict)
                    {
                        if (pair.Value == Status.Wait)
                        {
                            _dict[pair.Key] = Status.Downloadling;
                            return pair.Key;
                        }
                    }
                }
                Thread.Sleep(300);
            }
            return null;
        }

        private void ThreadStart()
        {
            while (!_released)
            {
                var data = GetPiceData();
                if (data == null) break;
                bool ret = _baidu1.DownloadPice(data);
                data.SetFinish(ret);
            }
        }

        private void InsertToQueue()
        {
            ChangeDownloadPos();
            lock (_dict)
            {
                if (_dict.Count < BufferSize / UnitSize)
                {
                    if (_downloadPos >=_entry.size)
                    {
                        return;
                    }
                    long tes = _downloadPos + UnitSize > _entry.size ? _entry.size - _downloadPos : UnitSize;
                    _dict.Add(new PiceData(_entry, _downloadPos, tes, FinishCallBack), Status.Wait);
                    _downloadPos += tes;
                }
            }
        }

        private void ChangeDownloadPos()
        {
            lock (_dict)
            {
                if (_downloadPos < Position || _dict.Count == 0)
                {
                    _downloadPos = Position;
                }
                else
                {
                    var data = _dict.Keys.First();
                    if (data.Offsite > Position)
                    {
                        _downloadPos = Position;
                    }
                }
            }
        }


        #region implete stream
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return _entry.size;
            }
        }

        public override long Position
        {
            get
            {
                return _pos;
            }

            set
            {
                if (value != _pos)
                {
                    if (value >= Length)
                    {
                        _pos = Length;
                        return;
                    }
                    _pos = value;
                    ChangeDownloadPos();
                }
            }
        }

        public override void Flush()
        {
            return;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position >= Length) return 0;
            int readed = 0;
            bool needwait = false;
            while (true)
            {
                if (needwait)
                {
                    Thread.Sleep(100);
                    needwait = false;
                }

                byte[] datas = null;
                long dataoff = 0;

                lock (_dict)
                {
                    if (_dict.Count == 0)
                    {
                        needwait = true;
                        continue;
                    }

                    var data = _dict.Keys.First();
                    if (Position < data.Offsite || Position >= data.Offsite + data.Size)
                    {
                        _dict.Remove(data);
                        InsertToQueue();
                        continue;
                    }

                    if (_dict[data] != Status.Finish)
                    {
                        needwait = true;
                        continue;
                    }

                    datas = data.Data;
                    dataoff = data.Offsite;
                }

                if (datas != null)
                {
                    long needcopycount = dataoff + datas.Length - Position;
                    if (needcopycount > count - readed) needcopycount = count - readed;

                    Array.Copy(datas, Position - dataoff, buffer, offset + readed, needcopycount);
                    readed += (int)needcopycount;
                    Position += needcopycount;
                    if (Position >= Length || readed >= count)
                    {
                        break;
                    }
                }
            }
            return readed;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            _released = true;
            base.Close();
        }
        #endregion implete stream
    }
}

extern alias global2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using global2::Newtonsoft.Json;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections;
using System.Web.UI.WebControls;

namespace BaiduApi
{
    public class Baidu
    {
        private CookieCollection cookies = new CookieCollection();
        private CookieContainer cc = new CookieContainer();
        private string path = "";
        private string username = "asdkf1956@163.com";
        private string password = "test1956";

        private string Token = "";
        private string CodeString = "";
        private string Accept = "*/*";
        private string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.94 Safari/537.36";
        private string Referer = "";

        private string ContentType = "application/x-www-form-urlencoded";

        private string Bduss;
        private string SkyDrive_app_id = "250528";
        public TextBox text;

        public Baidu(string _userName, string _pwd)
        {
            username = _userName;
            password = _pwd;
            path = System.Environment.CurrentDirectory + "/data/data_" + DateTime.Now.ToString("MMddHHmmss") + ".txt";

            //访问百度首页，获取 BAIDUID Cookie
            GetPageByGet("http://www.baidu.com", Encoding.UTF8);
            
            //获取 Token, CodeString
            GetTokenAndCodeString();
            GetAllCookies(cc);
            //登录百度
            Login();
            
            PrintCookies();
            Bduss = Convert.ToString(cookies["BDUSS"]);

            //获取网盘目录结构（包含文件）
            //GetFileDir();
        }

        /// <summary>
        /// 获取 Token & CodeString
        /// </summary>
        private void GetTokenAndCodeString()
        {
            string url = string.Format("https://passport.baidu.com/v2/api/?getapi&tpl=ik&apiver=v3&tt={0}&class=login", Utility.GetCurrentTimeStamp());
            string html = GetPageByGet(url, Encoding.UTF8);
            Console.WriteLine(url);
            ResultData result = JsonConvert.DeserializeObject<ResultData>(html);
            if (result.ErrInfo.no == "0")
            {
                Token = result.Data.Token;
                CodeString = result.Data.CodeString;
            }
        }
        /// <summary>
        /// 登录
        /// </summary>
        private void Login()
        {
            string loginUrl = "https://passport.baidu.com/v2/api/?login";
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData.Add("staticpage", "http://zhidao.baidu.com/static/html/v3Jump_bf2a8d6e.html");
            postData.Add("charset", "GBK");
            postData.Add("token", Token);
            postData.Add("tpl", "ik");
            postData.Add("apiver", "v3");
            postData.Add("tt", Utility.GetCurrentTimeStamp().ToString());
            postData.Add("codestring", "");
            postData.Add("isPhone", "false");
            postData.Add("safeflg", "0");
            postData.Add("u", "http://www.baidu.com/");
            postData.Add("username", username);
            postData.Add("password", password);
            postData.Add("verifycode", "");
            postData.Add("mem_pass", "on");
            postData.Add("ppui_logintime", "22429");
            postData.Add("callback", "parent.bd__pcbs__7doo5q");
            string html = GetPageByPost(loginUrl, postData, Encoding.UTF8);

        }

        public string GetFileDir()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("app_id", SkyDrive_app_id.ToString());
            data.Add("BDUSS", Bduss.Substring(6).ToString());
            string url = "http://pan.baidu.com/api/list?";
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in data)
            {
                sb.Append(HttpUtility.UrlEncode(kv.Key));
                sb.Append("=");
                sb.Append(HttpUtility.UrlEncode(kv.Value));
                sb.Append("&");
            }
            //url = url + sb.ToString();
            string listStr = GetPageByPost(url, data, Encoding.UTF8);

            Regex reg = new Regex(@",.*,");
            Match list_m = reg.Match(listStr);
            reg = new Regex(@"\{.*?\}");
            MatchCollection mc = reg.Matches(list_m.Value);
            string fileList = "";
            foreach (Match item in mc) {
                if (GetJsonValue("isdir", item.Value) == "1")
                    fileList += "文件夹：";
                else
                    fileList += "文件：";
                fileList += GetJsonValue("server_filename", item.Value) + "\r\n";
            }
            return fileList;
        }

        private string GetJsonValue(string key, string input)
        {
            //key = @"""" + key;
            //key += @"""";
            //key += @":""";
            //key += @".*?""";
            string key1 = @""""+key+@""":"".*?""";
            string key2 = @"""" + key + @""":.*?,";
            string key3 = @"""" + key + @""":.*?}";
            bool isNum = false;
            //尝试匹配第一种情况
            Regex reg = new Regex(key1);
            Match m = reg.Match(input);
            if (m.Value == null || string.IsNullOrEmpty(m.Value))
            {
                //尝试匹配第二种情况
                reg = new Regex(key2);
                m = reg.Match(input);
                isNum = true;
            }
            if (m.Value == null || string.IsNullOrEmpty(m.Value))
            {
                //尝试匹配第三种情况
                reg = new Regex(key3);
                m = reg.Match(input);
                isNum = true;
            }
            string value="";
            if (!isNum)
            {
                reg = new Regex(@""".*?""");
                MatchCollection mc = reg.Matches(m.Value);
                value = mc[1].Value;
                value = value.Replace("\"", "");
                value = SwitchValue(value);
            }
            else {
                value = m.Value;
                value = value.Split(':')[1];
                value = value.Replace(",", "");
                value = value.Replace("}", "");
            }
            return value;
        }
        //[将\u编码转为中文]
        private string SwitchValue(string input) {
            Regex regex = new Regex(@"\\u(\w{4})");
            string result = regex.Replace(input, delegate(Match m)
            {
                string hexStr = m.Groups[1].Value;
                string charStr = ((char)int.Parse(hexStr, System.Globalization.NumberStyles.HexNumber)).ToString();
                return charStr;
            });
            return result;
        }

        //成员函数：下载
        public string DownLoad(string filePath)
        {
            string data = "";
            string url = "http://c.pcs.baidu.com/rest/2.0/pcs/file?";
            url += "method=download&path=" + filePath + "&app_id=" + SkyDrive_app_id;
            url += "&" + Bduss;
            //data = GetPageByGet(url, Encoding.UTF8);
            DownloadFile(url, path, Encoding.UTF8);
            return data;
        }
        public void DownloadFile(string url, string filename, Encoding encoder)
        {
            try
            {
                string html = "";

                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
                webReq.CookieContainer = cc;
                List<Cookie> list = GetAllCookies(cc);
                webReq.Method = "GET";

                HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
                cookies.Add(webResp.Cookies);
                Stream st = webResp.GetResponseStream();
                Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public static List<Cookie> GetAllCookies(CookieContainer cc)
        {
            List<Cookie> lstCookies = new List<Cookie>();

            Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, cc, new object[] { });
            StringBuilder sb = new StringBuilder();
            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies)
                    {
                        lstCookies.Add(c);
                        sb.AppendLine(c.Domain + ":" + c.Name + "____" + c.Value + "\r\n");
                    }
            }
            return lstCookies;
        }

        public void UpLoad(string filePath)
        {
            string url = "http://c.pcs.baidu.com/rest/2.0/pcs/file?";
            //url += "method=upload&path=" + filePath + "&ondup=overwrite&app_id=" + SkyDrive_app_id;
            //url += "&" + Bduss + "&file=" + path;
            //string data = GetPageByGet(url, Encoding.UTF8);
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("method", "upload");//方法
            data.Add("path", "/data/00.jpg");//上传到网盘的路径
            data.Add("ondup", "overwrite");//模式，覆盖
            data.Add("app_id", SkyDrive_app_id.ToString());
            data.Add("BDUSS", Bduss.Substring(6).ToString());
            //data.Add("file", GetFileString());
            FileUpLoad(url, data, Encoding.UTF8);
        }

        private string GetFileString()
        {
            //StreamReader sr = new StreamReader(path, Encoding.UTF8);
            //string strLine = sr.ReadLine();
            //StringBuilder sb = new StringBuilder();
            //while (strLine != null)
            //{
            //    sb.Append(strLine);
            //    strLine = sr.ReadLine();
            //}
            //sr.Close();
            //return sb.ToString();
            return "百度网盘文件上传测试！";
        }
        private string FileUpLoad(string url, Dictionary<string, string> postData, Encoding encoder)
        {
            String fileToUpload = System.Environment.CurrentDirectory+"/data/00.jpg";
            String fileFormName = "file";
            String contenttype = "image/jpeg";
            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in postData)
            {
                sb.Append(HttpUtility.UrlEncode(kv.Key));
                sb.Append("=");
                sb.Append(HttpUtility.UrlEncode(kv.Value));
                sb.Append("&");
            }
            string uploadUrl = url + sb.ToString();
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(uploadUrl);
            webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webrequest.Method = "POST";
            sb = new StringBuilder();
            sb.Append("--");
            sb.Append(boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append(fileFormName);
            sb.Append("\"; filename=\"");
            sb.Append(Path.GetFileName(fileToUpload));
            sb.Append("\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: ");
            sb.Append(contenttype);
            sb.Append("\r\n");
            sb.Append("\r\n");

            string postHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            FileStream fileStream = new FileStream(fileToUpload, FileMode.Open, FileAccess.Read);
            long length = postHeaderBytes.Length + fileStream.Length + boundaryBytes.Length;
            webrequest.ContentLength = length;
            Stream requestStream = webrequest.GetRequestStream();
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

            byte[] buffer = new Byte[(int)fileStream.Length];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                requestStream.Write(buffer, 0, bytesRead);
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            requestStream.Close();

            HttpWebResponse webResp = (HttpWebResponse)webrequest.GetResponse();
            Stream stream = webResp.GetResponseStream();
            StreamReader sr = new StreamReader(stream, encoder);
            string response = sr.ReadToEnd();
            /*
             {"path":"\/data\/00.jpg","size":27669,"ctime":1390114937,"mtime":1390114937,
             * "md5":"02865d5c3c3cfc2304d99db4e8113d1f","fs_id":1880927245,"isdir":0,
             * "request_id":1661118567}
             */
            return "";
        }
        /// <summary>
        /// 以 Post 方式提交网页数据,获得服务器返回的数据
        /// </summary>
        /// <param name="url"> Url </param>
        /// <param name="postData">Post 数据</param>
        /// <param name="encoder">网页编码</param>
        /// <returns>服务器返回的数据</returns>
        private string GetPageByPost(string url, Dictionary<string, string> postData, Encoding encoder)
        {
            string html = "";

            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.CookieContainer = cc;
            webReq.Method = "POST";

            webReq.Accept = this.Accept;
            webReq.UserAgent = this.UserAgent;
            webReq.Referer = this.Referer;

            Stream reqStream = null;
            if (postData != null && postData.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, string> kv in postData)
                {
                    sb.Append(HttpUtility.UrlEncode(kv.Key));
                    sb.Append("=");
                    sb.Append(HttpUtility.UrlEncode(kv.Value));
                    sb.Append("&");
                }

                byte[] data = Encoding.UTF8.GetBytes(sb.ToString().TrimEnd('&'));

                webReq.ContentType = ContentType;
                webReq.ContentLength = data.Length;
                reqStream = webReq.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                if (reqStream != null)
                {
                    reqStream.Close();
                }
            }

            HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
            cookies.Add(webResp.Cookies);
            Stream stream = webResp.GetResponseStream();
            StreamReader sr = new StreamReader(stream, encoder);
            html = sr.ReadToEnd();

            sr.Close();
            stream.Close();

            return html;
        }
        /// <summary>
        /// 以 Get 方式提交网页数据,获得服务器返回的数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoder"></param>
        /// <returns></returns>
        private string GetPageByGet(string url, Encoding encoder)
        {
            string html = "";

            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.CookieContainer = cc;
            webReq.Method = "GET";

            HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse();
            cookies.Add(webResp.Cookies);
            Stream stream = webResp.GetResponseStream();
            StreamReader sr = new StreamReader(stream, encoder);
            html = sr.ReadToEnd();

            sr.Close();
            stream.Close();

            return html;
        }

        /// <summary>
        /// 打印 Cookies
        /// </summary>
        public void PrintCookies()
        {
            Console.WriteLine("---------------------------------Cookies----------------------------------------");

            foreach (Cookie cookie in cookies)
            {
                Console.WriteLine("{0}: {1} Domain: {2}", cookie.Name, cookie.Value, cookie.Domain);
            }

            Console.WriteLine("--------------------------------------------------------------------------------");
        }


        

    }
}

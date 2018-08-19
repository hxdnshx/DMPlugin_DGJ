﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DMPlugin_DGJ.LWLAPI
{
    class LwlApiBaseModule : SongsSearchModule
    {
        private string ServiceName = "undefined";
        protected void SetServiceName(string name) => ServiceName = name;

        private const string API_PROTOCOL = "https://";
        private const string API_HOST = "api.lwl12.com";
        private const string API_PATH = "/music/";

        protected const string INFO_PREFIX = "内置-";
        protected const string INFO_AUTHOR = "Genteure & LWL12";
        protected const string INFO_EMAIL = "dgj@genteure.com";
        protected const string INFO_VERSION = "1.1";
        protected const bool INFO_LYRIC = true;

        protected override SongInfo Search(string keyword)
        {
            string result_str;
            try
            {
                result_str = Fetch(API_PROTOCOL, API_HOST, API_PATH + ServiceName + $"/search?keyword={keyword}");
            }
            catch (Exception ex)
            {
                Log("搜索歌曲时网络错误：" + ex.Message);
                return null;
            }

            JObject song = null;
            try
            {
                JObject info = JObject.Parse(result_str);
                if (info["code"].ToString() == "200")
                {
                    song = (info["result"] as JArray)?[0] as JObject;
                }
            }
            catch (Exception ex)
            {
                Log("搜索歌曲解析数据错误：" + ex.Message);
                return null;
            }

            string songid = "";
            string songname = "";
            string[] songartists;
            string url = "";
            string lyric = "";

            try
            {
                songid = song["id"].ToString();
                songname = song["name"].ToString();
                songartists = (song["artist"] as JArray).ToArray().Select(x => x.ToString()).ToArray();
            }
            catch (Exception ex)
            { Log("歌曲信息获取结果错误：" + ex.Message); return null; }

            try
            {
                JObject lobj = JObject.Parse(Fetch(API_PROTOCOL, API_HOST, API_PATH + ServiceName + $"/lyric?id={songid}"));
                if (lobj["result"]["lwlyric"] != null)
                {
                    lyric = lobj["result"]["lwlyric"].ToString();
                }
                else if (lobj["result"]["lyric"] != null)
                {
                    lyric = lobj["result"]["lyric"].ToString();
                }
                else
                { Log("歌词获取错误(id:" + songid + ")"); }
            }
            catch (Exception ex)
            { Log("歌词获取错误(ex:" + ex.ToString() + ",id:" + songid + ")"); }

            return new SongInfo(this, songid, songname, songartists)
            {
                Lyric = lyric
            };

            // return SongItem.init(this, songname, songid, who, songartists, url, lyric);
        }

        protected override string GetDownloadUrl(SongInfo songInfo)
        {
            try
            {
                JObject dlurlobj = JObject.Parse(Fetch(API_PROTOCOL, API_HOST, API_PATH + ServiceName + $"/song?id={songInfo.Id}"));

                if (dlurlobj["code"].ToString() == "200")
                {
                    return dlurlobj["result"]["url"].ToString();
                }
                else
                {
                    Log($"歌曲 {songInfo.Name} 因为版权不能下载");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log($"歌曲 {songInfo.Name} 疑似版权不能下载(ex:{ex.Message})");
                return null;
            }
        }

        protected override DownloadStatus Download(SongItem item)
        {
            throw new NotImplementedException();
        }

        private static string Fetch(string prot, string host, string path, string data = null, string referer = null)
        {
            string address;
            if (GetDNSResult(host, out string ip))
                address = prot + ip + path;
            else
                address = prot + host + path;

            var request = (HttpWebRequest)WebRequest.Create(address);

            request.Timeout = 10000;
            request.Host = host;
            request.UserAgent = "DMPlugin_DGJ/" + Center.Plg_ver;

            if (referer != null)
                request.Referer = referer;

            if (data != null)
            {
                var postData = Encoding.UTF8.GetBytes(data);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.Length;
                using (var stream = request.GetRequestStream())
                    stream.Write(postData, 0, postData.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            return responseString;
        }
        private static string Fetch(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 10000;
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            return responseString;
        }
        private static bool GetDNSResult(string domain, out string result)
        {
            if (DNSList.TryGetValue(domain, out DNSResult result_from_d))
            {
                if (result_from_d.TTLTime > DateTime.Now)
                {
                    result = result_from_d.IP;
                    return true;
                }
                else
                {
                    DNSList.Remove(domain);
                    if (RequestDNSResult(domain, out DNSResult? result_from_api, out Exception exception))
                    {
                        DNSList.Add(domain, result_from_api.Value);
                        result = result_from_api.Value.IP;
                        return true;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
            }
            else
            {
                if (RequestDNSResult(domain, out DNSResult? result_from_api, out Exception exception))
                {
                    DNSList.Add(domain, result_from_api.Value);
                    result = result_from_api.Value.IP;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
        }
        private static bool RequestDNSResult(string domain, out DNSResult? dnsResult, out Exception exception)
        {
            dnsResult = null;
            exception = null;

            try
            {
                var http_result = Fetch("http://119.29.29.29/d?ttl=1&dn=" + domain);
                if (http_result == string.Empty)
                    return false;

                MatchCollection matches = regex.Matches(http_result);
                if (matches.Count < 1)
                {
                    exception = new Exception("HTTPDNS 返回结果不正确");
                    return false;
                }
                GroupCollection group = matches[0].Groups;

                dnsResult = new DNSResult()
                {
                    IP = group[1].Value + "." + group[2].Value + "." + group[3].Value + "." + group[4].Value,
                    TTLTime = DateTime.Now + TimeSpan.FromSeconds(double.Parse(group[5].Value))
                };
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        private static readonly Dictionary<string, DNSResult> DNSList = new Dictionary<string, DNSResult>();
        private static readonly Regex regex = new Regex(@"(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)", RegexOptions.Compiled);
        private struct DNSResult
        {
            internal string IP;
            internal DateTime TTLTime;
        }
    }
}

using Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace API
{
    public class LCUManager
    {
        private string port;
        private string token;
        private Process[] ps;
        public int ProcessIndex { get; private set; }
        public LCUManager()
        {
            ps = Process.GetProcessesByName("LeagueClient");            
        }

        public void SelectProcess(int index)
        {
            StringBuilder path = new StringBuilder(ps[index].MainModule.FileName);
            path.Remove(path.Length - 16, 16);
            path.Append("lockfile");
            string data;
            using (FileStream fs = new FileStream(path.ToString(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader fileStream = new StreamReader(fs, Encoding.UTF8);
                data = fileStream.ReadToEnd();
                fileStream.Close();
            }
            string[] list = data.Split(':');
            port = list[2];
            token = list[3];
        }
        public int GetProcessCount()
        {
            return ps.Length;
        }
        public List<string> GetAllProcessMainModuleFileNames()
        {
            List<string> res = new List<string>();
            for (int i = 0; i < ps.Length; i++)
            {
                res.Add(ps[i].MainModule.FileName);
            }
            return res;
        }
        public void GetSummonerInfo(Action<SummonerInfo> success, Action<string> fail)
        {
            string summonerurl = "/lol-summoner/v1/current-summoner";
            string url = string.Format("https://riot:{0}@127.0.0.1:{1}{2}", token, port, summonerurl);
            NetworkCredential myCred = new NetworkCredential("riot", token);
            LCUHttpHelper.Get(url, res => {
                var info = JsonConvert.DeserializeObject<SummonerInfo>(res);
                success(info);

            }, fail, myCred);
          
        }

        
        public void CreatLabby(string js, Action<string> success, Action<string> fail)
        {
            string url = string.Format("https://riot:{0}@127.0.0.1:{1}/lol-lobby/v2/lobby", token, port);
            NetworkCredential myCred = new NetworkCredential("riot", token);
            LCUHttpHelper.Post(url, js, suc=>{
                if (suc.Contains("\"canStartActivity\":true"))
                {
                    success(suc);
                }
                else
                {
                    fail?.Invoke(suc);
                }
            }, fail, myCred);          
        }
        public void CreatLabby(int queueid, Action<string> success, Action<string> fail)
        {
            string js = "{\"queueId\":" + queueid + "}";
            CreatLabby(js, success, fail);
        }
        public void CreatLabby(CustomLobby customLobby, Action<string> success, Action<string> fail)
        {
            CreatLabby(customLobby.toJsonStr(), success, fail);
        }

        public void GetGameMode(Action<List<GameMode>> success, Action<string> fail)
        {
            LCUHttpHelper.Get("http://static.developer.riotgames.com/docs/lol/gameModes.json", res =>
            {
                success(JsonConvert.DeserializeObject<List<GameMode>>(res));
            },fail);
        }

        public void GetCustomLabbyAvailableBots(Action<List<AvailableBotInfo>> success, Action<string> fail)
        {

            string url = string.Format("https://riot:{0}@127.0.0.1:{1}/lol-lobby/v2/lobby/custom/available-bots", token, port);
            NetworkCredential myCred = new NetworkCredential("riot", token);
            LCUHttpHelper.Get(url, suc => {
                success(JsonConvert.DeserializeObject<List<AvailableBotInfo>>(suc));
            }, fail, myCred);
        }
        public void AddCustomLabbyBot(CreatBotInfo creatBotInfo,Action<string> success, Action<string> fail)
        {
            string url = string.Format("https://riot:{0}@127.0.0.1:{1}/lol-lobby/v1/lobby/custom/bots", token, port);
            NetworkCredential myCred = new NetworkCredential("riot", token);
            LCUHttpHelper.Post(url, creatBotInfo.ToJsonStr(), success, fail, myCred);
        }


    }
}

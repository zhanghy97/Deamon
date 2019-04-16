using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Collections.Specialized;

namespace Deamon
{
    class Program
    {
        static void Main(string[] args)
        {
            Process[] processIdAry = Process.GetProcessesByName("Deamon");

            if (processIdAry.Count() > 1)
            {
                for (int i = 1; i < processIdAry.Count(); i++)
                {
                    processIdAry[i].Kill();
                }
            }
            else
            {
                Console.WriteLine("守护程序启动成功！");
            }

            Ccout();
        }


        public static void Ccout()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            string[] pathList = Regex.Split(configuration["Path"], ",", RegexOptions.IgnoreCase);

            string[] phonelist = Regex.Split(configuration["PhoneNumber"], ",", RegexOptions.IgnoreCase);

            string logPath = configuration["LogPath"];

            while (true)
            {
                Thread.Sleep(5000);
                //获取所有进程内的进程名并加入列表
                Process[] ps = Process.GetProcesses();
                var pcNameList = new List<string>();
                foreach (Process p in ps)
                {
                    pcNameList.Add(p.ProcessName);
                }
                foreach (string pcPath in pathList)
                {
                    string[] b = Regex.Split(pcPath, "/", RegexOptions.IgnoreCase);
                    bool exists = ((IList)pcNameList).Contains(b[1]);
                    if (exists)
                    {
                        Console.WriteLine(b[1] + "程序正常运行");
                    }
                    else
                    {
                        foreach (string phone in phonelist) {
                            Sendsms(b[1], phone);
                        }
                        string path = logPath;
                        FileStream fs = new FileStream(path, FileMode.Append);
                        StreamWriter sw = new StreamWriter(fs);
                        sw.WriteLine("进程停止时间：");
                        sw.WriteLine(DateTime.Now.ToLocalTime().ToString());
                        sw.Close();
                        fs.Close();

                        Console.WriteLine(b[1] + "进程中断，正在执行重启任务");
                        //重启这个进程
                        var decodeProcess = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = b[0],
                                UseShellExecute = false,
                                RedirectStandardInput = false,
                                RedirectStandardOutput = false,
                                RedirectStandardError = false,
                                CreateNoWindow = true
                            }
                        };

                        decodeProcess.Start();
                        FileStream fs1 = new FileStream(path, FileMode.Append);
                        StreamWriter sw1 = new StreamWriter(fs1);
                        sw1.WriteLine("进程重启时间：");
                        sw1.WriteLine(DateTime.Now.ToLocalTime().ToString());
                        sw1.Close();
                        fs1.Close();
                        Console.WriteLine("重启成功");
                    }
                }
            }
        }
        public static void Sendsms(string nr, string phonelist)
        {
            var builder1 = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration1 = builder1.Build();


            string key = configuration1["Key"];
            string url = configuration1["Ip:Port"];
            var values = new NameValueCollection
            {
                { "phonelist", phonelist },
                { "content", nr+"进程意外关闭！关闭时间：" +DateTime.Now.ToLocalTime().ToString()},
                { "__Click",key}
            };

            new OA().WPostGB2312(url, values);

        }
    }
}
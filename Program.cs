using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Alidns.Model.V20150109;
using Serilog;
using Serilog.Core;
using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Xml.Serialization;
using System.IO;

namespace aliyunDDNS
{
    class Program
    {
        static Logger logconsole = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        static Logger logfile = new LoggerConfiguration().WriteTo.File("c:/log/AliyunDDNS/", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10).CreateLogger();
        static void Main(string[] args)
        {
            //检测配置文件
            SettingModel sm = new SettingModel();
            if (!File.Exists("c:/dotnetsettings/aliyunddns.xml"))
            {
                WarningLog("检测无配置文件，生成配置文件中");
                Object2Xml<SettingModel>("c:/dotnetsettings/", "aliyunddns.xml", sm);
                WarningLog("配置文件已生成，请配置参数后重启程序");
                System.Environment.Exit(0);
            }
            else
            {
                sm = Xml2Object<SettingModel>("c:/dotnetsettings/aliyunddns.xml");
                if (sm == null || string.IsNullOrEmpty(sm.RR) || string.IsNullOrEmpty(sm.DomainName) || string.IsNullOrEmpty(sm.AccessKey) || string.IsNullOrEmpty(sm.AccessSecret))
                {
                    WarningLog("参数配置不能为空，请检查配置文件");
                    System.Environment.Exit(0);
                }
            }
            WarningLog("参数初始化成功，开始监听端口IP");
            while (true)
            {
                try
                {
                    DefaultAcsClient client = new DefaultAcsClient(DefaultProfile.GetProfile("", sm.AccessKey, sm.AccessSecret));
                    string currentIP = new WebClient().DownloadString("https://echo-ip.starworks.cc");
                    InfoLog("当前IP：" + currentIP);
                    var domainRecords = client.GetAcsResponse(new DescribeDomainRecordsRequest
                    {
                        DomainName = sm.DomainName,
                        RRKeyWord = sm.RR,
                    }).DomainRecords;
                    DescribeDomainRecordsResponse.DescribeDomainRecords_Record home_Record = domainRecords.First(x => x.RR == sm.RR);
                    if (home_Record._Value != currentIP)
                    {
                        client.GetAcsResponse(new UpdateDomainRecordRequest
                        {
                            RecordId = home_Record.RecordId,
                            RR = home_Record.RR,
                            Type = home_Record.Type,
                            _Value = currentIP,
                            TTL = home_Record.TTL,
                        });
                        WarningLog($"地址更改，从{home_Record._Value}改为{currentIP}");
                    }
                    else
                    {
                        InfoLog("IP无变更");
                    }
                }
                catch (Exception ex)
                {
                    WarningLog(ex.ToString());
                }
                
                Thread.Sleep(sm.CheckTime*1000);
            }

        }
        public static void InfoLog(string str)
        {
            logconsole.Information(str);
        }
        public static void WarningLog(string str)
        {
            logconsole.Warning(str);
            logfile.Warning(str);
        }
        public static T Xml2Object<T>(string path)
        {
            T Obj = default(T);

            XmlSerializer xml = new XmlSerializer(typeof(T));
            try
            {
                FileStream fs = File.OpenRead(path);
                StreamReader sr = new StreamReader(fs);
                Obj = (T)xml.Deserialize(sr);
                sr.Close();
                fs.Close();
            }
            catch (Exception ex)
            {

            }

            return Obj;
        }
        public static int Object2Xml<T>(string path, string filename, T obj)
        {
            int result = 0;
            try
            {
                if (!Directory.Exists(path))
                { Directory.CreateDirectory(path); }
                TextWriter tw = new StreamWriter(path + filename, false);
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(tw, obj);
                tw.Close();
                result = 1;
            }
            catch (Exception ex)
            {

            }

            return result;
        }
    }
}

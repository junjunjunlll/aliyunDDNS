using System;
using System.Collections.Generic;
using System.Text;

namespace aliyunDDNS
{
    public class SettingModel
    {
        public string RR { get; set; } = string.Empty;
        public string DomainName { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string AccessSecret { get; set; } = string.Empty;
        public int CheckTime { get; set; } = 60;

        internal void Deconstruct(out string rr, out string domainname, out string accesskey, out string accesssecret, out int checktime)
        {
            rr = RR;
            domainname = DomainName;
            accesskey = AccessKey;
            accesssecret = AccessSecret;
            checktime = CheckTime;
        }
    }
}

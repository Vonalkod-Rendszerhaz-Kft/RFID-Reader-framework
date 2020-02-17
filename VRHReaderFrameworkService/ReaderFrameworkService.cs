using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace VRHReaderFrameworkService
{
    public partial class ReaderFrameworkService : ServiceBase
    {
        public ReaderFrameworkService()
        {
            InitializeComponent();
        }

        private string GetSettingValueFromAppConfigForDLL(string settingName, string appName = "VRHReaderFrameworkService")
        {
            string sectionName = "applicationSettings/" + appName + ".Properties.Settings";
            System.Configuration.ClientSettingsSection section  = (System.Configuration.ClientSettingsSection)System.Configuration.ConfigurationManager.GetSection(sectionName);
            foreach ( System.Configuration.SettingElement setting in section.Settings)
            {
                string value = setting.Value.ValueXml.InnerText;
                string name = setting.Name;
                if (name.ToLower().StartsWith(settingName.ToLower()))
                {
                    return value;
                }
            }
            return string.Empty;
        }

        protected override void OnStart(string[] args)
        {
            VRHReaderFrameworkMain.clsMainWorker.Start(GetSettingValueFromAppConfigForDLL("BaseDirectory"),"Service");
        }

        protected override void OnStop()
        {
            VRHReaderFrameworkMain.clsMainWorker.Stop();
        }
    }
}

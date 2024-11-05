using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesafioAlura.RPA.Helpers
{
    public class InfoHelper
    {
        public static string GetBuildVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var result = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.MinorRevision);

            return result;
        }

        public static string Intro
        {
            get
            {
                return @"  ____  ____   _      ____  _____ ____    _    _____ ___ ___  
 |  _ \|  _ \ / \    |  _ \| ____/ ___|  / \  |  ___|_ _/ _ \ 
 | |_) | |_) / _ \   | | | |  _| \___ \ / _ \ | |_   | | | | |
 |  _ <|  __/ ___ \  | |_| | |___ ___) / ___ \|  _|  | | |_| |
 |_| \_\_| /_/   \_\ |____/|_____|____/_/   \_\_|   |___\___/ 
                                                              ";
            }
        }

        public static string Title
        {
            get { return string.Format("Desafio Alura RPA - V {0}", GetBuildVersion()); }
        }

        public static async Task<bool> Internet(string baseAddress = "https://www.google.com.br/", int timeout = 5)
        {
            var handler = new HttpClientHandler();
            handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            var client = new HttpClient(handler)
            //var client = new HttpClient()
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(timeout)
            };
            try
            {
                var s = await client.GetAsync(client.BaseAddress);
                return s.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException.Message);
                return false;
            }
            return false;
        }
    }
}

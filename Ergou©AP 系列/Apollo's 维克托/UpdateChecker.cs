using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Viktor
{
    internal class UpdateChecker
    {
        private static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        public static void Init(string user, string assembly)
        {
            try
            {
                var request =
                    WebRequest.Create(
                        String.Format(
                            "https://raw.githubusercontent.com/{0}/LeagueSharp/master/{1}/AssemblyInfo.cs",
                            user, assembly));
                var response = request.GetResponse();
                var data = response.GetResponseStream();
                string version;
                using (var sr = new StreamReader(data))
                {
                    version = sr.ReadToEnd();
                }
                const string pattern = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}";
                var serverVersion = new Version(new Regex(pattern).Match(version).Groups[0].Value);

                if (serverVersion > Version)
                {
                    Program.ShowNotification(
                        "Update avalible: " + Version + " => " + serverVersion, System.Drawing.Color.Red, 10000);
                }
                else if (serverVersion == Version)
                {
                    Program.ShowNotification("No Update avalible: " + Version, System.Drawing.Color.GreenYellow, 5000);
                }
                else if (serverVersion < Version)
                {
                    Program.ShowNotification(
                        "Beta Version: " + Version + " => " + serverVersion, System.Drawing.Color.Yellow, 10000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Program.ShowNotification(
                    "Couldnt check the Version", System.Drawing.Color.Red, 5000);
            }
        }
    }
}

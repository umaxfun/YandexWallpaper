using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace YWP
{
    static class Program
    {
        const string YandexImagesWallpaperUrl = "http://yandex.ru/images/today?size=1920x1080";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string logPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) ,"YWP"
                        ) + DateTime.Now.ToString("yyMMdd")+".log";

            var log = new LogFile(logPath, true, LogFile.LogType.Txt);
            
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => log.Log(args.ExceptionObject.ToString());

            var webClient = new WebClient { UseDefaultCredentials = false, Proxy = WebRequest.GetSystemWebProxy() };
            webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

            var recentFileName = "wp_" + DateTime.Now.ToString("ddMMyyyy") + ".jpg";
            
            byte[] data = null;
            try
            {
                data = webClient.DownloadData(YandexImagesWallpaperUrl);
            }
            catch (WebException ex)
            {
                var resp = ex.Response as HttpWebResponse;
                if(resp == null) return;
                if (resp.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
                {
                    NetworkCredential creds;

                    CredUi.GetCredentialsVistaAndUp(
                        string.Format("Enter proxy login/password for  {0}", webClient.Proxy.GetProxy(new Uri(YandexImagesWallpaperUrl)).Host), "",
                        out creds);
                    if (creds == null) return;
                    webClient.Proxy.Credentials = creds;
                    try
                    {
                        data = webClient.DownloadData(YandexImagesWallpaperUrl);
                    }
                    catch (Exception ex2)
                    {
                        log.Log(ex2.ToString());
                        return; 
                    }
                }
            }
            
            if (data == null)
            {
                log.Log("Strange things happen out there! No images have been received.");
                return;
            }
            
            if (!File.Exists(recentFileName))
            {
                File.WriteAllBytes(recentFileName, data);
                SetWallpaper(recentFileName);
            }
            else
            {
                var data2 = File.ReadAllBytes(recentFileName);
                if (!AreByteArraysEqualEqual(data, data2))
                {
                    File.WriteAllBytes(recentFileName, data);
                }
                SetWallpaper(recentFileName);
            }
        }

        private static void SetWallpaper(string recentFileName)
        {
            Wallpaper.Set(new Uri(new FileInfo(recentFileName).FullName, UriKind.Absolute), Wallpaper.Style.Fill);
        }

        static bool AreByteArraysEqualEqual(IList<byte> arr1, IList<byte> arr2)
        {
            if (arr1 == null || arr2 == null)
                return false;

            if (arr1.Count != arr2.Count)
                return false;

            //and here is the check loop
            for (int i = 0; i < arr1.Count; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }
            return true;
        }


    }
}

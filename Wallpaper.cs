using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace YWP
{
    public static class Wallpaper
    {
        // PInvoke prototypes go from winapi blueprint

        // ReSharper disable InconsistentNaming 
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable MemberCanBePrivate.Local

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style
        {
            Tiled,
            Centered,
            Stretched,
            Fill
        }
        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore InconsistentNaming 

        public static void Set(Uri uri, Style style)
        {
            var s = new System.Net.WebClient().OpenRead(uri.ToString());

            if (s == null) return;
            var img = System.Drawing.Image.FromStream(s);
            var tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if(key==null) return;

            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString(CultureInfo.InvariantCulture));
                key.SetValue(@"TileWallpaper", 0.ToString(CultureInfo.InvariantCulture));
            }

            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString(CultureInfo.InvariantCulture));
                key.SetValue(@"TileWallpaper", 0.ToString(CultureInfo.InvariantCulture));
            }

            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString(CultureInfo.InvariantCulture));
                key.SetValue(@"TileWallpaper", 1.ToString(CultureInfo.InvariantCulture));
            }
            if (style == Style.Fill)
            {
                key.SetValue(@"WallpaperStyle", 10.ToString(CultureInfo.InvariantCulture));
                key.SetValue(@"TileWallpaper", 0.ToString(CultureInfo.InvariantCulture));
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
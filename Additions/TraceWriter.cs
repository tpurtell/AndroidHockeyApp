using System;
using System.IO;
using Java.Lang;
using Java.Util;
using Exception = System.Exception;
using Process = Android.OS.Process;

namespace Net.Hockeyapp.Android
{
    public static class TraceWriter
    {
        public static void WriteTrace(object exception)
        {
            DateTime date = DateTime.UtcNow;
            // Create filename from a random uuid
            string filename = UUID.RandomUUID().ToString();
            string path = Path.Combine(Constants.FilesPath, filename + ".stacktrace");
            Console.WriteLine("Writing unhandled exception to: {0}", path);
            try
            {
                using (var f = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var sw = new StreamWriter(f))
                {
                    // Write the stacktrace to disk
                    sw.WriteLine("Package: {0}", Constants.AppPackage);
                    sw.WriteLine("Version: {0}", Constants.AppVersion);
                    sw.WriteLine("Android: {0}", Constants.AndroidVersion);
                    sw.WriteLine("Manufacturer: {0}", Constants.PhoneManufacturer);
                    sw.WriteLine("Model: {0}", Constants.PhoneModel);
                    sw.WriteLine("Date: {0}", date);
                    sw.WriteLine("\n");
                    sw.WriteLine(exception);
                }
            }
            catch (Exception another)
            {
                Console.WriteLine("Error saving exception stacktrace! {0}", another);
            }
            Process.KillProcess(Process.MyPid());
            JavaSystem.Exit(10);
            Environment.Exit(10);
        }
    }
}
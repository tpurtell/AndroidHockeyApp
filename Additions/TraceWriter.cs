// public domain ... derived from https://github.com/bitstadium/HockeySDK-Android/blob/db7fff12beecea715f2894cb69ba358ea324ad17/src/main/java/net/hockeyapp/android/internal/ExceptionHandler.java
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Net.Hockeyapp.Android
{
    public static class TraceWriter
    {
        public static void WriteTrace(object exception)
        {
            DateTime date = DateTime.UtcNow;
            // Create filename from a random uuid
            var bytes = new byte[16];
            new Random().NextBytes(bytes);
            string filename = new SoapHexBinary(bytes).ToString();
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
                    sw.WriteLine();
                    try
                    {
                        sw.WriteLine(exception);
                    }
                    catch (Exception e)
                    {
                        // checking for https://bugzilla.xamarin.com/show_bug.cgi?id=10379
                        // null reference exceptions with no stack trace from inside unhandled exception handler
                        // may not be a problem, investigating.
                        sw.WriteLine();
                        sw.WriteLine("Exception writing exception: {0}", e);
                        throw e;
                    }
                }
            }
            catch (Exception another)
            {
                Console.WriteLine("Error saving exception stacktrace! {0}", another);
            }
            Process.GetCurrentProcess().Kill();
            Environment.Exit(10);
        }
    }
}
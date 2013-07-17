// public domain ... derived from https://github.com/bitstadium/HockeySDK-Android/blob/db7fff12beecea715f2894cb69ba358ea324ad17/src/main/java/net/hockeyapp/android/internal/ExceptionHandler.java
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Net.Hockeyapp.Android
{
    public static class TraceWriter
    {        
        private static CrashManagerListener _listener;

        private static string _appPackage = "Unknown: call TraceWriter.InitializeConstants after CrashManager.Initialize";
        private static string _appVersion = "Unknown: call TraceWriter.InitializeConstants after CrashManager.Initialize";
        private static string _androidVersion = "Unknown: call TraceWriter.InitializeConstants after CrashManager.Initialize";
        private static string _phoneManufacturer = "Unknown: call TraceWriter.InitializeConstants after CrashManager.Initialize";
        private static string _phoneModel = "Unknown: call TraceWriter.InitializeConstants after CrashManager.Initialize";
        private static string _filesPath = ".";

        /// <summary>
        /// Copy build properties into c# land so that the handler won't crash accessing java.
        /// </summary>
        public static void InitializeConstants()
        {
            _appPackage = Constants.AppPackage;
            _appVersion = Constants.AppVersion;
            _androidVersion = Constants.AndroidVersion;
            _phoneManufacturer = Constants.PhoneManufacturer;
            _phoneModel = Constants.PhoneModel;
            _filesPath = Constants.FilesPath;
        }

        //ToDo: Check if static variables survive lifecycles of different activities!
        /// <summary>
        /// Initialize the TraceWriter with a given CrashManagerListener
        /// </summary>
        /// <param name="listener"></param>
        /// <remarks>Thuis is important to use the UserID, Contact and Description from CrashManagerListener.</remarks>
        public static void Initialize(CrashManagerListener listener)
        {
            _listener = listener;
            InitializeConstants();
        }

        /// <summary>
        /// Writes the given object (usually an exception) to disc so that it can be picked up by the CrashManager and send to Hockeyapp.
        /// </summary>
        /// <param name="exception">The object to write (usually an exception)</param>
        /// <remarks>This method controls exactly what is written to disc. UserID, Contact and Description from CrashManagerListener are NOT used!</remarks>
        public static void WriteTrace(object exception)
        {
            var date = DateTime.Now;
            // Create filename from a random uuid
            //ToDo: Why not use Guid.NewGuid().ToString() ???            
            var bytes = new byte[16];
            new Random().NextBytes(bytes);
            var filename = new SoapHexBinary(bytes).ToString();
            var path = Path.Combine(_filesPath, filename + ".stacktrace");
            Console.WriteLine("Writing unhandled exception to: {0}", path);
            try
            {
                using (var f = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var sw = new StreamWriter(f))
                {
                    // Write the stacktrace to disk
                    sw.WriteLine("Package: {0}", _appPackage);
                    sw.WriteLine("Version: {0}", _appVersion);
                    sw.WriteLine("Android: {0}", _androidVersion);
                    sw.WriteLine("Manufacturer: {0}", _phoneManufacturer);
                    sw.WriteLine("Model: {0}", _phoneModel);
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
                        throw new Exception("Problem writing exception", e);
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

        /// <summary>
        /// Writes the given exception to disc using the standard Exceptionhandler.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <remarks>All features from CrashManagerListener are used (including informations like UserId, Contact and Description).</remarks>
        public static void WriteTraceAndInfo(Exception exception)
        {
            var throwable = Java.Lang.Throwable.FromException(exception);
            ExceptionHandler.SaveException(new Java.Lang.Throwable(throwable), _listener);
        }

    }
}
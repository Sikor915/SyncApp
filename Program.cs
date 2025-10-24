using System.Runtime.InteropServices;
using Timer = System.Timers.Timer;

namespace SyncApp
{
    class SyncApp
    {
        static Dictionary<string, FileStuff> sourceFiles = new Dictionary<string, FileStuff>();
        static Dictionary<string, FileStuff> targetFiles = new Dictionary<string, FileStuff>();

        static string source = "";
        static string target = "";
        static string log = "";

        static Timer intervalTimer;
        public static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: SyncApp <sourceDirectory> <targetDirectory> <logDirectory> <intervalInMinutes>");
                return;
            }
            source = args[0];
            target = args[1];
            log = args[2];
            if (!int.TryParse(args[3], out int intervalInMinutes))
            {
                Console.WriteLine("Invalid interval specified. Please provide a valid number.");
                return;
            }

            if (intervalInMinutes <= 0)
            {
                Console.WriteLine("Interval must be a positive integer.");
                return;
            }

            Console.WriteLine("Welcome to SyncApp!");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsSync();
                SetUpTimer(intervalInMinutes, true);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                LinuxSync();
                SetUpTimer(intervalInMinutes, false);
            }

            Console.WriteLine("The application will run in the background. Press Enter to exit.");
            Console.ReadLine();
        }

        static void WindowsSync()
        {
            Console.WriteLine("Syncing on Windows...");

            if (!Path.Exists(log))
            {
                Directory.CreateDirectory(log);
            }
            
            string logFilePath = Path.Combine(log, "sync_" + DateTime.Now.ToString("yyyyMMdd") + ".log");
            File.AppendAllText(logFilePath, "-----------------\nSync started at " + DateTime.Now + "\n");

            if (!Path.Exists(source) || !Path.Exists(target))
            {
                Console.WriteLine("One or more specified directories do not exist.");
                File.AppendAllText(logFilePath, "One or more specified directories do not exist.\n");

                return;
            }
            
            DirectoryInfo sourceDir = new(source);
            DirectoryInfo targetDir = new(target);

            foreach (var file in sourceDir.GetFiles("*", SearchOption.AllDirectories))
            {
                Console.WriteLine("Processing source file: {0}", file.FullName);
                File.AppendAllText(logFilePath, "Processing source file: {file.FullName}\n");

                string relativePath = Path.GetRelativePath(source, file.FullName);
                sourceFiles[relativePath] = new FileStuff(relativePath, file);
            }

            foreach (var file in targetDir.GetFiles("*", SearchOption.AllDirectories))
            {
                Console.WriteLine("Processing target file: {0}", file.FullName);
                File.AppendAllText(logFilePath, $"Processing target file: {file.FullName}\n");

                string relativePath = Path.GetRelativePath(target, file.FullName);
                targetFiles[relativePath] = new FileStuff(relativePath, file);
            }

            RemoveEntries(logFilePath);

            UpdateEntries(logFilePath);
        }

        static void LinuxSync()
        {
            Console.WriteLine("Syncing on Linux...");

            if (!Path.Exists(log))
            {
                Directory.CreateDirectory(log);
            }
            
            string logFilePath = Path.Combine(log, "sync_" + DateTime.Now.ToString("yyyyMMdd") + ".log");
            File.AppendAllText(logFilePath, "-----------------\nSync started at " + DateTime.Now + "\n");

            if (!Path.Exists(source) || !Path.Exists(target))
            {
                Console.WriteLine("One or more specified directories do not exist.");
                File.AppendAllText(logFilePath, "One or more specified directories do not exist.\n");

                return;
            }
            
            DirectoryInfo sourceDir = new(source);
            DirectoryInfo targetDir = new(target);

            foreach (var file in sourceDir.GetFiles("*", SearchOption.AllDirectories))
            {
                Console.WriteLine("Processing source file: {0}", file.FullName);
                File.AppendAllText(logFilePath, $"Processing source file: {file.FullName}\n");

                string relativePath = Path.GetRelativePath(source, file.FullName);
                sourceFiles[relativePath] = new FileStuff(relativePath, file);
            }

            foreach (var file in targetDir.GetFiles("*", SearchOption.AllDirectories))
            {
                Console.WriteLine("Processing target file: {0}", file.FullName);
                File.AppendAllText(logFilePath, $"Processing target file: {file.FullName}\n");

                string relativePath = Path.GetRelativePath(target, file.FullName);
                targetFiles[relativePath] = new FileStuff(relativePath, file);
            }

            RemoveEntries(logFilePath);

            UpdateEntries(logFilePath);
        }

        static void RemoveEntries(string logPath)
        {
            foreach (var targetEntry in targetFiles)
            {
                if (!sourceFiles.ContainsKey(targetEntry.Key))
                {
                    Console.WriteLine("Deleting file: {0}", targetEntry.Key);
                    File.AppendAllText(logPath, $"Deleting file: {targetEntry.Key}\n");

                    string targetFilePath = Path.Combine(target, targetEntry.Key);
                    File.Delete(targetFilePath);
                }
            }
        }

        static void UpdateEntries(string logPath)
        {
            foreach (var sourceEntry in sourceFiles)
            {
                if (targetFiles.TryGetValue(sourceEntry.Key, out FileStuff targetFileStuff))
                {
                    if (sourceEntry.Value.File.Length != targetFileStuff.File.Length ||
                        sourceEntry.Value.File.LastWriteTimeUtc != targetFileStuff.File.LastWriteTimeUtc)
                    {
                        Console.WriteLine("Updating file: {0}", sourceEntry.Key);
                        File.AppendAllText(logPath, $"Updating file: {sourceEntry.Key}\n");

                        string targetFilePath = Path.Combine(target, sourceEntry.Key);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
                        sourceEntry.Value.File.CopyTo(targetFilePath, true);
                    }
                }
                else
                {
                    Console.WriteLine("Copying new file: {0}", sourceEntry.Key);
                    File.AppendAllText(logPath, $"Copying new file: {sourceEntry.Key}\n");

                    string targetFilePath = Path.Combine(target, sourceEntry.Key);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
                    sourceEntry.Value.File.CopyTo(targetFilePath);
                }
            }
        }

        static void SetUpTimer(int intervalInMinutes, bool isWindows)
        {
            intervalTimer = new Timer(intervalInMinutes * 60 * 1000);
            intervalTimer.Elapsed += (sender, e) => OnIntervalEvent(isWindows);
            intervalTimer.AutoReset = true;
            intervalTimer.Enabled = true;
        }

        static void OnIntervalEvent(bool isWindows)
        {
            Console.WriteLine("Interval elapsed, performing sync...");

            if (isWindows)
            {
                WindowsSync();
            }
            else
            {
                LinuxSync();
            }
        }
    }
    class FileStuff
    {
        public string Path { get; set; }
        public FileInfo File { get; set; } 

        public FileStuff(string path, FileInfo file)
        {
            Path = path.Replace('/', '\\');
            File = file;
        }
    }
}
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace SyncApp
{
    class SyncApp
    {
        Dictionary<string, FileStuff> sourceFiles = new Dictionary<string, FileStuff>();
        Dictionary<string, FileStuff> targetFiles = new Dictionary<string, FileStuff>();
        public void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: SyncApp <sourceDirectory> <targetDirectory> <logDirectory>");
                return;
            }
            string sourceDirectory = args[0];
            string targetDirectory = args[1];
            string logDirectory = args[2];
            Console.WriteLine("Welcome to SyncApp!");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsSync(sourceDirectory, targetDirectory, logDirectory);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                LinuxSync(sourceDirectory, targetDirectory, logDirectory);
            }
        }

        void WindowsSync(string source, string target, string log)
        {
            Console.WriteLine("Syncing on Windows...");

            if (!Path.Exists(source) || !Path.Exists(target) || !Path.Exists(log))
            {
                Console.WriteLine("One or more specified directories do not exist.");
                return;
            }
            
            DirectoryInfo sourceDir = new(source);
            DirectoryInfo targetDir = new(target);

            foreach (var file in sourceDir.GetFiles("*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(source, file.FullName);
                sourceFiles[relativePath] = new FileStuff(relativePath, file);
            }

            foreach (var file in targetDir.GetFiles("*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(target, file.FullName);
                targetFiles[relativePath] = new FileStuff(relativePath, file);
            }
        }

        void LinuxSync(string source, string target, string log)
        {
            Console.WriteLine("Syncing on Linux...");
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
using System;

namespace SyncApp
{
    class SyncApp
    {
        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: SyncApp <sourceDirectory> <targetDirectory> <logDirectory>");
                return;
            }
            Console.WriteLine("Welcome to SyncApp!");
            // Application logic goes here
        }
    }
}
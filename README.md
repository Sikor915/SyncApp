# SyncApp

Usage: 
- Compile Program.cs with dotnet build \<path\>
- Run ./SyncApp \<path_to_source_folder\> \<path_to_target_folder\> \<path_to_log_folder\> \<Interval in minutes\>

The program will then perform one-way sync between source and target, logging everything to console and file in log path. After that, the timer will start performing the sync every given interval.
Works on both Windows and Ubuntu

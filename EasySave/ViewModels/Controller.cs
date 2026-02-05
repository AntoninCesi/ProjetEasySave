using System;
using System.Collections.Generic;
using System.Linq;
using EasySave.ExecutionManagement;
using EasySave.View;
using EasySave.Messaging;
using EasySave.Models;
using EasySave.StateManagement;
using EasySave.Services;

namespace EasySave.ViewModels
{
    public class Controller
    {
        private const int MAX_JOBS = 5; // Limit imposed by specifications

        private BackupExecutionManager backupManager;
        private ConsoleUI console;
        private MessageProvider messageProvider;
        private List<BackupJob> jobs;

        public Controller(string[] args)
        {
            // Initialize dependencies
            messageProvider = new MessageProvider(false); // false = English
            console = new ConsoleUI(messageProvider);

            var stateManager = new BackupStateManager();
            var logService = new LogService();
            backupManager = new BackupExecutionManager(stateManager, logService);

            jobs = new List<BackupJob>();

            if (args.Length == 0)
            {
                this.CreateUI();
            }
            else if (args.Length > 0)
            {
                ProcessCommandLineArgs(args[0]);
            }
        }

        private void ProcessCommandLineArgs(string arg)
        {
            try
            {
                // Expected format: "1-3" or "1;3"
                if (arg.Contains('-'))
                {
                    // Sequential execution: 1-3 = jobs 1, 2, 3
                    string[] parts = arg.Split('-');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int start) &&
                        int.TryParse(parts[1], out int end))
                    {
                        List<int> jobsToExecute = new List<int>();
                        for (int i = start; i <= end; i++)
                        {
                            if (IsBackupJobExist(i))
                            {
                                jobsToExecute.Add(i);
                            }
                            else
                            {
                                console.Display(new Message(MessageType.JobNotFoundWithId, i));
                            }
                        }

                        if (jobsToExecute.Count > 0)
                        {
                            ExecuteBackupJob(jobsToExecute.ToArray());
                        }
                    }
                    else
                    {
                        console.Display(new Message(MessageType.InvalidFormat));
                    }
                }
                else if (arg.Contains(';'))
                {
                    // Execution of specific jobs: 1;3 = jobs 1 and 3
                    string[] parts = arg.Split(';');
                    List<int> jobsToExecute = new List<int>();

                    foreach (string part in parts)
                    {
                        if (int.TryParse(part.Trim(), out int jobId))
                        {
                            if (IsBackupJobExist(jobId))
                            {
                                jobsToExecute.Add(jobId);
                            }
                            else
                            {
                                console.Display(new Message(MessageType.JobNotFoundWithId, jobId));
                            }
                        }
                    }

                    if (jobsToExecute.Count > 0)
                    {
                        ExecuteBackupJob(jobsToExecute.ToArray());
                    }
                }
                else if (int.TryParse(arg, out int singleJob))
                {
                    // Single job execution
                    if (IsBackupJobExist(singleJob))
                    {
                        ExecuteBackupJob(new int[] { singleJob });
                    }
                    else
                    {
                        console.Display(new Message(MessageType.JobNotFoundWithId, singleJob));
                    }
                }
                else
                {
                    console.Display(new Message(MessageType.InvalidFormat));
                }
            }
            catch (Exception ex)
            {
                console.Display(new Message(MessageType.Error, ex.Message));
            }
        }

        public void CreateUI()
        {
            bool running = true;

            while (running)
            {
                console.ShowMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateNewJob();
                        break;
                    case "2":
                        ExecuteJobMenu();
                        break;
                    case "3":
                        ExecuteAllJobs();
                        break;
                    case "4":
                        ShowJobStates();
                        break;
                    case "5":
                        DeleteJobMenu();
                        break;
                    case "6":
                        console.Display(new Message(MessageType.Goodbye));
                        running = false;
                        break;
                    default:
                        console.Display(new Message(MessageType.InvalidChoice));
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void CreateNewJob()
        {
            Console.Clear();
            Console.WriteLine("=== Create a new backup job ===\n");

            // Check 5 jobs limit
            if (jobs.Count >= MAX_JOBS)
            {
                Console.WriteLine($"✗ Error: You have reached the limit of {MAX_JOBS} backup jobs.");
                console.Display(new Message(MessageType.BackToMenu));
                Console.ReadKey();
                return;
            }

            Console.Write("Job name: ");
            string name = Console.ReadLine();

            // Check that name doesn't already exist
            if (jobs.Any(j => j.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"✗ Error: A job named '{name}' already exists.");
                console.Display(new Message(MessageType.BackToMenu));
                Console.ReadKey();
                return;
            }

            Console.Write("Source path (e.g., C:\\Source or \\\\server\\share): ");
            string source = Console.ReadLine();

            Console.Write("Destination path (e.g., C:\\Backup or \\\\server\\backup): ");
            string dest = Console.ReadLine();

            Console.Write("Type (1=FULL, 2=DIFFERENTIAL): ");
            string typeChoice = Console.ReadLine();

            BackupType type = typeChoice == "1" ? BackupType.FULL : BackupType.DIFFERENTIAL;

            jobs.Add(new BackupJob
            {
                Name = name,
                SourcePath = source,
                DestinationPath = dest,
                Type = type
            });

            Console.WriteLine($"\n✓ Job '{name}' created successfully! ({jobs.Count}/{MAX_JOBS})");
            console.Display(new Message(MessageType.BackToMenu));
            Console.ReadKey();
        }

        private void ExecuteJobMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Execute a job ===\n");

            if (jobs.Count == 0)
            {
                Console.WriteLine("No jobs available. Create a job first.");
                console.Display(new Message(MessageType.BackToMenu));
                Console.ReadKey();
                return;
            }

            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jobs[i].Name} ({jobs[i].Type})");
            }

            Console.Write("\nJob number to execute: ");
            if (int.TryParse(Console.ReadLine(), out int jobId) && jobId > 0 && jobId <= jobs.Count)
            {
                ExecuteBackupJob(new int[] { jobId });
            }
            else
            {
                console.Display(new Message(MessageType.JobNotFound));
            }

            console.Display(new Message(MessageType.BackToMenu));
            Console.ReadKey();
        }

        private void ExecuteAllJobs()
        {
            Console.Clear();
            Console.WriteLine("=== Sequential execution of all jobs ===\n");

            if (jobs.Count == 0)
            {
                Console.WriteLine("No jobs available.");
                console.Display(new Message(MessageType.BackToMenu));
                Console.ReadKey();
                return;
            }

            int[] allJobIds = Enumerable.Range(1, jobs.Count).ToArray();
            ExecuteBackupJob(allJobIds);

            console.Display(new Message(MessageType.BackToMenu));
            Console.ReadKey();
        }

        private void ShowJobStates()
        {
            Console.Clear();
            Console.WriteLine("=== Job states ===\n");

            if (jobs.Count == 0)
            {
                Console.WriteLine("No jobs created.");
            }
            else
            {
                for (int i = 0; i < jobs.Count; i++)
                {
                    var job = jobs[i];
                    Console.WriteLine($"[{i + 1}] Job: {job.Name}");
                    Console.WriteLine($"    Status: {job.Status}");
                    Console.WriteLine($"    Type: {job.Type}");
                    Console.WriteLine($"    Source: {job.SourcePath}");
                    Console.WriteLine($"    Destination: {job.DestinationPath}");
                    Console.WriteLine($"    Progress: {job.Progress}%");
                    Console.WriteLine();
                }
            }

            console.Display(new Message(MessageType.BackToMenu));
            Console.ReadKey();
        }

        private void DeleteJobMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Delete a job ===\n");

            if (jobs.Count == 0)
            {
                Console.WriteLine("No jobs available.");
                console.Display(new Message(MessageType.BackToMenu));
                Console.ReadKey();
                return;
            }

            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jobs[i].Name}");
            }

            Console.Write("\nJob number to delete: ");
            if (int.TryParse(Console.ReadLine(), out int jobId) && jobId > 0 && jobId <= jobs.Count)
            {
                string jobName = jobs[jobId - 1].Name;
                jobs.RemoveAt(jobId - 1);
                Console.WriteLine($"\n✓ Job '{jobName}' deleted! ({jobs.Count}/{MAX_JOBS} remaining)");
            }
            else
            {
                console.Display(new Message(MessageType.JobNotFound));
            }

            console.Display(new Message(MessageType.BackToMenu));
            Console.ReadKey();
        }

        private bool IsBackupJobExist(int jobId)
        {
            return jobId > 0 && jobId <= jobs.Count;
        }

        private void ExecuteBackupJob(int[] jobIds)
        {
            foreach (int id in jobIds)
            {
                if (id > 0 && id <= jobs.Count)
                {
                    var job = jobs[id - 1];
                    console.Display(new Message(MessageType.JobStarted, job.Name));
                    backupManager.ExecuteJob(job);
                }
            }
        }
    }
}
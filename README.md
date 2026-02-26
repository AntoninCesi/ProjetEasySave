# EasySave

A professional backup management application with real-time control, parallel execution, and Docker logging integration.

## Overview

EasySave is a backup solution designed for Windows that provides both full and differential backup strategies with advanced features including pause/resume/stop controls, file encryption, priority-based parallel execution, and remote logging capabilities.

## Features

### Core Functionality
- **Full Backup**: Complete copy of all files from source to destination
- **Differential Backup**: Copy only modified files since last backup
- **Multi-Job Management**: Create and manage multiple backup configurations
- **Real-Time Progress Tracking**: Live updates on files copied, size, and transfer speed

### Advanced Controls
- **Pause/Resume**: Suspend and continue backups instantly
- **Stop**: Cancel running backups with automatic cleanup of incomplete files
- **Selective Execution**: Choose which jobs to run via checkboxes
- **Parallel Execution**: Run multiple backups simultaneously

### Performance Features
- **Priority Files**: Configure file extensions to transfer first
- **Large File Management**: Only one large file transfers at a time to prevent saturation
- **Interruptible Copy**: Response time under 1 second for pause/stop operations
- **Block-Level Operations**: 80KB block size for optimal performance

### Security & Monitoring
- **Selective Encryption**: Automatically encrypt specified file types
- **Business Software Detection**: Pause backups when critical software is running
- **Comprehensive Logging**: Track all operations with timestamps and details

### Logging System
- **Multiple Formats**: JSON or XML log output
- **Local Storage**: Save logs to local file system
- **Docker Integration**: Send logs to remote Docker container via TCP
- **Asynchronous Logging**: Non-blocking log transmission with automatic retry

### User Interface
- **Multi-Language Support**: English and French 
- **Modern WPF Interface**: Clean, professional design with rounded buttons
- **Settings Management**: Configure all options from a central settings window
- **Progress Windows**: Individual progress tracking for each backup job

## Architecture

### Project Structure
```
ProjetEasySave/
├── EasySave/                 # Core library
│   ├── Models/               # Domain models
│   ├── Strategies/           # Backup strategies
│   ├── ExecutionManagement/  # Job orchestration
│   ├── Services/             # Business logic services
│   ├── Observer/             # Progress tracking
│   └── Factory/              # Object creation
├── EasySave.GUI/             # WPF interface
│   ├── ViewModels/           # MVVM view models
│   ├── Views/                # XAML windows
│   └── Commands/             # UI commands
└── EasyLog/                  # Logging library
    └── Sinks/                # Log destinations
```

### Technologies
- .NET 10 / 8
- C# 12
- WPF (Windows Presentation Foundation)
- PlantUML for documentation

## Getting Started

### Prerequisites
- Windows 10 or later
- .NET 10.0 Runtime
- .NET 8.0 Runtime
### Installation

1. Download the latest release
2. Extract the archive
3. Run `EasySave.GUI.exe` for graphical interface or `EasySave.exe` for console

### Basic Usage

#### Creating a Backup Job

1. Launch the application
2. Click **Create Job**
3. Enter job name, source path, and destination path
4. Select backup type (Full or Differential)
5. Click **Create**

#### Running Backups

1. Check the jobs you want to run (all are selected by default)
2. Click **Run Selected**
3. Monitor progress in the individual progress windows
4. Use **Pause**, **Resume**, or **Stop** buttons as needed

#### Configuring Settings

1. Click **Settings**
2. Configure options:
   - **Log Format**: JSON or XML
   - **Encryption Extensions**: File types to encrypt (e.g., `.docx,.xlsx`)
   - **Business Software**: Process name to monitor
   - **Priority Extensions**: File types to transfer first
   - **Max File Size**: Threshold for large files (in KB)
   - **Log Destination**: Local, Docker, or Both
   - **Docker Configuration**: Host and port for remote logging
3. Click **Save**

## Configuration


## Docker Logging

### Setup Docker Server

1. Start a TCP server listening on the configured port
2. The application will send logs in the format: `DATE|EXTENSION|JSON_PAYLOAD`
3. Configure the host and port in Settings

### Log Format Example

```
2026-02-26|.json|{"Type":"FileCopied","Timestamp":"2026-02-26T14:30:45","JobName":"Documents","SourceFile":"C:\\file.txt",...}
```

### Features

- **Non-Blocking**: Logs are queued and sent by a background thread
- **Automatic Retry**: Failed logs are re-queued and resent
- **Auto-Reconnect**: Automatically reconnects if connection is lost
- **Thread-Safe**: Multiple backup jobs can log simultaneously



## Performance

- **Copy Speed**: Limited by disk I/O and network bandwidth
- **Parallel Jobs**: Configurable via priority extensions and file size limits
- **Memory Usage**: ~80KB buffer per active file transfer
- **CPU Usage**: Minimal, mostly I/O bound

## Limitations

- Windows-only (GUI requires WPF)
- Encryption uses external CryptoSoft service
- Business software detection by process name only
- No incremental backup (only full and differential)


## License

This project is an educational application developed.

## Authors

Yudracknight (Jennifer) , Luckas , Adem , Kemo (Hakam) and Antonin


---

**Version**: 3.0  
**Last Updated**: February 2026

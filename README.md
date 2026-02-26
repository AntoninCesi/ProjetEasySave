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

### Design Patterns
- **MVVM**: Model-View-ViewModel pattern for clean UI separation
- **Strategy**: Pluggable backup algorithms
- **Observer**: Real-time progress notifications
- **Factory**: Strategy creation and dependency injection
- **Singleton**: Shared services and configuration

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

Settings are stored in `%APPDATA%\EasySave\settings.json`

### Example Configuration

```json
{
  "LogFormat": "JSON",
  "EncryptionExtensions": ".docx,.xlsx,.pptx",
  "BusinessSoftware": "CalculatorApp",
  "Language": "en-US",
  "PriorityExtensions": [".pdf", ".docx"],
  "MaxParallelFileSizeKo": 10240,
  "LogDestination": "Both",
  "DockerHost": "localhost",
  "DockerPort": 5000
}
```

### Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `LogFormat` | Log file format | JSON |
| `EncryptionExtensions` | Comma-separated file extensions to encrypt | .docx,.xlsx,.pptx |
| `BusinessSoftware` | Process name to detect (pauses backup) | CalculatorApp |
| `Language` | UI language (en-US or fr-FR) | en-US |
| `PriorityExtensions` | File extensions to transfer first | [] |
| `MaxParallelFileSizeKo` | Large file threshold in kilobytes | 0 (disabled) |
| `LogDestination` | Where to send logs (Local/Docker/Both) | Local |
| `DockerHost` | Docker server hostname | localhost |
| `DockerPort` | Docker server TCP port | 5000 |

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

## Technical Details

### Parallel Execution Rules

When running multiple jobs in parallel:

1. **Priority Files First**: Files with priority extensions transfer before others across all jobs
2. **Single Large File**: Only one large file (above threshold) transfers at a time
3. **Independent Control**: Each job can be paused/resumed/stopped independently

### Interruptible Copy

Files are copied in 80KB blocks with pause/stop checks between each block:
- Ensures response time under 1 second
- Automatically cleans up incomplete files on stop
- Uses `CancellationToken` and `ManualResetEventSlim` for control

### Encryption

Files matching specified extensions are automatically encrypted using CryptoSoft service before being saved to the destination.

## Logs

Logs are written to `Logs/` directory (when local logging is enabled) with one file per day:
- `2026-02-26.json` or `2026-02-26.xml`

Log entries include:
- Timestamp
- Job name
- Source and destination paths
- File size
- Transfer time
- Event type (FileCopied, JobStarted, JobCompleted, Error)

## Troubleshooting

### Backup Won't Start

**Issue**: Nothing happens when clicking "Run Selected"  
**Solution**: Check if business software is running. Close it or change the setting.

### Pause/Stop Not Responding

**Issue**: Backup continues after clicking pause/stop  
**Solution**: If copying a very large file, wait up to 1 second. Encryption operations are not interruptible.

### Docker Logs Not Appearing

**Issue**: Logs sent locally but not to Docker  
**Solution**: 
- Verify Docker server is running
- Check host and port configuration
- Look for connection errors in local logs

### Files Remain Locked After Stop

**Issue**: Cannot delete or modify backup destination files  
**Solution**: The application properly closes all handles. Check for antivirus interference.

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

## Future Improvements

- Incremental backup strategy
- Cloud storage destinations (Azure, AWS, Google Cloud)
- Email notifications
- Backup scheduling
- Bandwidth throttling
- Dark mode UI

## License

This project is an educational application developed as part of software engineering coursework.

## Authors

Yudracknight (Jennifer) , Luckas , Adem , Kemo (Hakam) and Antonin

## Support

For questions or issues, please contact your project supervisor or refer to the technical documentation in the `Documentation/` folder.

---

**Version**: 3.0  
**Last Updated**: February 2026

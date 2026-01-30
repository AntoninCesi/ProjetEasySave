using EasySave.Models;
using EasySave.Services;
using System.Diagnostics;

namespace EasySave.ViewModels;

public class MainViewModel
{
    private readonly List<BackupJob> _jobs = new();
    private readonly LogService _logService = new();
    private readonly StateService _stateService = new();
}
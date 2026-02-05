using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave.Models;

public class BackupJob
{
	public string Name { get; set; } = string.Empty;
	public string SourcePath { get; set; } = string.Empty;
	public string TargetPath { get; set; } = string.Empty;
	public BackupType Type { get; set; }
}

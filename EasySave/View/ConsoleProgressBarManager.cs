using EasySave.Models;
using System;

/// <summary>
/// Gère l'affichage des barres de progression en bas de la fenêtre console,
/// sans écraser la zone menu / saisie utilisateur située au-dessus.
/// </summary>
public class ConsoleProgressBarManager
{

    private readonly object _consoleLock;
    private readonly int _barCount;
    private readonly int _barWidth;

    /// <summary>Ligne à partir de laquelle commencent les barres (calculée dynamiquement).</summary>
    public int BarStartLine => Console.WindowHeight - _barCount - 1;

    /// <param name="barCount">Nombre maximum de barres (= nombre maximum de jobs).</param>
    /// <param name="barWidth">Largeur en caractères de la partie [###---] de la barre.</param>
    /// <param name="consoleLock">
    ///     Lock partagé avec ConsoleUI pour éviter les conflits d'accès à la console.
    ///     Si null, un lock privé est créé (mais les conflits avec ConsoleUI resteront possibles).
    /// </param>
    public ConsoleProgressBarManager(int barCount, int barWidth = 40, object? consoleLock = null)
    {
        _barCount = barCount;
        _barWidth = barWidth;
        _consoleLock = consoleLock ?? new object();
    }

    /// <summary>
    /// Réserve les lignes du bas de la fenêtre en les remplissant d'espaces.
    /// À appeler après chaque Console.Clear() dans le menu.
    /// </summary>
    public void ReserveLines()
    {
        lock (_consoleLock)
        {
            // Sauvegarder la position courante
            int savedLeft = Console.CursorLeft;
            int savedTop = Console.CursorTop;

            for (int i = 0; i < _barCount; i++)
            {
                int line = BarStartLine + i;
                if (line < 0 || line >= Console.WindowHeight) continue;

                Console.SetCursorPosition(0, line);
                Console.Write(new string(' ', Console.WindowWidth - 1));
            }

            // Tracer un séparateur visuel juste au-dessus des barres
            int separatorLine = BarStartLine - 1;
            if (separatorLine >= 0)
            {
                Console.SetCursorPosition(0, separatorLine);
                Console.Write(new string('─', Console.WindowWidth - 1));
            }

            // Restaurer la position pour ne pas perturber l'affichage du menu
            Console.SetCursorPosition(savedLeft, Math.Min(savedTop, BarStartLine - 2));
        }
    }

    /// <summary>
    /// Met à jour la barre de progression d'un job spécifique.
    /// Thread-safe : peut être appelé depuis n'importe quel thread.
    /// </summary>
    /// <param name="jobIndex">Index du job (0-based).</param>
    /// <param name="progress">Informations de progression du job.</param>
    public void UpdateProgress(int jobIndex, FileInfos progress)
    {
        if (jobIndex < 0 || jobIndex >= _barCount) return;

        lock (_consoleLock)
        {
            int targetLine = BarStartLine + jobIndex;
            if (targetLine < 0 || targetLine >= Console.WindowHeight) return;

            // ── Sauvegarde de la position du curseur ──────────────────────────
            // C'est la clé : on restaure après l'écriture pour que la saisie
            // utilisateur (Console.ReadLine) ne soit pas perturbée.
            int savedLeft = Console.CursorLeft;
            int savedTop = Console.CursorTop;

            try
            {
                Console.SetCursorPosition(1, targetLine);

                // ── Calcul du pourcentage ─────────────────────────────────────
                double percent = (progress.FilesSaved *100)/ progress.TotalSize;

                // ── Barre graphique ───────────────────────────────────────────
                int filled = (int)(_barWidth * percent);
                int empty = _barWidth - filled;
                string bar = $"[{new string('#', filled)}{new string('-', empty)}]";

                // ── Informations textuelles ───────────────────────────────────tt
                //string jobName = (progress.JobName ?? $"Job {jobIndex}").PadRight(15).Substring(0, 15); 
                string pct = $"{percent * 100,5:F1}%";
                string files = $"{progress.FilesSaved}/{progress.TotalSize} fichiers";
                string size = FormatSize(progress.TotalSize);
                string elapsed = progress.TotalBackupTime.ToString(@"mm\:ss");

                string line = $" {pct} {bar}  {files}  {size}  {elapsed} ";

                // Tronquer ou padder pour ne jamais déborder de la largeur de la fenêtre
                int maxWidth = Console.WindowWidth - 1;
                if (line.Length > maxWidth)
                    line = line.Substring(0, maxWidth);
                else
                    line = line.PadRight(maxWidth);

                Console.Write(line);
            }
            finally
            {
                // ── Restauration du curseur ───────────────────────────────────
                // Toujours exécuté, même en cas d'exception, pour ne jamais
                // laisser le curseur coincé dans la zone des barres.
                Console.SetCursorPosition(savedLeft, savedTop);
            }
        }
    }

    private static string FormatSize(long bytes)
    {
        if (bytes >= 1_073_741_824) return $"{bytes / 1_073_741_824.0:F1} GB";
        if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:F1} MB";
        if (bytes >= 1_024) return $"{bytes / 1_024.0:F1} KB";
        return $"{bytes} B";
    }
}

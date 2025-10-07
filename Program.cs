using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MusicRenamer
{
    class Program
    {
        // zbieramy nieudane operacje, na końcu zapis do pliku
        static readonly List<string> failedRenames = new List<string>();

        static void Main(string[] args)
        {
            RenameFiles();

            // zapis logu (tylko jeśli są błędy)
            if (failedRenames.Count > 0)
            {
                string logPath = Path.Combine(Directory.GetCurrentDirectory(), "RenameFailures.txt");
                try
                {
                    File.WriteAllLines(logPath, failedRenames);
                    Console.WriteLine($"Zapisano listę nieudanych zmian do pliku: {logPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Nie udało się zapisać logu: {ex.Message}");
                }
            }
        }

        // proste przenoszenie z kilkoma próbami i liniowym backoffem 120ms * próba
        static void MoveFileWithRetry(string from, string to, int maxAttempts = 5)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    File.Move(from, to);
                    return;
                }
                catch (IOException) when (++attempts <= maxAttempts)
                {
                    System.Threading.Thread.Sleep(120 * attempts);
                }
                catch (UnauthorizedAccessException) when (attempts++ < maxAttempts)
                {
                    System.Threading.Thread.Sleep(120 * attempts);
                }
            }
        }

        // Rename odporne na case-only + rollback i logowanie na błąd
        static void SafeRename(string srcPath, string dstPath)
        {
            bool equalIgnoreCase = string.Equals(srcPath, dstPath, StringComparison.OrdinalIgnoreCase);

            try
            {
                // Jeżeli to zmiana na kompletnie inną nazwę i plik docelowy już istnieje — pomijamy (nie nadpisujemy)
                if (!equalIgnoreCase && File.Exists(dstPath))
                    return;

                if (equalIgnoreCase && srcPath != dstPath)
                {
                    // CASE-ONLY: dwa kroki przez nazwę tymczasową + rollback przy błędzie
                    string dir = Path.GetDirectoryName(srcPath) ?? "";
                    string tmp = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".tmp");

                    try
                    {
                        // 1) do nazwy tymczasowej
                        MoveFileWithRetry(srcPath, tmp);

                        try
                        {
                            // 2) do nazwy docelowej
                            MoveFileWithRetry(tmp, dstPath);
                        }
                        catch
                        {
                            // rollback do starej nazwy (żeby nie zostać z GUID-em)
                            try { if (File.Exists(tmp) && !File.Exists(srcPath)) MoveFileWithRetry(tmp, srcPath); } catch { }
                            failedRenames.Add($"FILE (case-only): {srcPath} -> {dstPath}");
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        failedRenames.Add($"FILE: {srcPath} -> {dstPath} | {ex.Message}");
                        throw;
                    }
                }
                else
                {
                    // Zwykła zmiana nazwy (różna także ignorując case)
                    try
                    {
                        MoveFileWithRetry(srcPath, dstPath);
                    }
                    catch (Exception ex)
                    {
                        failedRenames.Add($"FILE: {srcPath} -> {dstPath} | {ex.Message}");
                        throw;
                    }
                }
            }
            catch
            {
                // błąd już zarejestrowany; kontynuujemy pętlę wyżej
            }
        }

        public static void RenameFiles()
        {
            Regex regex1 = new Regex(@"^\d\d\s[\w* [(\.]+");
            Regex regex2 = new Regex(@"^\d\d\.\s[\w* [(\.]+");
            Regex regex3 = new Regex(@"^\d\d\-[\w* [(\.]+");
            Regex regex4 = new Regex(@"^\d\d\-\s[\w* [(\.]+");
            Regex regex5 = new Regex(@"^\d\d\.[\w* [(\.]+");

            Regex regex1a = new Regex(@"^\d\s[\w* [(\.]+");
            Regex regex2b = new Regex(@"^\d\.\s[\w* [(\.]+");
            Regex regex3c = new Regex(@"^\d\-[\w* [(\.]+");
            Regex regex4d = new Regex(@"^\d\-\s[\w* [(\.]+");
            Regex regex5e = new Regex(@"^\d\.[\w* [(\.]+");

            string root = Directory.GetCurrentDirectory();
            var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                string ext = Path.GetExtension(filePath);
                if (!ext.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                    continue;

                string dir = Path.GetDirectoryName(filePath) + Path.DirectorySeparatorChar;
                string fileName = Path.GetFileName(filePath);

                string fileNameRenamed = fileName;

                // popraw prefiks na "NN - "
                if (regex1.IsMatch(fileName))
                    fileNameRenamed = fileName.Substring(0, 2) + " - " + fileName.Substring(3);
                else if (regex2.IsMatch(fileName))
                    fileNameRenamed = fileName.Substring(0, 2) + " - " + fileName.Substring(4);
                else if (regex3.IsMatch(fileName))
                    fileNameRenamed = fileName.Substring(0, 2) + " - " + fileName.Substring(3);
                else if (regex4.IsMatch(fileName))
                    fileNameRenamed = fileName.Substring(0, 2) + " - " + fileName.Substring(4);
                else if (regex5.IsMatch(fileName))
                    fileNameRenamed = fileName.Substring(0, 2) + " - " + fileName.Substring(3);
                else if (regex1a.IsMatch(fileName))
                    fileNameRenamed = "0" + fileName.Substring(0, 1) + " - " + fileName.Substring(2);
                else if (regex2b.IsMatch(fileName))
                    fileNameRenamed = "0" + fileName.Substring(0, 1) + " - " + fileName.Substring(3);
                else if (regex3c.IsMatch(fileName))
                    fileNameRenamed = "0" + fileName.Substring(0, 1) + " - " + fileName.Substring(2);
                else if (regex4d.IsMatch(fileName))
                    fileNameRenamed = "0" + fileName.Substring(0, 1) + " - " + fileName.Substring(3);
                else if (regex5e.IsMatch(fileName))
                    fileNameRenamed = "0" + fileName.Substring(0, 1) + " - " + fileName.Substring(2);

                // zachowaj wymuszone .mp3 po substringach
                fileNameRenamed = Path.ChangeExtension(fileNameRenamed, ".mp3");

                string srcPath = filePath;
                string dstPath = Path.Combine(dir, fileNameRenamed);

                if (string.Equals(srcPath, dstPath, StringComparison.Ordinal))
                    continue;

                SafeRename(srcPath, dstPath); // ta z retry/rollback/logiem
            }
        }
    }
}

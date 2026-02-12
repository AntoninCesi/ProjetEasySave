using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Services
{
    /// <summary>
    /// CryptoSoft service for encrypting files using multi-threading.
    /// Implements XOR encryption with parallel processing for performance.
    /// </summary>
    public class CryptoSoftService
    {
        private static readonly object _lockObject = new object();
        private const int CHUNK_SIZE = 1024 * 64; // 64 KB chunks

        /// <summary>
        /// Encrypts a file using XOR algorithm with multi-threading
        /// </summary>
        /// <param name="sourceFile">Path to the source file</param>
        /// <param name="destinationFile">Path to save encrypted file</param>
        /// <param name="key">Encryption key</param>
        /// <returns>True if encryption succeeded, false otherwise</returns>
        public static bool EncryptFile(string sourceFile, string destinationFile, string key = "DefaultKey2025")
        {
            try
            {
                Console.WriteLine($"[CryptoSoft] Starting encryption: {Path.GetFileName(sourceFile)}");

                // Validate input
                if (!File.Exists(sourceFile))
                {
                    Console.WriteLine($"[CryptoSoft] ERROR: Source file not found: {sourceFile}");
                    return false;
                }

                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("[CryptoSoft] ERROR: Encryption key cannot be empty");
                    return false;
                }

                // Read entire file into memory
                byte[] fileData = File.ReadAllBytes(sourceFile);
                byte[] keyBytes = GenerateKeyBytes(key, fileData.Length);

                // Split data into chunks for parallel processing
                int processorCount = Environment.ProcessorCount;
                int chunkSize = Math.Max(CHUNK_SIZE, fileData.Length / processorCount);
                int chunkCount = (int)Math.Ceiling((double)fileData.Length / chunkSize);

                Console.WriteLine($"[CryptoSoft] Using {processorCount} threads, {chunkCount} chunks");

                // Create array to hold encrypted data
                byte[] encryptedData = new byte[fileData.Length];

                // Process chunks in parallel using multiple threads
                Parallel.For(0, chunkCount, chunkIndex =>
                {
                    int startIndex = chunkIndex * chunkSize;
                    int endIndex = Math.Min(startIndex + chunkSize, fileData.Length);
                    int currentChunkSize = endIndex - startIndex;

                    // Simulate multi-process work (each thread represents a process)
                    Thread.Sleep(10); // Small delay to simulate real encryption work

                    // XOR encryption on this chunk
                    for (int i = 0; i < currentChunkSize; i++)
                    {
                        int fileIndex = startIndex + i;
                        encryptedData[fileIndex] = (byte)(fileData[fileIndex] ^ keyBytes[fileIndex]);
                    }

                    // Thread-safe logging
                    lock (_lockObject)
                    {
                        Console.WriteLine($"[CryptoSoft] Thread {Thread.CurrentThread.ManagedThreadId}: Processed chunk {chunkIndex + 1}/{chunkCount}");
                    }
                });

                // Ensure destination directory exists
                string? destDirectory = Path.GetDirectoryName(destinationFile);
                if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }

                // Write encrypted data to destination
                File.WriteAllBytes(destinationFile, encryptedData);

                Console.WriteLine($"[CryptoSoft] Encryption completed: {Path.GetFileName(destinationFile)}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CryptoSoft] ERROR during encryption: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Decrypts a file (XOR is symmetric, so same process as encryption)
        /// </summary>
        /// <param name="sourceFile">Path to encrypted file</param>
        /// <param name="destinationFile">Path to save decrypted file</param>
        /// <param name="key">Decryption key</param>
        /// <returns>True if decryption succeeded, false otherwise</returns>
        public static bool DecryptFile(string sourceFile, string destinationFile, string key = "DefaultKey2025")
        {
            // XOR is symmetric: encrypting twice returns original
            return EncryptFile(sourceFile, destinationFile, key);
        }

        /// <summary>
        /// Checks if a file extension should be encrypted based on user settings
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="encryptionExtensions">Comma-separated list of extensions (e.g., ".docx,.xlsx,.pdf")</param>
        /// <returns>True if file should be encrypted</returns>
        public static bool ShouldEncrypt(string filePath, string encryptionExtensions)
        {
            if (string.IsNullOrWhiteSpace(encryptionExtensions))
                return false;

            string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            
            // Parse extensions from settings
            var extensions = encryptionExtensions
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim().ToLowerInvariant())
                .Select(ext => ext.StartsWith(".") ? ext : "." + ext);

            return extensions.Contains(fileExtension);
        }

        /// <summary>
        /// Generates a key byte array matching the length of the data
        /// </summary>
        private static byte[] GenerateKeyBytes(string key, int length)
        {
            byte[] keyBytes = new byte[length];
            byte[] originalKeyBytes = Encoding.UTF8.GetBytes(key);

            // Repeat key to match data length
            for (int i = 0; i < length; i++)
            {
                keyBytes[i] = originalKeyBytes[i % originalKeyBytes.Length];
            }

            return keyBytes;
        }

        /// <summary>
        /// Gets the encrypted file path (adds .encrypted extension)
        /// </summary>
        public static string GetEncryptedPath(string originalPath)
        {
            return originalPath + ".encrypted";
        }

        /// <summary>
        /// Removes .encrypted extension from file path
        /// </summary>
        public static string GetDecryptedPath(string encryptedPath)
        {
            if (encryptedPath.EndsWith(".encrypted", StringComparison.OrdinalIgnoreCase))
            {
                return encryptedPath.Substring(0, encryptedPath.Length - 10);
            }
            return encryptedPath;
        }
    }
}

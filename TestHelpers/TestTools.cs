using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mp3Player.Core.Extensions;
using NUnit.Framework;

// ReSharper disable UnusedMember.Global
namespace Mp3Player.TestHelpers
{
    /// <summary>
    /// The test tools.
    /// </summary>
    public class TestTools
    {
        private readonly List<string> messages;
        private readonly string testDirectoryRoot;
        private const string TEST_FIXTURE_PATH = "TestFixture";
        private const int INCREMENT_WAIT = 2;
        private const int MAX_IO_RETRY_COUNT = 5;

        /// <summary>
        /// The tmp file extension.
        /// </summary>
        public const string TMP_FILE_EXTENSION = ".tmp";

        /// <summary>
        /// Gets or sets a value indicating whether [show debug messages].
        /// </summary>
        public bool ShowDebugMessages { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTools"/> class.
        /// </summary>
        public TestTools()
        {
            ShowDebugMessages = true;
            messages = new List<string>();
            testDirectoryRoot = Environment.CurrentDirectory;
        }

        /// <summary>
        /// Create test path.
        /// </summary>
        public string CreateRootPathAndTestPath()
        {
            // 30.11.2021 MInsberg  : Try if test can write to the directory 
            var canWriteToDirectory = !IsDirectoryWritable(testDirectoryRoot);
            Assert.IsTrue(canWriteToDirectory, $"No rights to write to {testDirectoryRoot}");

            var rootPath = GetRootTestDirectory();
            // 25.11.2021 MInsberg  :  Create the root Directory this will never deleted by the tests
            if (!Directory.Exists(rootPath))
            {
                var rootPathResult = Directory.CreateDirectory(rootPath);
                Assert.IsTrue(rootPathResult.Exists, $"{rootPathResult} does not exists");
            }

            // 25.11.2021 MInsberg  : Create one unique directory per test  
            var path = Path.Combine(rootPath, $"{Guid.NewGuid()}");
            while (Directory.Exists(path))
            {
                path = Path.Combine(rootPath, $"{Guid.NewGuid()}");
            }
            var result = Directory.CreateDirectory(path);
            Assert.IsTrue(result.Exists, $"{path} does not exists");
            return path;
        }

        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationFile">The destination file.</param>
        public void CopyFile(string sourceFile, string destinationFile)
        {
            Assert.IsTrue(File.Exists(sourceFile));
            File.Copy(sourceFile, destinationFile);
            Assert.IsTrue(File.Exists(destinationFile));
        }

        /// <summary>
        /// Gets the new name of the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The new unique file name</returns>
        public string GetNewFileName(string path)
        {
            return Path.Combine(path, Guid.NewGuid() + TMP_FILE_EXTENSION);
        }

        /// <summary>
        /// The create new test file async.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="data">The data to write</param>
        /// <returns>
        /// The <see cref="Task"/> filename of the new file.
        /// </returns>
        public async Task<string> CreateNewTestFileAsync(string path, string data)
        {
            var result = GetNewFileName(path);
            await using var stream = File.CreateText(result);
            await stream.WriteAsync(data).ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// The delete directory.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        public void DeleteDirectory(string path)
        {
            // 30.11.2021 MInsberg  : First try the simple way.
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception exception)
            {
                WriteDebugMessage($"ERROR: Can't delete directory {path}. Exception: {exception.GetExceptionMessage()}");
            }

            if (Directory.Exists(path))
            {
                DeleteDirectorySave(path);
            }
        }

        /// <summary>
        /// Writes the debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="queueMessage"></param>
        public void WriteDebugMessage(string message, bool queueMessage = true)
        {
            if (ShowDebugMessages)
            {
                Console.WriteLine(message);
                if (queueMessage)
                {
                    messages.Add(message);
                }
            }
        }

        /// <summary>
        /// Flushes the text fixture messages.
        /// </summary>
        public void FlushTextFixtureMessages()
        {
            foreach (var message in messages)
            {
                if (ShowDebugMessages)
                {
                    Console.WriteLine(message);
                }
            }

            messages.Clear();
        }

        /// <summary>
        /// Deletes the files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns>True is all files are deleted otherwise false</returns>
        public bool DeleteFiles(string[] files)
        {
            var result = true;
            if (files == null || files.Length.Equals(0))
            {
                return true;
            }

            foreach (var file in files)
            {
                result = result && DeleteFile(file);
            }
            return result;
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>True is file is deleted otherwise false</returns>
        public bool DeleteFile(string fileName)
        {
            var count = 1;
            while (File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception exception)
                {
                    // Wait 1 ms, 2 ms, 4 ms, 8ms, ...
                    Wait(count);
                    WriteDebugMessage($"ERROR: Can't delete file {fileName}. " +
                                      $"Try {count} of {MAX_IO_RETRY_COUNT}. " +
                                      $"Exception: {exception.GetExceptionMessage()}");
                    count++;
                }

                if (count >= MAX_IO_RETRY_COUNT)
                {
                    break;
                }

                count++;
            }
            return File.Exists(fileName);
        }

        /// <summary>
        /// Gets the root test directory.
        /// </summary>
        /// <returns>The root test directory</returns>
        private string GetRootTestDirectory()
        {
            return Path.Combine(testDirectoryRoot, TEST_FIXTURE_PATH);
        }

        /// <summary>
        /// Deletes the directory save.
        /// </summary>
        /// <param name="path">The path.</param>
        private void DeleteDirectorySave(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            ClearAttributes(path);

            // 30.11.2021 MInsberg  : First delete all files in the directory 
            var files = Directory.GetFiles(path);
            var result = DeleteFiles(files);
            if (!result)
            {
                WriteDebugMessage($"Can't delete all files in {path}");
            }

            // 30.11.2021 MInsberg  : Delete sub directories and files in sub directories
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                DeleteDirectorySave(dir);
            }

            // 30.11.2021 MInsberg  : Delete the directory.
            var count = 1;
            while (Directory.Exists(path))
            {
                try
                {

                    // 30.11.2021 MInsberg  : If any sub directory of file can't be deleted at this point try is again
                    Directory.Delete(path, true);
                }
                catch (Exception exception)
                {
                    // Wait 1 ms, 2 ms, 4 ms, 8ms, ...
                    Wait(count);
                    WriteDebugMessage($"ERROR: Can't delete file {path}. " +
                                      $"Try { count} of { MAX_IO_RETRY_COUNT}. " +
                                      $"Exception: {exception.GetExceptionMessage()}");

                    count++;
                }

                if (count >= MAX_IO_RETRY_COUNT)
                {
                    break;
                }

                count++;
            }
        }

        /// <summary>
        /// Waits the specified wait.
        /// </summary>
        /// <param name="power">The wait.</param>
        /// <returns>The incremented wait time</returns>
        private void Wait(int power)
        {
            var wait = (int)Math.Pow(INCREMENT_WAIT, power);
            Thread.Sleep(wait);
        }

        /// <summary>
        /// Clears the attributes.
        /// </summary>
        /// <param name="currentDir">The current dir.</param>
        private void ClearAttributes(string currentDir)
        {
            if (!Directory.Exists(currentDir))
            {
                return;
            }

            File.SetAttributes(currentDir, FileAttributes.Normal);

            string[] subDirs = Directory.GetDirectories(currentDir);
            foreach (string dir in subDirs)
            {
                ClearAttributes(dir);
            }

            string[] files = Directory.GetFiles(currentDir);
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }
        }

        /// <summary>
        /// Determines whether [is directory writable] [the specified path].
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <c>true</c> if [is directory writable] [the specified path]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDirectoryWritable(string path)
        {
            try
            {

                var fileName = CreateNewTestFileAsync(path, Guid.NewGuid().ToString()).Result;

                return File.Exists(fileName) && DeleteFile(fileName);
            }
            catch (Exception exception)
            {
                WriteDebugMessage($"ERROR: Can't write to path {path}. " +
                                  $"Exception: {exception.GetBaseException()}");

                return false;
            }
        }
    }
}

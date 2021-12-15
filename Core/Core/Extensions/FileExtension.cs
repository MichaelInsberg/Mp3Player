using System.IO;

namespace Mp3Player.Core.Extensions
{
    /// <summary>
    /// The file extension class
    /// </summary>
    public static class FileExtension
    {
        /// <summary>
        /// Determines whether the specified size in byte has size.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        /// <param name="sizeInByte">The size in byte.</param>
        /// <returns>
        ///   <c>true</c> if the file has at least the specified size in byte; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasSize(this FileInfo fileInfo, int sizeInByte = 0)
        {
            return fileInfo.Exists && fileInfo.Length >= sizeInByte;
        }
    }
}

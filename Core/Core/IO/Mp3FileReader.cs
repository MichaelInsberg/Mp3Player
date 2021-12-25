using System;
using System.IO;
using System.Threading.Tasks;
using Id3;
using Mp3Player.Core.Constants;
using Mp3Player.Core.DataClasses;
using Mp3Player.Core.Enumerations;
using Mp3Player.Core.Extensions;

namespace Mp3Player.Core.IO
{
    /// <summary>
    ///  The MP3 file reader class
    /// </summary>
    public class Mp3FileReader
    {
        /// <summary>
        /// Reads the file asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The MP3 file information</returns>
        /// <exception cref="System.ArgumentNullException">fileName</exception>
        /// <exception cref="System.ArgumentException">The {nameof(fileName)} has not the minimal file site of {FileConstants.MINIMAL_ALLOWED_MP3_BYTE_FILE_SIZE} byte</exception>
        public Task<Mp3FileInformation>ReadFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (!new FileInfo(fileName).HasSize(FileConstants.MINIMAL_ALLOWED_MP3_BYTE_FILE_SIZE))
            {
                throw new ArgumentException(
                    $"The {nameof(fileName)} has not the minimal file site of {FileConstants.MINIMAL_ALLOWED_MP3_BYTE_FILE_SIZE} byte");
            }

            return GetMp3FileInformationAsync(fileName);
        }

        /// <summary>
        /// Gets the MP3 file information asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The MP3 file information</returns>
        private async Task<Mp3FileInformation> GetMp3FileInformationAsync(string fileName)
        {
            var mp3Information = new Mp3FileInformation();
            await using var stream = File.OpenRead(fileName);
            using var mp3 = new Mp3(stream);
            var mp3Tags = mp3.GetAllTags();
            if (mp3Tags == null)
            {
                mp3Information.Status = Mp3TagStatus.NoTagFound;
                return mp3Information;
            }
            mp3Information.Status = Mp3TagStatus.TagFound;
            foreach (var mp3Tag in mp3Tags)
            {
                if (mp3Tag.Artists.Value != null)
                {
                    foreach (var artist in mp3Tag.Artists.Value)
                    {
                        mp3Information.AddArtist(artist);
                    }
                }
            }

            return mp3Information;
        }
    }
}

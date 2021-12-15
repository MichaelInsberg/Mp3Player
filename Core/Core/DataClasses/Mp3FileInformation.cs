using System.Collections.Generic;
using Mp3Player.Core.Enumerations;

namespace Mp3Player.Core.DataClasses
{
    /// <summary>
    /// The Mp3 file information class
    /// </summary>
    public class Mp3FileInformation
    {
        /// <summary>
        /// Gets the artists.
        /// </summary>
        public List<string> Artists { get; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public Mp3TagStatus Status { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mp3FileInformation"/> class.
        /// </summary>
        public Mp3FileInformation()
        {
            Artists = new List<string>();
            Status = Mp3TagStatus.Undefined;
        }

        /// <summary>
        /// Adds the artist.
        /// </summary>
        /// <param name="artistName">Name of the artist.</param>
        public void AddArtist(string artistName)
        {
            if (!Artists.Contains(artistName))
            {
                Artists.Add(artistName);
            }
        }
    }
}

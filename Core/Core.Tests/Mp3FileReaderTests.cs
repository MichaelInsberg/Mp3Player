using System.Threading.Tasks;
using Mp3Player.Core.IO;
using Mp3Player.TestHelpers;
using NUnit.Framework;

namespace Mp3Player.Core.Tests
{

    /// <summary>
    /// The MP3 file reader test class
    /// </summary>
    [TestFixture]
    public class Mp3FileReaderTests : TestBase<Mp3FileReader>
    {
        private const string DEATH_GRIPS_GET_GOT = @".\Mp3TestFiles\Death Grips - Get Got.mp3";

        /// <summary>
        /// Reads the file asynchronous result should be valid values.
        /// </summary>
        [Test]
        public async Task ReadFileAsync_ValidValuesAsync()
        {
            var result = await ObjectToTest.ReadFileAsync(DEATH_GRIPS_GET_GOT);
            Assert.IsNotNull(result, $"{nameof(ObjectToTest.ReadFileAsync)}");
        }

        /// <inheritdoc />
        protected override Mp3FileReader CreateTestObject()
        {
            return new Mp3FileReader();
        }
    }
}

using Mp3Player.Core.DataClasses;
using Mp3Player.TestHelpers;
using NUnit.Framework;

namespace Mp3Player.Core.Tests.DataClasses
{
    /// <summary>
    /// The Mp3 file information tests
    /// </summary>
    [TestFixture]
    public class Mp3FileInformationTests : TestBase<Mp3FileInformation>
    {
        /// <inheritdoc />
        protected override Mp3FileInformation CreateTestObject()
        {
            return new Mp3FileInformation();
        }
    }
}

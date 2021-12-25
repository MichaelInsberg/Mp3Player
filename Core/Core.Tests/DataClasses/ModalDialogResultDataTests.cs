using Mp3Player.Core.DataClasses;
using Mp3Player.TestHelpers;
using NUnit.Framework;

namespace Mp3Player.Core.Tests.DataClasses
{
    [TestFixture]
    public class ModalDialogResultDataTests : TestBase<ModalDialogResultData>
    {
        [Test]
        public void ConstructorDefaults_ValidValues()
        {
            // Assert
        }

        /// <inheritdoc />
        protected override ModalDialogResultData CreateTestObject()
        {
            return new ModalDialogResultData();
        }
    }
}

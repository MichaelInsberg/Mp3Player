using Mp3Player.Core.DataClasses;
using Mp3Player.TestHelpers;
using NUnit.Framework;

namespace Mp3Player.Core.Tests.DataClasses
{
    [TestFixture]
    public class ModalDialogDataTests : TestBase<ModalDialogData>
    {
        [Test]
        public void ConstructorDefaults_ValidValues()
        {
            // Assert
        }

        /// <inheritdoc />
        protected override ModalDialogData CreateTestObject()
        {
            return new ModalDialogData();
        }
    }
}

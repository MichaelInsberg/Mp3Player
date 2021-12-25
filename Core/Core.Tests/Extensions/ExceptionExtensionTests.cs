using System;
using Mp3Player.Core.Extensions;
using Mp3Player.TestHelpers;
using NUnit.Framework;

namespace Mp3Player.Core.Tests.Extensions
{
    /// <summary>
    /// The exception extension tests
    /// </summary>
    [TestFixture]
    public class ExceptionExtensionTests : StaticTestBase
    {
        /// <inheritdoc />
        protected override Type GetTestType()
        {
            return typeof(ExceptionExtension);
        }
    }
}

using System;

namespace Mp3Player.TestHelpers
{
    /// <summary>
    /// The static test base.
    /// </summary>
    public abstract class StaticTestBase : TestFixtureBase
    {
        /// <summary>
        /// Gets or sets the exposed class to test.
        /// </summary>
        protected dynamic ExposedClassToTest { get; set; }


        /// <inheritdoc />
        public override void SetUp()
        {
            base.SetUp();

            ExposedClassToTest = ExposedClass.From(GetTestType());
        }

        /// <summary>
        /// The get test type.
        /// </summary>
        /// <returns>
        /// The <see cref="Type"/> static test type.
        /// </returns>
        protected abstract Type GetTestType();
    }
}

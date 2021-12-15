using System;
using NUnit.Framework;

namespace Mp3Player.TestHelpers
{
    /// <summary>
    /// Defines the Test base.
    /// </summary>
    /// <typeparam name="T">Where T can be anything</typeparam>
    [TestFixture]
    public abstract class TestBase<T> : TestFixtureBase
    {
        /// <summary>
        /// Gets the object to test.
        /// </summary>
        protected T ObjectToTest { get; private set; }

        /// <summary>
        /// Gets the exposed object to test.
        /// </summary>
        protected dynamic ExposedObjectToTest { get; private set; }


        /// <inheritdoc />
        public override void SetUp()
        {
            base.SetUp();
            Identifier = Guid.NewGuid();

            CreateAndInitMocks();

            ObjectToTest = CreateTestObject();
            ExposedObjectToTest = ExposedObject.From(ObjectToTest);
        }

        /// <inheritdoc />
        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                var obj = ObjectToTest as IDisposable;
                obj?.Dispose();
                ObjectToTest = default;
                var exposedObject = ExposedObjectToTest as IDisposable;
                exposedObject?.Dispose();
                ExposedObjectToTest = default;
            }
        }

        /// <summary>
        /// The object to test initialization not null.
        /// </summary>
        [Test]
        public void Initialization_NotNull() => Assert.IsNotNull(ObjectToTest);

        /// <summary>
        /// The create test object.
        /// </summary>
        /// <returns>
        /// The object for type T"/>.
        /// </returns>
        protected abstract T CreateTestObject();

        /// <summary>
        /// Creates the and initialize mocks.
        /// </summary>
        protected virtual void CreateAndInitMocks()
        {
        }
    }
}

using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Mp3Player.TestHelpers
{
    /// <summary>
    /// The test fixture base class
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    [TestFixture]
    public abstract class TestFixtureBase : IDisposable
    {
        private string testPath;
        private ConsoleTraceListener tracer;

        /// <summary>
        /// Gets the test tools.
        /// </summary>
        protected TestTools Tools { get; private set; }

        /// <summary>
        /// Gets the test path.
        /// </summary>
        protected string TestPath
        {
            get
            {
                // 25.11.2021 MInsberg  : Create root path and one unique path per test id needed 
                if (string.IsNullOrWhiteSpace(testPath))
                {
                    testPath = Tools.CreateRootPathAndTestPath();

                }

                return testPath;
            }
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        protected Guid Identifier { get; set; }


        /// <summary>
        /// Called when [time setup].
        /// </summary>
        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            tracer = new ConsoleTraceListener();
            _ = Trace.Listeners.Add(tracer);
            Tools = new TestTools();
            WriteDebugMessage($"OneTime setup test {GetTestName()}", false);
        }

        /// <summary>
        /// Called when [time tear down].
        /// </summary>
        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            Trace.Flush();
            Tools?.FlushTextFixtureMessages();
            WriteDebugMessage($"OneTime teardown test {GetTestName()}", false);
            if (tracer != null)
            {
                Trace.Listeners.Remove(tracer);
            }
            Tools = null;
        }


        /// <summary>
        /// The test setup.
        /// </summary>
        [SetUp]
        public virtual void SetUp()
        {
            WriteDebugMessage($"Setup test {GetTestName()}");
            testPath = null;
            Identifier = Guid.NewGuid();
        }

        /// <summary>
        /// The tear down.
        /// </summary>
        [TearDown]
        public virtual void TearDown()
        {
            Tools.DeleteDirectory(testPath);
            // 30.11.2021 MInsberg  :
            // Set to null to be sure that a new test directory will be created in next test if necessary
            testPath = null;
            WriteDebugMessage($"TearDown test {GetTestName()}");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Writes the debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="queueMessage"></param>
        public void WriteDebugMessage(string message, bool queueMessage = true)
        {
            if (Tools != null)
            {
                Tools.WriteDebugMessage(message, queueMessage);
            }
        }

        /// <summary>
        /// Gets the name of the test.
        /// </summary>
        /// <param name="fullName">if set to <c>true</c> [full name].</param>
        /// <returns>The full name of the test or the name</returns>
        protected string GetTestName(bool fullName = false)
        {
            return fullName ?
                TestContext.CurrentContext.Test.FullName :
                TestContext.CurrentContext.Test.Name;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="dispose"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                tracer?.Dispose();
                tracer = null;
            }
        }
    }
}

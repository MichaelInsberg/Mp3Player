using System;

namespace Mp3Player.Core.Extensions
{
    /// <summary>
    /// The exception extension class
    /// </summary>
    public static class ExceptionExtension
    {
        /// <summary>
        /// Gets the exception message.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">exception</exception>
        public static string GetExceptionMessage(this Exception exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }
            string result = string.Empty;

            var currentException = exception;
            while (currentException != null)
            {
                result = string.IsNullOrEmpty(result) ?
                    $"{result}{Environment.NewLine}" :
                    $"MESSAGES: {currentException.Message}";
                currentException = currentException.InnerException;
            }
            result = $"{result}{Environment.NewLine}TARGET SITE:{exception.TargetSite}";
            result = $"{result}{Environment.NewLine}STACKTRACE:{exception.StackTrace}";
            return result;
        }
    }
}

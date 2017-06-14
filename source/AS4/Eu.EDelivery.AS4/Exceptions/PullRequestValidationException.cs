﻿using System;

namespace Eu.EDelivery.AS4.Exceptions
{
    public class PullRequestValidationException : AS4Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestValidationException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        private PullRequestValidationException(string message) : base(message) { }

        /// <summary>
        /// Invalids the signature.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
        public static Exception InvalidSignature(string messageId)
        {
            var exception = new PullRequestValidationException("Signature verifycation failed") { ErrorCode = ErrorCode.Ebms0101 };
            exception.AddMessageId(messageId);

            return exception;
        }

        /// <summary>
        /// Missings the MPC certificate.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="mpc">The MPC.</param>
        /// <returns></returns>
        public static Exception MissingMpcCertificate(string messageId, string mpc)
        {
            var exception = new PullRequestValidationException($"No Certificate found for Mpc: {mpc}") { ErrorCode = ErrorCode.Ebms0101 };
            exception.AddMessageId(messageId);

            return exception;
        }
    }
}
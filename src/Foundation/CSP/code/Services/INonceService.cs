using System;

namespace Foundation.CSP.Services
{
    /// <summary>
    /// Service for generating and managing cryptographically secure nonce tokens for CSP headers.
    /// Nonce tokens are used to allow specific inline scripts while maintaining CSP security.
    /// </summary>
    public interface INonceService
    {
        /// <summary>
        /// Gets the current nonce token for the request.
        /// If no nonce exists, a new one will be generated.
        /// </summary>
        /// <returns>A cryptographically secure nonce token</returns>
        string GetCurrentNonce();

        /// <summary>
        /// Generates a new cryptographically secure nonce token.
        /// </summary>
        /// <returns>A new nonce token</returns>
        string GenerateNonce();

        /// <summary>
        /// Clears the current nonce token, forcing generation of a new one on next request.
        /// </summary>
        void ClearNonce();
    }
}

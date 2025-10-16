using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Sitecore.Configuration;

namespace Foundation.CSP.Services
{
    /// <summary>
    /// Service for generating and managing cryptographically secure nonce tokens for CSP headers.
    /// Uses HttpContext to store nonce per request, ensuring consistent nonce across the request lifecycle.
    /// </summary>
    public class NonceService : INonceService
    {
        private const string NonceContextKey = "CSP_Nonce_Token";
        private readonly int _nonceLength;

        public NonceService()
        {
            _nonceLength = Settings.GetIntSetting("CSP.NonceLength", 32);
        }

        /// <summary>
        /// Gets the current nonce token for the request.
        /// If no nonce exists, a new one will be generated and stored in HttpContext.
        /// </summary>
        /// <returns>A cryptographically secure nonce token</returns>
        public string GetCurrentNonce()
        {
            var context = HttpContext.Current;
            if (context?.Items == null)
            {
                // Fallback for non-web contexts (e.g., unit tests)
                return GenerateNonce();
            }

            if (context.Items[NonceContextKey] is string existingNonce)
            {
                return existingNonce;
            }

            var newNonce = GenerateNonce();
            context.Items[NonceContextKey] = newNonce;
            return newNonce;
        }

        /// <summary>
        /// Generates a new cryptographically secure nonce token using RNGCryptoServiceProvider.
        /// </summary>
        /// <returns>A new base64-encoded nonce token</returns>
        public string GenerateNonce()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var nonceBytes = new byte[_nonceLength];
                rng.GetBytes(nonceBytes);
                return Convert.ToBase64String(nonceBytes);
            }
        }

        /// <summary>
        /// Clears the current nonce token from HttpContext, forcing generation of a new one on next request.
        /// </summary>
        public void ClearNonce()
        {
            var context = HttpContext.Current;
            if (context?.Items != null && context.Items.Contains(NonceContextKey))
            {
                context.Items.Remove(NonceContextKey);
            }
        }
    }
}

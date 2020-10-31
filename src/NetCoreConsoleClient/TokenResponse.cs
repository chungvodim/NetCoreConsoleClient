using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace NetCoreConsoleClient
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        //
        // Summary:
        //     Gets the identity token.
        //
        // Value:
        //     The identity token.
        public string IdentityToken { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
        //
        // Summary:
        //     Gets the refresh token.
        //
        // Value:
        //     The refresh token.
        public string RefreshToken { get; set; }
        //
        // Summary:
        //     Gets the error description.
        //
        // Value:
        //     The error description.
        public string ErrorDescription { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}

using System.Collections.Concurrent;

namespace GLMS.Web.Services
{
    public class TokenStorageService
    {
        private static readonly ConcurrentDictionary<string, string> _tokens = new();

        public void SetToken(string key, string token)
        {
            System.Diagnostics.Debug.WriteLine($"=== TOKEN STORAGE - SET ===");
            System.Diagnostics.Debug.WriteLine($"Key: {key}");
            System.Diagnostics.Debug.WriteLine($"Token is null or empty: {string.IsNullOrEmpty(token)}");

            if (!string.IsNullOrEmpty(token))
            {
                _tokens[key] = token;
                System.Diagnostics.Debug.WriteLine($"Token stored. Total tokens: {_tokens.Count}");
                System.Diagnostics.Debug.WriteLine($"Token (first 20 chars): {token.Substring(0, Math.Min(20, token.Length))}...");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Attempted to store empty token!");
            }
        }

        public string GetToken(string key)
        {
            System.Diagnostics.Debug.WriteLine($"=== TOKEN STORAGE - GET ===");
            System.Diagnostics.Debug.WriteLine($"Key: {key}");
            System.Diagnostics.Debug.WriteLine($"Total tokens in storage: {_tokens.Count}");
            System.Diagnostics.Debug.WriteLine($"All keys: {string.Join(", ", _tokens.Keys)}");

            _tokens.TryGetValue(key, out var token);
            System.Diagnostics.Debug.WriteLine($"Token found: {!string.IsNullOrEmpty(token)}");

            if (!string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine($"Token (first 20 chars): {token.Substring(0, Math.Min(20, token.Length))}...");
            }

            return token;
        }

        public void RemoveToken(string key)
        {
            _tokens.TryRemove(key, out _);
            System.Diagnostics.Debug.WriteLine($"Token removed for key: {key}");
        }
    }
}
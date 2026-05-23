using System;

namespace Fintech.Api.Services.Auth
{
    public class RevokedToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime RevokedAt { get; set; } = DateTime.Now;

        public override bool Equals(object? obj)
        {
            if(obj is RevokedToken revokedToken)
            {
                return this.Token == revokedToken.Token;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Token.GetHashCode();
        }
    }
}
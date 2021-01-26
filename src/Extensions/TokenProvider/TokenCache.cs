using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    [Serializable]
    public class TokenCache : ClientCertificateToken
    {
        public DateTime LastGetTime { get; set; }

        public bool IsExpired()
        {
            if (expires_in <= 0 || ((DateTime.UtcNow - LastGetTime.ToUniversalTime()).TotalSeconds - expires_in) >= -30)
            {
                return true;
            }
            return false;
        }
    }
}

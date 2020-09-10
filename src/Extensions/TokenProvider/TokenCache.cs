using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    [Serializable]
    public class TokenCache : ClientCertificateToken
    {
        public DateTime LastGetTime { get; set; }

        public bool IsExpired()
        {
            if (((DateTime.UtcNow - LastGetTime).TotalSeconds - expires_in) >= -30)
            {
                return true;
            }
            return false;
        }
    }
}

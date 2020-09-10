using System;
namespace Xfrogcn.AspNetCore.Extensions
{
    /// <summary>
    /// 认证结果
    /// </summary>
    [Serializable]
    public class ClientCertificateToken
    {
        public string token_type { get; set; }

        public string access_token { get; set; }

        public long expires_in { get; set; }

    }
}

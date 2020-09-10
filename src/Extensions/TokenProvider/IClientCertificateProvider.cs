using System;
using System.Collections.Generic;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions
{
    public interface IClientCertificateProvider
    {
        ClientCertificateManager GetClientCertificateManager(string clientId);
    }
}

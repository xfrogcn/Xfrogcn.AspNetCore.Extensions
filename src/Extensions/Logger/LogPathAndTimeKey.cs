using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xfrogcn.AspNetCore.Extensions
{

    internal class LogPathAndTimeKey : IEqualityComparer<LogPathAndTimeKey>
    {
        public string Path { get; set; }

        public DateTimeOffset Time { get; set; }

        public bool Equals([AllowNull] LogPathAndTimeKey x, [AllowNull] LogPathAndTimeKey y)
        {
            return x.Path == y.Path;
        }

        public int GetHashCode([DisallowNull] LogPathAndTimeKey obj)
        {
            return (obj.Path ?? "").GetHashCode();
        }
    }
}

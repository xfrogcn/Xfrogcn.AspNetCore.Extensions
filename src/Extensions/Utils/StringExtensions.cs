using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xfrogcn.AspNetCore.Extensions
{
    public static class StringExtensions
    {
        private static Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
        public static string RandomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

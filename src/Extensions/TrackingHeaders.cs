using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class TrackingHeaders : ICollection<string>
    {
        private readonly Dictionary<string, Regex> _dic = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase);

        public int Count => _dic.Count;

        public bool IsReadOnly => false;

        public void Add(string item)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return;
            }
            lock (_dic)
            {
                _dic[item] = new Regex($"^{item.Replace("*", ".*")}$", RegexOptions.IgnoreCase);
            }
        }

        public IEnumerable<Regex> HeaderRegex => _dic.Values;

        public void Clear()
        {
            lock (_dic)
            {
                _dic.Clear();
            }
        }

        public bool Contains(string item)
        {
            return _dic.ContainsKey(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _dic.Keys.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _dic.Keys.GetEnumerator();
        }

        public bool Remove(string item)
        {
            bool isok = false;
            lock (_dic)
            {
                if (_dic.ContainsKey(item))
                {
                    isok = true;
                    _dic.Remove(item);
                }
            }
            return isok;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dic.Keys.GetEnumerator();
        }
    }
}

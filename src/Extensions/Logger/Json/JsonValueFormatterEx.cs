using Serilog.Formatting.Json;
using System;
using System.Globalization;
using System.IO;

namespace Xfrogcn.AspNetCore.Extensions.Logger.Json
{
    /// <summary>
    /// 扩展的JsonValueFormatter，优化了在写ES时的一些问题
    /// </summary>
    class JsonValueFormatterEx : JsonValueFormatter
    {
        readonly WebApiConfig _config;
        public JsonValueFormatterEx(WebApiConfig config, string typeTagName) : base(typeTagName)
        {
            _config = config;
        }
        protected override void FormatLiteralValue(object value, TextWriter output)
        {
            var str = value as string;
            if (str != null)
            {
                str = TrimString(_config, str);
                value = str;
            }
            // 将数字类型统一转换为值类型，以防止elasticserach 类型冲突
            if (value is ValueType)
            {
                if (value is int || value is uint || value is long || value is ulong || value is decimal ||
                    value is byte || value is sbyte || value is short || value is ushort)
                {
                    value = formatExactNumericValue((IFormattable)value, output);
                }

                if (value is double)
                {
                    value = formatDoubleValue((double)value, output);
                }

                if (value is float)
                {
                    value = formatFloatValue((float)value, output);
                }

                if (value is bool)
                {
                    value = formatBooleanValue((bool)value, output);
                }
            }

            base.FormatLiteralValue(value, output);
        }

        object formatBooleanValue(bool value, TextWriter output)
        {
            return value ? "true" : "false";
        }

        object formatFloatValue(float value, TextWriter output)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        object formatDoubleValue(double value, TextWriter output)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        object formatExactNumericValue(IFormattable value, TextWriter output)
        {
            return value.ToString(null, CultureInfo.InvariantCulture);
        }


        public static string TrimString(WebApiConfig config, string str)
        {
            var oldStr = str.AsSpan();
            if (str != null)
            {
                if (config != null && str.Length > config.MaxLogLength)
                {
                    int l = config.MaxLogLength;
                    int endLength = 12;
                    int sl = str.Length;
                    int trimLength = str.Length - l - endLength;
                    if (trimLength > 0)
                    {
                        str = string.Concat(oldStr.Slice(0, l - endLength), $"(省略{trimLength}字......)".AsSpan(), oldStr.Slice(str.Length - endLength));
                        if (!config.IgnoreLongLog)
                        {
                            // 拆分
                            int nowIndex = 0;
                            int nowLength = l;
                            int seq = 0;
                            while (nowIndex < sl)
                            {
                                if ((nowIndex + nowLength) > sl)
                                {
                                    nowLength = sl - nowIndex;
                                }
                                string cl = new string(oldStr.Slice(nowIndex, nowLength));
                                Serilog.Log.Information("{long_log} {log_seq} {segment}", true, seq, cl);
                                seq++;
                                nowIndex += nowLength;
                            }
                        }

                    }
                }
            }
            return str;

        }
    }
}

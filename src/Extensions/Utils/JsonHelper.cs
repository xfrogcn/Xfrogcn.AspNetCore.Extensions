using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Xfrogcn.AspNetCore.Extensions
{
    public class JsonHelper
    {
        private JsonSerializerOptions setting = null;

        /// <summary>
        /// 使用默认序列化设置：日期IsoDateFormat，日期格式 yyyy-MM-dd HH:mm:ss，时区 Local，忽略空字符，缩进模式，驼峰格式 枚举字面转换
        /// </summary>
        public JsonHelper() : this(
             new JsonSerializerOptions()
             {
                 PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                 PropertyNameCaseInsensitive = true, //属性不区分大小写
                 WriteIndented = true,
                 IgnoreNullValues = true
             })
        {
            setting.Converters.Add(new JsonStringEnumConverter());
            setting.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        }

        /// <summary>
        /// 使用完全自定义的序列化
        /// </summary>
        /// <param name="setting"></param>
        public JsonHelper(JsonSerializerOptions setting)
        {
            this.setting = setting;
        }



        public string ToJson(object obj)
        {
            if (obj == null)
                return "";
            return JsonSerializer.Serialize(obj, setting);
        }

        public Task ToJsonAsync(Stream stream, object obj)
        {
            if (stream == null || obj == null)
            {
                return Task.CompletedTask;
            }
            var jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(obj, setting);
            BinaryWriter sw = new BinaryWriter(stream);
            sw.Write(jsonUtf8Bytes);
            return Task.CompletedTask;
        }

        public object ToObject(string json, Type type)
        {
            if (String.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize(json, type, setting);
        }

        public TObject ToObject<TObject>(string json)
        {
            return (TObject)ToObject(json, typeof(TObject));
        }

        public ValueTask<object> ToObjectAsync(Stream stream, Type type)
        {
            if (stream == null)
            {
                return new ValueTask<object>(null);
            }
            return JsonSerializer.DeserializeAsync(stream, type, setting);
        }

        public ValueTask<TObject> ToObjectAsync<TObject>(Stream stream)
        {
            if (stream == null)
            {
                return new ValueTask<TObject>(null);
            }
            return JsonSerializer.DeserializeAsync<TObject>(stream, setting);
        }
    }
}

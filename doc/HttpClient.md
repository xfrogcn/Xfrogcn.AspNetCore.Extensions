# HttpClient扩展

扩展库在*HttpClient*上增加了一些列的扩展方法，通过这些扩展方法可更容易编写API调用代码。

## PostAsync&lt;TResponse&gt;

Task&lt;TResponse&gt; `PostAsync`&lt;TResponse&gt;(this HttpClient client, string url, object body, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null)

发送对象到服务端，并获取指定类型的应答

- url: string类型，请求地址
- body: object类型，需要发送的对象
- method: string类型，请求的方法，默认为POST
- queryString: NameValueCollection类型，查询字符串
- headers: NameValueCollection，附加请求头

**注意：** 关于发送的body对象，如果object是字符串类型，将直接发送，不会再通过Json序列化。另外，你也可以通过body直接传递HttpRequestMessage类型。

## PostAsync

Task&lt;string&gt; PostAsync(this HttpClient client, string url, object body, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null)

发送对象到服务端，并获取应答字符串

- url: string类型，请求地址
- body: object类型，需要发送的对象
- method: string类型，请求的方法，默认为POST
- queryString: NameValueCollection类型，查询字符串
- headers: NameValueCollection，附加请求头

## GetAsync&lt;TResponse&gt;

Task&lt;TResponse&gt; GetAsync&lt;TResponse&gt;(this HttpClient client, string url, NameValueCollection queryString = null, NameValueCollection headers = null)

发送Get请求，并获取TResponse类型的应答

- url: string类型，请求地址
- queryString: NameValueCollection类型，查询字符串
- headers: NameValueCollection，附加请求头

## GetAsync

Task&lt;string&gt; GetAsync(this HttpClient client, string url, NameValueCollection queryString = null, NameValueCollection headers = null)

发送Get请求，并获取String类型的应答

- url: string类型，请求地址
- queryString: NameValueCollection类型，查询字符串
- headers: NameValueCollection，附加请求头

## SubmitFormAsync&lt;TResponse&gt;

Task&lt;TResponse&gt; SubmitFormAsync&lt;TResponse&gt;(this HttpClient client, string url, Dictionary<string, string> formData, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null, bool ignoreEncode = false)

向服务器提交表单数据，并获取TResponse类型的应答

- url: string类型，请求地址
- formData: Dictionary&lt;string, string&gt;类型，表单数据
- method: string类型，请求的方法，默认为POST
- queryString: NameValueCollection类型，查询字符串
- headers: NameValueCollection，附加请求头
- ignoreEncode: bool类型，指定是否使用FormUrlEncodedContent作为请求内容。默认使用，使用该内容发送，如果数据过长，可能会引发System.UriFormatException异常

## SubmitFormAsync

Task&lt;string&gt; SubmitFormAsync(this HttpClient client, string url, Dictionary<string, string> formData, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null, bool ignoreEncode = false)

向服务器提交表单数据，并获取String类型的应答

- url: string类型，请求地址
- formData: Dictionary&lt;string, string&gt;类型，表单数据
- method: string类型，请求的方法，默认为POST
- queryString: NameValueCollection类型，查询字符串
- headers: NameValueCollection，附加请求头
- ignoreEncode: bool类型，指定是否使用FormUrlEncodedContent作为请求内容。默认使用，使用该内容发送，如果数据过长，可能会引发System.UriFormatException异常


## UploadFileAsync&lt;TResponse&gt;

Task&lt;TResponse&gt; UploadFileAsync&lt;TResponse&gt;(
            this HttpClient client, 
            string url, 
            string partKey,
            string file,
            string fileMediaType = null,
            string boundary = null,
            Dictionary<string, string> formData = null, 
            string method = "POST", 
            NameValueCollection queryString = null, 
            NameValueCollection headers = null)

上次本地文件

- url: string类型，请求地址
- partKey: string类型，上传内容部分名称
- file: string类型，文件路径
- fileMediaType：string类型，文件MIME类型
- boundary：string类型，分割字符串
- formData: Dictionary&lt;string, string&gt;类型，表单数据
- method: string类型，请求的方法，默认为POST
- queryString: NameValueCollection类型，查询字符串
- headers: NameValueCollection，附加请求头

## UploadStreamAsync&lt;TResponse&gt;

Task&lt;TResponse&gt; UploadStreamAsync&lt;TResponse&gt;(
            this HttpClient client,
            string url,
            string partKey,
            Stream fileStream,
            string fileName,
            string fileMediaType = null,
            string boundary = null,
            Dictionary<string, string> formData = null,
            string method = "POST",
            NameValueCollection queryString = null,
            NameValueCollection headers = null)

上传流数据到服务器

- url: string类型，请求地址
- partKey: string类型，上传内容部分名称
- fileStream: Stream类型，数据流
- fileMediaType：string类型，文件MIME类型
- boundary：string类型，分割字符串
- formData: Dictionary&lt;string, string&gt;类型，表单数据
- method: string类型，请求的方法，默认为POST
- queryString: NameValueCollection类型，查询字符串
- headers: NameValueCollection，附加请求头


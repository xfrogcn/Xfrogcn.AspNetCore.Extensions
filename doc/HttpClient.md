# HttpClient扩展

扩展库在HttpClient上增加了一些列的扩展方法，通过这些扩展方法可更容易编写API调用代码。

## PostAsync&lt;TResponse&gt;
Task<TResponse> `PostAsync`&lt;TResponse&gt;(this HttpClient client, string url, object body, string method = "POST", NameValueCollection queryString = null, NameValueCollection headers = null)

发送对象到服务端，并获取指定类型的应答

- url: string类型，请求地址
- body: object类型，需要发送的对象
- method: string类型，请求的方法，默认为POST
- queryString: NameValueCollection类型，查询字符串
- headers: NameValueCollection，附加请求头

**注意：** 关于发送的body对象，如果object是字符串类型，将直接发送，不会再通过Json序列化。另外，你也可以通过body直接传递HttpRequestMessage类型。


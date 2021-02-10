# Http请求与应答消息扩展

## HttpResponseMessage.GetObjectAsync&lt;TResponse&gt;

Task&lt;TResponse>&gt; GetObjectAsync&lt;TResponse&gt;(this HttpResponseMessage response, bool copy = false)

从HttpResponseMessage消息中去读指定类型的应答内容

- copy：bool类型，是否拷贝内容，默认false


## HttpRequestMessage.GetObjectAsync&lt;TResponse&gt;

Task&lt;TResponse&gt; GetObjectAsync&lt;TResponse&gt;(this HttpRequestMessage request, bool copy = false)

从HttpRequestMessage消息中获取指定类型的请求内容

- copy：bool类型，是否拷贝内容，默认false

## HttpResponseMessage.WriteObjectAsync

Task WriteObjectAsync(this HttpResponseMessage rsponse, object body)

将body对象写入HttpResponseMessage

- body: object类型，写入对象

## HttpRequestMessage.WriteObjectAsync

Task WriteObjectAsync(this HttpRequestMessage request, object body)

将body对象写入HttpRequestMessage

- body: object类型，写入对象

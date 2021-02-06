# 轻量级的实体映射

轻量级的实体映射用于数据层、DTO对象、视图层的实体自动转换或自动拷贝。

核心功能

- 在使用之前无需手动定义类型之间的映射关系
- 采用动态编译、缓存转换委托，提升性能。
- 支持通过特性定义属性映射关系
- 支持插入自定义的转换处理方法
- 支持列表转换
- 支持嵌套类型转换
- 支持循环引用及引用关系维持
- 支持转换模式或拷贝模式
- 支持生成预定义的拷贝委托
- 为了保持其轻量性，目前支持以下转换
  - 值类型转换
  - 数值类型之间的兼容转换（如int-->uint）
  - 支持值类型与其可空类型间的兼容转换
  - 字典类型转换
  - 列表类型转换
  - 枚举类型与string类型间的转换
  - **不支持**结构体之间的转换以及结构体与类之间的转换

## 启用

启用轻量级的实体映射，有两种方式：

- 如果你是和扩展库其他功能同时使用，可直接通过UseExtensions即可

```c#
    using IHost host = Host.CreateDefaultBuilder()
                         // UseExtensions会自动注入Mapper
                         .UseExtensions()
                         .ConfigureServices(sc =>
                         {
                             // 通过ConfigureLightweightMapper来配置映射
                             sc.ConfigureLightweightMapper(options =>
                             {
                                //
                             });
                         })
                         .Build();
```

- 如果你需要单独使用，可通过IServiceCollection上的AddLightweightMapper方法启用

```c#
    //实体转换
    serviceDescriptors.AddLightweightMapper()
        .ConfigureLightweightMapper(options =>
                             {
                                //
                             });
```

## 配置自定义转换逻辑

你可以通过映射设置上的AddConvert来配置对应设置实体转换的`后置`逻辑，如下所示。

```c#
    //实体转换
    serviceDescriptors.AddLightweightMapper()
        .ConfigureLightweightMapper(options =>
        {
            // 通过AddConvert可自定义转换逻辑
            // 以下定义从SourceA转换到TargetB时，自动设置属性C的值
            options.AddConvert<SourceA, TargetB>((mapper, a, b) =>
            {
                b.C = "C";
            });
        });
```

## 使用

你可以通过IMapperProvider的GetMapper方法或IMapper<,>直接获取Mapper实例。

- 通过IMapperProvider

```c#
// 通过IMapperProvider
var mapperProvider = host.Services.GetRequiredService<IMapperProvider>();
var mapper = mapperProvider.GetMapper<SourceA, TargetA>();
var targetA = mapper.Convert(sourceA);
```

- 通过IMapper<,>

```c#
var mapperB = host.Services.GetRequiredService<IMapper<SourceA, TargetB>>();
var targetB = mapperB.Convert(sourceA);
```

## 通过特性指定属性映射关系

默认映射按照属性名称进行，你也可以通过MapperPropertyNameAttribute特性进行指定。

MapperPropertyNameAttribute:

| 属性名 | 类型 | 说明 |
| ----- | ---- | ---- |
| Name | String | 目标或源的名称 |
| TargetType | Type | 映射到的目标类型 |
| SourceType | Type | 映射到当前类型的来源类型 |

通过SourceType或TargetType你可以根据需求灵活的在源类型或目标类型上设置映射关系。

## 拷贝

实体映射也提供了拷贝方法，通过该方法可以将源实体属性拷贝到目标实体。

- 通过IMapper<,>的CopyTo方法进行默认拷贝：

```c#
var mapperB = host.Services.GetRequiredService<IMapper<SourceA, TargetB>>();
var targetB1 = new TargetB();
mapperB.CopyTo(sourceA, targetB1);
```

- 通过DefineCopyTo方法定义排除字段外的拷贝委托

```c#
var mapperB = host.Services.GetRequiredService<IMapper<SourceA, TargetB>>();
 // 只拷贝指定字段之外的属性
var copyProc = mapperB.DefineCopyTo(a =>
new
{
    a.A //忽略属性A
});
var targetB2 = new TargetB();
copyProc(sourceA, targetB2);
```

## 示例

以上代码的完整实例可参考`./examples/AutoMapper`目录

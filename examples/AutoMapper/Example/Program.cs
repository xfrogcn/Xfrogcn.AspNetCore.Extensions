using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Xfrogcn.AspNetCore.Extensions;
using Xfrogcn.AspNetCore.Extensions.AutoMapper;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder()
                         // UseExtensions会自动注入Mapper
                         .UseExtensions()
                         .ConfigureServices(sc =>
                         {
                             // 通过ConfigureLightweightMapper来配置映射
                             sc.ConfigureLightweightMapper(options =>
                             {
                                 // 通过AddConvert可自定义转换逻辑
                                 // 以下定义从SourceA转换到TargetB时，自动设置属性C的值
                                 options.AddConvert<SourceA, TargetB>((mapper, a, b) =>
                                 {
                                     b.C = "C";
                                 });
                             });
                         })
                         .Build();

            // 你也可以通过AddLightweightMapper单独注入
            //IServiceCollection sc = new ServiceCollection()
            //    .AddLightweightMapper();

            // 通过IMapperProvider
            var mapperProvider = host.Services.GetRequiredService<IMapperProvider>();
            var mapper = mapperProvider.GetMapper<SourceA, TargetA>();
            var sourceA = new SourceA()
            {
                A = "A",
                B = 1
            };
            var targetA = mapper.Convert(sourceA);

            Console.WriteLine($"TargetA: A: {targetA.A} B: {targetA.B}");

            // 也可以直接获取IMapper<,>实例
            var mapperB = host.Services.GetRequiredService<IMapper<SourceA, TargetB>>();
            var targetB = mapperB.Convert(sourceA);
            Console.WriteLine($"TargetB: A: {targetB.A} B: {targetB.B} C:{targetB.C}");

            //拷贝
            var targetB1 = new TargetB();
            mapperB.CopyTo(sourceA, targetB1);
            Console.WriteLine($"TargetB1: A: {targetB1.A} B: {targetB1.B} C:{targetB1.C}");

            // 只拷贝指定字段之外的属性
            var copyProc = mapperB.DefineCopyTo(a =>
            new
            {
                a.A //忽略属性A
            });
            var targetB2 = new TargetB();
            copyProc(sourceA, targetB2);
            Console.WriteLine($"TargetB2: A: {targetB2.A} B: {targetB2.B} C:{targetB2.C}");


            Console.ReadLine();
        }
    }
}

using System;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Xfrogcn.AspNetCore.Extensions.Logger;

namespace Serilog
{
    public static class ConditionMapLoggerConfigurationExtensions
    {
        static TimeSpan DefaultCheckInterval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 根据指定的日志属性值(字符串类型)动态生成日志Sink
        /// </summary>
        /// <param name="loggerSinkConfiguration">Sink配置</param>
        /// <param name="keyPropertyName">日志属性名称</param>
        /// <param name="defaultKey">日志属性不存在时所使用的默认值</param>
        /// <param name="configure">根据key生成目标Sink的配置委托</param>
        /// <param name="disposeCondition">Sink符合此条件时自动Dispose</param>
        /// <param name="checkInternal">Sink Dispose条件的检查间隔</param>
        /// <param name="sinkMapCountLimit">Map后目标Sink的最大数量，如果为0，立即Dispose，如果为null不Dispose，否则保留此数量的Sink</param>
        /// <param name="restrictedToMinimumLevel">重定向最小日志层级</param>
        /// <param name="levelSwitch">可动态修改的最小日志层级</param>
        /// <returns></returns>
        public static LoggerConfiguration MapCondition(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string keyPropertyName,
            string defaultKey,
            Action<string, LoggerSinkConfiguration> configure,
            Func<string, bool> disposeCondition,
            TimeSpan? checkInternal = null,
            int? sinkMapCountLimit = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null)
        {
            return MapCondition(loggerSinkConfiguration, le =>
            {
                if (le.Properties.TryGetValue(keyPropertyName, out var v) &&
                    v is ScalarValue sv)
                {
                    return sv.Value?.ToString() ?? defaultKey;
                }

                return defaultKey;
            }, configure, disposeCondition, checkInternal, sinkMapCountLimit, restrictedToMinimumLevel, levelSwitch);
        }

        /// <summary>
        /// 根据指定的日志属性值(TKey类型)动态生成日志Sink
        /// </summary>
        /// <typeparam name="TKey">Map时的Key类型</typeparam>
        /// <param name="loggerSinkConfiguration">Sink配置</param>
        /// <param name="keyPropertyName">日志属性名称</param>
        /// <param name="defaultKey">日志属性不存在时所使用的默认值</param>
        /// <param name="configure">根据key生成目标Sink的配置委托</param>
        /// <param name="disposeCondition">Sink符合此条件时自动Dispose</param>
        /// <param name="checkInternal">Sink Dispose条件的检查间隔</param>
        /// <param name="sinkMapCountLimit">Map后目标Sink的最大数量，如果为0，立即Dispose，如果为null不Dispose，否则保留此数量的Sink</param>
        /// <param name="restrictedToMinimumLevel">重定向最小日志层级</param>
        /// <param name="levelSwitch">可动态修改的最小日志层级</param>
        /// <returns></returns>
        public static LoggerConfiguration MapCondition<TKey>(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string keyPropertyName,
            TKey defaultKey,
            Action<TKey, LoggerSinkConfiguration> configure,
            Func<TKey, bool> disposeCondition,
            TimeSpan? checkInternal = null,
            int? sinkMapCountLimit = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null)
        {
            if (loggerSinkConfiguration == null) throw new ArgumentNullException(nameof(loggerSinkConfiguration));
            if (keyPropertyName == null) throw new ArgumentNullException(nameof(keyPropertyName));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return MapCondition(loggerSinkConfiguration, le =>
            {
                if (le.Properties.TryGetValue(keyPropertyName, out var v) &&
                    v is ScalarValue sv &&
                    sv.Value is TKey key)
                {
                    return key;
                }

                return defaultKey;
            }, configure, disposeCondition, checkInternal, sinkMapCountLimit, restrictedToMinimumLevel, levelSwitch);
        }


        /// <summary>
        /// 根据指定KeySelector所生成的Key动态生成日志Sink
        /// </summary>
        /// <typeparam name="TKey">Map时的Key类型</typeparam>
        /// <param name="loggerSinkConfiguration">Sink配置</param>
        /// <param name="keySelector">生成Key的委托</param>
        /// <param name="configure">根据key生成目标Sink的配置委托</param>
        /// <param name="disposeCondition">Sink符合此条件时自动Dispose</param>
        /// <param name="checkInternal">Sink Dispose条件的检查间隔</param>
        /// <param name="sinkMapCountLimit">Map后目标Sink的最大数量，如果为0，立即Dispose，如果为null不Dispose，否则保留此数量的Sink</param>
        /// <param name="restrictedToMinimumLevel">重定向最小日志层级</param>
        /// <param name="levelSwitch">可动态修改的最小日志层级</param>
        /// <returns></returns>
        public static LoggerConfiguration MapCondition<TKey>(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            Func<LogEvent, TKey> keySelector,
            Action<TKey, LoggerSinkConfiguration> configure,
            Func<TKey, bool> disposeCondition,
            TimeSpan? checkInternal = null,
            int? sinkMapCountLimit = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null)
        {
            if (loggerSinkConfiguration == null) throw new ArgumentNullException(nameof(loggerSinkConfiguration));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            if (sinkMapCountLimit.HasValue && sinkMapCountLimit.Value < 0) throw new ArgumentOutOfRangeException(nameof(sinkMapCountLimit));


            if(checkInternal == null)
            {
                checkInternal = DefaultCheckInterval;
            }

            return loggerSinkConfiguration.Sink(
                new MappedSink<TKey>(
                    (LogEvent logEvent, out TKey key) =>
                    {
                        key = keySelector(logEvent);
                        return true;
                    },
                    configure,
                    disposeCondition,
                    checkInternal.Value,
                    sinkMapCountLimit),
                restrictedToMinimumLevel,
                levelSwitch);
        }


        /// <summary>
        /// 根据指定的日志属性值(字符串类型)动态生成日志Sink
        /// </summary>
        /// <param name="loggerSinkConfiguration">Sink配置</param>
        /// <param name="keyPropertyName">日志属性名称</param>
        /// <param name="configure">根据key生成目标Sink的配置委托</param>
        /// <param name="disposeCondition">Sink符合此条件时自动Dispose</param>
        /// <param name="checkInternal">Sink Dispose条件的检查间隔</param>
        /// <param name="sinkMapCountLimit">Map后目标Sink的最大数量，如果为0，立即Dispose，如果为null不Dispose，否则保留此数量的Sink</param>
        /// <param name="restrictedToMinimumLevel">重定向最小日志层级</param>
        /// <param name="levelSwitch">可动态修改的最小日志层级</param>
        /// <returns></returns>
        public static LoggerConfiguration MapCondition(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string keyPropertyName,
            Action<string, LoggerSinkConfiguration> configure,
            Func<string, bool> disposeCondition,
            TimeSpan? checkInternal = null,
            int? sinkMapCountLimit = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null)
        {
            return MapCondition(
                loggerSinkConfiguration,
                (LogEvent le, out string key) =>
                {
                    if (le.Properties.TryGetValue(keyPropertyName, out var v) &&
                        v is ScalarValue sv)
                    {
                        key = sv.Value?.ToString();
                        return true;
                    }

                    key = null;
                    return false;
                }, configure, disposeCondition, checkInternal, sinkMapCountLimit, restrictedToMinimumLevel, levelSwitch);
        }


        /// <summary>
        /// 根据指定的日志属性值(TKey类型)动态生成日志Sink
        /// </summary>
        /// <typeparam name="TKey">Map时的Key类型</typeparam>
        /// <param name="loggerSinkConfiguration">Sink配置</param>
        /// <param name="keyPropertyName">日志属性名称</param>
        /// <param name="configure">根据key生成目标Sink的配置委托</param>
        /// <param name="disposeCondition">Sink符合此条件时自动Dispose</param>
        /// <param name="checkInternal">Sink Dispose条件的检查间隔</param>
        /// <param name="sinkMapCountLimit">Map后目标Sink的最大数量，如果为0，立即Dispose，如果为null不Dispose，否则保留此数量的Sink</param>
        /// <param name="restrictedToMinimumLevel">重定向最小日志层级</param>
        /// <param name="levelSwitch">可动态修改的最小日志层级</param>
        /// <returns></returns>
        public static LoggerConfiguration MapCondition<TKey>(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string keyPropertyName,
            Action<TKey, LoggerSinkConfiguration> configure,
            Func<TKey, bool> disposeCondition,
            TimeSpan? checkInternal = null,
            int? sinkMapCountLimit = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null)
        {
            if (loggerSinkConfiguration == null) throw new ArgumentNullException(nameof(loggerSinkConfiguration));
            if (keyPropertyName == null) throw new ArgumentNullException(nameof(keyPropertyName));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            return MapCondition(
                loggerSinkConfiguration,
                (LogEvent le, out TKey key) =>
                {
                    if (le.Properties.TryGetValue(keyPropertyName, out var v) &&
                        v is ScalarValue sv &&
                        sv.Value is TKey k)
                    {
                        key = k;
                        return true;
                    }

                    key = default(TKey);
                    return false;
                },
                configure, disposeCondition, checkInternal,  sinkMapCountLimit, restrictedToMinimumLevel, levelSwitch);
        }


        /// <summary>
        /// 根据指定的KeySelector委托生成的Key(TKey类型)动态生成日志Sink
        /// </summary>
        /// <typeparam name="TKey">Map时的Key类型</typeparam>
        /// <param name="loggerSinkConfiguration">Sink配置</param>
        /// <param name="keySelector">生成Key的委托</param>
        /// <param name="configure">根据key生成目标Sink的配置委托</param>
        /// <param name="disposeCondition">Sink符合此条件时自动Dispose</param>
        /// <param name="checkInternal">Sink Dispose条件的检查间隔</param>
        /// <param name="sinkMapCountLimit">Map后目标Sink的最大数量，如果为0，立即Dispose，如果为null不Dispose，否则保留此数量的Sink</param>
        /// <param name="restrictedToMinimumLevel">重定向最小日志层级</param>
        /// <param name="levelSwitch">可动态修改的最小日志层级</param>
        /// <returns></returns>
        public static LoggerConfiguration MapCondition<TKey>(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            KeySelector<TKey> keySelector,
            Action<TKey, LoggerSinkConfiguration> configure,
            Func<TKey, bool> disposeCondition,
            TimeSpan? checkInternal = null,
            int? sinkMapCountLimit = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null)
        {
            if (loggerSinkConfiguration == null) throw new ArgumentNullException(nameof(loggerSinkConfiguration));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            if (sinkMapCountLimit.HasValue && sinkMapCountLimit.Value < 0) throw new ArgumentOutOfRangeException(nameof(sinkMapCountLimit));

            if (checkInternal == null)
            {
                checkInternal = DefaultCheckInterval;
            }

            return loggerSinkConfiguration.Sink(new MappedSink<TKey>(keySelector, configure, disposeCondition, checkInternal.Value, sinkMapCountLimit), restrictedToMinimumLevel, levelSwitch);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace TCGProfiler.Testing;

public sealed class ManagedMethodProfiler
{
    private const string HarmonyId =
        "TCGProfiler.ManagedMethods";

    private static readonly Dictionary<
        MethodBase,
        MethodStats>? ActiveStatsProxy = null;

    private static ManagedMethodProfiler?
        _active;

    private readonly Harmony _harmony =
        new(HarmonyId);

    private readonly HashSet<MethodBase>
        _patched = new();

    private readonly Dictionary<
        MethodBase,
        MethodStats> _stats = new();

    private bool _enabled;

    public void Enable()
    {
        if (_enabled) return;

        _enabled = true;

        _stats.Clear();

        _active = this;

        DiscoverAndPatch();
    }

    public void Disable()
    {
        if (!_enabled) return;

        _harmony.UnpatchSelf();

        _patched.Clear();

        if (_active == this) _active = null;

        _enabled = false;
    }

    private void DiscoverAndPatch()
    {
        var behaviours =
            Resources.FindObjectsOfTypeAll<MonoBehaviour>();

        var types =
            new HashSet<Type>();

        foreach (var behaviour in behaviours)
        {
            if (behaviour == null ||
                behaviour.gameObject == null ||
                !behaviour.gameObject.scene.IsValid() ||
                !behaviour.enabled ||
                !behaviour.gameObject.activeInHierarchy)
                continue;

            if (behaviour is TcgProfiler) continue;

            types.Add(
                behaviour.GetType()
            );
        }

        foreach (var type in types)
        {
            Patch(type, "Update");
            Patch(type, "LateUpdate");
            Patch(type, "FixedUpdate");
        }
    }

    private void Patch(
        Type type,
        string name)
    {
        var method =
            type.GetMethod(
                name,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly,
                null,
                Type.EmptyTypes,
                null
            );

        if (method == null ||
            method.ReturnType != typeof(void) ||
            method.IsAbstract ||
            _patched.Contains(method))
            return;

        try
        {
            _harmony.Patch(
                method,
                new HarmonyMethod(
                    typeof(ManagedMethodProfiler),
                    nameof(Prefix)
                ),
                new HarmonyMethod(
                    typeof(ManagedMethodProfiler),
                    nameof(Postfix)
                )
            );

            _patched.Add(method);

            _stats[method] =
                new MethodStats();
        }
        catch
        {
            /*
             * A failed diagnostic patch should not
             * abort the entire benchmark suite.
             */
        }
    }

    private static void Prefix(
        out long __state)
    {
        __state =
            Stopwatch.GetTimestamp();
    }

    private static void Postfix(
        MethodBase __originalMethod,
        long __state)
    {
        var profiler =
            _active;

        if (profiler == null) return;

        if (!profiler._stats.TryGetValue(
                __originalMethod,
                out var stats))
            return;

        var ticks =
            Stopwatch.GetTimestamp() -
            __state;

        stats.Calls++;

        stats.TotalTicks +=
            ticks;

        if (ticks > stats.MaxTicks)
            stats.MaxTicks =
                ticks;
    }

    /*
     * Minor correction to Enable/Disable:
     * we need the active static instance because
     * Harmony callbacks are static.
     */

    private void SetActive()
    {
        _active = this;
    }

    public void WriteResults(
        string path)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(path)!
        );

        var exists =
            File.Exists(path);

        using var writer =
            new StreamWriter(
                path,
                true
            );

        if (!exists)
            writer.WriteLine(
                "Method,Calls,TotalMilliseconds,AverageMicroseconds,MaxMilliseconds"
            );

        foreach (var pair in
                 _stats
                     .OrderByDescending(x => x.Value.TotalTicks))
        {
            var method =
                pair.Key;

            var stats =
                pair.Value;

            if (stats.Calls == 0) continue;

            var totalMs =
                stats.TotalTicks *
                1000.0 /
                Stopwatch.Frequency;

            var averageUs =
                totalMs *
                1000.0 /
                stats.Calls;

            var maxMs =
                stats.MaxTicks *
                1000.0 /
                Stopwatch.Frequency;

            var name =
                $"{method.DeclaringType?.FullName}.{method.Name}";

            writer.WriteLine(
                $"{Csv(name)}," +
                $"{stats.Calls}," +
                $"{totalMs:F6}," +
                $"{averageUs:F6}," +
                $"{maxMs:F6}"
            );
        }
    }

    public void Reset()
    {
        foreach (var stats in _stats.Values)
        {
            stats.Calls = 0;
            stats.TotalTicks = 0;
            stats.MaxTicks = 0;
        }
    }

    private static string Csv(string value)
    {
        return
            "\"" +
            value.Replace(
                "\"",
                "\"\""
            ) +
            "\"";
    }

    private sealed class MethodStats
    {
        public long Calls;

        public long MaxTicks;

        public long TotalTicks;
    }
}
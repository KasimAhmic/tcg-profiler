using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class ActiveCharacterRendererCensusTestCase
    : TestCase
{
    private readonly List<Record> _records = new();

    public ActiveCharacterRendererCensusTestCase(
        bool enabled = true)
        : base(
            "ActiveCharacterRendererCensus",
            enabled,
            1.0f,
            0.25f)
    {
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        _records.Clear();

        foreach (var renderer in
                 profiler.CharacterRenderers)
        {
            if (renderer == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy)
                continue;

            _records.Add(
                CreateRecord(renderer)
            );
        }
    }

    public override void Complete(
        TcgProfiler profiler,
        TestResult result)
    {
        WriteGroupedCsv(
            profiler.ActiveCharacterRendererResultsPath
        );

        result.Metadata["ActiveRenderers"] =
            _records.Count.ToString();

        result.Metadata["VisibleRenderers"] =
            _records.Count(x => x.Visible)
                .ToString();

        result.Metadata["MaterialSlots"] =
            _records.Sum(x => x.MaterialSlots)
                .ToString();

        result.Metadata["Triangles"] =
            _records.Sum(x => x.Triangles)
                .ToString();
    }

    private static Record CreateRecord(
        Renderer renderer)
    {
        Mesh? mesh = null;

        if (renderer is SkinnedMeshRenderer skinned)
            mesh = skinned.sharedMesh;
        else
            mesh =
                renderer
                    .GetComponent<MeshFilter>()
                    ?.sharedMesh;

        long triangles = 0;

        if (mesh != null)
            for (var i = 0;
                 i < mesh.subMeshCount;
                 i++)
            {
                if (mesh.GetTopology(i) !=
                    MeshTopology.Triangles)
                    continue;

                triangles +=
                    mesh.GetIndexCount(i) /
                    3L;
            }

        var materials =
            renderer.sharedMaterials;

        return new Record
        {
            RendererType =
                renderer.GetType().Name,

            ObjectName =
                renderer.gameObject.name,

            MeshName =
                mesh?.name ?? string.Empty,

            Visible =
                renderer.isVisible,

            ShadowMode =
                renderer.shadowCastingMode.ToString(),

            MaterialSlots =
                materials.Length,

            Triangles =
                triangles,

            Materials =
                materials
                    .Where(x => x != null)
                    .Select(x => x.name)
                    .ToArray(),

            Shaders =
                materials
                    .Where(x =>
                        x != null &&
                        x.shader != null)
                    .Select(x => x.shader.name)
                    .ToArray()
        };
    }

    private void WriteGroupedCsv(
        string path)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(path)!
        );

        using var writer =
            new StreamWriter(path);

        writer.WriteLine(
            "Category," +
            "Name," +
            "RendererCount," +
            "VisibleCount," +
            "MaterialSlots," +
            "Triangles"
        );

        WriteGroups(
            writer,
            "RendererType",
            _records
                .GroupBy(x => x.RendererType)
        );

        WriteGroups(
            writer,
            "ObjectName",
            _records
                .GroupBy(x => x.ObjectName)
        );

        WriteGroups(
            writer,
            "Mesh",
            _records
                .Where(x => !string.IsNullOrWhiteSpace(
                    x.MeshName))
                .GroupBy(x => x.MeshName)
        );

        WriteMaterialGroups(
            writer
        );

        WriteShaderGroups(
            writer
        );

        WriteGroups(
            writer,
            "ShadowMode",
            _records
                .GroupBy(x => x.ShadowMode)
        );
    }

    private static void WriteGroups(
        StreamWriter writer,
        string category,
        IEnumerable<IGrouping<string, Record>> groups)
    {
        foreach (var group in
                 groups.OrderByDescending(x => x.Count()))
            writer.WriteLine(
                string.Join(
                    ",",
                    Csv(category),
                    Csv(group.Key),
                    group.Count(),
                    group.Count(x => x.Visible),
                    group.Sum(x => x.MaterialSlots),
                    group.Sum(x => x.Triangles)
                )
            );
    }

    private void WriteMaterialGroups(
        StreamWriter writer)
    {
        var groups =
            _records
                .SelectMany(record =>
                    record.Materials.Select(material =>
                        new
                        {
                            Record = record,
                            Name = material
                        }))
                .GroupBy(x => x.Name)
                .OrderByDescending(x => x.Count());

        foreach (var group in groups)
            writer.WriteLine(
                string.Join(
                    ",",
                    Csv("Material"),
                    Csv(group.Key),
                    group.Count(),
                    group.Count(x => x.Record.Visible),
                    group.Count(),
                    group.Sum(x => x.Record.Triangles)
                )
            );
    }

    private void WriteShaderGroups(
        StreamWriter writer)
    {
        var groups =
            _records
                .SelectMany(record =>
                    record.Shaders
                        .Distinct()
                        .Select(shader =>
                            new
                            {
                                Record = record,
                                Name = shader
                            }))
                .GroupBy(x => x.Name)
                .OrderByDescending(x => x.Count());

        foreach (var group in groups)
            writer.WriteLine(
                string.Join(
                    ",",
                    Csv("Shader"),
                    Csv(group.Key),
                    group.Count(),
                    group.Count(x => x.Record.Visible),
                    group.Sum(x => x.Record.MaterialSlots),
                    group.Sum(x => x.Record.Triangles)
                )
            );
    }

    private static string Csv(
        string value)
    {
        return
            "\"" +
            value.Replace(
                "\"",
                "\"\""
            ) +
            "\"";
    }

    private sealed class Record
    {
        public string RendererType { get; set; } =
            string.Empty;

        public string ObjectName { get; set; } =
            string.Empty;

        public string MeshName { get; set; } =
            string.Empty;

        public string ShadowMode { get; set; } =
            string.Empty;

        public bool Visible { get; set; }

        public int MaterialSlots { get; set; }

        public long Triangles { get; set; }

        public string[] Materials { get; set; } =
            Array.Empty<string>();

        public string[] Shaders { get; set; } =
            Array.Empty<string>();
    }
}
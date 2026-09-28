using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class CharacterRendererCensusTestCase : TestCase
{
    private readonly List<CharacterRendererRecord> _records = new();

    public CharacterRendererCensusTestCase(
        bool enabled = true)
        : base(
            "CharacterRendererCensus",
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

        foreach (var renderer in profiler.CharacterRenderers)
        {
            if (renderer == null) continue;

            var record =
                CreateRecord(renderer);

            _records.Add(record);
        }
    }

    public override void Complete(
        TcgProfiler profiler,
        TestResult result)
    {
        WriteDetailedCsv(
            profiler.CharacterRendererResultsPath
        );

        WriteSummaryCsv(
            profiler.CharacterRendererSummaryPath
        );

        result.Metadata["CharacterRenderers"] =
            _records.Count.ToString();

        result.Metadata["SkinnedRenderers"] =
            _records.Count(x => x.RendererType == nameof(SkinnedMeshRenderer))
                .ToString();

        result.Metadata["MeshRenderers"] =
            _records.Count(x => x.RendererType == nameof(MeshRenderer))
                .ToString();

        result.Metadata["TotalMaterialSlots"] =
            _records.Sum(x => x.MaterialSlotCount)
                .ToString();
    }

    private static CharacterRendererRecord CreateRecord(
        Renderer renderer)
    {
        Mesh? mesh = null;

        if (renderer is SkinnedMeshRenderer skinned)
        {
            mesh = skinned.sharedMesh;
        }
        else
        {
            var meshFilter =
                renderer.GetComponent<MeshFilter>();

            if (meshFilter != null) mesh = meshFilter.sharedMesh;
        }

        var materials =
            renderer.sharedMaterials;

        var materialNames =
            materials
                .Where(x => x != null)
                .Select(x => x.name)
                .ToArray();

        var shaderNames =
            materials
                .Where(x => x != null && x.shader != null)
                .Select(x => x.shader.name)
                .Distinct()
                .ToArray();

        long triangles = 0;

        var submeshCount =
            mesh?.subMeshCount ?? 0;

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

        return new CharacterRendererRecord
        {
            RendererType =
                renderer.GetType().Name,

            GameObjectName =
                renderer.gameObject.name,

            HierarchyPath =
                BuildHierarchyPath(
                    renderer.transform
                ),

            MeshName =
                mesh != null
                    ? mesh.name
                    : string.Empty,

            MaterialSlotCount =
                materials.Length,

            MaterialNames =
                string.Join(
                    " | ",
                    materialNames
                ),

            ShaderNames =
                string.Join(
                    " | ",
                    shaderNames
                ),

            SubmeshCount =
                submeshCount,

            TriangleCount =
                triangles,

            ShadowCastingMode =
                renderer.shadowCastingMode
                    .ToString(),

            ReceiveShadows =
                renderer.receiveShadows,

            Enabled =
                renderer.enabled,

            ActiveInHierarchy =
                renderer.gameObject.activeInHierarchy,

            Visible =
                renderer.isVisible
        };
    }

    private void WriteDetailedCsv(
        string path)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(path)!
        );

        using var writer =
            new StreamWriter(path);

        writer.WriteLine(
            "RendererType," +
            "GameObjectName," +
            "HierarchyPath," +
            "MeshName," +
            "MaterialSlotCount," +
            "MaterialNames," +
            "ShaderNames," +
            "SubmeshCount," +
            "TriangleCount," +
            "ShadowCastingMode," +
            "ReceiveShadows," +
            "Enabled," +
            "ActiveInHierarchy," +
            "Visible"
        );

        foreach (var record in _records)
            writer.WriteLine(
                string.Join(
                    ",",
                    Csv(record.RendererType),
                    Csv(record.GameObjectName),
                    Csv(record.HierarchyPath),
                    Csv(record.MeshName),
                    record.MaterialSlotCount,
                    Csv(record.MaterialNames),
                    Csv(record.ShaderNames),
                    record.SubmeshCount,
                    record.TriangleCount,
                    Csv(record.ShadowCastingMode),
                    record.ReceiveShadows,
                    record.Enabled,
                    record.ActiveInHierarchy,
                    record.Visible
                )
            );
    }

    private void WriteSummaryCsv(
        string path)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(path)!
        );

        using var writer =
            new StreamWriter(path);

        writer.WriteLine(
            "Category,Name,Count"
        );

        WriteGroup(
            writer,
            "RendererType",
            _records
                .GroupBy(x => x.RendererType)
                .Select(x => new GroupEntry
                {
                    Name = x.Key,
                    Count = x.Count()
                })
        );

        WriteGroup(
            writer,
            "GameObjectName",
            _records
                .GroupBy(x => x.GameObjectName)
                .Select(x => new GroupEntry
                {
                    Name = x.Key,
                    Count = x.Count()
                })
        );

        WriteGroup(
            writer,
            "MeshName",
            _records
                .Where(x => !string.IsNullOrWhiteSpace(x.MeshName))
                .GroupBy(x => x.MeshName)
                .Select(x => new GroupEntry
                {
                    Name = x.Key,
                    Count = x.Count()
                })
        );

        var materials =
            _records
                .SelectMany(x => x.MaterialNames
                    .Split(
                        new[] { " | " },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                )
                .GroupBy(x => x)
                .Select(x => new GroupEntry
                {
                    Name = x.Key,
                    Count = x.Count()
                });

        WriteGroup(
            writer,
            "Material",
            materials
        );

        var shaders =
            _records
                .SelectMany(x => x.ShaderNames
                    .Split(
                        new[] { " | " },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                )
                .GroupBy(x => x)
                .Select(x => new GroupEntry
                {
                    Name = x.Key,
                    Count = x.Count()
                });

        WriteGroup(
            writer,
            "Shader",
            shaders
        );
    }

    private static void WriteGroup(
        StreamWriter writer,
        string category,
        IEnumerable<GroupEntry> entries)
    {
        foreach (var entry in
                 entries.OrderByDescending(x => x.Count))
            writer.WriteLine(
                $"{Csv(category)}," +
                $"{Csv(entry.Name)}," +
                $"{entry.Count}"
            );
    }

    private static string BuildHierarchyPath(
        Transform transform)
    {
        var names =
            new Stack<string>();

        var current =
            transform;

        while (current != null)
        {
            names.Push(
                current.name
            );

            current =
                current.parent;
        }

        return string.Join(
            "/",
            names
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

    private sealed class GroupEntry
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    private sealed class CharacterRendererRecord
    {
        public string RendererType { get; set; } =
            string.Empty;

        public string GameObjectName { get; set; } =
            string.Empty;

        public string HierarchyPath { get; set; } =
            string.Empty;

        public string MeshName { get; set; } =
            string.Empty;

        public int MaterialSlotCount { get; set; }

        public string MaterialNames { get; set; } =
            string.Empty;

        public string ShaderNames { get; set; } =
            string.Empty;

        public int SubmeshCount { get; set; }

        public long TriangleCount { get; set; }

        public string ShadowCastingMode { get; set; } =
            string.Empty;

        public bool ReceiveShadows { get; set; }

        public bool Enabled { get; set; }

        public bool ActiveInHierarchy { get; set; }

        public bool Visible { get; set; }
    }
}
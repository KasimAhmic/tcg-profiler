using System.Collections.Generic;
using TCGProfiler.Testing;
using UnityEngine;

namespace TCGProfiler.TestCases;

public sealed class DisableUnusedCharacterLodsTestCase
    : TestCase
{
    private int _candidateGroups;
    private int _disabledMaterialSlots;
    private int _disabledRenderers;
    private int _disabledSkinnedRenderers;
    private long _disabledTriangles;
    private int _lodGroups;

    public DisableUnusedCharacterLodsTestCase(
        bool enabled = true)
        : base(
            "CharacterUnusedLODs_Disabled",
            enabled)
    {
    }

    public override void Setup(
        TcgProfiler profiler)
    {
        profiler.DiscoverCharacters();

        _lodGroups = 0;
        _candidateGroups = 0;
        _disabledRenderers = 0;
        _disabledSkinnedRenderers = 0;
        _disabledMaterialSlots = 0;
        _disabledTriangles = 0;

        var processed =
            new HashSet<Renderer>();

        var groups =
            Resources.FindObjectsOfTypeAll<LODGroup>();

        foreach (var group in groups)
        {
            if (group == null ||
                group.gameObject == null ||
                !group.gameObject.scene.IsValid() ||
                !group.gameObject.activeInHierarchy)
                continue;

            var lods =
                group.GetLODs();

            if (lods.Length == 0) continue;

            _lodGroups++;

            /*
             * Only operate on LODGroups that appear to
             * belong to one of our character hierarchies.
             */
            var isCharacterGroup = false;

            foreach (var lod in lods)
            {
                foreach (var renderer in lod.renderers)
                {
                    if (renderer == null) continue;

                    if (profiler.CharacterRenderers.Contains(
                            renderer))
                    {
                        isCharacterGroup = true;
                        break;
                    }
                }

                if (isCharacterGroup) break;
            }

            if (!isCharacterGroup) continue;

            /*
             * For this diagnostic we only touch a group
             * when at least one of its renderers is visible
             * from the current fixed benchmark camera.
             *
             * That prevents us from blindly disabling every
             * LOD of an entirely off-camera character.
             */
            var hasVisibleRenderer = false;

            foreach (var lod in lods)
            {
                foreach (var renderer in lod.renderers)
                {
                    if (renderer == null ||
                        !renderer.enabled ||
                        !renderer.gameObject.activeInHierarchy)
                        continue;

                    if (renderer.isVisible)
                    {
                        hasVisibleRenderer = true;
                        break;
                    }
                }

                if (hasVisibleRenderer) break;
            }

            if (!hasVisibleRenderer) continue;

            _candidateGroups++;

            foreach (var lod in lods)
            foreach (var renderer in lod.renderers)
            {
                if (renderer == null ||
                    !renderer.enabled ||
                    !renderer.gameObject.activeInHierarchy ||
                    renderer.isVisible ||
                    !processed.Add(renderer))
                    continue;

                _disabledRenderers++;

                if (renderer is SkinnedMeshRenderer) _disabledSkinnedRenderers++;

                _disabledMaterialSlots +=
                    renderer.sharedMaterials.Length;

                var mesh =
                    GetMesh(renderer);

                if (mesh != null)
                    for (var i = 0;
                         i < mesh.subMeshCount;
                         i++)
                    {
                        if (mesh.GetTopology(i) !=
                            MeshTopology.Triangles)
                            continue;

                        _disabledTriangles +=
                            mesh.GetIndexCount(i) /
                            3L;
                    }

                renderer.enabled =
                    false;
            }
        }

        Debug.Log(
            $"[TCGProfiler] CharacterUnusedLODs_Disabled: " +
            $"{_candidateGroups} character LOD groups; " +
            $"disabled {_disabledRenderers} non-visible " +
            $"LOD renderers"
        );
    }

    public override Dictionary<string, string>
        GetMetadata(TcgProfiler profiler)
    {
        return new Dictionary<string, string>
        {
            ["LODGroups"] =
                _lodGroups.ToString(),

            ["CandidateCharacterLODGroups"] =
                _candidateGroups.ToString(),

            ["DisabledRenderers"] =
                _disabledRenderers.ToString(),

            ["DisabledSkinnedRenderers"] =
                _disabledSkinnedRenderers.ToString(),

            ["DisabledMaterialSlots"] =
                _disabledMaterialSlots.ToString(),

            ["DisabledTriangles"] =
                _disabledTriangles.ToString()
        };
    }

    private static Mesh? GetMesh(
        Renderer renderer)
    {
        if (renderer is SkinnedMeshRenderer skinned) return skinned.sharedMesh;

        return renderer
            .GetComponent<MeshFilter>()
            ?.sharedMesh;
    }
}
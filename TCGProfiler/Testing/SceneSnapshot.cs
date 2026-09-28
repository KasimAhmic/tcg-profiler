using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TCGProfiler.Testing;

public sealed class SceneSnapshot
{
    private readonly Dictionary<
            Animator,
            AnimatorCullingMode>
        _animators = new();

    private readonly Dictionary<
            Light,
            LightShadows>
        _lights = new();

    private readonly Dictionary<
            Renderer,
            Material[]>
        _materials = new();

    private readonly Dictionary<
            Renderer,
            bool>
        _receiveShadows = new();

    private readonly Dictionary<
            Renderer,
            bool>
        _rendererEnabled = new();

    private readonly Dictionary<
            Renderer,
            ShadowCastingMode>
        _shadowCasting = new();

    private readonly Dictionary<
            SkinnedMeshRenderer,
            bool>
        _updateWhenOffscreen = new();

    public SceneSnapshot(
        TcgProfiler profiler)
    {
        foreach (var animator in
                 profiler.CharacterAnimators)
            if (animator != null)
                _animators[animator] =
                    animator.cullingMode;

        foreach (var renderer in
                 profiler.CharacterRenderers)
        {
            if (renderer == null) continue;

            _rendererEnabled[renderer] =
                renderer.enabled;

            _shadowCasting[renderer] =
                renderer.shadowCastingMode;

            _receiveShadows[renderer] =
                renderer.receiveShadows;

            _materials[renderer] =
                renderer.sharedMaterials;
        }

        foreach (var renderer in
                 profiler.CharacterSkinnedRenderers)
            if (renderer != null)
                _updateWhenOffscreen[renderer] =
                    renderer.updateWhenOffscreen;

        foreach (var light in
                 Resources.FindObjectsOfTypeAll<Light>())
        {
            if (light == null ||
                !light.gameObject.scene.IsValid())
                continue;

            _lights[light] =
                light.shadows;
        }
    }

    public void Restore()
    {
        foreach (var pair in _animators)
            if (pair.Key != null)
                pair.Key.cullingMode =
                    pair.Value;

        foreach (var pair in
                 _rendererEnabled)
            if (pair.Key != null)
                pair.Key.enabled =
                    pair.Value;

        foreach (var pair in
                 _shadowCasting)
            if (pair.Key != null)
                pair.Key.shadowCastingMode =
                    pair.Value;

        foreach (var pair in
                 _receiveShadows)
            if (pair.Key != null)
                pair.Key.receiveShadows =
                    pair.Value;

        foreach (var pair in
                 _updateWhenOffscreen)
            if (pair.Key != null)
                pair.Key.updateWhenOffscreen =
                    pair.Value;

        foreach (var pair in _lights)
            if (pair.Key != null)
                pair.Key.shadows =
                    pair.Value;

        foreach (var pair in
                 _materials)
            if (pair.Key != null)
                pair.Key.sharedMaterials =
                    pair.Value;
    }
}
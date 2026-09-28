using UnityEngine;

namespace TCGProfiler.Testing;

public sealed class QualitySnapshot
{
    public QualitySnapshot()
    {
        QualityLevel =
            QualitySettings.GetQualityLevel();

        ShadowDistance =
            QualitySettings.shadowDistance;

        ShadowCascades =
            QualitySettings.shadowCascades;

        ShadowResolution =
            QualitySettings.shadowResolution;

        ShadowProjection =
            QualitySettings.shadowProjection;

        ShadowNearPlaneOffset =
            QualitySettings.shadowNearPlaneOffset;

        AntiAliasing =
            QualitySettings.antiAliasing;

        LodBias =
            QualitySettings.lodBias;

        PixelLightCount =
            QualitySettings.pixelLightCount;

        AnisotropicFiltering =
            QualitySettings.anisotropicFiltering;

        RealtimeReflectionProbes =
            QualitySettings.realtimeReflectionProbes;
    }

    public int QualityLevel { get; }

    public float ShadowDistance { get; }

    public int ShadowCascades { get; }

    public ShadowResolution ShadowResolution { get; }

    public ShadowProjection ShadowProjection { get; }

    public float ShadowNearPlaneOffset { get; }

    public int AntiAliasing { get; }

    public float LodBias { get; }

    public int PixelLightCount { get; }

    public AnisotropicFiltering AnisotropicFiltering { get; }

    public bool RealtimeReflectionProbes { get; }

    public void Restore()
    {
        QualitySettings.SetQualityLevel(
            QualityLevel,
            true
        );

        // Then explicitly restore these in case the game had
        // individually overridden anything in the preset.
        QualitySettings.shadowDistance =
            ShadowDistance;

        QualitySettings.shadowCascades =
            ShadowCascades;

        QualitySettings.shadowResolution =
            ShadowResolution;

        QualitySettings.shadowProjection =
            ShadowProjection;

        QualitySettings.shadowNearPlaneOffset =
            ShadowNearPlaneOffset;

        QualitySettings.antiAliasing =
            AntiAliasing;

        QualitySettings.lodBias =
            LodBias;

        QualitySettings.pixelLightCount =
            PixelLightCount;

        QualitySettings.anisotropicFiltering =
            AnisotropicFiltering;

        QualitySettings.realtimeReflectionProbes =
            RealtimeReflectionProbes;
    }
}
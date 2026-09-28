using TCGProfiler.Testing;

namespace TCGProfiler.TestCases;

public sealed class CheapCharacterTransparentShaderTestCase
    : CharacterShaderReplacementTestCase
{
    public CheapCharacterTransparentShaderTestCase(
        bool enabled = true)
        : base(
            "CheapShader_Transparent",
            enabled)
    {
    }

    protected override bool ShouldReplaceShader(
        string shaderName)
    {
        return
            EqualsShader(
                shaderName,
                "Shader Graphs/Shader_Transparent"
            ) ||
            EqualsShader(
                shaderName,
                "Shader Graphs/Shader_Eye_Water"
            );
    }
}
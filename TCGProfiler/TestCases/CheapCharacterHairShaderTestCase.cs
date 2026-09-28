using TCGProfiler.Testing;

namespace TCGProfiler.TestCases;

public sealed class CheapCharacterHairShaderTestCase
    : CharacterShaderReplacementTestCase
{
    public CheapCharacterHairShaderTestCase(
        bool enabled = true)
        : base(
            "CheapShader_Hair",
            enabled)
    {
    }

    protected override bool ShouldReplaceShader(
        string shaderName)
    {
        return EqualsShader(
            shaderName,
            "HDRP/Shader_Hair"
        );
    }
}
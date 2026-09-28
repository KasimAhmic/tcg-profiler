using TCGProfiler.Testing;

namespace TCGProfiler.TestCases;

public sealed class CheapCharacterSkinShaderTestCase
    : CharacterShaderReplacementTestCase
{
    public CheapCharacterSkinShaderTestCase(
        bool enabled = true)
        : base(
            "CheapShader_Skin",
            enabled)
    {
    }

    protected override bool ShouldReplaceShader(
        string shaderName)
    {
        return
            EqualsShader(
                shaderName,
                "Shader Graphs/Shader_Skin_Body"
            ) ||
            EqualsShader(
                shaderName,
                "Shader Graphs/Shader_Skin_Head"
            );
    }
}
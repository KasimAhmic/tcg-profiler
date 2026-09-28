using TCGProfiler.Testing;

namespace TCGProfiler.TestCases;

public sealed class CheapCharacterApparelShaderTestCase
    : CharacterShaderReplacementTestCase
{
    public CheapCharacterApparelShaderTestCase(
        bool enabled = true)
        : base(
            "CheapShader_Apparel",
            enabled)
    {
    }

    protected override bool ShouldReplaceShader(
        string shaderName)
    {
        return EqualsShader(
            shaderName,
            "Shader Graphs/Shader_Apparel"
        );
    }
}
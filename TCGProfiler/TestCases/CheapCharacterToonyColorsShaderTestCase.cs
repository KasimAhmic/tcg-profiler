using TCGProfiler.Testing;

namespace TCGProfiler.TestCases;

public sealed class CheapCharacterToonyColorsShaderTestCase
    : CharacterShaderReplacementTestCase
{
    public CheapCharacterToonyColorsShaderTestCase(
        bool enabled = true)
        : base(
            "CheapShader_ToonyColors",
            enabled)
    {
    }

    protected override bool ShouldReplaceShader(
        string shaderName)
    {
        return ShaderStartsWith(
            shaderName,
            "Toony Colors Pro 2/"
        );
    }
}
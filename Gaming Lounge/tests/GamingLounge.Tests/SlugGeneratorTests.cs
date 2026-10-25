using Gaming_Lounge.Services;

namespace GamingLounge.Tests;

public class SlugGeneratorTests
{
    [Theory]
    [InlineData("RTX Overdrive 03", "rtx-overdrive-03")]
    [InlineData("  PlayStation / Pulse  ", "playstation-pulse")]
    [InlineData("VR___Quantum---Pod!", "vr-quantum-pod")]
    [InlineData("Neon & Nexus", "neon-nexus")]
    public void Generate_NormalizesUrlUnsafeNames(string value, string expected)
    {
        var result = SlugGenerator.Generate(value);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateUnique_WhenBaseAndSuffixesExist_UsesNextAvailableSuffix()
    {
        string[] existingSlugs = ["rtx-overdrive-03", "rtx-overdrive-03-2"];

        var result = SlugGenerator.GenerateUnique(
            "RTX Overdrive 03",
            existingSlugs,
            maxLength: 120,
            fallback: "station");

        Assert.Equal("rtx-overdrive-03-3", result);
    }

    [Fact]
    public void GenerateUnique_WhenComparisonDiffersByCase_StillAvoidsCollision()
    {
        string[] existingSlugs = ["ELITE-PC-ARENA"];

        var result = SlugGenerator.GenerateUnique(
            "Elite PC Arena",
            existingSlugs,
            maxLength: 80,
            fallback: "category");

        Assert.Equal("elite-pc-arena-2", result);
    }

    [Fact]
    public void GenerateUnique_WhenNameHasNoSupportedCharacters_UsesFallback()
    {
        var result = SlugGenerator.GenerateUnique(
            "!!!",
            [],
            maxLength: 120,
            fallback: "station");

        Assert.Equal("station", result);
    }

    [Fact]
    public void GenerateUnique_WhenSuffixIsRequired_StaysWithinMaximumLength()
    {
        var longName = new string('a', 20);
        string[] existingSlugs = [longName];

        var result = SlugGenerator.GenerateUnique(
            longName,
            existingSlugs,
            maxLength: 20,
            fallback: "station");

        Assert.Equal("aaaaaaaaaaaaaaaaaa-2", result);
        Assert.Equal(20, result.Length);
    }
}

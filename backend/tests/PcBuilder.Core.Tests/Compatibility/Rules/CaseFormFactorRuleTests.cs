using PcBuilder.Core.Compatibility;
using PcBuilder.Core.Compatibility.Rules;
using PcBuilder.Core.Enums;

namespace PcBuilder.Core.Tests.Compatibility.Rules;

public class CaseFormFactorRuleTests
{
    private const FormFactorSupport AllSizes =
        FormFactorSupport.ATX | FormFactorSupport.MicroATX | FormFactorSupport.MiniITX;

    private const FormFactorSupport MicroAndMini =
        FormFactorSupport.MicroATX | FormFactorSupport.MiniITX;

    private readonly CaseFormFactorRule _rule = new();

    [Theory]
    [InlineData(FormFactor.ATX, AllSizes)]
    [InlineData(FormFactor.MicroATX, AllSizes)]       // smaller board in a bigger case is fine
    [InlineData(FormFactor.MiniITX, AllSizes)]
    [InlineData(FormFactor.MicroATX, MicroAndMini)]
    [InlineData(FormFactor.MiniITX, FormFactorSupport.MiniITX)]
    public void SupportedFormFactor_ReturnsNoIssue(FormFactor board, FormFactorSupport caseSupport)
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(formFactor: board),
            Case = TestParts.Case(supportedFormFactors: caseSupport)
        };

        Assert.Empty(_rule.Check(build));
    }

    [Theory]
    [InlineData(FormFactor.ATX, MicroAndMini)]
    [InlineData(FormFactor.ATX, FormFactorSupport.MiniITX)]
    [InlineData(FormFactor.MicroATX, FormFactorSupport.MiniITX)]
    public void UnsupportedFormFactor_ReturnsError(FormFactor board, FormFactorSupport caseSupport)
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(formFactor: board),
            Case = TestParts.Case(supportedFormFactors: caseSupport)
        };

        Assert.Equal(Severity.Error, Assert.Single(_rule.Check(build)).Severity);
    }

    [Fact]
    public void AtxBoardInMicroAtxCase_MessageNamesPartsAndSupportedSizes()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(name: "MAG B550 Tomahawk", formFactor: FormFactor.ATX),
            Case = TestParts.Case(name: "MasterBox Q300L", supportedFormFactors: MicroAndMini)
        };

        var issue = Assert.Single(_rule.Check(build));

        Assert.Equal("R05", issue.RuleCode);
        Assert.Equal(
            "MAG B550 Tomahawk anakartı ATX boyutunda, MasterBox Q300L kasası ise sadece " +
            "Micro-ATX, Mini-ITX anakartları destekliyor.",
            issue.Message);
    }

    [Fact]
    public void CaseWithNoSupportedSizes_SaysSoInMessage()
    {
        var build = new BuildSnapshot
        {
            Motherboard = TestParts.Motherboard(formFactor: FormFactor.ATX),
            Case = TestParts.Case(supportedFormFactors: FormFactorSupport.None)
        };

        Assert.Contains("hiçbir anakart boyutunu desteklemiyor", Assert.Single(_rule.Check(build)).Message);
    }

    [Fact]
    public void NoCase_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Motherboard = TestParts.Motherboard(formFactor: FormFactor.ATX) };

        Assert.Empty(_rule.Check(build));
    }

    [Fact]
    public void NoMotherboard_RuleIsSkipped()
    {
        var build = new BuildSnapshot { Case = TestParts.Case(supportedFormFactors: FormFactorSupport.MiniITX) };

        Assert.Empty(_rule.Check(build));
    }
}

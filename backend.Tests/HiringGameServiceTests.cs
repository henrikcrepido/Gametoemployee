using backend.Services;

namespace backend.Tests;

public sealed class HiringGameServiceTests
{
    private readonly HiringGameService service = new();

    [Fact]
    public void Evaluate_UnlocksFullStackFixer_WhenFrontendAndBackendIssuesAreResolved()
    {
        var evaluation = service.Evaluate(["issue-react-refresh", "issue-api-validation"]);

        Assert.Equal(65, evaluation.CompletionPercent);
        Assert.Contains(evaluation.Achievements, achievement => achievement.Name == "Full-Stack Fixer" && achievement.Unlocked);
        Assert.Contains(evaluation.SkillRatings, rating => rating.Skill == "React" && rating.Score == 100);
        Assert.Contains(evaluation.SkillRatings, rating => rating.Skill == ".NET" && rating.Score == 100);
    }

    [Fact]
    public void Evaluate_ReportsReleaseCandidate_WhenEveryIssueIsResolved()
    {
        var snapshot = service.GetSnapshot();
        var evaluation = service.Evaluate(snapshot.Issues.Select(issue => issue.Id).ToArray());

        Assert.Equal(100, evaluation.CompletionPercent);
        Assert.Equal("Release candidate", evaluation.CurrentStage.Title);
        Assert.All(evaluation.Achievements, achievement => Assert.True(achievement.Unlocked));
    }
}

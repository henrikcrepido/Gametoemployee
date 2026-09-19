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
        Assert.Equal(2, evaluation.ResolvedIssues.Count);
        Assert.Equal(2, evaluation.OpenIssues.Count);
        Assert.Contains(evaluation.Achievements, achievement => achievement.Name == "Full-Stack Fixer" && achievement.Unlocked);
        Assert.Contains(evaluation.SkillRatings, rating => rating.Skill == "React" && rating.Score == 100);
        Assert.Contains(evaluation.SkillRatings, rating => rating.Skill == ".NET" && rating.Score == 100);
        Assert.Contains(evaluation.ResolvedIssues, issue => issue.Id == "issue-api-validation" && issue.Complexity == "Hard");
    }

    [Fact]
    public void Evaluate_ReportsReleaseCandidate_WhenEveryIssueIsResolved()
    {
        var snapshot = service.GetSnapshot();
        var evaluation = service.Evaluate(snapshot.Issues.Select(issue => issue.Id).ToArray());

        Assert.Equal(100, evaluation.CompletionPercent);
        Assert.Equal("Release candidate", evaluation.CurrentStage.Title);
        Assert.Empty(evaluation.OpenIssues);
        Assert.All(evaluation.Achievements, achievement => Assert.True(achievement.Unlocked));
    }

    [Fact]
    public void GetSnapshot_ExposesStartingBugListWithComplexityLevels()
    {
        var snapshot = service.GetSnapshot();

        Assert.Equal(4, snapshot.Issues.Count);
        Assert.All(snapshot.Issues, issue => Assert.False(string.IsNullOrWhiteSpace(issue.Complexity)));
        Assert.Contains(snapshot.Issues, issue => issue.Id == "issue-release-readiness" && issue.Complexity == "Easy");
    }
}

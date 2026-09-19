using backend.Models;

namespace backend.Services;

public sealed class HiringGameService
{
    private readonly IReadOnlyList<GameRequirement> requirements =
    [
        new(
            "req-inbox",
            "Maintain the PIM support inbox",
            "Review incoming maintenance tickets and turn each one into a usable development issue."),
        new(
            "req-fixes",
            "Resolve hidden full-stack bugs",
            "Fix issues across the React frontend and the .NET backend in the PIM system."),
        new(
            "req-review",
            "Prepare code for review and deployment",
            "Show that the candidate can complete fixes, keep quality high, and reach a releasable state.")
    ];

    private readonly IReadOnlyList<MaintenanceTicket> inbox =
    [
        new(
            "ticket-101",
            "Product cards do not refresh after attribute changes",
            "Maintenance Support",
            "High",
            "Merchandisers report stale product data in the PIM overview after edits are saved.",
            ["frontend", "react", "catalog"]),
        new(
            "ticket-102",
            "Validation misses required SKU formatting",
            "Maintenance Support",
            "Critical",
            "The backend accepts malformed SKUs, which breaks downstream integrations.",
            ["backend", ".net", "validation"]),
        new(
            "ticket-103",
            "Deployments fail without smoke-test evidence",
            "Maintenance Support",
            "Medium",
            "Support wants proof that fixes are verified before the candidate claims the ticket is done.",
            ["quality", "testing", "release"])
    ];

    private readonly IReadOnlyList<PimIssue> issues =
    [
        new(
            "issue-react-refresh",
            "ticket-101",
            "Refresh the product overview after edits",
            "Ensure the React UI shows updated PIM values after a save so reviewers can verify the fix.",
            "React",
            30,
            "Medium"),
        new(
            "issue-api-validation",
            "ticket-102",
            "Enforce SKU validation in the API",
            "Reject invalid SKUs in the .NET backend before they reach the integration layer.",
            ".NET",
            35,
            "Hard"),
        new(
            "issue-review-proof",
            "ticket-103",
            "Add verifiable test coverage for maintenance fixes",
            "Back the candidate's changes with tests so the simulated deployment review can pass.",
            "Testing",
            20,
            "Medium"),
        new(
            "issue-release-readiness",
            "ticket-103",
            "Document release readiness for deployment review",
            "Summarize what was fixed and which blocker tickets are safe to release.",
            "Delivery",
            15,
            "Easy")
    ];

    public GameSnapshot GetSnapshot()
    {
        return new(
            "PIM Quest",
            "A candidate improves a product information management system by handling maintenance tickets, resolving hidden bugs, and proving the fixes are ready for review.",
            requirements,
            inbox,
            issues,
            Evaluate(Array.Empty<string>()));
    }

    public GameEvaluation Evaluate(IReadOnlyList<string>? resolvedIssueIds)
    {
        var resolvedIds = new HashSet<string>(resolvedIssueIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var resolvedIssues = issues.Where(issue => resolvedIds.Contains(issue.Id)).ToArray();
        var openIssues = issues.Where(issue => !resolvedIds.Contains(issue.Id)).ToArray();
        var totalPoints = issues.Sum(issue => issue.Points);
        var earnedPoints = resolvedIssues.Sum(issue => issue.Points);
        var completionPercent = totalPoints == 0 ? 0 : (int)Math.Round(earnedPoints * 100d / totalPoints, MidpointRounding.AwayFromZero);

        var skillRatings = issues
            .GroupBy(issue => issue.SkillArea)
            .Select(group =>
            {
                var skillPoints = group.Sum(issue => issue.Points);
                var earnedSkillPoints = resolvedIssues.Where(issue => issue.SkillArea == group.Key).Sum(issue => issue.Points);
                var score = skillPoints == 0 ? 0 : (int)Math.Round(earnedSkillPoints * 100d / skillPoints, MidpointRounding.AwayFromZero);
                return new SkillRating(group.Key, score, GetLevel(score));
            })
            .OrderByDescending(skill => skill.Score)
            .ThenBy(skill => skill.Skill, StringComparer.Ordinal)
            .ToArray();

        var achievements = new[]
        {
            new Achievement(
                "Inbox Triage",
                "Resolve at least one maintenance ticket from the PIM inbox.",
                resolvedIssues.Length >= 1),
            new Achievement(
                "Full-Stack Fixer",
                "Resolve both the React and .NET issues in the PIM workflow.",
                resolvedIssues.Any(issue => issue.SkillArea == "React") && resolvedIssues.Any(issue => issue.SkillArea == ".NET")),
            new Achievement(
                "Review Ready",
                "Complete the testing and release-readiness work needed for simulated deployment review.",
                resolvedIds.Contains("issue-review-proof") && resolvedIds.Contains("issue-release-readiness")),
            new Achievement(
                "PIM Champion",
                "Clear every mapped issue in the maintenance sprint.",
                resolvedIssues.Length == issues.Count)
        };

        return new(
            resolvedIssues.Select(issue => issue.Id).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            openIssues,
            resolvedIssues,
            completionPercent,
            GetStage(completionPercent),
            skillRatings,
            achievements,
            BuildSummary(completionPercent, resolvedIssues.Length));
    }

    private static string GetLevel(int score) =>
        score switch
        {
            >= 85 => "Expert",
            >= 60 => "Advanced",
            >= 35 => "Intermediate",
            > 0 => "Emerging",
            _ => "Not started"
        };

    private static GameStage GetStage(int completionPercent) =>
        completionPercent switch
        {
            >= 100 => new("Release candidate", "The candidate has cleared every PIM ticket and is ready for final review."),
            >= 65 => new("Feature driver", "The candidate can resolve cross-stack issues and move the maintenance sprint forward."),
            >= 30 => new("Bug solver", "The candidate is delivering meaningful fixes but still has release blockers open."),
            > 0 => new("Support trainee", "The candidate has started triage and landed an initial fix."),
            _ => new("Briefed", "The candidate has reviewed the backlog but has not resolved an issue yet.")
        };

    private static string BuildSummary(int completionPercent, int resolvedCount)
    {
        return resolvedCount switch
        {
            0 => "Start by picking a support ticket from the PIM inbox and turning it into a verified fix.",
            _ when completionPercent >= 100 => "All maintenance tickets are covered, the hidden bugs are resolved, and the build is ready for review.",
            _ => $"The candidate has resolved {resolvedCount} issue(s) and completed {completionPercent}% of the maintenance sprint."
        };
    }
}

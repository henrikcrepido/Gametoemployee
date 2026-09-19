namespace backend.Models;

public sealed record GameRequirement(string Id, string Title, string Description);

public sealed record MaintenanceTicket(
    string Id,
    string Title,
    string Requester,
    string Priority,
    string Description,
    IReadOnlyList<string> Tags);

public sealed record PimIssue(
    string Id,
    string TicketId,
    string Title,
    string Description,
    string SkillArea,
    int Points,
    string Complexity);

public sealed record SkillRating(string Skill, int Score, string Level);

public sealed record Achievement(string Name, string Description, bool Unlocked);

public sealed record GameStage(string Title, string Description);

public sealed record GameEvaluation(
    IReadOnlyList<string> ResolvedIssueIds,
    IReadOnlyList<PimIssue> OpenIssues,
    IReadOnlyList<PimIssue> ResolvedIssues,
    int CompletionPercent,
    GameStage CurrentStage,
    IReadOnlyList<SkillRating> SkillRatings,
    IReadOnlyList<Achievement> Achievements,
    string Summary);

public sealed record GameSnapshot(
    string ProductName,
    string Scenario,
    IReadOnlyList<GameRequirement> Requirements,
    IReadOnlyList<MaintenanceTicket> Inbox,
    IReadOnlyList<PimIssue> Issues,
    GameEvaluation Evaluation);

public sealed record EvaluationRequest(IReadOnlyList<string>? ResolvedIssueIds);

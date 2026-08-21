namespace Lorcaire.Application.Dashboard;

public sealed record DashboardSummary(
    string Greeting,
    int GoalCount,
    int ActiveGoalCount,
    int ProjectCount,
    int PendingTaskCount,
    int ResourceCount,
    IReadOnlyList<DashboardTaskItem> PendingTasks,
    IReadOnlyList<DashboardEventItem> UpcomingEvents,
    IReadOnlyList<DashboardActivityItem> RecentActivity);

public sealed record DashboardTaskItem(
    Guid Id,
    string Title,
    string? Description,
    string? ProjectName);

public sealed record DashboardEventItem(
    Guid Id,
    string Title,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt);

public sealed record DashboardActivityItem(
    Guid Id,
    string Description,
    DateTimeOffset OccurredAt);

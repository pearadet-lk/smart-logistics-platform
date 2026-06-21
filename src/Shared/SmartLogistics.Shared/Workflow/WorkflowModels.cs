namespace SmartLogistics.Shared.Workflow;

public enum WorkflowStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected,
    Published
}

public sealed class WorkflowInstance
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public WorkflowStatus Status { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public sealed class WorkflowStep
{
    public Guid Id { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; }
    public int Order { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public sealed class ApprovalHistory
{
    public Guid Id { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
}

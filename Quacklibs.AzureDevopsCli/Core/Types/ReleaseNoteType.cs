namespace Quacklibs.AzureDevopsCli.Core.Types;

public record ReleaseNoteType(string Title)
{
    public List<ReleasedFeature> Features { get; } = new();

    public void Add(ReleasedFeature feature)
    {
        Features.Add(feature);
    }
}


public record ReleasedFeature(string Status,
                              List<MergedPullRequest> MergedPullRequest,
                              List<ReleasedWorkItem> WorkItems)
{
    public required int Id { get; init; }
    public string Status { get; set; }

    public List<MergedPullRequest> PullRequest { get; set; }
    public List<ReleasedWorkItem> WorkItemPart { get; set; }



}


public record MergedPullRequest(string Submitter, string Title, string Details);

public record ReleasedWorkItem(string Name, string Status);


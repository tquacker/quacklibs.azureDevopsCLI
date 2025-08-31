using McMaster.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.Location.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Quacklibs.AzureDevopsCli.Core.Behavior;
using Quacklibs.AzureDevopsCli.Services;
using System.Collections.Concurrent;
using System.Net;

namespace Quacklibs.AzureDevopsCli.Commands.PullRequests
{
    [Command("read", "r")]
    internal class PullRequestReadCommand : BaseCommand
    {
        private readonly AzureDevopsService _service;
        private readonly SettingsService _optionsService;

        public PullRequestReadCommand(AzureDevopsService service, SettingsService optionsService)
        {
            _service = service;
            _optionsService = optionsService;
        }

        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var gitClient = _service.GetClient<GitHttpClient>();
            var identityClient = _service.GetClient<LocationHttpClient>();
            var userEmail = _optionsService.Settings.GetSettingSafe(e => EnvironmentSettings.UserEmail);

            //var identitiesWithThisEmail = await identityClient.ReadIdentitiesAsync(IdentitySearchFilter.MailAddress, filterValue: base.EnvironmentSettings.UserEmail);
            var identiesWithThisUserId = await identityClient.GetConnectionDataAsync(ConnectOptions.None, lastChangeId: -1);

            Console.WriteLine($"Querying for {identiesWithThisUserId.AuthenticatedUser.DisplayName}");
            var identity = identiesWithThisUserId.AuthenticatedUser.Id;

            // if (!EnsureOneIdentity(identitiesWithThisEmail, userEmail))
            //     return ExitCodes.Error;

            List<GitRepository> repositories = await gitClient.GetRepositoriesAsync();
            var sanitizedRepos = repositories.Where(e => e?.IsDisabled is false)
                                             .Where(e => e?.IsInMaintenance is false)
                                             .ToList();
                                   

            var allRelevantPrs = new ConcurrentBag<GitPullRequest>();

            AnsiConsole.Write($"\n Polling {repositories.Count} non disabled, non in-maintanance repo's for pr's. this may take a while \n ");

            await Parallel.ForEachAsync(sanitizedRepos, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, async (repo, cancellationToken) =>
            {
                try
                {
                    // Get PRs where current user is the creator
                    var createdPRs = await gitClient.GetPullRequestsAsync(repo.Id, new GitPullRequestSearchCriteria
                    {
                        Status = PullRequestStatus.Active, CreatorId = identiesWithThisUserId.AuthenticatedUser.Id
                    }, cancellationToken: cancellationToken);

                    // Get PRs where current user is a reviewer
                    var reviewerPRs = await gitClient.GetPullRequestsAsync(repo.Id, new GitPullRequestSearchCriteria { Status = PullRequestStatus.Active, ReviewerId = identiesWithThisUserId.AuthenticatedUser.Id }, cancellationToken: cancellationToken);

                    // Combine and deduplicate by PR ID
                    createdPRs.AddRange(reviewerPRs);
                    var combinedCollection = createdPRs.DistinctBy(e => e.PullRequestId);

                    foreach (var review in combinedCollection)
                    {
                        allRelevantPrs.Add(review);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"repo {repo.Name} failed: {ex.Message}");
                }
            });

            if (!allRelevantPrs.Any())
            {
                Console.WriteLine("No pr's");
            }
            var table = TableBuilder<GitPullRequest>
                        .Create()
                        .WithTitle("Pull requests")
                        .WithColumn("Id", new(e => e.PullRequestId.ToString()))
                        .WithColumn("Title", new(e => e.Title))
                        .WithColumn("Date", new(e => e.CreationDate.ToString("dd-MM-yyyy")))
                        .WithColumn("Repo", new(e => e.Repository?.Name))
                        .WithColumn("Submitter", new(e => e.CreatedBy?.DisplayName))
                        .WithColumn("IsReviewed", new(e => e.Reviewers.Any(rv => rv.Vote >= 5) ? "true" : "false"))
                        .WithColumn("Link", new(e => e.RemoteUrl?.ToString()?.AsUrlMarkup()))
                        .WithRows(allRelevantPrs)
                        .Build();

            AnsiConsole.Write(table);

            return ExitCodes.Ok;
        }

        private static bool EnsureOneIdentity(IdentitiesCollection identitiesWithThisEmail, string userEmail)
        {
            if (!identitiesWithThisEmail.Any())
            {
                AnsiConsole.WriteLine($"No identity found for {userEmail}");
                return false;
            }
            else if (identitiesWithThisEmail.Count > 1)
            {
                AnsiConsole.WriteLine($"multiple identities found for {userEmail}, this is not supported");
                return false;
            }
            else
            {
                AnsiConsole.WriteLine($"Searching PR's for {identitiesWithThisEmail.First().DisplayName}");
                return true;
            }
        }
    }
}
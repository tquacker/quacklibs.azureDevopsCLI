using McMaster.Extensions.CommandLineUtils;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.Profile;
using Microsoft.VisualStudio.Services.Profile.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Quacklibs.AzureDevopsCli.Core.Behavior;
using Quacklibs.AzureDevopsCli.Services;
using System.Collections.Concurrent;

namespace Quacklibs.AzureDevopsCli.Commands.PullRequests
{
    [Command("read", "r")]
    internal class PullRequestReadCommand : BaseCommand
    {
        private readonly AzureDevopsService _service;
        private readonly AppOptionsService _optionsService;

        public PullRequestReadCommand(AzureDevopsService service, AppOptionsService optionsService)
        {
            _service = service;
            _optionsService = optionsService;
        }

        public override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            var gitClient = _service.GetClient<GitHttpClient>();
            var identityClient = _service.GetClient<IdentityHttpClient>();
            var userEmail = _optionsService.Defaults.GetSettingSafe(e => e.UserEmail);
     
            var identitiesWithThisEmail = await identityClient.ReadIdentitiesAsync(IdentitySearchFilter.General, filterValue: _optionsService.Defaults.UserEmail);

            if (!EnsureOneIdentity(identitiesWithThisEmail, userEmail))
                return ExitCodes.Error;

            List<GitRepository> repositories = await gitClient.GetRepositoriesAsync();

            var allRelevantPrs = new ConcurrentBag<GitPullRequest>();

            AnsiConsole.Write($"\n Polling {repositories.Count} repo's for pr's. this may take a while \n ");

            await Parallel.ForEachAsync(repositories, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, async (repo, cancellationToken) =>
            {
                // Get PRs where current user is the creator
                var createdPRs = await gitClient.GetPullRequestsAsync(repo.Id, new GitPullRequestSearchCriteria
                {
                    Status = PullRequestStatus.Active,
                    CreatorId = identitiesWithThisEmail.First().Id
                }, cancellationToken: cancellationToken);

                // Get PRs where current user is a reviewer
                var reviewerPRs = await gitClient.GetPullRequestsAsync(repo.Id, new GitPullRequestSearchCriteria
                {
                    Status = PullRequestStatus.Active, 
                    ReviewerId = identitiesWithThisEmail.First().Id
                }, cancellationToken: cancellationToken);

                // Combine and deduplicate by PR ID
                var combinedCollection = createdPRs.Concat(reviewerPRs)
                                                   .GroupBy(pr => pr.PullRequestId)
                                                   .Select(g => g.First())
                                                   .ToList();

                foreach (var review in combinedCollection)
                {
                    allRelevantPrs.Add(review);
                }
            });

            var table = new TableBuilder<GitPullRequest>().WithColumn("Id", new(e => e.PullRequestId.ToString()))
                                                          .WithColumn("Title", new(e => e.Title))
                                                          .WithColumn("Date", new(e => e.CreationDate.ToString("dd-MM-yyyy")))
                                                          .WithColumn("Repo", new(e => e.Repository.Name))
                                                          .WithColumn("Submitter", new(e => e.CreatedBy.DisplayName))
                                                          .WithColumn("IsReviewed", new(e => e.Reviewers.Any(rv => rv.Vote >= 5) ? "true" : "false"))
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
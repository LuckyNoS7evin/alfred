
using Bot.Core;
using System.Threading.Tasks;

namespace Bot.Data.Interfaces
{
    public interface IGitHubCredentialRepository
    {
        Task<bool> SaveAsync(GitHubCredentials ownerGitHubCredentials);
        Task<GitHubCredentials> GetAsync(ulong userId);
    }
}

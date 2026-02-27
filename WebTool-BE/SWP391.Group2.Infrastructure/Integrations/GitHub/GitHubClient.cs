using SWP391.Group2.Application.Abstractions.GitHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SWP391.Group2.Infrastructure.Integrations.GitHub
{
    public class GitHubClient : IGitHubClient
    {
        private readonly HttpClient _http;

        public GitHubClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<GitHubCommitDto>> GetCommitsAsync(
            string org,
            string repoName,
            DateTime fromUtc,
            DateTime toUtc,
            string token,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(org)) throw new ArgumentException("org is required");
            if (string.IsNullOrWhiteSpace(repoName)) throw new ArgumentException("repoName is required");
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("token is required");

            // GitHub API: GET /repos/{owner}/{repo}/commits?since=...&until=...
            // ISO 8601 required
            var since = Uri.EscapeDataString(fromUtc.ToString("O"));
            var until = Uri.EscapeDataString(toUtc.ToString("O"));

            var url = $"https://api.github.com/repos/{org}/{repoName}/commits?since={since}&until={until}&per_page=100";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Headers.UserAgent.ParseAdd("SWP391-WebTool/1.0");          // bắt buộc với GitHub
            req.Headers.Accept.ParseAdd("application/vnd.github+json");
            req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

            using var res = await _http.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new Exception($"GitHub API failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {Trim(body)}");

            // Parse JSON (list)
            using var doc = JsonDocument.Parse(body);

            var list = new List<GitHubCommitDto>();

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var sha = item.GetProperty("sha").GetString() ?? "";
                var htmlUrl = item.TryGetProperty("html_url", out var hu) ? hu.GetString() : null;

                var commit = item.GetProperty("commit");
                var message = commit.GetProperty("message").GetString() ?? "";

                // committed date (committer.date)
                var committer = commit.GetProperty("committer");
                var dateStr = committer.GetProperty("date").GetString() ?? "";
                var committedAt = DateTime.Parse(dateStr).ToUniversalTime();

                string? authorLogin = null;
                if (item.TryGetProperty("author", out var author) && author.ValueKind != JsonValueKind.Null)
                {
                    if (author.TryGetProperty("login", out var login))
                        authorLogin = login.GetString();
                }

                list.Add(new GitHubCommitDto(
                    sha,
                    message,
                    committedAt,
                    htmlUrl ?? "",
                    authorLogin
                ));
            }

            return list;
        }

        private static string Trim(string s)
            => s.Length <= 300 ? s : s[..300];
    }
}

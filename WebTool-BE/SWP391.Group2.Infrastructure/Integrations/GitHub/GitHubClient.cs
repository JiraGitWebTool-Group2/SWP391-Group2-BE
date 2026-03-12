using SWP391.Group2.Application.Abstractions.GitHub;
using System.Net.Http.Headers;
using System.Text.Json;

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
            if (string.IsNullOrWhiteSpace(org))
                throw new ArgumentException("org is required");

            if (string.IsNullOrWhiteSpace(repoName))
                throw new ArgumentException("repoName is required");

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("token is required");

            if (fromUtc > toUtc)
                throw new ArgumentException("fromUtc must be less than or equal to toUtc");

            var since = Uri.EscapeDataString(fromUtc.ToString("O"));
            var until = Uri.EscapeDataString(toUtc.ToString("O"));

            var results = new List<GitHubCommitDto>();
            var page = 1;
            const int pageSize = 100;

            while (true)
            {
                var url =
                    $"https://api.github.com/repos/{org}/{repoName}/commits" +
                    $"?since={since}&until={until}&per_page={pageSize}&page={page}";

                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Headers.UserAgent.ParseAdd("SWP391-WebTool/1.0");
                req.Headers.Accept.ParseAdd("application/vnd.github+json");
                req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

                using var res = await _http.SendAsync(req, ct);
                var body = await res.Content.ReadAsStringAsync(ct);

                if (!res.IsSuccessStatusCode)
                {
                    throw new Exception(
                        $"GitHub API failed for repo '{org}/{repoName}' at page {page}: " +
                        $"{(int)res.StatusCode} {res.ReasonPhrase}. Body: {Trim(body)}");
                }

                using var doc = JsonDocument.Parse(body);

                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    throw new Exception($"GitHub API returned invalid commit list for repo '{org}/{repoName}'.");

                var countThisPage = 0;

                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (!item.TryGetProperty("sha", out var shaEl)) continue;
                    if (!item.TryGetProperty("commit", out var commitEl)) continue;
                    if (!commitEl.TryGetProperty("message", out var msgEl)) continue;
                    if (!commitEl.TryGetProperty("committer", out var committerEl)) continue;
                    if (!committerEl.TryGetProperty("date", out var dateEl)) continue;

                    var sha = shaEl.GetString() ?? "";
                    var message = msgEl.GetString() ?? "";
                    var dateStr = dateEl.GetString() ?? "";

                    if (string.IsNullOrWhiteSpace(sha) || string.IsNullOrWhiteSpace(dateStr))
                        continue;

                    if (!DateTimeOffset.TryParse(dateStr, out var parsedDate))
                        continue;

                    var htmlUrl = item.TryGetProperty("html_url", out var hu)
                        ? hu.GetString()
                        : null;

                    string? authorLogin = null;
                    if (item.TryGetProperty("author", out var author) &&
                        author.ValueKind != JsonValueKind.Null &&
                        author.TryGetProperty("login", out var login))
                    {
                        authorLogin = login.GetString();
                    }

                    results.Add(new GitHubCommitDto(
                        sha,
                        message,
                        parsedDate.UtcDateTime,
                        htmlUrl ?? "",
                        authorLogin
                    ));

                    countThisPage++;
                }

                if (countThisPage < pageSize)
                    break;

                page++;
            }

            return results;
        }

        private static string Trim(string? s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return s.Length <= 300 ? s : s[..300];
        }
    }
}
using SWP391.Group2.Application.Abstractions.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SWP391.Group2.Infrastructure.Integrations.Jira
{
    public class JiraClient : IJiraClient
    {
        private readonly HttpClient _http;

        public JiraClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<JiraIssueDto>> SearchIssuesAsync(
            string baseUrl,
            string jql,
            string token,
            string? storyPointsFieldKey,
            string? sprintFieldKey,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentException("baseUrl is required");
            if (string.IsNullOrWhiteSpace(jql)) throw new ArgumentException("jql is required");
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("token is required");

            baseUrl = baseUrl.TrimEnd('/');
            storyPointsFieldKey = string.IsNullOrWhiteSpace(storyPointsFieldKey) ? "customfield_10016" : storyPointsFieldKey.Trim();
            sprintFieldKey = string.IsNullOrWhiteSpace(sprintFieldKey) ? null : sprintFieldKey.Trim();

            var fields = new List<string>
            {
                "summary",
                "description",
                "issuetype",
                "priority",
                "status",
                "assignee",
                "created",
                "updated",
                "resolutiondate",
                "parent"
            };

            if (!string.IsNullOrWhiteSpace(storyPointsFieldKey))
                fields.Add(storyPointsFieldKey);

            if (!string.IsNullOrWhiteSpace(sprintFieldKey))
                fields.Add(sprintFieldKey);

            var pageSize = 100;
            var startAt = 0;
            var results = new List<JiraIssueDto>();

            while (true)
            {
                var url =
                    $"{baseUrl}/rest/api/3/search" +
                    $"?jql={Uri.EscapeDataString(jql)}" +
                    $"&startAt={startAt}" +
                    $"&maxResults={pageSize}" +
                    $"&fields={Uri.EscapeDataString(string.Join(",", fields))}";

                using var req = new HttpRequestMessage(HttpMethod.Get, url);

                if (token.Contains(":"))
                {
                    var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
                    req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
                }
                else
                {
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                req.Headers.Accept.ParseAdd("application/json");

                using var res = await _http.SendAsync(req, ct);
                var body = await res.Content.ReadAsStringAsync(ct);

                if (!res.IsSuccessStatusCode)
                    throw new Exception($"Jira API failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {Trim(body)}");

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                var issuesEl = root.GetProperty("issues");
                var pageCount = 0;

                foreach (var item in issuesEl.EnumerateArray())
                {
                    pageCount++;

                    var key = item.TryGetProperty("key", out var keyEl) ? keyEl.GetString() ?? "" : "";
                    var browseUrl = BuildBrowseUrl(baseUrl, key);

                    var fieldsEl = item.GetProperty("fields");

                    var summary = fieldsEl.TryGetProperty("summary", out var s) ? s.GetString() ?? "" : "";

                    string? description = null;
                    if (fieldsEl.TryGetProperty("description", out var d) && d.ValueKind != JsonValueKind.Null)
                        description = d.GetRawText();

                    var rawIssueType = ExtractName(fieldsEl, "issuetype");
                    var rawPriority = ExtractName(fieldsEl, "priority");
                    var rawStatus = ExtractName(fieldsEl, "status");

                    decimal? storyPoints = null;
                    if (!string.IsNullOrWhiteSpace(storyPointsFieldKey)
                        && fieldsEl.TryGetProperty(storyPointsFieldKey, out var sp)
                        && sp.ValueKind != JsonValueKind.Null)
                    {
                        if (sp.ValueKind == JsonValueKind.Number && sp.TryGetDecimal(out var dec))
                            storyPoints = dec;
                    }

                    string? assigneeAccountId = null;
                    string? assigneeDisplayName = null;
                    if (fieldsEl.TryGetProperty("assignee", out var a) && a.ValueKind != JsonValueKind.Null)
                    {
                        if (a.TryGetProperty("accountId", out var acc))
                            assigneeAccountId = acc.GetString();

                        if (a.TryGetProperty("displayName", out var dn))
                            assigneeDisplayName = dn.GetString();
                    }

                    DateTime? jiraCreatedAt = ParseDateTime(fieldsEl, "created");
                    DateTime? jiraUpdatedAt = ParseDateTime(fieldsEl, "updated");
                    DateTime? jiraResolvedAt = ParseDateTime(fieldsEl, "resolutiondate");

                    string? parentIssueKey = null;
                    if (fieldsEl.TryGetProperty("parent", out var parentEl)
                        && parentEl.ValueKind != JsonValueKind.Null
                        && parentEl.TryGetProperty("key", out var parentKeyEl))
                    {
                        parentIssueKey = parentKeyEl.GetString();
                    }

                    string? sprintExternalId = null;
                    string? sprintName = null;
                    if (!string.IsNullOrWhiteSpace(sprintFieldKey)
                        && fieldsEl.TryGetProperty(sprintFieldKey, out var sprintEl)
                        && sprintEl.ValueKind != JsonValueKind.Null)
                    {
                        if (sprintEl.ValueKind == JsonValueKind.Object)
                        {
                            if (sprintEl.TryGetProperty("id", out var idEl))
                                sprintExternalId = idEl.ToString();

                            if (sprintEl.TryGetProperty("name", out var nameEl))
                                sprintName = nameEl.GetString();
                        }
                        else if (sprintEl.ValueKind == JsonValueKind.Array)
                        {
                            var first = sprintEl.EnumerateArray().FirstOrDefault();
                            if (first.ValueKind == JsonValueKind.Object)
                            {
                                if (first.TryGetProperty("id", out var idEl))
                                    sprintExternalId = idEl.ToString();

                                if (first.TryGetProperty("name", out var nameEl))
                                    sprintName = nameEl.GetString();
                            }
                        }
                    }

                    results.Add(new JiraIssueDto(
                        key,
                        summary,
                        description,
                        NormalizeIssueType(rawIssueType),
                        NormalizePriority(rawPriority),
                        NormalizeStatus(rawStatus),
                        rawIssueType,
                        rawPriority,
                        rawStatus,
                        storyPoints,
                        browseUrl,
                        assigneeAccountId,
                        assigneeDisplayName,
                        jiraCreatedAt,
                        jiraUpdatedAt,
                        jiraResolvedAt,
                        parentIssueKey,
                        sprintExternalId,
                        sprintName
                    ));
                }

                var total = root.TryGetProperty("total", out var totalEl) ? totalEl.GetInt32() : results.Count;
                startAt += pageCount;

                if (pageCount == 0 || startAt >= total)
                    break;
            }

            return results;
        }

        private static string ExtractName(JsonElement fieldsEl, string propName)
        {
            if (!fieldsEl.TryGetProperty(propName, out var p) || p.ValueKind == JsonValueKind.Null)
                return "";

            if (p.TryGetProperty("name", out var name))
                return name.GetString() ?? "";

            return "";
        }

        private static DateTime? ParseDateTime(JsonElement fieldsEl, string propName)
        {
            if (!fieldsEl.TryGetProperty(propName, out var p) || p.ValueKind == JsonValueKind.Null)
                return null;

            var text = p.GetString();
            if (DateTimeOffset.TryParse(text, out var dto))
                return dto.UtcDateTime;

            return null;
        }

        private static string BuildBrowseUrl(string baseUrl, string issueKey)
            => $"{baseUrl}/browse/{issueKey}";

        private static string NormalizeStatus(string s)
        {
            s = (s ?? "").Trim().ToUpperInvariant();
            return s switch
            {
                "TO DO" or "TODO" => "TODO",
                "IN PROGRESS" => "IN_PROGRESS",
                "IN REVIEW" or "REVIEW" => "IN_REVIEW",
                "DONE" => "DONE",
                "BLOCKED" => "BLOCKED",
                _ => "TODO"
            };
        }

        private static string NormalizePriority(string s)
        {
            s = (s ?? "").Trim().ToUpperInvariant();
            return s switch
            {
                "LOW" => "LOW",
                "MEDIUM" => "MEDIUM",
                "HIGH" => "HIGH",
                "HIGHEST" => "HIGHEST",
                _ => "MEDIUM"
            };
        }

        private static string NormalizeIssueType(string s)
        {
            s = (s ?? "").Trim().ToUpperInvariant();
            return s switch
            {
                "EPIC" => "EPIC",
                "STORY" => "STORY",
                "TASK" => "TASK",
                "BUG" => "BUG",
                "SUB-TASK" or "SUBTASK" => "SUBTASK",
                _ => "TASK"
            };
        }

        private static string Trim(string s)
            => s.Length <= 500 ? s : s[..500];
    }
}

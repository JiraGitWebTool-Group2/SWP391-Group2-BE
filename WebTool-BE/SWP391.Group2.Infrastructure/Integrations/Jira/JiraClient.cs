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
            int maxResults,
            string token,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentException("baseUrl is required");
            if (string.IsNullOrWhiteSpace(jql)) throw new ArgumentException("jql is required");
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("token is required");
            if (maxResults <= 0) maxResults = 50;
            if (maxResults > 100) maxResults = 100; // Jira thường giới hạn 100/page

            baseUrl = baseUrl.TrimEnd('/');

            // Jira Cloud: /rest/api/3/search
            // fields chọn vừa đủ để nhẹ payload
            var fields = "summary,description,issuetype,priority,status,customfield_10016,assignee";
            var url =
                $"{baseUrl}/rest/api/3/search" +
                $"?jql={Uri.EscapeDataString(jql)}" +
                $"&maxResults={maxResults}" +
                $"&fields={Uri.EscapeDataString(fields)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);

            // token: với Jira Cloud thường là email:api_token (Basic)
            // Nhưng ProjectIntegrations đang lưu token 1 cục, nên ta hỗ trợ 2 mode:
            // - Nếu token có dấu ':' => coi như "email:apiToken" => Basic
            // - Không có ':' => Bearer (cho self-hosted / PAT)
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

            var issuesEl = doc.RootElement.GetProperty("issues");
            var list = new List<JiraIssueDto>();

            foreach (var item in issuesEl.EnumerateArray())
            {
                var key = item.GetProperty("key").GetString() ?? "";

                var selfUrl = item.TryGetProperty("self", out var selfProp) ? selfProp.GetString() : null;
                var browseUrl = BuildBrowseUrl(baseUrl, key, selfUrl);

                var fieldsEl = item.GetProperty("fields");

                var summary = fieldsEl.TryGetProperty("summary", out var s) ? (s.GetString() ?? "") : "";

                string? description = null;
                // Jira description là Atlassian Document Format (ADF) -> JSON object
                // Ta không cố “giải nén” text ở đây, lưu raw JSON string là safest.
                if (fieldsEl.TryGetProperty("description", out var d) && d.ValueKind != JsonValueKind.Null)
                    description = d.GetRawText();

                var issueType = ExtractName(fieldsEl, "issuetype");
                var priority = ExtractName(fieldsEl, "priority");
                var status = ExtractName(fieldsEl, "status");

                decimal? storyPoints = null;
                // customfield_10016 thường là Story Points ở Jira Cloud, nhưng có thể khác instance.
                // Nếu khác, sau này ta cho config field key trong ProjectIntegrations.
                if (fieldsEl.TryGetProperty("customfield_10016", out var sp) && sp.ValueKind != JsonValueKind.Null)
                {
                    if (sp.ValueKind == JsonValueKind.Number && sp.TryGetDecimal(out var dec)) storyPoints = dec;
                }

                string? assigneeAccountId = null;
                if (fieldsEl.TryGetProperty("assignee", out var a) && a.ValueKind != JsonValueKind.Null)
                {
                    if (a.TryGetProperty("accountId", out var acc))
                        assigneeAccountId = acc.GetString();
                }

                list.Add(new JiraIssueDto(
                    key,
                    summary,
                    description,
                    issueType,
                    priority,
                    status,
                    storyPoints,
                    browseUrl,
                    assigneeAccountId
                ));
            }

            return list;
        }

        private static string ExtractName(JsonElement fieldsEl, string propName)
        {
            if (!fieldsEl.TryGetProperty(propName, out var p) || p.ValueKind == JsonValueKind.Null)
                return "";

            if (p.TryGetProperty("name", out var name))
                return name.GetString() ?? "";

            return "";
        }

        private static string BuildBrowseUrl(string baseUrl, string issueKey, string? selfUrl)
        {
            // Jira Cloud thường browse link dạng: {baseUrl}/browse/{KEY}
            // Self-hosted cũng thường vậy. Dùng cái này cho thống nhất.
            return $"{baseUrl}/browse/{issueKey}";
        }

        private static string Trim(string s)
            => s.Length <= 300 ? s : s[..300];
    }
}

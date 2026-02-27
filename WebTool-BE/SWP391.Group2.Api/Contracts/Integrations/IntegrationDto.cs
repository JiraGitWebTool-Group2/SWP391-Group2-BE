namespace SWP391.Group2.Api.Contracts.Integrations
{
    public record IntegrationDto(
        int ProjectId,
        string Provider,
        string? BaseUrl,
        string? ProjectKey,
        string? Org,
        bool HasToken,           // không trả token ra
        DateTime UpdatedAt
    );
}

namespace Api.IntegrationTests.Common;

public record ErrorResponse(string Type, string Title, string Detail, int Status);

public record ValidationErrorResponse(
    string Type,
    string Title,
    string Detail,
    int Status,
    Dictionary<string, string[]> Errors);

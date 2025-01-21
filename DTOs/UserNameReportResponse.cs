namespace SimilarityDemo.DTOs;

public record UserNameReportResponse(bool IsAvailable, double Score, List<string> Suggestions);

namespace ExitInterviewSystem.Helpers
{
    public interface IPagerInfo
    {
        int Page { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        int FromRecord { get; }
        int ToRecord { get; }
    }
}

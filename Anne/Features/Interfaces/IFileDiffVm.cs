namespace Anne.Features.Interfaces
{
    public interface IFileDiffVm
    {
        string Path { get; }
        string Diff { get; }
        DiffLine[] DiffLines { get; }
    }
}
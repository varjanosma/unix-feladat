namespace Lecture2Lab.Sources;

public sealed class OrderImportException : Exception
{
    public OrderImportException(string path, Exception inner)
        : base($"Could not read orders from '{path}': {inner.Message}", inner) => Path = path;

    public string Path { get; }
}

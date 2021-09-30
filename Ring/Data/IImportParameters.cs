namespace Ring.Data
{
    public interface IImportParameters
    {
        string File { get; set; }
        string SchemaName { get; set; }
        bool TrimValue { get; set; }

    }
}

namespace Ring.Data.Models
{
    internal sealed class ImportParameters : IImportParameters
    {
        public string File { get; set; }
        public string SchemaName { get; set; }
        public bool TrimValue { get; set; }
    }
}

namespace Ring.Schema.Models
{
    internal sealed class LexiconIndex
    {
        internal readonly int FieldId;
        internal readonly int Index;

        public LexiconIndex(int fieldId, int lexiconIndex)
        {
            FieldId = fieldId;
            Index = lexiconIndex;
        }
#if DEBUG
	    public override string ToString()
	    {
		    return FieldId  +"," + Index;
	    }
#endif
    }
}
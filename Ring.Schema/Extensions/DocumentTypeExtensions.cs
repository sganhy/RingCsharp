using Ring.Schema.Helpers;
using Ring.Schema.Models;
using Ring.Schema.Validators;

namespace Ring.Schema.Extensions;

internal static class DocumentTypeExtensions
{
	internal static IDocumentValidator GetValidator(this DocumentType documentType)
	{
		switch (documentType)
		{
			case DocumentType.XmlNative: return new NativeDocumentValidator();
			case DocumentType.XmlClfy: return new ClfyDocumentValidator();
		}
		throw new NotImplementedException();
	}

	internal static SchemaTemplate? GetSchemaTemplate(this DocumentType documentType)
		=> ResourceHelper.GetSchemaTemplate(documentType);
	

}

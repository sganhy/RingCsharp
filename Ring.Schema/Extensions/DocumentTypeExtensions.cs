using Ring.Schema.Builders;
using Ring.Schema.Helpers;
using Ring.Schema.Models;
using Ring.Schema.Validators;

namespace Ring.Schema.Extensions;

internal static class DocumentTypeExtensions
{
	internal static IDocumentValidator GetValidator(this DocumentType documentType)
	{
		// Code size: 29 (0x1d)
		switch (documentType)
		{
			case DocumentType.XmlNative: return new NativeDocumentValidator();
			case DocumentType.XmlClfy: return new ClfyDocumentValidator();
		}
		throw new NotImplementedException();
	}

	internal static IMetaBuilder GetMetaBuilder(this DocumentType documentType)
	{
		// Code size: 29 (0x1d)
		switch (documentType)
		{
			case DocumentType.XmlNative: return new NativeMetaBuilder();
			case DocumentType.XmlClfy: return new ClfyMetaBuilder();
		}
		throw new NotImplementedException();
	}

	internal static SchemaTemplate? GetSchemaTemplate(this DocumentType documentType)
		=> ResourceHelper.GetSchemaTemplate(documentType);
	

}

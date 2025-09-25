using Ring.Schema.Attributes;

namespace Ring.Schema.Enums;

internal enum DocumentType: byte
{
	XmlNative = 1,
	XmlClfy = 20,
	Undefined = 127,
}

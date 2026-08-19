using Ring.Data.Enums;
using Ring.Schema.Enums;

namespace Ring.Schema.Extensions;

internal static class ConstraintTypeExtensions
{
	internal static AlterQueryType ToAlterQueryType(this ConstraintType constraintType)
	{
		// Code size: 43 (0x2b)
		switch (constraintType)
		{
			case ConstraintType.PrimaryKey: return AlterQueryType.CreatePrimaryKey;
			case ConstraintType.NotNull: return AlterQueryType.CreateNotNull;
			case ConstraintType.Check: return AlterQueryType.CreateCheckConstraint;
			case ConstraintType.Default: return AlterQueryType.CreateDefaultConstraint;
		}
		return AlterQueryType.Undefined;
	}
}

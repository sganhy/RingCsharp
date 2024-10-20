﻿using Ring.Schema.Enums;

namespace Ring.Schema.Models
{
    /// <summary>
    ///     Logical field sourceType (64 bytes by fields)
    /// </summary>
    internal sealed class Field : BaseEntity
    {
        internal readonly bool CaseSensitive;
        internal readonly string DefaultValue;
        internal readonly bool Multilingual;
        internal readonly bool NotNull;
        internal readonly int Size;
        internal readonly FieldType Type;

        /// <summary>
        ///     Ctor
        /// </summary>
        internal Field(int id, string name, string description, FieldType type, int size, string defaultValue,
            bool baseline, bool notNull, bool caseSensitif, bool active, bool multilingual)
            : base(id, name, description, active, baseline)
        {
            Type = type;
            Size = size;
            DefaultValue = defaultValue;
            NotNull = notNull;
            CaseSensitive = caseSensitif;
            Multilingual = multilingual;
        }
    }
}
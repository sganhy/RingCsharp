using System;
using System.Collections.Generic;

namespace Ring.Schema.Models
{
    internal sealed class Lexicon : BaseEntity
    {
        internal readonly Field FromField;
        internal readonly Guid Guid;
        internal readonly Language[] Languages;
        internal readonly Relation Relation;
        internal readonly int SchemaId;
        internal readonly Table Table;
        internal readonly Field ToField;
        internal readonly Dictionary<string, string>[] Translations;
        internal readonly bool UpperCaseSearch; // force to store only source value in upper case (not both as usual)

        /// <summary>
        ///     Ctor
        /// </summary>
        internal Lexicon(int id, string name, string description, int schemaId,
            Table table, Field toField, Field fromField, Relation relation, Language[] languages,
            Guid guid, Dictionary<string, string>[] translations, bool upperCaseSearch,
            bool baseline, bool active)
            : base(id, name, description, active, baseline)
        {
            SchemaId = schemaId;
            Table = table;
            ToField = toField;
            Relation = relation;
            Languages = languages;
            Guid = guid;
            FromField = fromField;
            Translations = translations;
            UpperCaseSearch = upperCaseSearch;
        }
    }
}
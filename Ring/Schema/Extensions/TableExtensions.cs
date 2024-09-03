using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using Index = Ring.Schema.Models.Index;

namespace Ring.Schema.Extensions;

internal static class TableExtensions
{
    /// <summary>
    /// Get field by name, case sensitive search ==> O(log n) complexity
    /// </summary>
    /// <param name="table">table object</param>
    /// <param name="name">field name</param>
    /// <returns>Field object</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Field? GetField(this Table table, string name)
    {
        int indexerLeft = 0, indexerRigth = table.Fields.Length-1, indexerMiddle, indexerCompare;
        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            indexerCompare = string.CompareOrdinal(name, table.Fields[indexerMiddle].Name);
            if (indexerCompare == 0) return table.Fields[indexerMiddle];
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return null;
    }

    /// <summary>
    /// Get field by name, case unsensitive search ==> O(n) complexity
    /// </summary>
    /// <param name="table">table object</param>
    /// <param name="fieldName">field name</param>
    /// <param name="comparisonType">StringComparison enum</param>
    /// <returns>Field object</returns>
    internal static Field? GetField(this Table table, string name, StringComparison comparisonType)
    {
        for (var i = table.Fields.Length - 1; i >= 0; --i)
            if (table.Fields[i] != null && string.Equals(name, table.Fields[i].Name, comparisonType))
                return table.Fields[i];
        return null;
    }

    /// <summary>
    /// Get Fields by id ==> O(log n) complexity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Field? GetField(this Table table, int id)
    {
        var i=0;
        var fieldCount=table.Fields.Length;
        while (i<fieldCount)
        {
            if (table.Fields[i].Id == id) return table.Fields[i];
            ++i;
        }
        return null;
    }

    /// <summary>
    /// Get index field by name, case sensitive search ==> O(log n) complexity
    /// </summary>
    /// <param name="table">table object</param>
    /// <param name="fieldName">field name</param>
    /// <returns>Field index or -1 if not found</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetFieldIndex(this Table table, string name)
    {
        int indexerLeft = 0, indexerRigth = table.Fields.Length - 1, indexerMiddle, indexerCompare;
        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            indexerCompare = string.CompareOrdinal(name, table.Fields[indexerMiddle].Name);
            if (indexerCompare == 0) return indexerMiddle;
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return -1;
    }

    /// <summary>
    /// Get relation object by name ==> O(log n) complexity
    /// </summary>
    /// <param name="table">Table object</param>
    /// <param name="name">Relation name</param>
    /// <returns>Relation object</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Relation? GetRelation(this Table table, string name)
    {
        int indexerLeft = 0, indexerRigth = table.Relations.Length - 1, indexerMiddle, indexerCompare;
        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            indexerCompare = string.CompareOrdinal(name, table.Relations[indexerMiddle].Name);
            if (indexerCompare==0) return table.Relations[indexerMiddle];
            if (indexerCompare>0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return null;
    }

    /// <summary>
    /// Get relation object by name ==> O(n) complexity
    /// </summary>
    /// <param name="table">Table object</param>
    /// <param name="name">Relation name</param>
    /// <param name="comparisonType">Comparison Type</param>
    /// <returns>Relation object</returns>
    internal static Relation? GetRelation(this Table table, string name, StringComparison comparisonType)
    {
        for (var i = table.Relations.Length - 1; i >= 0; --i)
            if (string.Equals(name, table.Relations[i].Name, comparisonType)) return table.Relations[i];
        return null;
    }

    /// <summary>
    /// Get relation object by id ==> O(n) complexity
    /// </summary>
    /// <returns>Relation object</returns>
    internal static Relation? GetRelation(this Table table, int id)
    {
        for (var i = table.Relations.Length - 1; i >= 0; --i)
            if (id == table.Relations[i].Id) return table.Relations[i];
        return null;
    }

    /// <summary>
    /// Get relation object by id ==> O(n) complexity
    /// </summary>
    /// <returns>Index object</returns>
    internal static Index? GetIndex(this Table table, string name)
    {
        int indexerLeft = 0, indexerRigth = table.Indexes.Length - 1, indexerMiddle, indexerCompare;
        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            indexerCompare = string.CompareOrdinal(name, table.Indexes[indexerMiddle].Name);
            if (indexerCompare == 0) return table.Indexes[indexerMiddle];
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return null;
    }

    /// <summary>
    /// Get index relation by name, case sensitive search ==> O(log n) complexity
    /// </summary>
    /// <param name="table">table object</param>
    /// <param name="fieldName">relation name</param>
    /// <returns>Field index or -1 if not found</returns>
    internal static int GetRelationIndex(this Table table, string name)
    {
        int indexerLeft = 0, indexerRigth = table.Relations.Length - 1, indexerMiddle, indexerCompare;
        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            indexerCompare = string.CompareOrdinal(name, table.Relations[indexerMiddle].Name);
            if (indexerCompare == 0) return indexerMiddle;
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return -1;
    }

    internal static Field? GetPrimaryKey(this Table table) =>
        (table.Fields.Length > 0 && (table.Type == TableType.Business || table.Type == TableType.Lexicon)) ?
        table.Fields[table.ColumnMapper[0]] : null;

    /// <summary>
    /// Get first unique index
    /// </summary>
    internal static Index? GetFirstKey(this Table table)
    {
        if (table.Indexes != null && table.Indexes.Length > 0)
            for (var i = 0; i < table.Indexes.Length; ++i)
                if (table.Indexes[i].Unique) return table.Indexes[i];
        return null;
    }

    internal static Meta[] ToMeta(this Table table, int schemaId) {
        var result = new List<Meta>(table.Fields.Length+table.Relations.Length+table.Indexes.Length+1);
        int i;
        for (i=0; i < table.Fields.Length; ++i) result.Add(table.Fields[i].ToMeta(table.Id));
        for (i=0; i < table.Relations.Length; ++i) result.Add(table.Relations[i].ToMeta(table.Id));
        for (i=0; i < table.Indexes.Length; ++i) result.Add(table.Indexes[i].ToMeta(table.Id));
        var meta = new Meta();
        // first - define Object type
        meta.SetEntityType(EntityType.Table);
        meta.SetEntityId(table.Id);
        meta.SetEntityName(table.Name);
        meta.SetEntityDescription(table.Description);
        meta.SetEntityRefId(schemaId);
        meta.SetTableCached(table.Cached);
        meta.SetTableReadonly(table.Readonly);
        result.Add(meta);
        return result.ToArray(); 
    }

    internal static void LoadColumnMapper(this Table table)
    {
        var fieldCount = table.Fields.Length;
        var relationCount = table.Relations.Length;
        var columnCount = fieldCount + relationCount; // potentatial column count
        var index = 0;
        var colPosition = 0;
        var i=0;
        // copy
        var columns = new IColumn[columnCount];
        for (; i < fieldCount; ++i) columns[i] = table.Fields[i];
        for (i=0; i < relationCount; ++i) columns[i+fieldCount] = table.Relations[i];
        // sort by Id
        Array.Sort(columns, (x, y) => x.Id.CompareTo(y.Id));
        i = 0;
        while (i<columnCount)
        {
            index = table.GetFieldIndex(columns[i].Name);
            if (index < 0)
            {
                var relation = columns[i];
                if (relation.RelationType == RelationType.Mto || relation.RelationType == RelationType.Otop)
                    table.ColumnMapper[colPosition++] = table.GetRelationIndex(columns[i].Name) + fieldCount;
            } 
            else table.ColumnMapper[colPosition++] = index;
            ++i;
        }

    }

    internal static void LoadRecordIndex(this Table table)
    {
        var fieldCount = table.Fields.Length;
        var relationCount = table.Relations.Length;
        var i = 0;
        var currentIndex = 0;
        while (i < relationCount)
        {
            var relation = table.Relations[i];
            if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop)
            {
                relation.SetRecordIndex(currentIndex + fieldCount);
                ++currentIndex;
            }
            else relation.SetRecordIndex(-1);
            ++i;
        }
    }


}

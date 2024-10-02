using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Schema.Extensions;

internal static class SchemaExtensions
{
    internal static Parameter? GetParameter(this DbSchema schema, ParameterType parameterType)
        => ParameterExtensions.GetParameter(schema.Parameters, parameterType, schema.Id);

    /// <summary>
    /// Get table object by name (case sensitive) --> O(log n)
    /// </summary>
    internal static Sequence? GetSequence(this DbSchema schema, string name)
    {
        var span = new ReadOnlySpan<Sequence>(schema.Sequences);
        int indexerLeft = 0, indexerRigth = span.Length - 1;
        while (indexerLeft <= indexerRigth)
        {
            var indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name);
            if (indexerCompare == 0) return span[indexerMiddle];
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return null;
    }

    /// <summary>
    /// Get table object by Id
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Table? GetTable(this DbSchema schema, int id)
    {
        var span = new ReadOnlySpan<Table>(schema.TablesById);
        int indexerLeft = 0, indexerRigth = span.Length - 1, indexerMiddle, indexerCompare;
        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            indexerCompare = id - span[indexerMiddle].Id;
            if (indexerCompare == 0L) return span[indexerMiddle];
            if (indexerCompare > 0L) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return null;
    }

    /// <summary>
    /// Get table object by name (case sensitive) --> O(log n)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Table? GetTable(this DbSchema schema, string name)
    {
        var span = new ReadOnlySpan<Table>(schema.TablesByName);
        int indexerLeft = 0, indexerRigth = span.Length - 1, indexerMiddle, indexerCompare;
        while (indexerLeft <= indexerRigth)
        {
            indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            indexerCompare = string.CompareOrdinal(name, span[indexerMiddle].Name); 
            if (indexerCompare == 0) return span[indexerMiddle];
            if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
            else indexerRigth = indexerMiddle - 1;
        }
        return null;
    }

    /// <summary>
    /// Load relationships objects into partial schema 
    /// </summary>
    /// <param name="schema">Partial built in schema</param>
    /// <param name="schemaItems">Should be sorted by name</param>
    internal static void LoadRelations(this DbSchema schema, Meta[] schemaItems)
    {
        var relationDicoIndex = new Dictionary<int,int>(schema.TablesById.Length*2); // (tableId, relation index)
        var spanMeta = new ReadOnlySpan<Meta>(schemaItems);
        var spanTablesById = new ReadOnlySpan<Table>(schema.TablesById);

        // load dico
        foreach (var table in spanTablesById) relationDicoIndex.Add(table.Id, 0);

        // load relation
        foreach (var meta in spanMeta)
        {
            if (meta.IsRelation)
            {
                var fromTable = schema.GetTable(meta.ReferenceId); // get table by id
                var toTable = schema.GetTable(meta.DataType);
                if (toTable != null && fromTable!=null)
                {
                    var relation = meta.ToRelation(toTable);
#pragma warning disable CS8601 // Possible null reference assignment. Cannot be null here !!
                    fromTable.Relations[relationDicoIndex[fromTable.Id]] = relation;
#pragma warning restore CS8601 
                    ++relationDicoIndex[fromTable.Id];
                }
            }
        }
        // load inverse relations
        schema.LoadInverseRelations(schemaItems);
        // load mtm relations
        schema.LoadMtm();
    }

    internal static void LoadColumnMappers(this DbSchema schema)
    {
        var span = new Span<Table>(schema.TablesByName);
        foreach (var tbl in span) tbl.LoadColumnMapper();
    }

    internal static void LoadRecordIndexes(this DbSchema schema)
    {
        var span = new Span<Table>(schema.TablesByName);
        foreach (var tbl in span) tbl.LoadRelationRecordIndex();
    }

    internal static int GetMtmTableCount(this DbSchema schema)
    {
        var result = 0;
        var span = new ReadOnlySpan<Table>(schema.TablesById);
        foreach (var table in span)
            for (var j=table.Relations.Length-1;j >= 0; --j)
                if (table.Relations[j].Type == RelationType.Mtm) ++result;
        return result >> 1;
    }

    /// <summary>
    /// Get sorted list of logical table name
    /// TODO improve performance
    /// </summary>
    internal static string[] GetTableIndex(this DbSchema schema)
    {
        var mtmCount = schema.GetMtmTableCount();
        var tableCount = schema.TablesById.Length;
        var mtmTaleDico = new HashSet<string>();
        var mtmIndex = 0;
        var result = new string[tableCount + mtmCount]; // reduce re-allocations
        for (var i = 0; i < tableCount; ++i) result[i] = schema.TablesById[i].Name;
        for (var i = 0; i < tableCount; ++i)
            for (var j = schema.TablesById[i].Relations.Length - 1; j >= 0; --j)
            {
                var relation = schema.TablesById[i].Relations[j];
                if (relation.Type == RelationType.Mtm && !mtmTaleDico.Contains(relation.ToTable.Name))
                {
                    result[mtmIndex + tableCount] = relation.ToTable.Name;
                    mtmTaleDico.Add(relation.ToTable.Name);
                    ++mtmIndex;
                }
            }
        Array.Sort(result, (x, y) => string.CompareOrdinal(x, y));
        return result;
    }

    #region private methods 
    private static void LoadInverseRelations(this DbSchema schema, Span<Meta> schemaItems)
    {
        foreach (var meta in schemaItems)
        {
            if (meta.IsRelation)
            {
                var fromTable = schema.GetTable(meta.ReferenceId); // get table by id
                if (fromTable != null)
                {
                    var relation = fromTable.GetRelation(meta.Name);
                    var invRelation = relation?.ToTable.GetRelation(meta.Value??string.Empty); 
                    if (relation != null && invRelation!=null)  relation.SetInverseRelation(invRelation);
                }
            }
        }
    }

    private static void LoadMtm(this DbSchema schema)
    {
        var ddlBuilder = schema.Provider.GetDdlBuilder();
        var tableBuilder = new TableBuilder();
        var span = new Span<Table>(schema.TablesById);
        Table mtmTable;
        var mtm = new Dictionary<string,Table>(schema.GetMtmTableCount()*2); // store mtm physical name
        foreach (var table in span)
        {
            for (var j=table.Relations.Length - 1; j >= 0; --j)
            {
                if (table.Relations[j].Type == RelationType.Mtm) 
                {
                    // step 1 - generate physical name
                    var relation = table.Relations[j];
                    var metaTable = new Meta(0,(byte)EntityType.Relation,0,(int)TableType.Mtm,0L, relation.GetMtmName(), 
                        null,null,true);
                    var emptyTable = Meta.GetEmptyTable(metaTable);
                    var physicalName = ddlBuilder.GetPhysicalName(emptyTable, schema);
                    var inverseRelation = relation.InverseRelation;

                    if (!mtm.ContainsKey(physicalName))
                    {
                        mtmTable = tableBuilder.GetMtm(emptyTable, physicalName);
                        //  step 2 - load relations - sort relation
                        if (string.CompareOrdinal(relation.Name, inverseRelation.Name) < 0)
                        {
                            mtmTable.Relations[0] = relation.GetRelation(RelationType.Mto);
                            mtmTable.Relations[1] = inverseRelation.GetRelation(RelationType.Mto);
                        }
                        else
                        {
                            mtmTable.Relations[1] = relation.GetRelation(RelationType.Mto);
                            mtmTable.Relations[0] = inverseRelation.GetRelation(RelationType.Mto);
                        }
                        mtmTable.Columns[0] = mtmTable.Relations[0];
                        mtmTable.Columns[1] = mtmTable.Relations[1];
                        mtmTable.Indexes[0].Columns[0] = mtmTable.Relations[0].Name;
                        mtmTable.Indexes[0].Columns[1] = mtmTable.Relations[1].Name;
                        mtm.Add(physicalName, mtmTable);
                    }
                    else mtmTable = mtm[physicalName];
                    // step 3 - create two new relations
                    table.Relations[j] = CreateMtmRelation(relation, mtmTable);
                    table.Relations[j].SetInverseRelation(inverseRelation);
                }
            }
        }
    }

    private static Relation CreateMtmRelation(Relation relation, Table mtmTable)
    {
        var meta = relation.ToMeta(0);
        return meta.ToRelation(mtmTable);
    }

    #endregion 

}

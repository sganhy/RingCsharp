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
        var i = 0;
        var count= schemaItems.Length;
        var tableCount= schema.TablesById.Length;
        var relationDicoIndex = new Dictionary<int,int>(schema.TablesById.Length*2); // (tableId, relation index)
        // load dico
        for (; i<tableCount; ++i) relationDicoIndex.Add(schema.TablesById[i].Id, 0);
        // load relation
        for (i=0; i<count; ++i)
        {
            if (schemaItems[i].IsRelation())
            {
                var meta = schemaItems[i];
                var fromTable = schema.GetTable(meta.ReferenceId); // get table by id
                var toTable = schema.GetTable(meta.DataType);
                if (toTable != null && fromTable!=null)
                {
                    var relation = meta.ToRelation(toTable);
                    if (relation==null) continue; // useless test here 
                    fromTable.Relations[relationDicoIndex[fromTable.Id]] = relation;
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
        foreach (var tbl in schema.TablesByName) tbl.LoadColumnInformation();
    }

    internal static void LoadRecordIndexes(this DbSchema schema)
    {
        foreach (var tbl in schema.TablesByName) tbl.LoadRelationRecordIndex();
    }

    internal static int GetMtmTableCount(this DbSchema schema)
    {
        var count = 0;
        for (var i = 0; i < schema.TablesById.Length; ++i)
            for (var j = schema.TablesById[i].Relations.Length - 1; j >= 0; --j)
                if (schema.TablesById[i].Relations[j].Type == RelationType.Mtm) ++count;
        return count >> 1;
    }

    #region private methods 
    private static void LoadInverseRelations(this DbSchema schema, Span<Meta> schemaItems)
    {
        foreach (var meta in schemaItems)
        {
            if (meta.IsRelation())
            {
                var fromTable = schema.GetTable(meta.ReferenceId); // get table by id
                if (fromTable != null)
                {
                    var relation = fromTable.GetRelation(meta.Name);
                    var invRelation = relation?.ToTable.GetRelation(meta.GetInverseRelation()??string.Empty); 
                    if (relation != null && invRelation!=null)  relation.SetInverseRelation(invRelation);
                }
            }
        }
    }

    private static void LoadMtm(this DbSchema schema)
    {
        var count = schema.TablesById.Length;
        var ddlBuilder = schema.Provider.GetDdlBuilder();
        var tableBuilder = new TableBuilder();
        var i = 0;
        Table table;
        Table mtmTable;
        
        var mtm = new Dictionary<string,Table>(schema.GetMtmTableCount()*2); // store mtm physical name
        for (; i<count; ++i)
        {
            table = schema.TablesById[i];
            for (var j=table.Relations.Length - 1; j >= 0; --j)
            {
                if (table.Relations[j].Type == RelationType.Mtm) 
                {
                    // step 1 - generate physical name
                    var relation = table.Relations[j];
                    var emptyTable = MetaExtensions.GetEmptyTable(new Meta(relation.GetMtmName()),TableType.Mtm);
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
        return MetaExtensions.ToRelation(meta, mtmTable) ?? default!;
    }

    #endregion 

}

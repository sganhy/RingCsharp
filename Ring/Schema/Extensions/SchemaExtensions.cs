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
        int indexerLeft = 0, indexerRigth = schema.Sequences.Length - 1;
        while (indexerLeft <= indexerRigth)
        {
            var indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = string.CompareOrdinal(name, schema.Sequences[indexerMiddle].Name);
            if (indexerCompare == 0) return schema.Sequences[indexerMiddle];
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
        int indexerLeft = 0, indexerRigth = schema.TablesById.Length - 1;
        while (indexerLeft <= indexerRigth)
        {
            var indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = id - schema.TablesById[indexerMiddle].Id;
            if (indexerCompare == 0L) return schema.TablesById[indexerMiddle];
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
        int indexerLeft = 0, indexerRigth = schema.TablesById.Length - 1;
        while (indexerLeft <= indexerRigth)
        {
            var indexerMiddle = indexerLeft + indexerRigth;
            indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
            var indexerCompare = string.CompareOrdinal(name, schema.TablesByName[indexerMiddle].Name); 
            if (indexerCompare == 0) return schema.TablesByName[indexerMiddle];
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
                var toTable = schema.GetTable(meta.DataType); // get table by id
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

    internal static int GetMtmTableCount(this DbSchema schema)
    {
        var count = 0;
        for (var i = 0; i < schema.TablesById.Length; ++i)
            for (var j = schema.TablesById[i].Relations.Length - 1; j >= 0; --j)
                if (schema.TablesById[i].Relations[j].Type == RelationType.Mtm) ++count;
        return count >> 1;
    }

    #region private methods 
    private static void LoadInverseRelations(this DbSchema schema, Meta[] schemaItems)
    {
        var count = schemaItems.Length;
        for (var i=0; i<count; ++i)
        {
            if (schemaItems[i].IsRelation())
            {
                var meta = schemaItems[i];
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
                        mtmTable = TableBuilder.GetMtmTable(emptyTable, physicalName);
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
        var result = MetaExtensions.ToRelation(meta, mtmTable) ?? default!;
        return result;
    }

    #endregion 

}

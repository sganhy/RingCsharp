using Ring.Data.Helpers;
using Ring.Data.Models;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Data.Core.Extensions
{
    internal static class PathEvaluatorQueryExtension
    {
        internal static void AppendField(this PathEvaluatorQuery query, Field field)
        {
            if (FieldExist(query, field.Name)) return;
            if (field.IsPrimaryKey())
            {
                if (query.FromRelation != null && (query.FromRelation.Type == RelationType.Otof || query.FromRelation.Type == RelationType.Otm))
                    query.Fields.Add(field);
            }
            else query.Fields.Add(field);
        }
        internal static void AppendRelation(this PathEvaluatorQuery query, Relation relation)
        {
            if (!RelationExist(query, relation.Name))
                query.Relations.Add(relation);
        }

        internal static void ClearResults(this PathEvaluatorQuery query)
        {
            //1) clear result
            query.Result.ClearData();

            /*
            //2) clear relationship
            if (_result._extraInfo != null) _result._extraInfo.Clear();
            /*
            { 
                foreach (int kkey in _result._extraInfo.Keys)
                    _result._extraInfo[kkey] = 0L;
               
                for (long i = 0; i < _result._extraInfo.Count; i++)
                {
                    _result._extraInfo.Values[i] = 0;
                }
            }
            //3) clear Sql
            if (Sql != null && Sql.Parameters.Count > 0) Sql.Parameters[0].Value = 0L;
            //4) clear _sqlMTM
            if (_sqlMTM != null && _sqlMTM.Parameters.Count > 0) _sqlMTM.Parameters[0].Value = 0L;
			*/
        }

        internal static void LoadResult(this PathEvaluatorQuery query, IDbConnection connection)
        {
            var id = 0L;
            #region manage parameters[0]
            if (query.Sql != null && query.FromRelation != null)
            {
                switch (query.FromRelation.Type)
                {
                    case RelationType.Mto:
                    case RelationType.Otop:
                        // set objid
                        id = query.ParentQuery.Result.GetRelation(query.FromRelation.Name);
                        query.Result.SetField(id);
                        // set param[0]
                        query.Sql.SetParameterValue(0, id.ToString());
                        break;
                    case RelationType.Otof:
                    case RelationType.Otm:
                        query.Sql.SetParameterValue(0, query.ParentQuery.Result.GetField());
                        break;
                    case RelationType.Mtm:
                        /*
                        objid = _parentQuery._result.GetField();
                        _sqlMTM.Connection = connection;
                        _sqlMTM.Parameters[0].Value = objid;
                        //_sqlMTM.ExecuteNonQuery();
                        objid = Convert.ToString(_sqlMTM.ExecuteScalar());
                        if (String.IsNullOrEmpty(objid) == true) objid = AdpField.DEFAULT_INT_VALUE;
                        _sqlMTM.Connection = null; // remove refence to db connection
                        _result.SetData(Record.FIELD_OBJID, objid);
                        Sql.Parameters[0].Value = objid;
						*/
                        break;
                }
            }
            #endregion
            /*
            #region load result with query
            if (Sql != null && (objid != AdpField.DEFAULT_INT_VALUE || _fromRelation == null))
            {
                try
                {
                    Sql.Connection = connection;
                    _queryLaunched = true;
                    //Sql.ExecuteNonQuery();
                    if (_fromRelation != null && _fromRelation.Type == AdpRelationType.OTM)
                    {
                        using (OracleDataAdapter result = new OracleDataAdapter(Sql))
                        using (DataSet ds = new DataSet())
                        {
                            result.Fill(ds);
                            LoadResult(ds);
                        }
                    }
                    else
                    {
                        using (OracleDataReader reader = Sql.ExecuteReader())
                        {
                            LoadResult(reader);
                        }
                    }
                    Sql.Connection = null; // remove reference to db connection 
                }
                catch (Exception ex)
                {
                    //_logger.Error(ex);
                    throw ex;
                }
            }
            #endregion 
            #region load result without query
            if (Sql == null && _fromRelation != null)
            {  // load result

                switch (_fromRelation.Type)
                {
                    case RelationType.Mto:
                    case RelationType.Otop:
                        //_result.SetField(Record.FIELD_OBJID,
                        //    _parentQuery._result.GetRelation(_fromRelation.Name));
                        _result.SetData(Record.FIELD_OBJID,
                            _parentQuery._result.GetRelation(_fromRelation.Name));
                        break;
                }
            }
            #endregion
			*/
        }

        internal static PathEvaluatorQuery Clone(this PathEvaluatorQuery query)
        {
            PathEvaluatorQuery result = new PathEvaluatorQuery();
            //result._fieldList = this._fieldList;         // NO NEED DEEP COPY
            //result._filterList = this._filterList;       // NO NEED DEEP COPY
            //result._fromRelation = this._fromRelation;   // NO NEED DEEP COPY 
            //result._parentQuery = null;                  // manage parent query later
            //result._queryLaunched = this._queryLaunched;
            //result._relationList = this._relationList;
            //result._result = new Record { RecordType = _result.RecordType };
            //result.Sql = (this.Sql != null) ? (OracleCommand)this.Sql.Clone() : null;
            //result._sqlMTM = (this._sqlMTM != null) ? (OracleCommand)this._sqlMTM.Clone() : null;
            //result._parentPath = _parentPath;
            return result;
        }

        internal static void BuildQuery(this PathEvaluatorQuery query, IDbConnection connection) => query.Sql = SqlHelper.GetQuery(connection, query);

        #region private methods

        private static bool FieldExist(this PathEvaluatorQuery pathEvaluatorQuery, string fieldName)
        {
            var i = 0;
            while (i < pathEvaluatorQuery.Fields.Count)
            {
                if (string.CompareOrdinal(pathEvaluatorQuery.Fields[i].Name, fieldName) == 0) return true;
                ++i;
            }
            return false;
        }
        private static bool RelationExist(this PathEvaluatorQuery pathEvaluatorQuery, string relationName)
        {
            var i = 0;
            while (i < pathEvaluatorQuery.Relations.Count)
            {
                if (string.CompareOrdinal(pathEvaluatorQuery.Fields[i].Name, relationName) == 0) return true;
                ++i;
            }
            return false;
        }

        #endregion

    }
}

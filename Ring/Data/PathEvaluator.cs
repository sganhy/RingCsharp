using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Models;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Data
{
    public class PathEvaluator : IDisposable
    {
        private readonly Dictionary<string, PathEvaluatorResult> _resultList;        // <path:field, (value, type)> 
        private readonly Dictionary<string, PathEvaluatorQuery> _queryList;
        private readonly Database _schema;
        private bool _queryBuilded;
        private TimeSpan _executionTime;
        private bool _disposed;

        /// <summary>
        /// Ctor
        /// </summary>
        public PathEvaluator(int schemaId, int focusType, long focusLowid)
        {
            var schema = Global.Databases.GetSchema(schemaId);
            if (schema == null) throw new ArgumentException(string.Format(Constants.ErrPathInvalidSchemaId, schemaId.ToString()));
            _schema = schema;
            var table = schema.GetTable(focusType);
            if (table == null) throw new ArgumentException(string.Format(Constants.ErrPathInvalidFocusType, focusType.ToString()));

            _resultList = new Dictionary<string, PathEvaluatorResult>();
            _queryList = new Dictionary<string, PathEvaluatorQuery>();

            // set root}
            var root = new PathEvaluatorQuery();
            var rcd = new Record(table);
            rcd.SetField(focusLowid);
            root.Result = rcd;
            root.ParentPath = string.Empty;
            _queryList.Add(Constants.RootKey, root);
            _queryBuilded = false;
            _disposed = false;
        }
        private PathEvaluator(Dictionary<string, PathEvaluatorResult> resultList, Dictionary<string, PathEvaluatorQuery> queryList, Database schema)
        {
            // constructor for clone method
            _queryBuilded = false;
            _disposed = false;
            _resultList = resultList;
            _queryList = queryList;
            _schema = schema;
        }


        #region properties
        public int ObectType => _queryList[Constants.RootKey].Result.Table.Id;
        internal int LogicalQueryCount => _queryList.Count;
        public long ObjectId
        {
            get
            {
                return long.Parse(_queryList[Constants.RootKey].Result.GetField());
            }
            set
            {
                ClearResults();
                _queryList[Constants.RootKey].Result.SetField(value);
            }
        }
        public bool IsEmpty => _queryList.Count == 1 &&
                             _queryList[Constants.RootKey].Fields != null &&
                             _queryList[Constants.RootKey].Fields.Count == 0 &&
                             _queryList[Constants.RootKey].Relations != null &&
                             _queryList[Constants.RootKey].Relations.Count == 0;
        public TimeSpan ExecutionTime => _executionTime;

        #endregion
        #region indexers
        public string this[string path]
        {
            get
            {
                string result = null;
                if (_resultList != null && _resultList.ContainsKey(path))
                    result = _resultList[path].Value;
                return result;
            }
        }
        #endregion

        internal string GetValue(string path) => _resultList != null && _resultList.ContainsKey(path) ? _resultList[path].Value : null;
        internal FieldType GetType(string path) => _resultList != null && _resultList.ContainsKey(path) ? _resultList[path].Type : FieldType.NotDefined;
        public void AppendPath(string path)
        {
            if (path == null || _resultList.ContainsKey(path)) return;

            _queryBuilded = false;

            // path list + type
            //_resultList.Add(path.Trim(), Constants.DefaultPathEvaluatorResult);

            // split path
            var relationList = path.Split(Constants.PathSeparator);
            var currentPath = Constants.RootKey;
            string currentRelation;

            // valid path ?
            if (!ValidPath(path, out currentRelation)) throw new ArgumentException(currentRelation);
            var currentParentQuery = _queryList[currentPath];
            Field field;
            var parentPath = string.Empty;

            #region  manage relations
            for (var i = 0; i < relationList.Length - 1; ++i)
            {
                currentRelation = relationList[i].Trim();
                currentPath += currentRelation + Constants.PathSeparator;
                if (!_queryList.ContainsKey(currentPath))
                {
                    // then create the PathEvaluatorQuery object  
                    var newQuery = new PathEvaluatorQuery();
                    var rcd = new Record();

                    // manage extra filter
                    var index = currentRelation.IndexOf('(');
                    if (index >= 0)
                    {
                        var temp = currentRelation.Substring(0, index); // temp <-- current relation;
                        /*
                        newQuery.Filters.Add(new PathEvaluatorFilter(
                            currentParentQuery.Result.Table.GetRelation(temp).To,
                            currentRelation.Substring(index), path));
                        */
                        currentRelation = temp;
                    }
                    newQuery.FromRelation = currentParentQuery.Result.Table.GetRelation(currentRelation);
                    if (newQuery.FromRelation != null) // valid relation name
                    {
                        rcd.RecordType = newQuery.FromRelation.To.Name;
                        newQuery.Result = rcd;
                        newQuery.ParentQuery = currentParentQuery;
                        newQuery.ParentPath = parentPath;

                        // manage flex attribute to retrieve datatype
                        /*
                            if (rcd._recordType.FlexAttribute == true)
                                newQuery.AppendField(_schema.GetAdpObject(rcd._recordType.Id).GetField(FLEX_ATTRIBUTE_TYPE_FIELD));
                                */
                        _queryList.Add(currentPath, newQuery);

                        // get relation name to build select and update the parent query
                        switch (newQuery.FromRelation.Type)
                        {
                            case RelationType.Mto:
                            case RelationType.Otop:
                                currentParentQuery.AppendRelation(newQuery.FromRelation);
                                break;
                            case RelationType.Otm:
                            case RelationType.Otof:
                                field = currentParentQuery.Result.Table.GetField(currentParentQuery.Result.Table.PrimaryKey.Name);
                                newQuery.AppendField(field);
                                currentParentQuery.AppendField(field);
                                break;
                        }
                        // update the parent query
                        currentParentQuery = newQuery;
                    }
                    else
                    {
                        // focus_obj2time_bomb mgt
                        if (currentRelation == Constants.TimeBombReference)
                        {
                            rcd.RecordType = Constants.TimeBombTableName;
                            newQuery.Result = rcd;
                            //rcd.SetField(_eventId);
                            newQuery.ParentQuery = currentParentQuery;
                            newQuery.ParentPath = parentPath;
                            _queryList.Add(currentPath, newQuery);
                            currentParentQuery = newQuery;
                        }
                        else
                            throw new ArgumentException(string.Format(Constants.ErrPathInvalidRelationName, path, currentRelation,
                                currentParentQuery.Result.RecordType));
                    }
                }
                else currentParentQuery = _queryList[currentPath];
                parentPath = currentPath;
            }
            #endregion 
            #region manage fields (from last element)
            field = currentParentQuery.Result.Table.GetField(relationList[relationList.Length - 1].Trim());
            if (field != null)
            {
                //_typeList[path.Trim()] = (int)field.Type;
                currentParentQuery.AppendField(field);
            }
            #endregion
        }
        public void Clear()
        {
            var root = new PathEvaluatorQuery();
            var rcd = new Record { RecordType = _queryList[Constants.RootKey].Result.RecordType };
            _resultList.Clear();
            _queryList.Clear();
            _queryList.Add(Constants.RootKey, root);
            _queryList[Constants.RootKey].Result = rcd;
            _queryBuilded = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RetrievePaths()
        {
            // build sqlcommands object
            var connection = _schema.Connections.Get();
            try
            {
                RetrievePaths(connection);
            }
            finally
            {
                // return connection immediatly
                _schema.Connections.Put(connection);
            }
        }
        internal void RetrievePaths(IDbConnection connection)
        {
            var startTime = DateTime.Now;
            var queryList = _queryList.ToArrayOfValue();
            if (!_queryBuilded) BuildQueries(connection, queryList);

            LoadResults(connection, queryList);
            _executionTime = DateTime.Now - startTime;
        }

        public bool IsValidPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string status;
            return ValidPath(path, out status);
        }
        public PathEvaluator Clone(long focusLowid)
        {
            // Dictionary<string, PathEvaluatorResult> resultList, Dictionary< string, PathEvaluatorQuery > queryList, Database schema
            var result = new PathEvaluator(new Dictionary<string, PathEvaluatorResult>(_resultList.Count * 2),
                            new Dictionary<string, PathEvaluatorQuery>(_queryList.Count * 2), _schema);
            // no deep copy here 
            // 1) query list
            foreach (var entry in _queryList)
            {
                if (entry.Value != null)
                {
                    var newQuery = entry.Value.Clone();
                    result._queryList.Add(entry.Key, newQuery);
                }
                else result._queryList.Add(entry.Key, null);
            }

            // 2) set new value
            result._queryList[Constants.RootKey].Result.SetField(focusLowid);
            //result._queryList[ROOT_KEY].Result.SetField(Record.FIELD_OBJID, focusLowid);
            //if (_queryBuilded) result._queryList[Constants.RootKey].SetParameter(0, focusLowid);

            // 3) result list
            foreach (var entry in _resultList)
            {
                result._resultList.Add(entry.Key, null);
            }

            // 4) manage parent query !!!
            foreach (var entry in result._queryList)
                if (entry.Value != null)
                    entry.Value.ParentQuery = result._queryList[entry.Value.ParentPath];
            result._queryList[Constants.RootKey].ParentQuery = null;
            return result;
        }
        public bool ContainsPath(string path) => _resultList != null && _resultList.ContainsKey(path);

        public void Dispose()
        {
            Dispose(true);
        }

        ~PathEvaluator()
        {
            Dispose(false);
        }

        #region private methods
        private bool ValidPath(string path, out string status)
        {
            status = string.Empty;
            if (path == null || _queryList.Count <= 0) return false;
            var result = true;
            var rcd = new Record(_queryList[Constants.RootKey].Result.Table);
            var i = 0;
            string field;
            var relationList = path.Split(Constants.PathSeparator);
            #region validate relation
            while (i < relationList.Length - 1 && result)
            {
                var currentRelation = relationList[i].Trim();
                #region check relation
                field = null;
                var index = currentRelation.IndexOf('(');
                // check where clause field 
                if (index >= 0 && index < currentRelation.Length - 1)
                {
                    field = currentRelation.Substring(index + 1);
                    currentRelation = currentRelation.Substring(0, index);
                    index = field.IndexOf('!');
                    if (index >= 0 && index < field.Length - 1)
                    {
                        field = field.Substring(0, index);
                    }
                    else
                    {
                        index = field.IndexOf('=');
                        if (index >= 0 && index < field.Length - 1)
                            field = field.Substring(0, index);
                    }
                    field = field.Trim();
                    // alias ? eg. T2.objid 
                    index = field.IndexOf('.');
                    if (index >= 0 && index < field.Length - 1)
                    {
                        field = field.Substring(index + 1);
                    }
                }
                var rel = rcd.Table.GetRelation(currentRelation);
                // check relationship
                if (rel != null)
                {
                    //rcd.Table.Id = rel.To;
                    if (field != null)
                    {
                        result = rcd.IsFieldExist(field);
                        if (!result) status = string.Format(Constants.ErrPathInvalidFieldName, path,
                                                        field, rcd.RecordType);
                    }
                }
                else
                {
                    status = string.Format(Constants.ErrPathInvalidRelationName, path, currentRelation, rcd.RecordType);
                    result = false;
                }
                #endregion 
                ++i;
            }
            #endregion 
            #region validate field
            field = relationList[relationList.Length - 1];
            if (!result) return result;
            result = rcd.IsFieldExist(field);
            if (!result) status = string.Format(Constants.ErrPathInvalidFieldName, path, field, rcd.RecordType);

            #endregion 
            return result;
        }
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                //if (disposing)
                {
                    //_resultList = null;
                    //_typeList = null;
                    //_pathList = null;
                    if (_queryList != null)
                    {
                        foreach (var entry in _queryList) entry.Value.Dispose();
                        //_queryList = null;
                    }
                    _disposed = true;
                    // This object will be cleaned up by the Dispose method. 
                    // Therefore, you should call GC.SupressFinalize to 
                    // take this object off the finalization queue 
                    // and prevent finalization code for this object 
                    // from executing a second time.
                    //GC.SuppressFinalize(this);
                }
            }
        }
        private void BuildQueries(IDbConnection connection, PathEvaluatorQuery[] queryList)
        {
            // build queries
            for (var i = 0; i < queryList.Length; ++i) queryList[i].BuildQuery(connection);

            // querries builded
            _queryBuilded = true;
        }
        /// <summary>
        /// Load result to _pathList
        /// </summary>
        private void LoadResults(IDbConnection connection, PathEvaluatorQuery[] queryList)
        {
            // first pass
            for (var i = 0; i < queryList.Length; ++i) queryList[i].LoadResult(connection);

            //foreach (var entry in _queryList) entry.Value.BuildQuery(connection);
        }
        private void ClearResults()
        {
            if (!_queryBuilded) return;
            foreach (var keyValue in _resultList)
            {
                _queryList[keyValue.Key].ClearResults();
                _resultList[keyValue.Key].Value = null;
            }
        }

        #endregion
    }
}


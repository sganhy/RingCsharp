using Ring.Schema.Models;
using System;
using System.Collections.Generic;

namespace Ring.Data.Models
{
    internal sealed class PathEvaluatorQuery : IDisposable
    {
        private Record _result;
        private PathEvaluatorQuery _parentQuery;
        public Relation FromRelation;
        public readonly List<Field> Fields;
        public readonly List<Relation> Relations;
        public readonly List<PathEvaluatorFilter> Filters;
        public IDbCommand Sql;
        private string _parentPath;

        /// <summary>
        /// Ctor
        /// </summary>
        public PathEvaluatorQuery()
        {
            _result = null;
            FromRelation = null;
            _parentQuery = null;
            Fields = new List<Field>();
            Relations = new List<Relation>();
            Filters = new List<PathEvaluatorFilter>();
            Sql = null;
        }

        #region properties
        public Record Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }
        public PathEvaluatorQuery ParentQuery
        {
            get
            {
                return _parentQuery;
            }
            set
            {
                _parentQuery = value;
            }
        }
        public string ParentPath
        {
            get
            {
                return _parentPath;
            }
            set
            {
                _parentPath = value;
            }
        }
        #endregion

        public void Dispose()
        {
            _result = null;
            _parentQuery = null; // remove ref to parent
            _parentPath = null;
            // unmanaged ressources -->
            if (Sql != null) Sql.Dispose();
        }




    }
}

using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Collections.Generic;

namespace Ring.Schema.Core.Rules.Impl
{
    internal sealed class ValidationResult
    {
        private readonly List<ValidationItem> _items;
        private long _errorCount;
        private long _fatalCount;
        private long _warningCount;

        /// <summary>
        /// Ctor
        /// </summary>
        public ValidationResult()
        {
            _items = new List<ValidationItem>();
            _errorCount = 0L;
            _fatalCount = 0L;
            _warningCount = 0L;
        }


        internal long ErrorCount => _errorCount;

        internal long FatalCount => _fatalCount;

        internal long WarningCount => _warningCount;

        /// <summary>
        /// Validation results 
        /// </summary>
        internal IEnumerable<ValidationItem> Validations
        {
            get { return _items; }
        }

        internal void AddItem(long id, long lineNumber, string name, string description, LogLevel level)
        {
            if (_errorCount + _fatalCount <= Constants.MaxValidation)
                _items.Add(new ValidationItem(id, lineNumber, name, description, level));
            if (level == LogLevel.Error) ++_errorCount;
            if (level == LogLevel.Warning) ++_warningCount;
            if (level == LogLevel.Fatal) ++_fatalCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="previousResult"></param>
        internal void Merge(ValidationResult previousResult)
        {
            if (previousResult != null)
            {
                _items.AddRange(previousResult.Validations);
                _errorCount += previousResult._errorCount;
                _fatalCount += previousResult._fatalCount;
                _warningCount += previousResult._warningCount;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal bool IsBlockingDefect => _errorCount + _fatalCount > 0;
    }
}

using Microsoft.Extensions.Logging;
using Ring.Schema.Enums;
using Ring.Schema.Helpers;
using Ring.Schema.Models;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Schema;

internal sealed class ValidationResult
{
	private const int MaxValidation = 150; // number max of validation displayed
	private readonly List<ValidationItem> _items;
	private long _errorCount;
	private long _criticalCount;
	private long _warningCount;

	/// <summary>
	/// Ctor
	/// </summary>
	public ValidationResult()
	{
        _items = [];
		_errorCount = 0L;
		_criticalCount = 0L;
		_warningCount = 0L;
	}

	internal long ErrorCount => _errorCount;
	internal long CriticalCount => _criticalCount;
	internal long WarningCount => _warningCount;

	/// <summary>
	/// Validation results 
	/// </summary>
	internal List<ValidationItem> Validations => _items;
	internal void AddItem(LogType logType, string var1, [CallerLineNumber] int lineNumber = 0) => AddItem(logType, lineNumber, var1);
	internal void AddItem(LogType logType, string var1, string var2, [CallerLineNumber] int lineNumber = 0) => AddItem(logType, lineNumber, var1, var2);
	internal void AddItem(LogType logType, string var1, string var2, string var3, [CallerLineNumber] int lineNumber = 0) => AddItem(logType, lineNumber, var1, var2, var3);
	private void AddItem(LogType logType, int lineNumber = 0, params object[] args)
	{
		// Code size: 194 (0xc2)
		var logItem = ResourceHelper.GetLogItem(logType);
		var level = logItem?.Level ?? LogLevel.None;
		var id = logItem?.Id ?? 0;
		var name = logItem?.Name ?? string.Empty;
		var description = logItem?.Description ?? string.Empty;
		description = string.Format(CultureInfo.InvariantCulture, description, args);
		if (_errorCount + _criticalCount <= MaxValidation) _items.Add(new ValidationItem(id, lineNumber, name, description, level));
		if (level == LogLevel.Error) ++_errorCount;
		if (level == LogLevel.Warning) ++_warningCount;
		if (level == LogLevel.Critical) ++_criticalCount;
	}

	/// <summary>
	/// 
	/// </summary>
	internal bool IsBlockingDefect => _errorCount + _criticalCount > 0;
}

using Ring.Logging;
using Ring.Schema.Enums;
using Ring.Schema.Helpers;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Schema;

public sealed class ValidationResult
{
	private const int MaxValidation = 150; // number max of validation displayed
	private readonly List<ValidationItem> _items;
	private int _errorCount;
	private int _criticalCount;
	private int _warningCount;

	/// <summary>
	/// Ctor
	/// </summary>
	public ValidationResult()
	{
        _items = [];
		_errorCount = 0;
		_criticalCount = 0;
		_warningCount = 0;
	}

	public int ErrorCount => _errorCount;
	public int CriticalCount => _criticalCount;
	public int WarningCount => _warningCount;
	public ReadOnlyCollection<ValidationItem> Validations => _items.AsReadOnly();
	internal void AddItem(LogLevel level, int id, string name, string description, [CallerLineNumber] int lineNumber = 0) => _items.Add(new ValidationItem(id, lineNumber, name, description, level));
	internal void AddError(LogType logType, string name, string description, [CallerLineNumber] int lineNumber = 0) => _items.Add(new ValidationItem((int)logType, lineNumber, name, description, LogLevel.Error));
	internal void AddWarn(LogType logType, string name, string description, [CallerLineNumber] int lineNumber = 0) => _items.Add(new ValidationItem((int)logType, lineNumber, name, description, LogLevel.Warning));
	internal void AddItem(LogType logType, string var1, [CallerLineNumber] int lineNumber = 0) => AddItem(logType, lineNumber, var1);
	internal void AddItem(LogType logType, string var1, string var2, [CallerLineNumber] int lineNumber = 0) => AddItem(logType, lineNumber, var1, var2);
	internal void AddItem(LogType logType, string var1, string var2, string var3, [CallerLineNumber] int lineNumber = 0) => AddItem(logType, lineNumber, var1, var2, var3);
	internal bool IsBlockingDefect => _errorCount + _criticalCount > 0;

	#region private methods
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
	#endregion 

}

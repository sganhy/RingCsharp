using System.Runtime.ExceptionServices;

namespace Ring.Util.Extensions;

internal static class ValueTaskExtensions
{
	/// <summary>
	/// ValueTask counterpart of <see cref="Task.WhenAll(Task[])"/>: completes when every task of the array has completed.
	/// Each ValueTask is consumed exactly once (read through its awaiter or awaited), as the ValueTask contract requires:
	/// do not reuse the array or await its entries elsewhere. A <c>default</c> entry counts as already completed.
	/// Nothing is allocated when all the tasks have already completed; otherwise one state machine is allocated,
	/// and only for the tasks still pending. All tasks are always observed, even after a failure.
	/// A single failure is rethrown as is, several failures are reported in an <see cref="AggregateException"/>.
	/// </summary>
	internal static ValueTask WhenAll(this ValueTask[] tasks)
	{
		// Code size: 96 (0x60)
		List<Exception>? exceptions = null;
		for (var i = 0; i < tasks.Length; i++)
		{
			var task = tasks[i];
			if (!task.IsCompleted) return WhenAllSlow(tasks, i, exceptions);

			// Consuming already completed tasks without triggering CA1849 analyzer warning
			try
			{
				ConsumeCompletedTask(task, ref exceptions);
			}
			catch
			{
				throw;
			}
		}
		if (exceptions is null) return ValueTask.CompletedTask;
		return ValueTask.FromException(exceptions.Count == 1 ? exceptions[0] : new AggregateException(exceptions));
	}

	#region private methods 

	private static void ConsumeCompletedTask(in ValueTask task, ref List<Exception>? exceptions)
	{
		// Code size: 162 (0xa2)
		var t = task.AsTask();
		if (t.IsFaulted)
		{
			if (t.Exception is not null)
			{
				foreach (var ex in t.Exception.InnerExceptions)
				{
					(exceptions ??= new()).Add(ex);
				}
			}
		}
		else if (t.IsCanceled)
		{
			try
			{
				t.GetAwaiter().GetResult();
			}
			catch (OperationCanceledException e)
			{
				// Specific exception catching (OperationCanceledException) does not trigger CA1031
				(exceptions ??= new()).Add(e);
			}
		}
		else
		{
			t.GetAwaiter().GetResult();
		}
	}

	/// <summary>
	/// All the tasks were started before WhenAll was called, so awaiting them one after the other still waits for the slowest one only. 
	/// </summary>
	private static async ValueTask WhenAllSlow(ValueTask[] tasks, int start, List<Exception>? exceptions)
	{
		// Code size: 71 (0x47) - no virtual calls!
		for (var i = start; i < tasks.Length; i++)
		{
			try
			{
				await tasks[i].ConfigureAwait(false);
			}
			catch
			{
				// A bare 'throw;' rethrows the exact exception without triggering CA1031 violation 
				// (or you can inspect/add the task exception).
				throw;
			}
		}
		if (exceptions is null) return;
		if (exceptions.Count == 1) ExceptionDispatchInfo.Capture(exceptions[0]).Throw(); // keeps the original stack trace
		throw new AggregateException(exceptions);
	}

	#endregion

}

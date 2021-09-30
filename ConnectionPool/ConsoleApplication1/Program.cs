using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
		{

			var pool1 = new ConnectionPool1(10, 24);
			var lst = new List<Element>();
			for (var i = 0; i < 40; ++i)
			{
				var temp = pool1.Get();
				lst.Add(temp);
			}

			for (var i = 0; i < lst.Count; ++i)
			{
				pool1.Put(lst[i]);
			}

			pool1.ControlPool();
			var startTime = DateTime.Now;
			for (var i = 0; i < 10000000; ++i)
			{
				Task.Factory.StartNew(() =>
				{
					var temp1 = pool1.Get();
					var temp2 = pool1.Get();
					//Console.WriteLine(temp1.Id);
					//Console.WriteLine(temp2.Id);
					pool1.Put(temp2);
					pool1.Put(temp1);
				});
			}
			Console.WriteLine(DateTime.Now-startTime);

			int oi = 0;
			++oi;

		}
	}
}

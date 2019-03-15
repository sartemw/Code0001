using System;

namespace Example
{
	class Program
	{
		static private int[] numbers;

		static void Main(string[] args)
		{
			numbers = new int[] {2};

			foreach (var item in numbers)
			{
				Console.WriteLine(item);
			}
			Console.WriteLine();
			numbers = new int[] { 5, 6 };

			foreach (var item in numbers)
			{
				Console.WriteLine(item);
			}

			Console.ReadLine();
		}
	}
}

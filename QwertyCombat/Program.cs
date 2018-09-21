using System;
using Eto.Forms;
using Eto.Drawing;

namespace QwertyCombat
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			new Application().Run(new MainForm());
		}
	}
}

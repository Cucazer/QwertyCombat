using System;
using Eto.Forms;
using Eto.Drawing;
//using Eto.WinForms;

namespace QwertyCombat
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			new Application(/*new Platform() - force WinForms instead of WPF*/).Run(new MainForm());
		}
	}
}

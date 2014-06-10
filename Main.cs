using System;
using Gtk;
using Gdk;


class MainClass
{
	public static void Main (string[] args)
	{
		//Gdk.Threads.Init ();
		Application.Init ();
		new Mandela_Window ();
		Application.Run ();
	}
}
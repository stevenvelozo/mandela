using System;
using Gtk;
using Gdk;
using GLib;
using Master_Super_Image;
using Master_Log_File;
using Master_Time_Span;
using Master_Chaos_Display;

//Todo: Save as Image, Linked List of History, Parameters, Gradient Designer (Everything but Save As in a sidebar?)
//
//Mandelbrot XMin[-1.39433003089995] XMax[-1.39431020057868] YMin[0.002951798228876] YMax[0.002966670969832] Screen_Color_Count[255] Iterations[500]
//Mandelbrot XMin[-0.477803931187928] XMax[-0.477559790575135] YMin[-0.534768014667847] YMax[-0.534584909208252] Screen_Color_Count[255] Iterations[500]
public class Mandela_Window : Gtk.Window
{
	Master_Chaos_Display.Chaos_Display Main_Chaos_Engine;

	ProgressBar Computation_Progress;
	Time_Span Computation_Progress_Time;

	AccelGroup Accelerator_Group;

	private Log_File Application_Log_File;

	bool Current_Quality_Set;
	CheckMenuItem[] Menu_Quality;

	public Mandela_Window () : base ("Mandela")
	{
		this.Application_Log_File = new Log_File("Mandela");
		this.Application_Log_File.Echo_To_Console = true;

		this.Write_To_Log("Loading and starting Mandela");

		this.Main_Chaos_Engine = new Chaos_Display( 1000, 650 );
		this.Main_Chaos_Engine.Write_Log += new Write_Log_Event_Handler ( this.Chained_Write_Log );
		this.Main_Chaos_Engine.Render_Chaos_Image += new EventHandler ( this.Start_Image_Render );

		this.Resizable = false;

		this.DeleteEvent += new DeleteEventHandler (OnMyWindowDelete);

		this.Computation_Progress = new ProgressBar();
		this.Computation_Progress.BarStyle = ProgressBarStyle.Continuous;

		Accelerator_Group = new AccelGroup ();

		this.Add ( User_Interface() );

		this.AddAccelGroup(Accelerator_Group);

		this.Render_Image();

		this.ShowAll ();
	}

	#region Chaos Control Shim Functions
	void Start_Image_Render ( object Sender, EventArgs Arguments )
	{
		this.Render_Image();
	}

	void Render_Image ()
	{
		TimeoutHandler Progress_Update_Handler = new TimeoutHandler ( this.Progress_Update );

		this.Computation_Progress_Time = new Time_Span();
		this.Computation_Progress.Fraction = 0.0;

		this.Main_Chaos_Engine.Render();

		GLib.Timeout.Add( 200, Progress_Update_Handler );
	}

	bool Progress_Update ()
	{

		this.Computation_Progress.Fraction = this.Main_Chaos_Engine.Progress;

		if ( this.Main_Chaos_Engine.Progress == 1.0 )
		{
			this.Computation_Progress.Text = this.Computation_Progress.Text + "Completed: " + this.Computation_Progress_Time.Human_Friendly_Time ( this.Computation_Progress_Time.Current_Time_Stamp ) + " (" + this.Computation_Progress_Time.Current_Time_Stamp.ToString() + "ms)";
			return false;
		}
		else
		{
			this.Computation_Progress.Text = "[Elapsed: " + this.Computation_Progress_Time.Human_Friendly_Time ( this.Computation_Progress_Time.Current_Time_Stamp ) + "] ";
			this.Computation_Progress.Text = this.Computation_Progress.Text + System.Math.Round(this.Main_Chaos_Engine.Progress * 100, 0).ToString() + "%";
			this.Computation_Progress.Text = this.Computation_Progress.Text + " [Estimated Time Remaining: " + this.Computation_Progress_Time.Human_Friendly_Time ( (int)(this.Computation_Progress_Time.Current_Time_Stamp / this.Main_Chaos_Engine.Progress) - this.Computation_Progress_Time.Current_Time_Stamp ) +  "]";
			return true;
		}
	}

	void Set_Render_Quality ( int Quality_Level )
	{
		//Todo:  Make this cludge set itself right when the renderer changes!  Likely, force it to "1"
		if ( !this.Current_Quality_Set )
		{
			this.Current_Quality_Set = true;

			this.Write_To_Log ("Render Quality Changed to: " + Quality_Level.ToString());

			switch ( Quality_Level )
			{
				case 0:
					this.Main_Chaos_Engine.Render_Quality = 0;
					break;

				case 1:
					this.Main_Chaos_Engine.Render_Quality = 1;
					break;

				case 2:
					this.Main_Chaos_Engine.Render_Quality = 3;
					break;

				case 3:
					this.Main_Chaos_Engine.Render_Quality = 5;
					break;

				case 4:
					this.Main_Chaos_Engine.Render_Quality = 7;
					break;

				case 5:
					this.Main_Chaos_Engine.Render_Quality = 9;
					break;

				case 6:
					this.Main_Chaos_Engine.Render_Quality = 11;
					break;

				case 7:
					this.Main_Chaos_Engine.Render_Quality = 13;
					break;
			}

			for (int tmp_Counter = 0; tmp_Counter < 8; tmp_Counter++)
			{
				if ( tmp_Counter != Quality_Level )
				{
					this.Menu_Quality[tmp_Counter].Active = false;
				}
			}
			this.Current_Quality_Set = false;
		}
	}
	#endregion


	void Write_To_Log ( string Log_Text )
	{
		this.Application_Log_File.Write_Log_File ( Log_Text );
	}

	#region UI Event Handlers
	void OnMyWindowDelete (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void Chained_Write_Log( object Sender, Write_Log_Event_Args Arguments )
	{
		//Write it to the textual log file if it is > 0
		if ( Arguments.Log_Level > 0 )
		{
			this.Application_Log_File.Write_Log_File ( Arguments.Log_Text );
		}
	}
	#endregion


	#region Menu Structure and Event Handlers
	void Menu_Switch_Engine_Mandelbrot ( object Sender, EventArgs Arguments )
	{
		this.Write_To_Log ( "Engine Switched to Mandelbrot" );
		this.Main_Chaos_Engine.Set_Engine( "Mandelbrot" );
		this.Render_Image();
	}

	void Menu_Switch_Engine_Julia ( object Sender, EventArgs Arguments )
	{
		this.Write_To_Log ( "Engine Switched to Julia" );
		this.Main_Chaos_Engine.Set_Engine( "Julia" );
		this.Render_Image();
	}

	void Menu_Switch_Engine_Grid ( object Sender, EventArgs Arguments )
	{
		this.Write_To_Log ( "Engine Switched to Debug Grid" );
		this.Main_Chaos_Engine.Set_Engine( "Grid Debug" );
		this.Render_Image();
	}

	void Menu_Copy_Parameters ( object Sender, EventArgs Arguments )
	{
		//Hmmm  shim class?
	}

	void Menu_Selection_Hide ( object Sender, EventArgs Arguments )
	{
		this.Main_Chaos_Engine.Hide_Selection();
	}

	void Menu_Selection_Show ( object Sender, EventArgs Arguments )
	{
		this.Main_Chaos_Engine.Show_Selection();
	}

	void Menu_Exit ( object Sender, EventArgs Arguments )
	{
		Application.Quit();
	}

	void Menu_Render (object sender, EventArgs args)
	{
		this.Render_Image();
	}

	//TODO: Learn to do this the right way with GTK ... the documentation is so frustrating sometimes.
	void Menu_Quality_Change_0 (object sender, EventArgs args)
	{
		this.Set_Render_Quality ( 0  );
	}

	void Menu_Quality_Change_1 (object sender, EventArgs args)
	{
		this.Set_Render_Quality ( 1 );
	}

	void Menu_Quality_Change_2 (object sender, EventArgs args)
	{
		this.Set_Render_Quality ( 2 );
	}

	void Menu_Quality_Change_3 (object sender, EventArgs args)
	{
		this.Set_Render_Quality ( 3 );
	}

	void Menu_Quality_Change_4 (object sender, EventArgs args)
	{
		this.Set_Render_Quality ( 4 );
	}

	void Menu_Quality_Change_5 (object sender, EventArgs args)
	{
		this.Set_Render_Quality ( 5 );
	}

	void Menu_Quality_Change_6 (object sender, EventArgs args)
	{
		this.Set_Render_Quality ( 6 );
	}

	void Menu_Quality_Change_7 (object sender, EventArgs args)
	{
		this.Set_Render_Quality ( 7 );
	}

	void Menu_Quality_Toggle_Visual (object sender, EventArgs args)
	{
		this.Main_Chaos_Engine.Render_Visual = ((CheckMenuItem)sender).Active;
	}

	private MenuBar Application_Menu()
	{
		MenuItem tmp_Menu_Item, tmp_Menu_Header;

		SeparatorMenuItem tmp_Separator;

		MenuBar Main_Menu = new MenuBar ();

		//The "File" menu
		Menu File_Menu = new Menu ();

		tmp_Menu_Item = new ImageMenuItem (Stock.Open, Accelerator_Group);
		//tmp_Menu_Item.Activated += new EventHandler ();
		File_Menu.Append (tmp_Menu_Item);

		tmp_Menu_Item = new ImageMenuItem (Stock.Save, Accelerator_Group);
		//tmp_Menu_Item.Activated += new EventHandler ();
		File_Menu.Append (tmp_Menu_Item);

		tmp_Menu_Item = new ImageMenuItem (Stock.SaveAs, Accelerator_Group);
		//tmp_Menu_Item.Activated += new EventHandler ();
		File_Menu.Append (tmp_Menu_Item);

		tmp_Menu_Item = new ImageMenuItem (Stock.New, Accelerator_Group);
		//tmp_Menu_Item.Activated += new EventHandler ();
		File_Menu.Append (tmp_Menu_Item);

		tmp_Separator = new SeparatorMenuItem();
		File_Menu.Append (tmp_Separator);

		tmp_Menu_Item = new MenuItem("_Export Image (unimplimented)");
		//tmp_Menu_Item.Activated += new EventHandler ();
		File_Menu.Append (tmp_Menu_Item);

		tmp_Separator = new SeparatorMenuItem();
		File_Menu.Append (tmp_Separator);

		tmp_Menu_Item = new ImageMenuItem (Stock.Quit, Accelerator_Group);
		tmp_Menu_Item.Activated += new EventHandler ( Menu_Exit );
		File_Menu.Append (tmp_Menu_Item);

		tmp_Menu_Header = new MenuItem("_File");
		tmp_Menu_Header.Submenu = File_Menu;

		Main_Menu.Append ( tmp_Menu_Header );



		Menu Edit_Menu = new Menu ();

		tmp_Menu_Item = new ImageMenuItem (Stock.Copy, Accelerator_Group);
		tmp_Menu_Item.Activated += new EventHandler ( Menu_Copy_Parameters );
		Edit_Menu.Append (tmp_Menu_Item);

		tmp_Menu_Item = new MenuItem("Copy _Image");
		//tmp_Menu_Item.Activated += new EventHandler (  );
		Edit_Menu.Append ( tmp_Menu_Item );

		tmp_Menu_Item = new ImageMenuItem (Stock.Paste, Accelerator_Group);
		//tmp_Menu_Item.Activated += new EventHandler ();
		Edit_Menu.Append (tmp_Menu_Item);

		tmp_Separator = new SeparatorMenuItem();
		Edit_Menu.Append (tmp_Separator);

		tmp_Menu_Item = new MenuItem("_Show Selection");
		tmp_Menu_Item.Activated += new EventHandler ( Menu_Selection_Show );
		tmp_Menu_Item.AddAccelerator ("activate", Accelerator_Group, new AccelKey (Gdk.Key.F11, Gdk.ModifierType.None, AccelFlags.Visible));
		Edit_Menu.Append ( tmp_Menu_Item );

		tmp_Menu_Item = new MenuItem("_Hide Selection");
		tmp_Menu_Item.Activated += new EventHandler ( Menu_Selection_Hide );
		tmp_Menu_Item.AddAccelerator ("activate", Accelerator_Group, new AccelKey (Gdk.Key.F12, Gdk.ModifierType.None, AccelFlags.Visible));
		Edit_Menu.Append ( tmp_Menu_Item );

		tmp_Menu_Header = new MenuItem("_Edit");
		tmp_Menu_Header.Submenu = Edit_Menu;
		Main_Menu.Append ( tmp_Menu_Header );


		Menu Engines_Menu = new Menu ();
		//
		tmp_Menu_Item = new MenuItem("_Mandelbrot");
		tmp_Menu_Item.Activated += new EventHandler ( Menu_Switch_Engine_Mandelbrot );
		Engines_Menu.Append ( tmp_Menu_Item );

		tmp_Menu_Item = new MenuItem("_Julia");
		tmp_Menu_Item.Activated += new EventHandler ( Menu_Switch_Engine_Julia );
		Engines_Menu.Append ( tmp_Menu_Item );

		tmp_Menu_Item = new MenuItem("Debug Grid");
		tmp_Menu_Item.Activated += new EventHandler ( Menu_Switch_Engine_Grid );
		Engines_Menu.Append ( tmp_Menu_Item );

		tmp_Menu_Header = new MenuItem("E_ngines");
		tmp_Menu_Header.Submenu = Engines_Menu;
		Main_Menu.Append ( tmp_Menu_Header );


		Menu Render_Menu = new Menu ();

		tmp_Menu_Item = new MenuItem("_Render Image");
		tmp_Menu_Item.AddAccelerator ("activate", Accelerator_Group, new AccelKey (Gdk.Key.R, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
		tmp_Menu_Item.Activated += new EventHandler ( Menu_Render );
		Render_Menu.Append (tmp_Menu_Item);

		tmp_Separator = new SeparatorMenuItem();
		Render_Menu.Append (tmp_Separator);
		//...

		//Quality Submenu
		Menu Render_Quality_Menu = new Menu();

		this.Menu_Quality = new CheckMenuItem[8];

		this.Menu_Quality[0] = new CheckMenuItem("_0 - Low");
		this.Menu_Quality[0].Activated += new EventHandler ( Menu_Quality_Change_0 );
		Render_Quality_Menu.Append ( this.Menu_Quality[0] );

		this.Menu_Quality[1] = new CheckMenuItem("_1 - Standard");
		((CheckMenuItem)this.Menu_Quality[1]).Active = true;
		this.Menu_Quality[1].Activated += new EventHandler ( Menu_Quality_Change_1 );
		Render_Quality_Menu.Append ( this.Menu_Quality[1] );

		this.Menu_Quality[2] = new CheckMenuItem("_2 - High");
		this.Menu_Quality[2].Activated += new EventHandler ( Menu_Quality_Change_2 );
		Render_Quality_Menu.Append ( this.Menu_Quality[2] );

		this.Menu_Quality[3] = new CheckMenuItem("_3 - Very High (Slow)");
		this.Menu_Quality[3].Activated += new EventHandler ( Menu_Quality_Change_3 );
		Render_Quality_Menu.Append ( this.Menu_Quality[3] );

		this.Menu_Quality[4] = new CheckMenuItem("_4 - Super High (Very Slow)");
		this.Menu_Quality[4].Activated += new EventHandler ( Menu_Quality_Change_4 );
		Render_Quality_Menu.Append ( this.Menu_Quality[4] );

		this.Menu_Quality[5] = new CheckMenuItem("_5 - Ultra Super High (Very Very Slow)");
		this.Menu_Quality[5].Activated += new EventHandler ( Menu_Quality_Change_5 );
		Render_Quality_Menu.Append ( this.Menu_Quality[5] );

		this.Menu_Quality[6] = new CheckMenuItem("_6 - Ultra Super High (Very Slow Squared)");
		this.Menu_Quality[6].Activated += new EventHandler ( Menu_Quality_Change_6 );
		Render_Quality_Menu.Append ( this.Menu_Quality[6] );

		this.Menu_Quality[7] = new CheckMenuItem("_7 - Gimongous");
		this.Menu_Quality[7].Activated += new EventHandler ( Menu_Quality_Change_7 );
		Render_Quality_Menu.Append ( this.Menu_Quality[7] );

		tmp_Menu_Item = new MenuItem("Render _Quality");
		tmp_Menu_Item.Submenu = Render_Quality_Menu;
		Render_Menu.Append ( tmp_Menu_Item );
		//End Quality Submenu

		tmp_Separator = new SeparatorMenuItem();
		Render_Menu.Append (tmp_Separator);

		tmp_Menu_Item = new CheckMenuItem ("Visual");
		((CheckMenuItem)tmp_Menu_Item).Active = true;
		tmp_Menu_Item.Activated += new EventHandler ( Menu_Quality_Toggle_Visual );
		Render_Menu.Append (tmp_Menu_Item );

		tmp_Menu_Header = new MenuItem("_Render");
		tmp_Menu_Header.Submenu = Render_Menu;
		Main_Menu.Append ( tmp_Menu_Header );

		return Main_Menu;
	}
	#endregion

	private Widget User_Interface()
	{
		VBox Window_Container = new VBox(false, 0);
		VBox Application_Divide = new VBox(false, 12);
		HBox Progress_Segment = new HBox(false, 6);

		Application_Divide.BorderWidth = 12;

		Application_Divide.PackStart( this.Main_Chaos_Engine.Display , false, false, 0 );

		Progress_Segment.PackStart( this.Computation_Progress );

		Application_Divide.PackStart( Progress_Segment, false, false, 0 );

		Window_Container.PackStart( this.Application_Menu() , false, false, 0 );
		Window_Container.PackStart( Application_Divide, false, false, 0 );

		return Window_Container;
	}
}

using System;
using Gtk;
using Gdk;
using GLib;
using MasterSuperImage;
using MasterLogFile;
using MasterTimeSpan;
using MasterChaosDisplay;

//Todo: Save as Image, Linked List of History, Parameters, Gradient Designer (Everything but Save As in a sidebar?)
//
//Mandelbrot XMin[-1.39433003089995] XMax[-1.39431020057868] YMin[0.002951798228876] YMax[0.002966670969832] Screen_Color_Count[255] Iterations[500]
//Mandelbrot XMin[-0.477803931187928] XMax[-0.477559790575135] YMin[-0.534768014667847] YMax[-0.534584909208252] Screen_Color_Count[255] Iterations[500]
public class MandelaWindow : Gtk.Window
{
	MasterChaosDisplay.ChaosDisplay tmpChaosDisplay;

	ProgressBar _ComputationProgressBar;
	TimeSpanMetric _ComputationProgressTimer;

	AccelGroup _AcceleratorGroup;

	private LogFile _ApplicationLogFile;

	bool _CurrentQualitySet;
	CheckMenuItem[] _MenuQuality;

	public MandelaWindow () : base ("Mandela")
	{
		this._ApplicationLogFile = new LogFile("Mandela");
		this._ApplicationLogFile.EchoToConsole = true;

		this.WriteToLog("Loading and starting Mandela");

		this.tmpChaosDisplay = new ChaosDisplay( 1000, 650 );
		this.tmpChaosDisplay.WriteLog += new WriteLogEventHandler ( this.ChainedWriteLog );
		this.tmpChaosDisplay.RenderChaosImage += new EventHandler ( this.StartImageRender );

		this.Resizable = false;

		this.DeleteEvent += new DeleteEventHandler (OnMyWindowDelete);

		this._ComputationProgressBar = new ProgressBar();
		this._ComputationProgressBar.BarStyle = ProgressBarStyle.Continuous;

		_AcceleratorGroup = new AccelGroup ();

		this.Add ( UserInterface() );

		this.AddAccelGroup(_AcceleratorGroup);

		this.RenderImage();

		this.ShowAll ();
	}

	#region Chaos Control Shim Functions
	void StartImageRender ( object pSender, EventArgs pArguments )
	{
		this.RenderImage();
	}

	void RenderImage ()
	{
		TimeoutHandler Progress_Update_Handler = new TimeoutHandler ( this.ProgressUpdate );

		this._ComputationProgressTimer = new TimeSpanMetric();
		this._ComputationProgressBar.Fraction = 0.0;

		this.tmpChaosDisplay.Render();

		GLib.Timeout.Add( 200, Progress_Update_Handler );
	}

	bool ProgressUpdate ()
	{

		this._ComputationProgressBar.Fraction = this.tmpChaosDisplay.Progress;

		if ( this.tmpChaosDisplay.Progress == 1.0 )
		{
			this._ComputationProgressBar.Text = this._ComputationProgressBar.Text + "Completed: " + this._ComputationProgressTimer.HumanFriendlyTime ( this._ComputationProgressTimer.Current_Time_Stamp ) + " (" + this._ComputationProgressTimer.Current_Time_Stamp.ToString() + "ms)";
			return false;
		}
		else
		{
			this._ComputationProgressBar.Text = "[Elapsed: " + this._ComputationProgressTimer.HumanFriendlyTime ( this._ComputationProgressTimer.Current_Time_Stamp ) + "] ";
			this._ComputationProgressBar.Text = this._ComputationProgressBar.Text + System.Math.Round(this.tmpChaosDisplay.Progress * 100, 0).ToString() + "%";
			this._ComputationProgressBar.Text = this._ComputationProgressBar.Text + " [Estimated Time Remaining: " + this._ComputationProgressTimer.HumanFriendlyTime ( (int)(this._ComputationProgressTimer.Current_Time_Stamp / this.tmpChaosDisplay.Progress) - this._ComputationProgressTimer.Current_Time_Stamp ) +  "]";
			return true;
		}
	}

	void SetRenderQuality ( int pQualityLevel )
	{
		//Todo:  Make this cludge set itself right when the renderer changes!  Likely, force it to "1"
		if (!this._CurrentQualitySet)
		{
			this._CurrentQualitySet = true;

			this.WriteToLog ("Render Quality Changed to: " + pQualityLevel.ToString());

			switch ( pQualityLevel )
			{
				case 0:
					this.tmpChaosDisplay.RenderQuality = 0;
					break;

				case 1:
					this.tmpChaosDisplay.RenderQuality = 1;
					break;

				case 2:
					this.tmpChaosDisplay.RenderQuality = 3;
					break;

				case 3:
					this.tmpChaosDisplay.RenderQuality = 5;
					break;

				case 4:
					this.tmpChaosDisplay.RenderQuality = 7;
					break;

				case 5:
					this.tmpChaosDisplay.RenderQuality = 9;
					break;

				case 6:
					this.tmpChaosDisplay.RenderQuality = 11;
					break;

				case 7:
					this.tmpChaosDisplay.RenderQuality = 13;
					break;
			}

			for (int tmpCounter = 0; tmpCounter < 8; tmpCounter++)
			{
				if ( tmpCounter != pQualityLevel )
				{
					this._MenuQuality[tmpCounter].Active = false;
				}
			}
			this._CurrentQualitySet = false;
		}
	}
	#endregion


	void WriteToLog ( string pLogText )
	{
		this._ApplicationLogFile.WriteLogFile ( pLogText );
	}

	#region UI Event Handlers
	void OnMyWindowDelete (object pSender, DeleteEventArgs pArguments)
	{
		Application.Quit ();
		pArguments.RetVal = true;
	}

	protected void ChainedWriteLog( object pSender, WriteLogEventArgs pArguments )
	{
		//Write it to the textual log file if it is > 0
		if ( pArguments.LogLevel > 0 )
		{
			this._ApplicationLogFile.WriteLogFile ( pArguments.LogText );
		}
	}
	#endregion


	#region Menu Structure and Event Handlers
	void MenuSwitchEngineMandelbrot ( object pSender, EventArgs pArguments )
	{
		this.WriteToLog ( "Engine Switched to Mandelbrot" );
		this.tmpChaosDisplay.SetEngine( "Mandelbrot" );
		this.RenderImage();
	}

	void MenuSwitchEngineJulia ( object pSender, EventArgs pArguments )
	{
		this.WriteToLog ( "Engine Switched to Julia" );
		this.tmpChaosDisplay.SetEngine( "Julia" );
		this.RenderImage();
	}

	void MenuSwitchEngineGrid ( object pSender, EventArgs pArguments )
	{
		this.WriteToLog ( "Engine Switched to Debug Grid" );
		this.tmpChaosDisplay.SetEngine( "Grid Debug" );
		this.RenderImage();
	}

	void Menu_Copy_Parameters ( object pSender, EventArgs pArguments )
	{
	}

	void MenuSelectionHide ( object pSender, EventArgs pArguments )
	{
		this.tmpChaosDisplay.HideSelection();
	}

	void MenuSelectionShow ( object pSender, EventArgs pArguments )
	{
		this.tmpChaosDisplay.ShowSelection();
	}

	void Menu_Exit ( object pSender, EventArgs pArguments )
	{
		Application.Quit();
	}

	void Menu_Render (object pSender, EventArgs pArguments)
	{
		this.RenderImage();
	}

	//TODO: Learn to do this the right way with GTK ... the documentation is so frustrating sometimes.
	void MenuQualityChange0 (object pSender, EventArgs pArguments)
	{
		this.SetRenderQuality ( 0  );
	}

	void MenuQualityChange1 (object pSender, EventArgs pArguments)
	{
		this.SetRenderQuality ( 1 );
	}

	void MenuQualityChange2 (object pSender, EventArgs pArguments)
	{
		this.SetRenderQuality ( 2 );
	}

	void MenuQualityChange3 (object pSender, EventArgs pArguments)
	{
		this.SetRenderQuality ( 3 );
	}

	void MenuQualityChange4 (object pSender, EventArgs pArguments)
	{
		this.SetRenderQuality ( 4 );
	}

	void MenuQualityChange5 (object pSender, EventArgs pArguments)
	{
		this.SetRenderQuality ( 5 );
	}

	void MenuQualityChange6 (object pSender, EventArgs pArguments)
	{
		this.SetRenderQuality ( 6 );
	}

	void MenuQualityChange7 (object pSender, EventArgs pArguments)
	{
		this.SetRenderQuality ( 7 );
	}

	void MenuQualityToggleVisual (object pSender, EventArgs pArguments)
	{
		this.tmpChaosDisplay.RenderVisual = ((CheckMenuItem)pSender).Active;
	}

	private MenuBar _ApplicationMenu()
	{
		MenuItem tmpMenuItem, tmpMenuHeader;

		SeparatorMenuItem tmpSeparator;

		MenuBar tmpMainMenu = new MenuBar ();

		//The "File" menu
		Menu tmpFileMenu = new Menu ();

		tmpMenuItem = new ImageMenuItem (Stock.Open, _AcceleratorGroup);
		//tmp_Menu_Item.Activated += new EventHandler ();
		tmpFileMenu.Append (tmpMenuItem);

		tmpMenuItem = new ImageMenuItem (Stock.Save, _AcceleratorGroup);
		//tmp_Menu_Item.Activated += new EventHandler ();
		tmpFileMenu.Append (tmpMenuItem);

		tmpMenuItem = new ImageMenuItem (Stock.SaveAs, _AcceleratorGroup);
		//tmp_Menu_Item.Activated += new EventHandler ();
		tmpFileMenu.Append (tmpMenuItem);

		tmpMenuItem = new ImageMenuItem (Stock.New, _AcceleratorGroup);
		//tmp_Menu_Item.Activated += new EventHandler ();
		tmpFileMenu.Append (tmpMenuItem);

		tmpSeparator = new SeparatorMenuItem();
		tmpFileMenu.Append (tmpSeparator);

		tmpMenuItem = new MenuItem("_Export Image (unimplimented)");
		//tmp_Menu_Item.Activated += new EventHandler ();
		tmpFileMenu.Append (tmpMenuItem);

		tmpSeparator = new SeparatorMenuItem();
		tmpFileMenu.Append (tmpSeparator);

		tmpMenuItem = new ImageMenuItem (Stock.Quit, _AcceleratorGroup);
		tmpMenuItem.Activated += new EventHandler ( Menu_Exit );
		tmpFileMenu.Append (tmpMenuItem);

		tmpMenuHeader = new MenuItem("_File");
		tmpMenuHeader.Submenu = tmpFileMenu;

		tmpMainMenu.Append ( tmpMenuHeader );



		Menu tmpEditMenu = new Menu ();

		tmpMenuItem = new ImageMenuItem (Stock.Copy, _AcceleratorGroup);
		tmpMenuItem.Activated += new EventHandler ( Menu_Copy_Parameters );
		tmpEditMenu.Append (tmpMenuItem);

		tmpMenuItem = new MenuItem("Copy _Image");
		//tmp_Menu_Item.Activated += new EventHandler (  );
		tmpEditMenu.Append ( tmpMenuItem );

		tmpMenuItem = new ImageMenuItem (Stock.Paste, _AcceleratorGroup);
		//tmp_Menu_Item.Activated += new EventHandler ();
		tmpEditMenu.Append (tmpMenuItem);

		tmpSeparator = new SeparatorMenuItem();
		tmpEditMenu.Append (tmpSeparator);

		tmpMenuItem = new MenuItem("_Show Selection");
		tmpMenuItem.Activated += new EventHandler ( MenuSelectionShow );
		tmpMenuItem.AddAccelerator ("activate", _AcceleratorGroup, new AccelKey (Gdk.Key.F11, Gdk.ModifierType.None, AccelFlags.Visible));
		tmpEditMenu.Append ( tmpMenuItem );

		tmpMenuItem = new MenuItem("_Hide Selection");
		tmpMenuItem.Activated += new EventHandler ( MenuSelectionHide );
		tmpMenuItem.AddAccelerator ("activate", _AcceleratorGroup, new AccelKey (Gdk.Key.F12, Gdk.ModifierType.None, AccelFlags.Visible));
		tmpEditMenu.Append ( tmpMenuItem );

		tmpMenuHeader = new MenuItem("_Edit");
		tmpMenuHeader.Submenu = tmpEditMenu;
		tmpMainMenu.Append ( tmpMenuHeader );


		Menu tmpEnginesMenu = new Menu ();
		tmpMenuItem = new MenuItem("_Mandelbrot");
		tmpMenuItem.Activated += new EventHandler ( MenuSwitchEngineMandelbrot );
		tmpEnginesMenu.Append ( tmpMenuItem );

		tmpMenuItem = new MenuItem("_Julia");
		tmpMenuItem.Activated += new EventHandler ( MenuSwitchEngineJulia );
		tmpEnginesMenu.Append ( tmpMenuItem );

		tmpMenuItem = new MenuItem("Debug Grid");
		tmpMenuItem.Activated += new EventHandler ( MenuSwitchEngineGrid );
		tmpEnginesMenu.Append ( tmpMenuItem );

		tmpMenuHeader = new MenuItem("E_ngines");
		tmpMenuHeader.Submenu = tmpEnginesMenu;
		tmpMainMenu.Append ( tmpMenuHeader );


		Menu tmpRenderMenu = new Menu ();

		tmpMenuItem = new MenuItem("_Render Image");
		tmpMenuItem.AddAccelerator ("activate", _AcceleratorGroup, new AccelKey (Gdk.Key.R, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
		tmpMenuItem.Activated += new EventHandler ( Menu_Render );
		tmpRenderMenu.Append (tmpMenuItem);

		tmpSeparator = new SeparatorMenuItem();
		tmpRenderMenu.Append (tmpSeparator);
		//...

		//Quality Submenu
		Menu tmpRenderQualityMenu = new Menu();

		this._MenuQuality = new CheckMenuItem[8];

		this._MenuQuality[0] = new CheckMenuItem("_0 - Low");
		this._MenuQuality[0].Activated += new EventHandler ( MenuQualityChange0 );
		tmpRenderQualityMenu.Append ( this._MenuQuality[0] );

		this._MenuQuality[1] = new CheckMenuItem("_1 - Standard");
		((CheckMenuItem)this._MenuQuality[1]).Active = true;
		this._MenuQuality[1].Activated += new EventHandler ( MenuQualityChange1 );
		tmpRenderQualityMenu.Append ( this._MenuQuality[1] );

		this._MenuQuality[2] = new CheckMenuItem("_2 - High");
		this._MenuQuality[2].Activated += new EventHandler ( MenuQualityChange2 );
		tmpRenderQualityMenu.Append ( this._MenuQuality[2] );

		this._MenuQuality[3] = new CheckMenuItem("_3 - Very High (Slow)");
		this._MenuQuality[3].Activated += new EventHandler ( MenuQualityChange3 );
		tmpRenderQualityMenu.Append ( this._MenuQuality[3] );

		this._MenuQuality[4] = new CheckMenuItem("_4 - Super High (Very Slow)");
		this._MenuQuality[4].Activated += new EventHandler ( MenuQualityChange4 );
		tmpRenderQualityMenu.Append ( this._MenuQuality[4] );

		this._MenuQuality[5] = new CheckMenuItem("_5 - Ultra Super High (Very Very Slow)");
		this._MenuQuality[5].Activated += new EventHandler ( MenuQualityChange5 );
		tmpRenderQualityMenu.Append ( this._MenuQuality[5] );

		this._MenuQuality[6] = new CheckMenuItem("_6 - Ultra Super High (Very Slow Squared)");
		this._MenuQuality[6].Activated += new EventHandler ( MenuQualityChange6 );
		tmpRenderQualityMenu.Append ( this._MenuQuality[6] );

		this._MenuQuality[7] = new CheckMenuItem("_7 - Gimongous");
		this._MenuQuality[7].Activated += new EventHandler ( MenuQualityChange7 );
		tmpRenderQualityMenu.Append ( this._MenuQuality[7] );

		tmpMenuItem = new MenuItem("Render _Quality");
		tmpMenuItem.Submenu = tmpRenderQualityMenu;
		tmpRenderMenu.Append ( tmpMenuItem );
		//End Quality Submenu

		tmpSeparator = new SeparatorMenuItem();
		tmpRenderMenu.Append (tmpSeparator);

		tmpMenuItem = new CheckMenuItem ("Visual");
		((CheckMenuItem)tmpMenuItem).Active = true;
		tmpMenuItem.Activated += new EventHandler ( MenuQualityToggleVisual );
		tmpRenderMenu.Append (tmpMenuItem );

		tmpMenuHeader = new MenuItem("_Render");
		tmpMenuHeader.Submenu = tmpRenderMenu;
		tmpMainMenu.Append ( tmpMenuHeader );

		return tmpMainMenu;
	}
	#endregion

	private Widget UserInterface()
	{
		VBox tmpWindowContainer = new VBox(false, 0);
		VBox tmpApplicationDivide = new VBox(false, 12);
		HBox tmpProgressSegment = new HBox(false, 6);

		tmpApplicationDivide.BorderWidth = 12;

		tmpApplicationDivide.PackStart( this.tmpChaosDisplay.Display , false, false, 0 );

		tmpProgressSegment.PackStart( this._ComputationProgressBar );

		tmpApplicationDivide.PackStart( tmpProgressSegment, false, false, 0 );

		tmpWindowContainer.PackStart( this._ApplicationMenu() , false, false, 0 );
		tmpWindowContainer.PackStart( tmpApplicationDivide, false, false, 0 );

		return tmpWindowContainer;
	}
}

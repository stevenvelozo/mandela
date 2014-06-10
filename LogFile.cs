using System;
using System.IO;

namespace Master_Log_File
{
	#region Event Arguments and Delegates
	/// <summary>
	/// This is the Write Log event arguments for logging events.
	/// </summary>
	public class Write_Log_Event_Args : EventArgs 
	{   
		private string arg_Log_Text = "";
		private int arg_Log_Level = 0;

		/// <summary>
		/// This generates the log text template to be sent when the event fires.
		/// </summary>
		/// <param name="ToLog_Text">The text to log.</param>
		/// <param name="ToLog_Level">The way the text is expected to be used; 0 for display, 1 for display and file, 2 for file only.</param>
		public Write_Log_Event_Args( string To_Log_Text, int To_Log_Level )
		{
			//Nothing fancy to be done, this is a data encapsulation class
			this.arg_Log_Text = To_Log_Text;
			this.arg_Log_Level = To_Log_Level;
		}

		// Properties.
		public string Log_Text
		{ 
			get 
			{ 
				return this.arg_Log_Text;
			}
		}

		public int Log_Level
		{
			get
			{
				return this.arg_Log_Level;
			}
		}    
	}

	/// <summary>
	/// This is a delegate that offers interfaces to log files and rolling log displays.
	/// </summary>	
	public delegate void Write_Log_Event_Handler (object sender, Write_Log_Event_Args e);
	#endregion

	#region Master Log File Class
	/// <summary>
	/// Base Main Rolling Log File class
	/// </summary>
	public class Log_File
	{
		private string File_Name_Preface;
		private string File_Path;
		private string File_Name;

		//The date of the log file -- to be prepended to the file name.
		private System.DateTime Log_Date;

		private FileInfo Log_File_Info;
		private StreamWriter Log_File_Stream_Writer;
		private bool Log_File_Open;
		
		private bool Write_To_Console;

		#region Initializers
		public Log_File ()
		{
			this.Initialize_File( this.GetHashCode().ToString(), "" );
		}
		
		public Log_File ( string File_Name_Preface )
		{
			this.Initialize_File( File_Name_Preface, "" );
		}

		/// <summary>
		/// Construct the Log File.
		/// </summary>
		/// <param name="FileNamePreface">The preface text appended to the file name.</param>
		/// <param name="FilePath">The location of the log file.</param>
		public Log_File( string File_Name_Preface, string File_Path )
		{
			this.Initialize_File( File_Name_Preface, File_Path );
		}

		private void Initialize_File( string File_Name_Preface, string File_Path )
		{
			//Initialize the class information
			if ( File_Path == "" )
				this.File_Path = System.AppDomain.CurrentDomain.BaseDirectory+"/";
			else
				this.File_Path = File_Path;

			this.File_Name_Preface = File_Name_Preface;
			
			this.Log_File_Open = false;
			
			this.Write_To_Console = false;
		}
		
		~Log_File()
		{
			//This is causing trouble.
			if ( this.Log_File_Open )
				this.Close_Log_File();
		}
		#endregion
		
		#region Data Access
		public string Log_File_Preface
		{
			get
			{
				return this.File_Name_Preface;
			}
			set
			{
				this.File_Name_Preface = value;
			}
		}
		
		public string Log_File_Path
		{
			get
			{
				return this.File_Path;
			}
			set
			{
				this.File_Path = value;
			}
		}
		
		public bool Echo_To_Console
		{
			get
			{
				return this.Write_To_Console;
			}
			set
			{
				this.Write_To_Console = value;
			}
		}
		#endregion
		
		/// <summary>
		/// Actually write the text to the log file.
		/// </summary>
		/// <param name="Log_Text">The text to be written.</param>
		public void Write_Log_File( string Log_Text )
		{
			try
			{
				if ( ( !this.Log_File_Open ) || ( System.DateTime.Now.Date != this.Log_Date.Date ) )
				{
					this.Open_Log_File();
				}

				//Now write out the time stamp and then the log text to a line in the log file
				//System.Diagnostics.Debug.WriteLine( string.Concat("WROTE [", CurrentTime.Hour, ":", CurrentTime.Minute, ":", CurrentTime.Second, "]", Log_Text) );
				this.Log_File_Stream_Writer.WriteLine( string.Concat("[", System.DateTime.Now.ToLongTimeString(), "]", Log_Text) );
				
				if ( this.Write_To_Console )
					Console.WriteLine( string.Concat("[", System.DateTime.Now.ToLongTimeString(), "]", Log_Text ) );

				//Since we send very little data, and the Stream Writer does not automatically
				//flush itself, we have to manually flush the stream after every write in order
				//to insure the lines will be written properly.
				this.Log_File_Stream_Writer.Flush();
			}
			catch
			{
			}
		}

		#region File Management
		/// <summary>
		/// Open the log file in the path with the specified name.
		/// </summary>
		private void Open_Log_File()
		{
			try
			{
				if ( Log_File_Open )
					this.Close_Log_File();

				//Set the log file date
				this.Log_Date = System.DateTime.Now;

				//Mash together the file name from the date and prefix
				this.File_Name = string.Concat( this.Log_Date.Year, "-", this.Log_Date.Month, "-", this.Log_Date.Day, "-", this.File_Name_Preface, ".log" );

				//Open up a file information class
				this.Log_File_Info = new FileInfo( string.Concat( this.File_Path, this.File_Name ) );

				//Now open up a stream writer to the opened file info class
				this.Log_File_Stream_Writer = this.Log_File_Info.AppendText();

				this.Log_File_Open = true;
			}
			catch
			{
			}
		}

		private void Close_Log_File()
		{
			try
			{
				this.Log_File_Stream_Writer.Close();
				this.Log_File_Open = false;
			}
			catch
			{
			}
        }
		#endregion
	}
	#endregion
	
	/// <summary>
	/// This class provides pass-through and shim event handlers for log files
	/// </summary>
	public class Pass_Through_Logged_Class
	{
		public event Write_Log_Event_Handler Write_Log;

		/// <summary>
		/// Write to the log file/display.
		/// </summary>
		/// <param name="Log_Text">The text to be written.</param>
		/// <param name="Log_Level">The numeric level of the log text.  In general, 0 is screen, 1 is both, 2 is file only.</param>
		protected virtual void Write_To_Log( string Log_Text, int Log_Level )
		{
			Write_Log_Event_Args e = new Write_Log_Event_Args( Log_Text, Log_Level );

			this.On_Write_Log ( e );
		}

		protected virtual void Write_To_Log( string Log_Text )
		{
			//Default log level is 1 (display and log to file)
			this.Write_To_Log( Log_Text, 1);
		}		


		protected virtual void On_Write_Log( Write_Log_Event_Args e )
		{
			if ( this.Write_Log != null )
			{
				//Invoke the event delegate
				Write_Log ( this, e );
			}
		}

		protected virtual void Chained_Write_Log( object sender, Write_Log_Event_Args e )
		{
			//A shim function to chain log events from objects here to the main application's events.
			this.On_Write_Log( e );
		}
	}
	
	/// <summary>
	/// This class provides pass-through and logging facilities, for classes that need to
	/// both log data and pass the data through to the next class via events and shims
	/// </summary>
	public class Master_Logged_Class : Pass_Through_Logged_Class
	{
		private Log_File Main_Log_File;
		
		public Master_Logged_Class()
		{
			this.Main_Log_File = new Log_File ();
		}
		
		#region Data Access
		/// <summary>
		/// The prefix to go before log files.
		/// Defaults to the classes unique hash code.
		/// </summary>
		protected string Log_File_Prefix
		{
			set
			{
				this.Main_Log_File.Log_File_Preface = value;
			}
			get
			{
				return this.Main_Log_File.Log_File_Preface;
			}
		}
		
		/// <summary>
		/// The path that the log file resides in.
		/// Defaults to the application path.
		/// </summary>
		protected string Log_File_Path
		{
			set
			{
				this.Main_Log_File.Log_File_Path = value;
			}
			get
			{
				return this.Main_Log_File.Log_File_Path;
			}
		}
		
		/// <summary>
		/// If this is true we will echo any logged events < 2 to the console
		/// </summary>
		protected bool Log_File_To_Console
		{
			set
			{
				this.Main_Log_File.Echo_To_Console = value;
			}
			get
			{
				return this.Main_Log_File.Echo_To_Console;
			}
		}
		#endregion

		/// <summary>
		/// Write to the log file/display.
		/// </summary>
		/// <param name="Log_Text">The text to be written.</param>
		/// <param name="Log_Level">The numeric level of the log text.  In general, 0 is screen, 1 is both, 2 is file only.</param>
		protected override void Write_To_Log( string Log_Text, int Log_Level )
		{
			Write_Log_Event_Args e = new Write_Log_Event_Args( Log_Text, Log_Level );

			this.On_Write_Log ( e );

			//Write it to the textual log file if it is > 0
			if ( e.Log_Level > 0 )
			{
				this.Main_Log_File.Write_Log_File ( e.Log_Text );
			}
		}

		protected override void Chained_Write_Log( object sender, Write_Log_Event_Args e )
		{
			//A shim function to chain log events from objects here to the main application's events.
			this.On_Write_Log( e );

			//Write it to the textual log file if it is > 0
			if ( e.Log_Level > 0 )
			{
				this.Main_Log_File.Write_Log_File ( e.Log_Text );
			}
		}
	}
}

using System;
using System.IO;

namespace MasterLogFile
{
	#region Event Arguments and Delegates
	/// <summary>
	/// This is the Write Log event arguments for logging events.
	/// </summary>
	public class WriteLogEventArgs : EventArgs
	{
		private string _LogText = "";
		private int _LogLevel = 0;

		/// <summary>
		/// This generates the log text template to be sent when the event fires.
		/// </summary>
		/// <param name="ToLog_Text">The text to log.</param>
		/// <param name="ToLog_Level">The way the text is expected to be used; 0 for display, 1 for display and file, 2 for file only.</param>
		public WriteLogEventArgs( string pLogText, int pLogLevel )
		{
			//Nothing fancy to be done, this is a data encapsulation class
			this._LogText = pLogText;
			this._LogLevel = pLogLevel;
		}

		// Properties.
		public string LogText
		{
			get { return this._LogText; }
		}

		public int LogLevel
		{
			get { return this._LogLevel; }
		}
	}

	/// <summary>
	/// This is a delegate that offers interfaces to log files and rolling log displays.
	/// </summary>
	public delegate void WriteLogEventHandler (object pSender, WriteLogEventArgs pArguments);
	#endregion

	#region Master Log File Class
	/// <summary>
	/// Base Main Rolling Log File class
	/// </summary>
	public class LogFile
	{
		private string _FileNamePreface;
		private string _FilePath;
		private string _FileName;

		//The date of the log file -- to be prepended to the file name.
		private System.DateTime _LogDate;

		private FileInfo _LogFileInfo;
		private StreamWriter _LogFileStreamWriter;
		private bool _LogFileOpen;

		private bool _WriteToConsole;

		#region Initializers
		public LogFile ()
		{
			this.InitializeFile( this.GetHashCode().ToString(), "" );
		}

		public LogFile (string pFileNamePreface)
		{
			this.InitializeFile( pFileNamePreface, "" );
		}

		/// <summary>
		/// Construct the Log File.
		/// </summary>
		/// <param name="FileNamePreface">The preface text appended to the file name.</param>
		/// <param name="FilePath">The location of the log file.</param>
		public LogFile( string pFileNamePreface, string pFilePath )
		{
			this.InitializeFile( pFileNamePreface, pFilePath );
		}

		private void InitializeFile( string pFileNamePreface, string pFilePath )
		{
			//Initialize the class information
			if ( pFilePath == "" )
				this._FilePath = System.AppDomain.CurrentDomain.BaseDirectory+"/";
			else
				this._FilePath = pFilePath;

			this._FileNamePreface = pFileNamePreface;

			this._LogFileOpen = false;

			this._WriteToConsole = false;
		}

		~LogFile()
		{
			//This is causing trouble.
			if ( this._LogFileOpen )
				this.CloseLogFile();
		}
		#endregion

		#region Data Access
		public string LogFilePrefix
		{
			get { return this._FileNamePreface; }
			set { this._FileNamePreface = value; }
		}

		public string Log_File_Path
		{
			get { return this._FilePath; }
			set { this._FilePath = value; }
		}

		public bool EchoToConsole
		{
			get { return this._WriteToConsole; }
			set { this._WriteToConsole = value; }
		}
		#endregion

		/// <summary>
		/// Actually write the text to the log file.
		/// </summary>
		/// <param name="Log_Text">The text to be written.</param>
		public void WriteLogFile( string pLogText )
		{
			try
			{
				if ( ( !this._LogFileOpen ) || ( System.DateTime.Now.Date != this._LogDate.Date ) )
					this.OpenLogFile();

				//Now write out the time stamp and then the log text to a line in the log file
				//System.Diagnostics.Debug.WriteLine( string.Concat("WROTE [", CurrentTime.Hour, ":", CurrentTime.Minute, ":", CurrentTime.Second, "]", Log_Text) );
				this._LogFileStreamWriter.WriteLine( string.Concat("[", System.DateTime.Now.ToLongTimeString(), "]", pLogText) );

				if ( this._WriteToConsole )
					Console.WriteLine( string.Concat("[", System.DateTime.Now.ToLongTimeString(), "]", pLogText ) );

				//Since we send very little data, and the Stream Writer does not automatically
				//flush itself, we have to manually flush the stream after every write in order
				//to insure the lines will be written properly.
				this._LogFileStreamWriter.Flush();
			}
			catch
			{
				// Meh
			}
		}

		#region File Management
		/// <summary>
		/// Open the log file in the path with the specified name.
		/// </summary>
		private void OpenLogFile()
		{
			try
			{
				if ( _LogFileOpen )
					this.CloseLogFile();

				//Set the log file date
				this._LogDate = System.DateTime.Now;

				//Mash together the file name from the date and prefix
				this._FileName = string.Concat( this._LogDate.Year, "-", this._LogDate.Month, "-", this._LogDate.Day, "-", this._FileNamePreface, ".log" );

				//Open up a file information class
				this._LogFileInfo = new FileInfo( string.Concat( this._FilePath, this._FileName ) );

				//Now open up a stream writer to the opened file info class
				this._LogFileStreamWriter = this._LogFileInfo.AppendText();

				this._LogFileOpen = true;
			}
			catch
			{
			}
		}

		private void CloseLogFile()
		{
			try
			{
				this._LogFileStreamWriter.Close();
				this._LogFileOpen = false;
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
	public class PassThroughLoggedClass
	{
		public event WriteLogEventHandler WriteLog;

		/// <summary>
		/// Write to the log file/display.
		/// </summary>
		/// <param name="Log_Text">The text to be written.</param>
		/// <param name="Log_Level">The numeric level of the log text.  In general, 0 is screen, 1 is both, 2 is file only.</param>
		protected virtual void WriteToLog( string pLogText, int pLogLevel )
		{
			WriteLogEventArgs pArguments = new WriteLogEventArgs( pLogText, pLogLevel );

			this.OnWriteLog ( pArguments );
		}

		protected virtual void WriteToLog( string pLogText )
		{
			//Default log level is 1 (display and log to file)
			this.WriteToLog( pLogText, 1);
		}


		protected virtual void OnWriteLog( WriteLogEventArgs pArguments )
		{
			if ( this.WriteLog != null )
			{
				//Invoke the event delegate
				WriteLog ( this, pArguments );
			}
		}

		protected virtual void ChainedWriteLog( object sender, WriteLogEventArgs pArguments )
		{
			//A shim function to chain log events from objects here to the main application's events.
			this.OnWriteLog( pArguments );
		}
	}

	/// <summary>
	/// This class provides pass-through and logging facilities, for classes that need to
	/// both log data and pass the data through to the next class via events and shims
	/// </summary>
	public class Master_Logged_Class : PassThroughLoggedClass
	{
		private LogFile _LogFile;

		public Master_Logged_Class()
		{
			this._LogFile = new LogFile ();
		}

		#region Data Access
		/// <summary>
		/// The prefix to go before log files.
		/// Defaults to the classes unique hash code.
		/// </summary>
		protected string LogFilePrefix
		{
			set { this._LogFile.LogFilePrefix = value; }
			get { return this._LogFile.LogFilePrefix; }
		}

		/// <summary>
		/// The path that the log file resides in.
		/// Defaults to the application path.
		/// </summary>
		protected string Log_File_Path
		{
			set { this._LogFile.Log_File_Path = value; }
			get { return this._LogFile.Log_File_Path; }
		}

		/// <summary>
		/// If this is true we will echo any logged events < 2 to the console
		/// </summary>
		protected bool LogFileToConsole
		{
			set { this._LogFile.EchoToConsole = value; }
			get { return this._LogFile.EchoToConsole; }
		}
		#endregion

		/// <summary>
		/// Write to the log file/display.
		/// </summary>
		/// <param name="Log_Text">The text to be written.</param>
		/// <param name="Log_Level">The numeric level of the log text.  In general, 0 is screen, 1 is both, 2 is file only.</param>
		protected override void WriteToLog( string pLogText, int pLogLevel )
		{
			WriteLogEventArgs tmpArguments = new WriteLogEventArgs( pLogText, pLogLevel );

			this.OnWriteLog ( tmpArguments );

			//Write it to the textual log file if it is > 0
			if ( tmpArguments.LogLevel > 0 )
			{
				this._LogFile.WriteLogFile ( tmpArguments.LogText );
			}
		}

		protected override void ChainedWriteLog( object pSender, WriteLogEventArgs pArguments )
		{
			//A shim function to chain log events from objects here to the main application's events.
			this.OnWriteLog( pArguments );

			//Write it to the textual log file if it is > 0
			if ( pArguments.LogLevel > 0 )
			{
				this._LogFile.WriteLogFile ( pArguments.LogText );
			}
		}
	}
}

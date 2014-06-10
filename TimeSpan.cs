/*
 * Time Span Class
 *
 * Used for timing with processor tick lapses as the unit (approximate ms)
 */

using System;

namespace Master_Time_Span
{
	/// <summary>
	/// Description of Time_Span.
	/// </summary>
	public class Time_Span
	{
		private int Processor_Tick_Start;
		private int Processor_Tick_Stop;

		private int Last_Difference;

		public Time_Span()
		{
			//No sense not starting the timer at instantiation
			this.Start();
		}

		#region Data Access
		public int Time_Difference
		{
			get
			{
				return this.Last_Difference;
			}
		}
		#endregion

		public void Start()
		{
			//Set the start time
			this.Processor_Tick_Start = System.Environment.TickCount;

			//Reset the difference
			this.Last_Difference = 0;
		}

		public void Time_Stamp()
		{
			//Set the stop time
			this.Processor_Tick_Stop = System.Environment.TickCount;

			//Figure out the difference
			this.Last_Difference = this.Processor_Tick_Stop - this.Processor_Tick_Start;
		}

		public int Current_Time_Stamp
		{
			get
			{
				return System.Environment.TickCount - this.Processor_Tick_Start;
			}
		}

		public string Human_Friendly_Time ( int Milliseconds )
		{
			int Seconds = 0;
			int Minutes = 0;
			int Hours = 0;

			int Days = 0;

			string CompleteDate = "";

			Seconds = Milliseconds / 1000;

			if ( Seconds > 0 )
			{
				Minutes = Seconds / 60;

				if ( Minutes > 0 )
				{
					Hours = Minutes / 60;

					if ( Hours > 0 )
					{
						Days = Hours / 24;
					}
				}
			}

			if ( Days > 0 )
				CompleteDate = CompleteDate + Days.ToString() + "d ";

			if ( Hours%24 < 10 )
			{
				CompleteDate = CompleteDate + "0";
			}
			CompleteDate = CompleteDate + (Hours%24).ToString() + ":";


			if ( Minutes%60 < 10 )
			{
				CompleteDate = CompleteDate + "0";
			}
			CompleteDate = CompleteDate + (Minutes%60).ToString() + ":";

			if ( Seconds%60 < 10 )
			{
				CompleteDate = CompleteDate + "0";
			}
			CompleteDate = CompleteDate + (Seconds%60).ToString();

			return CompleteDate;
		}
	}
}

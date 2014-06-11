/*
 * Time Span Class
 *
 * Used for timing with processor tick lapses as the unit (approximate ms)
 */
using System;

namespace MasterTimeSpan
{
	/// <summary>
	/// Description of TimeSpanMetric.
	/// </summary>
	public class TimeSpanMetric
	{
		private int _TickStart;
		private int _TickStop;

		private int _LastDifference;

		public TimeSpanMetric()
		{
			//No sense not starting the timer at instantiation
			this.Start();
		}

		#region Data Access
		public int TimeDifference
		{
			get { return this._LastDifference; }
		}
		#endregion

		public void Start()
		{
			//Set the start time
			this._TickStart = System.Environment.TickCount;

			//Reset the difference
			this._LastDifference = 0;
		}

		public void Time_Stamp()
		{
			//Set the stop time
			this._TickStop = System.Environment.TickCount;

			//Figure out the difference
			this._LastDifference = this._TickStop - this._TickStart;
		}

		public int Current_Time_Stamp
		{
			get { return System.Environment.TickCount - this._TickStart; }
		}

		public string HumanFriendlyTime ( int Milliseconds )
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
						Days = Hours / 24;
				}
			}

			if ( Days > 0 )
				CompleteDate = CompleteDate + Days.ToString() + "d ";

			if ( Hours%24 < 10 )
				CompleteDate = CompleteDate + "0";
			CompleteDate = CompleteDate + (Hours%24).ToString() + ":";


			if ( Minutes%60 < 10 )
				CompleteDate = CompleteDate + "0";
			CompleteDate = CompleteDate + (Minutes%60).ToString() + ":";

			if ( Seconds%60 < 10 )
				CompleteDate = CompleteDate + "0";
			CompleteDate = CompleteDate + (Seconds%60).ToString();

			return CompleteDate;
		}
	}
}

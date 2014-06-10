using System;
using System.Threading;
using Gdk;
using GLib;
using Master_Log_File;
using Master_Time_Span;

namespace Master_Chaos_Display
{
	//Render Method(s)
	class Chaos_Renderer : Pass_Through_Logged_Class
	{
		//The rendering machine to render on
		private Chaos_Rendering_Machine Output_Port;

		//The rendering engine to render
		private Chaos_Engine Rendering_Engine;

		//This is the amount that is completed of the render (from 0.0 to 1.0)
		private double Render_Progress_Amount;

		private bool Currently_Rendering;

		//If this is true we will render a visual preview and divide it into quads
		private bool Visual_Render;

		//If this is true we will only render 1/4 of the pixels for a quick preview mode
		private bool Quad_Sampling;

		public Chaos_Renderer ( Chaos_Rendering_Machine To_Output_Port, Chaos_Engine From_Rendering_Engine )
		{
			//Assign the object references for this renderer.
			this.Rendering_Engine = From_Rendering_Engine;
			this.Output_Port = To_Output_Port;

			this.Render_Progress_Amount = 0;

			//Default to visual interactive renders
			this.Visual_Render = true;
		}

		public void Render()
		{
			if ( !this.Currently_Rendering )
			{
				ThreadStart Render_Thread_Delegate;

				this.Currently_Rendering = true;

				//Now set the color count
				this.Rendering_Engine.Set_Parameter( "Screen_Color_Count", this.Output_Port.Pallete.Count.ToString() );

				if (this.Visual_Render && !this.Quad_Sampling)
				{
					Render_Thread_Delegate = new ThreadStart(this.Render_Full_Visual);
				}
				else
				{
					Render_Thread_Delegate = new ThreadStart(this.Render_Full);
				}

				System.Threading.Thread Render_Thread = new System.Threading.Thread(Render_Thread_Delegate);
				Render_Thread.Start();
			}
		}

		private void Render_Full_Visual()
		{
			this.Write_To_Log ( "*** Visually Hinted Image Computation Beginning ***" );
			this.Write_To_Log ( this.Rendering_Engine.Name + " " + this.Rendering_Engine.Parameter_Serialization );
			Time_Span Chaos_Computation_Timer = new Time_Span();

			int Screen_X_Counter, Screen_Y_Counter;
			double Chaos_X_Counter, Chaos_Y_Counter;
			double Chaos_X_Incriment, Chaos_Y_Incriment;
			int Total_Pixel_Count, Current_Pixel_Counter;

			//A thread-safe container for the display refresh event
			IdleHandler Idle_Message_Container_Refresh = new IdleHandler(this.Refresh_Display_Safe);

			this.Rendering_Engine.Render_Begin();

			//Compute the Pixel to Chaos space ratio
			Chaos_X_Incriment = (double)((this.Rendering_Engine.Chaos_Max_X - this.Rendering_Engine.Chaos_Min_X) / (double)Output_Port.Width );
			//Chaos_Y_Incriment = (double)((this.Rendering_Engine.Chaos_Max_Y - this.Rendering_Engine.Chaos_Min_Y) / (double)Output_Port.Height );
			Chaos_Y_Incriment = Chaos_X_Incriment;

			//Compute the pixel counter for the progress bar.
			Total_Pixel_Count = Output_Port.Width * Output_Port.Height;
			Current_Pixel_Counter = 0;

			Output_Port.Last_X_Origin = this.Rendering_Engine.Chaos_Min_X;
			Output_Port.Last_Y_Origin = this.Rendering_Engine.Chaos_Min_Y;
			Output_Port.Last_X_Ratio = Chaos_X_Incriment;
			Output_Port.Last_Y_Ratio = Chaos_Y_Incriment;
			this.Rendering_Engine.Current_Render_Ratio = Chaos_X_Incriment;

			//Clear the render port
			for (Screen_X_Counter = 0; Screen_X_Counter < Output_Port.Width; Screen_X_Counter++)
			{
				for (Screen_Y_Counter = 0; Screen_Y_Counter < Output_Port.Height; Screen_Y_Counter++)
				{
					Output_Port.Mark_Pixel ( Screen_X_Counter, Screen_Y_Counter, -1 );
				}
			}

			this.Write_To_Log ( " --> Viewport Cleared" );
			//Refresh the display
			Gdk.Threads.Enter();
			GLib.Idle.Add( Idle_Message_Container_Refresh );
			Gdk.Threads.Leave();

			//Now render a quad fractal preview
			Chaos_X_Counter = this.Rendering_Engine.Chaos_Min_X;
			for (Screen_X_Counter = 0; Screen_X_Counter < Output_Port.Width; Screen_X_Counter++)
			{
				Chaos_Y_Counter = this.Rendering_Engine.Chaos_Min_Y;
				for (Screen_Y_Counter = 0; Screen_Y_Counter < Output_Port.Height; Screen_Y_Counter++)
				{
					if ( (Screen_X_Counter % 2 == 0) && (Screen_Y_Counter % 2 == 0) )
					{
						Output_Port.Mark_Pixel_Quad ( Screen_X_Counter, Screen_Y_Counter, this.Rendering_Engine.Render_Pixel ( Chaos_X_Counter, Chaos_Y_Counter ) );
						Current_Pixel_Counter++;
					}

					//if ( Current_Pixel_Counter % (this.Output_Port.Width*4) == 1 )
					//{
					this.Render_Progress_Amount = ((double)Current_Pixel_Counter/(double)Total_Pixel_Count);
					//}

					Chaos_Y_Counter += Chaos_X_Incriment;
				}

				Chaos_X_Counter += Chaos_Y_Incriment;
			}

			this.Write_To_Log ( " --> Quad 0,0 Done [" + Current_Pixel_Counter.ToString()+"/"+Total_Pixel_Count.ToString()+"]" );
			//Refresh the display
			Gdk.Threads.Enter();
			GLib.Idle.Add( Idle_Message_Container_Refresh );
			Gdk.Threads.Leave();

			//Now render the 1,1 fractal set
			Chaos_X_Counter = this.Rendering_Engine.Chaos_Min_X;
			for (Screen_X_Counter = 0; Screen_X_Counter < Output_Port.Width; Screen_X_Counter++)
			{
				Chaos_Y_Counter = this.Rendering_Engine.Chaos_Min_Y;
				for (Screen_Y_Counter = 0; Screen_Y_Counter < Output_Port.Height; Screen_Y_Counter++)
				{
					if ( (Screen_X_Counter % 2 == 1) && (Screen_Y_Counter % 2 == 1) )
					{
						Output_Port.Mark_Pixel ( Screen_X_Counter, Screen_Y_Counter, this.Rendering_Engine.Render_Pixel ( Chaos_X_Counter, Chaos_Y_Counter ) );
						Current_Pixel_Counter++;
					}

					this.Render_Progress_Amount = ((double)Current_Pixel_Counter/(double)Total_Pixel_Count);

					Chaos_Y_Counter += Chaos_X_Incriment;
				}

				Chaos_X_Counter += Chaos_Y_Incriment;
			}

			this.Write_To_Log ( " --> Subquad 1,1 Done [" + Current_Pixel_Counter.ToString()+"/"+Total_Pixel_Count.ToString()+"]" );

			//Refresh the display
			Gdk.Threads.Enter();
			GLib.Idle.Add( Idle_Message_Container_Refresh );
			Gdk.Threads.Leave();

			//Now render the 1,0 fractal set
			Chaos_X_Counter = this.Rendering_Engine.Chaos_Min_X;
			for (Screen_X_Counter = 0; Screen_X_Counter < Output_Port.Width; Screen_X_Counter++)
			{
				Chaos_Y_Counter = this.Rendering_Engine.Chaos_Min_Y;
				for (Screen_Y_Counter = 0; Screen_Y_Counter < Output_Port.Height; Screen_Y_Counter++)
				{
					if ( (Screen_X_Counter % 2 == 1) && (Screen_Y_Counter % 2 == 0) )
					{
						Output_Port.Mark_Pixel ( Screen_X_Counter, Screen_Y_Counter, this.Rendering_Engine.Render_Pixel ( Chaos_X_Counter, Chaos_Y_Counter ) );
						Current_Pixel_Counter++;
					}

					this.Render_Progress_Amount = ((double)Current_Pixel_Counter/(double)Total_Pixel_Count);

					Chaos_Y_Counter += Chaos_X_Incriment;
				}

				Chaos_X_Counter += Chaos_Y_Incriment;
			}

			this.Write_To_Log ( " --> Subquad 1,0 Done [" + Current_Pixel_Counter.ToString()+"/"+Total_Pixel_Count.ToString()+"]" );
			//Refresh the display
			Gdk.Threads.Enter();
			GLib.Idle.Add( Idle_Message_Container_Refresh );
			Gdk.Threads.Leave();

			//Now render the 0,1 fractal set
			Chaos_X_Counter = this.Rendering_Engine.Chaos_Min_X;
			for (Screen_X_Counter = 0; Screen_X_Counter < Output_Port.Width; Screen_X_Counter++)
			{
				Chaos_Y_Counter = this.Rendering_Engine.Chaos_Min_Y;
				for (Screen_Y_Counter = 0; Screen_Y_Counter < Output_Port.Height; Screen_Y_Counter++)
				{
					if ( (Screen_X_Counter % 2 == 0) && (Screen_Y_Counter % 2 == 1) )
					{
						Output_Port.Mark_Pixel ( Screen_X_Counter, Screen_Y_Counter, this.Rendering_Engine.Render_Pixel ( Chaos_X_Counter, Chaos_Y_Counter ) );
						Current_Pixel_Counter++;
					}

					//if ( Current_Pixel_Counter % (this.Output_Port.Width*4) == 1 )
					//{
					this.Render_Progress_Amount = ((double)Current_Pixel_Counter/(double)Total_Pixel_Count);
					//}

					Chaos_Y_Counter += Chaos_X_Incriment;
				}

				Chaos_X_Counter += Chaos_Y_Incriment;
			}

			this.Write_To_Log ( " --> Subquad 0,1 Done [" + Current_Pixel_Counter.ToString()+"/"+Total_Pixel_Count.ToString()+"]" );

			//Refresh the display
			Gdk.Threads.Enter();
			GLib.Idle.Add( Idle_Message_Container_Refresh );
			Gdk.Threads.Leave();


			this.Rendering_Engine.Render_End();

			//Update the progress at 100%
			this.Render_Progress_Amount = 1;

			Chaos_Computation_Timer.Time_Stamp();
			this.Write_To_Log ( "*** Image Computation Complete (" + Chaos_Computation_Timer.Time_Difference.ToString() + "ms) ***" );
			this.Currently_Rendering = false;
		}

		private void Render_Full()
		{
			this.Write_To_Log ( "*** Silent Image Computation Beginning ***" );
			this.Write_To_Log ( this.Rendering_Engine.Name + " " + this.Rendering_Engine.Parameter_Serialization );
			Time_Span Chaos_Computation_Timer = new Time_Span();

			int Screen_X_Counter, Screen_Y_Counter;
			double Chaos_X_Counter, Chaos_Y_Counter;
			double Chaos_X_Incriment, Chaos_Y_Incriment;
			int Total_Pixel_Count, Current_Pixel_Counter;

			//A thread-safe container for the display refresh event
			IdleHandler Idle_Message_Container_Refresh = new IdleHandler(this.Refresh_Display_Safe);

			this.Rendering_Engine.Render_Begin();

			//Compute the Pixel to Chaos space ratio
			Chaos_X_Incriment = (double)((this.Rendering_Engine.Chaos_Max_X - this.Rendering_Engine.Chaos_Min_X) / (double)Output_Port.Width );
			//Chaos_Y_Incriment = (double)((this.Rendering_Engine.Chaos_Max_Y - this.Rendering_Engine.Chaos_Min_Y) / (double)Output_Port.Height );
			Chaos_Y_Incriment = Chaos_X_Incriment;

			//Compute the pixel counter for the progress bar.
			Total_Pixel_Count = Output_Port.Width * Output_Port.Height;
			Current_Pixel_Counter = 0;

			Output_Port.Last_X_Origin = this.Rendering_Engine.Chaos_Min_X;
			Output_Port.Last_Y_Origin = this.Rendering_Engine.Chaos_Min_Y;
			Output_Port.Last_X_Ratio = Chaos_X_Incriment;
			Output_Port.Last_Y_Ratio = Chaos_Y_Incriment;
			this.Rendering_Engine.Current_Render_Ratio = Chaos_X_Incriment;

			Chaos_X_Counter = this.Rendering_Engine.Chaos_Min_X;
			for (Screen_X_Counter = 0; Screen_X_Counter < Output_Port.Width; Screen_X_Counter++)
			{
				Chaos_Y_Counter = this.Rendering_Engine.Chaos_Min_Y;
				for (Screen_Y_Counter = 0; Screen_Y_Counter < Output_Port.Height; Screen_Y_Counter++)
				{
 					if ( this.Quad_Sampling  )
					{
						if ( (Screen_X_Counter % 2 == 0) && (Screen_Y_Counter % 2 == 0) )
						{
							Output_Port.Mark_Pixel_Quad ( Screen_X_Counter, Screen_Y_Counter, this.Rendering_Engine.Render_Pixel ( Chaos_X_Counter, Chaos_Y_Counter ) );
						}
						Current_Pixel_Counter++;
					}
					else
					{
						Output_Port.Mark_Pixel ( Screen_X_Counter, Screen_Y_Counter, this.Rendering_Engine.Render_Pixel ( Chaos_X_Counter, Chaos_Y_Counter ) );

						Current_Pixel_Counter++;
					}

					//if ( Current_Pixel_Counter % (this.Output_Port.Width*4) == 1 )
					//{
					this.Render_Progress_Amount = ((double)Current_Pixel_Counter/(double)Total_Pixel_Count);
					//}

					Chaos_Y_Counter += Chaos_X_Incriment;
				}

				Chaos_X_Counter += Chaos_Y_Incriment;
			}

			this.Rendering_Engine.Render_End();

			//It's now safe to refresh the display and such.
			Gdk.Threads.Enter();
			GLib.Idle.Add( Idle_Message_Container_Refresh );
			Gdk.Threads.Leave();


			//Update the progress at 100%
			this.Render_Progress_Amount = 1;

			Chaos_Computation_Timer.Time_Stamp();
			this.Write_To_Log ( "*** Image Computation Complete (" + Chaos_Computation_Timer.Time_Difference.ToString() + "ms) ***" );
			this.Currently_Rendering = false;
		}

		//Progress Bar Data Access
		public double Progress
		{
			get
			{
				return this.Render_Progress_Amount;
			}
		}

		public bool Rendering
		{
			get
			{
				return this.Currently_Rendering;
			}
		}

		public int Render_Quality
		{
			set
			{
				if ( value > 0 )
				{
					this.Write_To_Log ( "Quality Set To: " + value.ToString() );

					this.Rendering_Engine.Current_Render_Quality = value;
					this.Quad_Sampling = false;
				}
				else
				{
					this.Write_To_Log ( "Quality Set To: 0 (preview)" );

					//Zero or subzero means a special quality where the renderer is on 1 and we only do 1/4 pixels
					this.Rendering_Engine.Current_Render_Quality = 1;
					this.Quad_Sampling = true;
				}

			}
		}

		public bool Render_Visual
		{
			set
			{
				if (value)
				{
					this.Write_To_Log ( "Visually Hinted Render Mode Activated" );
				}
				else
				{
					this.Write_To_Log ( "Silent Render Mode Activated" );
				}

				this.Visual_Render = value;
			}
		}

		public Chaos_Engine Engine
		{
			set
			{
				this.Rendering_Engine = value;
			}
		}

		#region Display Refresh Event
		public event EventHandler Refresh_Display;

		public bool Refresh_Display_Safe()
		{
			EventArgs e = new EventArgs();

			this.Refresh_Display( this, e );

			return false;
		}
		#endregion
	}
}
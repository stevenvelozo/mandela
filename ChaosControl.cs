using System;
using Gtk;
using Master_Super_Image;
using Master_Time_Span;
using Master_Log_File;

namespace Master_Chaos_Display
{
	// Todo: Create some exposed methods for the UI to use such as zoom in, out, etc.
	// Todo: Make some event handlers for the engine list specifically to keep everything in tune with the current engine.
	// Todo: Set up a method to zoom out.
	class Chaos_Display : Pass_Through_Logged_Class
	{
		//The GTK Widget for capturing mouse input and such.
		private EventBox Chaos_Display_Box;

		//The Rendering Machine for the main output display
		private Chaos_Rendering_Machine Output_Display_Machine;

		//The Render Method to tie the Engine to the Machine
		private Chaos_Renderer Render_Method;

		//The list of engines
		private Chaos_Engine_List Chaos_Output_Engine_List;

		//This is true while the mouse is down!
		private bool Mouse_Button_Down;

		//This sets up the zoom factor for a click.
		//It should be changable with the mouse wheel (1.0 is same size and minimum, 2.0 is 1/2, 3.0 is 1/3, etc -- maximum is 200)
		private double Zoom_Factor;

		public Chaos_Display( int Display_Width, int Display_Height )
		{
			//Create a gradient of Dark Grey to White for our colors to start out.
			Super_Color Infinite_Color, Gradient_Start_Color, Gradient_End_Color;

			Infinite_Color = new Super_Color( 0, 0, 0 );

			Gradient_Start_Color = new Super_Color( 75, 0, 40 );
			Gradient_End_Color = new Super_Color( 0, 202, 243 );

			//Instantiate the display box and the event handler for it.
			this.Chaos_Display_Box = new EventBox();
			this.Chaos_Display_Box.ButtonPressEvent += new ButtonPressEventHandler ( this.Chaos_Display_Click_Event );
			this.Chaos_Display_Box.ButtonReleaseEvent += new ButtonReleaseEventHandler ( this.Chaos_Display_Click_Release );
			this.Chaos_Display_Box.MotionNotifyEvent += new MotionNotifyEventHandler ( this.Chaos_Display_Mouse_Motion );
			this.Chaos_Display_Box.ScrollEvent += new ScrollEventHandler ( this.Chaos_Display_Mouse_Scrolled );

			//The ubiquitous main view.
			this.Output_Display_Machine = new Chaos_Rendering_Machine("MainView", Display_Width, Display_Height, Infinite_Color, Gradient_Start_Color, Gradient_End_Color, 500 );
			this.Output_Display_Machine.Write_Log += new Write_Log_Event_Handler ( this.Chained_Machine_Write_Log );

			//Create a chaos engine list
			this.Chaos_Output_Engine_List = new Chaos_Engine_List();

			this.Instantiate_Engines();

			//The rendering method
			//Hook up the output machine and the engine with the renderer
			this.Render_Method = new Chaos_Renderer ( this.Output_Display_Machine, this.Chaos_Output_Engine_List.Engine );
			this.Render_Method.Refresh_Display += new EventHandler ( Render_Update_Display_Event );
			this.Render_Method.Write_Log += new Write_Log_Event_Handler ( Chained_Renderer_Write_Log );

			this.Reset_Rendering_Machine();
		}

		private void Instantiate_Engines()
		{
			Chaos_Engine tmp_Chaos_Engine;

			tmp_Chaos_Engine = new Mandelbrot_Engine();
			tmp_Chaos_Engine.Write_Log += new Write_Log_Event_Handler ( this.Chained_Engine_Write_Log );
			this.Chaos_Output_Engine_List.Add_Engine( tmp_Chaos_Engine.Name, tmp_Chaos_Engine );

			tmp_Chaos_Engine = new Julia_Engine();
			tmp_Chaos_Engine.Write_Log += new Write_Log_Event_Handler ( this.Chained_Engine_Write_Log );
			this.Chaos_Output_Engine_List.Add_Engine( tmp_Chaos_Engine.Name, tmp_Chaos_Engine );

			tmp_Chaos_Engine = new Grid_Test_Engine();
			tmp_Chaos_Engine.Write_Log += new Write_Log_Event_Handler ( this.Chained_Engine_Write_Log );
			this.Chaos_Output_Engine_List.Add_Engine( tmp_Chaos_Engine.Name, tmp_Chaos_Engine );

			tmp_Chaos_Engine = new Chaos_Engine();
			tmp_Chaos_Engine.Write_Log += new Write_Log_Event_Handler ( this.Chained_Engine_Write_Log );
			this.Chaos_Output_Engine_List.Add_Engine( tmp_Chaos_Engine.Name, tmp_Chaos_Engine );

			this.Chaos_Output_Engine_List.Move_First();
		}

		public void Render()
		{
			//This actually spawns a thread but uses safe message passing to gtk for progress bars etc.
			if ( !this.Render_Method.Rendering )
				this.Render_Method.Render();
		}

		public void Reset_Rendering_Machine()
		{
			this.Mouse_Button_Down = false;

			//In case someone starts refactoring the zoom factor without clicking, default to the center.
			this.Output_Display_Machine.Zoom_Centerpoint_X = (int)(this.Output_Display_Machine.Width / 2);
			this.Output_Display_Machine.Zoom_Centerpoint_Y = (int)(this.Output_Display_Machine.Height / 2);;

			this.Zoom_Factor = 4.0;
		}

		void Render_Update_Display_Event (object Sender, EventArgs Arguments )
		{
			this.Output_Display_Machine.Refresh();
		}

		void Chaos_Display_Refactor_Zoom ( int X_Centerpoint, int Y_Centerpoint )
		{
			double Chaos_X_Center, Chaos_Y_Center, tmpCoordinate;

			//Fairly elegant.
			//Compute the Cartesian centerpoint
			Chaos_X_Center = this.Output_Display_Machine.Last_X_Origin + (X_Centerpoint * this.Output_Display_Machine.Last_X_Ratio);
			Chaos_Y_Center = this.Output_Display_Machine.Last_Y_Origin + (Y_Centerpoint * this.Output_Display_Machine.Last_Y_Ratio);

			//Save the last clicked centerpoint for things like interdependant chaos renderers
			this.Output_Display_Machine.Last_Clicked_X_Centerpoint = Chaos_X_Center;
			this.Output_Display_Machine.Last_Clicked_Y_Centerpoint = Chaos_Y_Center;

			//Also save the top left point and width/height of the box
			this.Output_Display_Machine.Zoom_Centerpoint_X = X_Centerpoint;
			this.Output_Display_Machine.Zoom_Centerpoint_Y = Y_Centerpoint;

			this.Output_Display_Machine.Zoom_Width = (int)(this.Output_Display_Machine.Width / this.Zoom_Factor);
			this.Output_Display_Machine.Zoom_Height = (int)(this.Output_Display_Machine.Height / this.Zoom_Factor);

			this.Output_Display_Machine.Zoom_Top_Left_X = X_Centerpoint - (int)(this.Output_Display_Machine.Zoom_Width / 2);
			this.Output_Display_Machine.Zoom_Top_Left_Y = Y_Centerpoint - (int)(this.Output_Display_Machine.Zoom_Height / 2);

			this.Output_Display_Machine.Selection_Set();

			tmpCoordinate = Chaos_X_Center - ((this.Output_Display_Machine.Zoom_Width / 2) * this.Output_Display_Machine.Last_X_Ratio);
			this.Chaos_Output_Engine_List.Engine.Set_Parameter( "XMin", tmpCoordinate.ToString() );

			tmpCoordinate = Chaos_X_Center + ((this.Output_Display_Machine.Zoom_Width / 2) * this.Output_Display_Machine.Last_X_Ratio);
			this.Chaos_Output_Engine_List.Engine.Set_Parameter( "XMax", tmpCoordinate.ToString() );

			//Ignoring Y ratio to keep the aspect ratio proper for now.
			tmpCoordinate = Chaos_Y_Center - ((this.Output_Display_Machine.Zoom_Height / 2) * this.Output_Display_Machine.Last_X_Ratio);
			this.Chaos_Output_Engine_List.Engine.Set_Parameter( "YMin", tmpCoordinate.ToString() );
			tmpCoordinate = Chaos_Y_Center + ((this.Output_Display_Machine.Zoom_Height / 2) * this.Output_Display_Machine.Last_X_Ratio);
			this.Chaos_Output_Engine_List.Engine.Set_Parameter( "YMax", tmpCoordinate.ToString() );

			//this.Write_To_Log ("ZoomSet: Screen["+X_Centerpoint.ToString()+","+Y_Centerpoint.ToString()+"] Cartesian["+Chaos_X_Center.ToString()+","+Chaos_Y_Center.ToString()+"] Factor[1/"+this.Zoom_Factor.ToString()+"]");
		}

		#region Chaos Render Event
		public event EventHandler Render_Chaos_Image;

		//Safely call the "Render" event
		public bool Render_Chaos_Image_Safe()
		{
			EventArgs e = new EventArgs();

			this.Render_Chaos_Image( this, e );

			return false;
		}
		#endregion


		#region Mouse Event Handling
		void Chaos_Display_Mouse_Scrolled ( object Sender, ScrollEventArgs Arguments )
		{
			//Todo: alter this to scale based on size, but probably on a linear ramp.
			if ( !this.Rendering )
			{
				Gdk.EventScroll EventInfo = Arguments.Event;

				switch (EventInfo.Direction)
				{
					case Gdk.ScrollDirection.Up:
						this.Zoom_Factor = Math.Min( this.Zoom_Factor+0.25, 100.0 );
						//this.Write_To_Log ( "Zooming In To 1/"+this.Zoom_Factor.ToString() );
						this.Chaos_Display_Refactor_Zoom( this.Output_Display_Machine.Zoom_Centerpoint_X, this.Output_Display_Machine.Zoom_Centerpoint_Y );
						break;

					case Gdk.ScrollDirection.Down:
						this.Zoom_Factor = Math.Max( this.Zoom_Factor-0.25, 1.0 );
						//this.Write_To_Log ( "Zooming Out To 1/"+this.Zoom_Factor.ToString() );
						this.Chaos_Display_Refactor_Zoom( this.Output_Display_Machine.Zoom_Centerpoint_X, this.Output_Display_Machine.Zoom_Centerpoint_Y );
						break;
				}
			}

			Arguments.RetVal = true;
		}

		void Chaos_Display_Click_Event (object Sender, ButtonPressEventArgs Arguments)
		{
			if ( !this.Rendering )
			{
				Gdk.EventButton EventInfo = Arguments.Event;

				if ( (EventInfo.Button == 1) || (EventInfo.Button == 2) )
				{
					this.Chaos_Display_Refactor_Zoom ( (int)EventInfo.X, (int)EventInfo.Y );

					this.Mouse_Button_Down = true;
				}

				if ( (EventInfo.Button == 2) || (EventInfo.Button == 3) )
				{
					//Generate an event to render the image
					this.Render_Chaos_Image_Safe();
				}
			}

			Arguments.RetVal = true;
		}

		void Chaos_Display_Mouse_Motion (object Sender, MotionNotifyEventArgs Arguments)
		{
			if ( !this.Rendering )
			{
				if ( this.Mouse_Button_Down )
				{
					Gdk.EventMotion EventInfo = Arguments.Event;

					if ( (this.Output_Display_Machine.Zoom_Centerpoint_X != (int)EventInfo.X) || (this.Output_Display_Machine.Zoom_Centerpoint_Y != (int)EventInfo.Y) )
					{
						this.Chaos_Display_Refactor_Zoom ( (int)EventInfo.X, (int)EventInfo.Y );
					}
				}
			}

			Arguments.RetVal = true;
		}

		void Chaos_Display_Click_Release (object Sender, ButtonReleaseEventArgs Arguments)
		{
			if ( !this.Rendering )
			{
				//this.Write_To_Log ("Mouse Released");
				this.Mouse_Button_Down = false;
			}

			Arguments.RetVal = true;
		}
		#endregion

		#region Data Access and Control
		public bool Rendering
		{
			get
			{
				return this.Render_Method.Rendering;
			}
		}

		public double Progress
		{
			get
			{
				return this.Render_Method.Progress;
			}
		}

		public int Render_Quality
		{
			set
			{
				this.Render_Method.Render_Quality = value;
			}
		}

		public bool Render_Visual
		{
			set
			{
				this.Render_Method.Render_Visual = value;
			}
		}

		public Widget Display
		{
			get
			{
				VBox Chaos_Boundary = new VBox();

				this.Chaos_Display_Box.Add( this.Output_Display_Machine.Image );

				Chaos_Boundary.PackStart(this.Chaos_Display_Box, false, false, 0);

				return Chaos_Boundary;
			}
		}

		public void Set_Engine ( string Engine_Name )
		{
			this.Chaos_Output_Engine_List.Find_First_By_Name ( Engine_Name );

			this.Render_Method.Engine = this.Chaos_Output_Engine_List.Engine;

			this.Reset_Rendering_Machine();
		}

		public void Hide_Selection ()
		{
			this.Output_Display_Machine.Hide_Selection();
		}

		public void Show_Selection ()
		{
			this.Chaos_Display_Refactor_Zoom( this.Output_Display_Machine.Zoom_Centerpoint_X, this.Output_Display_Machine.Zoom_Centerpoint_Y );
		}
		#endregion

		#region Chained Log Events
		protected void Chained_Engine_Write_Log( object sender, Write_Log_Event_Args e )
		{
			this.Write_To_Log ( "Engine " + (sender as Chaos_Engine).Name+":"+e.Log_Text );
		}

		protected void Chained_Machine_Write_Log( object sender, Write_Log_Event_Args e )
		{
			this.Write_To_Log ( "Machine : "+e.Log_Text );
		}

		protected void Chained_Renderer_Write_Log( object sender, Write_Log_Event_Args e )
		{
			this.Write_To_Log ( "Renderer : "+e.Log_Text );
		}
		#endregion
	}
}
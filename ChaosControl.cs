using System;
using Gtk;
using MasterSuperImage;
using MasterTimeSpan;
using MasterLogFile;

namespace MasterChaosDisplay
{
	// Todo: Create some exposed methods for the UI to use such as zoom in, out, etc.
	// Todo: Make some event handlers for the engine list specifically to keep everything in tune with the current engine.
	// Todo: Set up a method to zoom out.
	class ChaosDisplay : PassThroughLoggedClass
	{
		//The GTK Widget for capturing mouse input and such.
		private EventBox _ChaosDisplayBox;

		//The Rendering Machine for the main output display
		private ChaosRenderingMachine _OutputChaosRenderingMachine;

		//The Render Method to tie the Engine to the Machine
		private ChaosRenderer _ChaosRenderer;

		//The list of engines
		private ChaosEngineList _ChaosEngineList;

		//This is true while the mouse is down!
		private bool _MouseButtonIsDown;

		//This sets up the zoom factor for a click.
		//It should be changable with the mouse wheel (1.0 is same size and minimum, 2.0 is 1/2, 3.0 is 1/3, etc -- maximum is 200)
		private double _ZoomFactor;

		public ChaosDisplay( int pDisplayWidth, int pDisplayHeight )
		{
			//Create a gradient of Dark Grey to White for our colors to start out.
			SuperColor tmpInfiniteColor, tmpGradientStartColor, tmpGradientEndColor;

			tmpInfiniteColor = new SuperColor( 0, 0, 0 );

			tmpGradientStartColor = new SuperColor( 75, 0, 40 );
			tmpGradientEndColor = new SuperColor( 0, 202, 243 );

			//Instantiate the display box and the event handler for it.
			this._ChaosDisplayBox = new EventBox();
			this._ChaosDisplayBox.ButtonPressEvent += new ButtonPressEventHandler ( this.ChaosDisplayClickEvent );
			this._ChaosDisplayBox.ButtonReleaseEvent += new ButtonReleaseEventHandler ( this.ChaosDisplayClickReleaseEvent );
			this._ChaosDisplayBox.MotionNotifyEvent += new MotionNotifyEventHandler ( this.ChaosDisplayMouseMotionEvent );
			this._ChaosDisplayBox.ScrollEvent += new ScrollEventHandler ( this.ChaosDisplayMouseScrolledEvent );

			//The ubiquitous main view.
			this._OutputChaosRenderingMachine = new ChaosRenderingMachine("MainView", pDisplayWidth, pDisplayHeight, tmpInfiniteColor, tmpGradientStartColor, tmpGradientEndColor, 500 );
			this._OutputChaosRenderingMachine.WriteLog += new WriteLogEventHandler ( this.ChainedMachineWriteLog );

			//Create a chaos engine list
			this._ChaosEngineList = new ChaosEngineList();

			this.InstantiateEngines();

			//The rendering method
			//Hook up the output machine and the engine with the renderer
			this._ChaosRenderer = new ChaosRenderer ( this._OutputChaosRenderingMachine, this._ChaosEngineList.Engine );
			this._ChaosRenderer.RefreshDisplay += new EventHandler ( RenderUpdateDisplayEvent );
			this._ChaosRenderer.WriteLog += new WriteLogEventHandler ( ChainedRendererWriteLog );

			this.ResetRenderingMachines();
		}

		private void InstantiateEngines()
		{
			ChaosEngine tmpChaosEngine;

			tmpChaosEngine = new Mandelbrot_Engine();
			tmpChaosEngine.WriteLog += new WriteLogEventHandler ( this.ChainedEngineWriteLog );
			this._ChaosEngineList.AddEngine( tmpChaosEngine.Name, tmpChaosEngine );

			tmpChaosEngine = new Julia_Engine();
			tmpChaosEngine.WriteLog += new WriteLogEventHandler ( this.ChainedEngineWriteLog );
			this._ChaosEngineList.AddEngine( tmpChaosEngine.Name, tmpChaosEngine );

			tmpChaosEngine = new Grid_Test_Engine();
			tmpChaosEngine.WriteLog += new WriteLogEventHandler ( this.ChainedEngineWriteLog );
			this._ChaosEngineList.AddEngine( tmpChaosEngine.Name, tmpChaosEngine );

			tmpChaosEngine = new ChaosEngine();
			tmpChaosEngine.WriteLog += new WriteLogEventHandler ( this.ChainedEngineWriteLog );
			this._ChaosEngineList.AddEngine( tmpChaosEngine.Name, tmpChaosEngine );

			this._ChaosEngineList.MoveFirst();
		}

		public void Render()
		{
			//This actually spawns a thread but uses safe message passing to gtk for progress bars etc.
			if ( !this._ChaosRenderer.Rendering )
				this._ChaosRenderer.Render();
		}

		public void ResetRenderingMachines()
		{
			this._MouseButtonIsDown = false;

			//In case someone starts refactoring the zoom factor without clicking, default to the center.
			this._OutputChaosRenderingMachine._ZoomCenterpointX = (int)(this._OutputChaosRenderingMachine.Width / 2);
			this._OutputChaosRenderingMachine._ZoomCenterpointY = (int)(this._OutputChaosRenderingMachine.Height / 2);;

			this._ZoomFactor = 4.0;
		}

		void RenderUpdateDisplayEvent (object pSender, EventArgs pArguments )
		{
			this._OutputChaosRenderingMachine.Refresh();
		}

		void ChaosDisplayRefactorZoom ( int pMidpointX, int pMidpointY )
		{
			double tmpXCenter, tmpYCenter, tmpCoordinate;

			//Fairly elegant.
			//Compute the Cartesian centerpoint
			tmpXCenter = this._OutputChaosRenderingMachine.LastXOrigin + (pMidpointX * this._OutputChaosRenderingMachine.LastXRatio);
			tmpYCenter = this._OutputChaosRenderingMachine.LastYOrigin + (pMidpointY * this._OutputChaosRenderingMachine.LastYRatio);

			//Save the last clicked centerpoint for things like interdependant chaos renderers
			this._OutputChaosRenderingMachine.LastClickedXCenterpoint = tmpXCenter;
			this._OutputChaosRenderingMachine.LastClickedYCenterpoint = tmpYCenter;

			//Also save the top left point and width/height of the box
			this._OutputChaosRenderingMachine._ZoomCenterpointX = pMidpointX;
			this._OutputChaosRenderingMachine._ZoomCenterpointY = pMidpointY;

			this._OutputChaosRenderingMachine._ZoomWidth = (int)(this._OutputChaosRenderingMachine.Width / this._ZoomFactor);
			this._OutputChaosRenderingMachine._ZoomHeight = (int)(this._OutputChaosRenderingMachine.Height / this._ZoomFactor);

			this._OutputChaosRenderingMachine._ZoomTopLeftX = pMidpointX - (int)(this._OutputChaosRenderingMachine._ZoomWidth / 2);
			this._OutputChaosRenderingMachine._ZoomTopLeftY = pMidpointY - (int)(this._OutputChaosRenderingMachine._ZoomHeight / 2);

			this._OutputChaosRenderingMachine.SelectionSet();

			tmpCoordinate = tmpXCenter - ((this._OutputChaosRenderingMachine._ZoomWidth / 2) * this._OutputChaosRenderingMachine.LastXRatio);
			this._ChaosEngineList.Engine.SetParameter( "XMin", tmpCoordinate.ToString() );

			tmpCoordinate = tmpXCenter + ((this._OutputChaosRenderingMachine._ZoomWidth / 2) * this._OutputChaosRenderingMachine.LastXRatio);
			this._ChaosEngineList.Engine.SetParameter( "XMax", tmpCoordinate.ToString() );

			//Ignoring Y ratio to keep the aspect ratio proper for now.
			tmpCoordinate = tmpYCenter - ((this._OutputChaosRenderingMachine._ZoomHeight / 2) * this._OutputChaosRenderingMachine.LastXRatio);
			this._ChaosEngineList.Engine.SetParameter( "YMin", tmpCoordinate.ToString() );
			tmpCoordinate = tmpYCenter + ((this._OutputChaosRenderingMachine._ZoomHeight / 2) * this._OutputChaosRenderingMachine.LastXRatio);
			this._ChaosEngineList.Engine.SetParameter( "YMax", tmpCoordinate.ToString() );

			//this.Write_To_Log ("ZoomSet: Screen["+X_Centerpoint.ToString()+","+Y_Centerpoint.ToString()+"] Cartesian["+Chaos_X_Center.ToString()+","+Chaos_Y_Center.ToString()+"] Factor[1/"+this.Zoom_Factor.ToString()+"]");
		}

		#region Chaos Render Event
		public event EventHandler RenderChaosImage;

		//Safely call the "Render" event
		public bool RenderChaosImageSafe()
		{
			EventArgs tmpArguments = new EventArgs();

			this.RenderChaosImage( this, tmpArguments );

			return false;
		}
		#endregion


		#region Mouse Event Handling
		void ChaosDisplayMouseScrolledEvent ( object pSender, ScrollEventArgs pArguments )
		{
			//Todo: alter this to scale based on size, but probably on a linear ramp.
			if ( !this.Rendering )
			{
				Gdk.EventScroll tmpEventInfo = pArguments.Event;

				switch (tmpEventInfo.Direction)
				{
					case Gdk.ScrollDirection.Up:
						this._ZoomFactor = Math.Min( this._ZoomFactor+0.25, 100.0 );
						//this.Write_To_Log ( "Zooming In To 1/"+this.Zoom_Factor.ToString() );
						this.ChaosDisplayRefactorZoom( this._OutputChaosRenderingMachine._ZoomCenterpointX, this._OutputChaosRenderingMachine._ZoomCenterpointY );
						break;

					case Gdk.ScrollDirection.Down:
						this._ZoomFactor = Math.Max( this._ZoomFactor-0.25, 1.0 );
						//this.Write_To_Log ( "Zooming Out To 1/"+this.Zoom_Factor.ToString() );
						this.ChaosDisplayRefactorZoom( this._OutputChaosRenderingMachine._ZoomCenterpointX, this._OutputChaosRenderingMachine._ZoomCenterpointY );
						break;
				}
			}

			pArguments.RetVal = true;
		}

		void ChaosDisplayClickEvent (object pSender, ButtonPressEventArgs pArguments)
		{
			if ( !this.Rendering )
			{
				Gdk.EventButton tmpEventInfo = pArguments.Event;

				if ( (tmpEventInfo.Button == 1) || (tmpEventInfo.Button == 2) )
				{
					this.ChaosDisplayRefactorZoom ( (int)tmpEventInfo.X, (int)tmpEventInfo.Y );

					this._MouseButtonIsDown = true;
				}

				if ( (tmpEventInfo.Button == 2) || (tmpEventInfo.Button == 3) )
					//Generate an event to render the image
					this.RenderChaosImageSafe();
			}

			pArguments.RetVal = true;
		}

		void ChaosDisplayMouseMotionEvent (object pSender, MotionNotifyEventArgs pArguments)
		{
			if ( !this.Rendering )
			{
				if ( this._MouseButtonIsDown )
				{
					Gdk.EventMotion EventInfo = pArguments.Event;

					if ( (this._OutputChaosRenderingMachine._ZoomCenterpointX != (int)EventInfo.X) || (this._OutputChaosRenderingMachine._ZoomCenterpointY != (int)EventInfo.Y) )
						this.ChaosDisplayRefactorZoom ( (int)EventInfo.X, (int)EventInfo.Y );
				}
			}

			pArguments.RetVal = true;
		}

		void ChaosDisplayClickReleaseEvent (object Sender, ButtonReleaseEventArgs Arguments)
		{
			if ( !this.Rendering )
				//this.Write_To_Log ("Mouse Released");
				this._MouseButtonIsDown = false;

			Arguments.RetVal = true;
		}
		#endregion

		#region Data Access and Control
		public bool Rendering
		{
			get { return this._ChaosRenderer.Rendering; }
		}

		public double Progress
		{
			get { return this._ChaosRenderer.Progress; }
		}

		public int RenderQuality
		{
			set { this._ChaosRenderer.RenderQuality = value; }
		}

		public bool RenderVisual
		{
			set { this._ChaosRenderer.RenderVisual = value; }
		}

		public Widget Display
		{
			get
			{
				VBox tmpChaosBoundary = new VBox();

				this._ChaosDisplayBox.Add( this._OutputChaosRenderingMachine.Image );

				tmpChaosBoundary.PackStart(this._ChaosDisplayBox, false, false, 0);

				return tmpChaosBoundary;
			}
		}

		public void SetEngine ( string pEngineName )
		{
			this._ChaosEngineList.FindFirstByName ( pEngineName );

			this._ChaosRenderer.Engine = this._ChaosEngineList.Engine;

			this.ResetRenderingMachines();
		}

		public void HideSelection ()
		{
			this._OutputChaosRenderingMachine.HideSelection();
		}

		public void ShowSelection ()
		{
			this.ChaosDisplayRefactorZoom( this._OutputChaosRenderingMachine._ZoomCenterpointX, this._OutputChaosRenderingMachine._ZoomCenterpointY );
		}
		#endregion

		#region Chained Log Events
		protected void ChainedEngineWriteLog( object pSender, WriteLogEventArgs pArguments )
		{
			this.WriteToLog ( "Engine " + (pSender as ChaosEngine).Name+":"+pArguments.LogText );
		}

		protected void ChainedMachineWriteLog( object pSender, WriteLogEventArgs pArguments )
		{
			this.WriteToLog ( "Machine : "+pArguments.LogText );
		}

		protected void ChainedRendererWriteLog( object pSender, WriteLogEventArgs pArguments )
		{
			this.WriteToLog ( "Renderer : "+pArguments.LogText );
		}
		#endregion
	}
}
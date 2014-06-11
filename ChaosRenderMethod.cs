using System;
using System.Threading;
using Gdk;
using GLib;
using MasterLogFile;
using MasterTimeSpan;

namespace MasterChaosDisplay
{
	//Render Method(s)
	class ChaosRenderer : PassThroughLoggedClass
	{
		//The rendering machine to render on
		private ChaosRenderingMachine _OutputPort;

		//The rendering engine to render
		private ChaosEngine _RenderingEngine;

		//This is the amount that is completed of the render (from 0.0 to 1.0)
		private double _RenderProgressAmount;

		private bool _CurrentlyRendering;

		//If this is true we will render a visual preview and divide it into quads
		private bool _VisualRender;

		//If this is true we will only render 1/4 of the pixels for a quick preview mode
		private bool _QuadSampling;

		public ChaosRenderer ( ChaosRenderingMachine pOutputPort, ChaosEngine pRenderingEngine )
		{
			//Assign the object references for this renderer.
			this._RenderingEngine = pRenderingEngine;
			this._OutputPort = pOutputPort;

			this._RenderProgressAmount = 0;

			//Default to visual interactive renders
			this._VisualRender = true;
		}

		public void Render()
		{
			if ( !this._CurrentlyRendering )
			{
				ThreadStart tmpRenderThreadStart;

				this._CurrentlyRendering = true;

				//Now set the color count
				this._RenderingEngine.SetParameter( "Screen_Color_Count", this._OutputPort.Pallete.Count.ToString() );

				if (this._VisualRender && !this._QuadSampling)
					tmpRenderThreadStart = new ThreadStart(this.RenderFullVisual);
				else
					tmpRenderThreadStart = new ThreadStart(this.RenderFull);

				System.Threading.Thread tmpRenderThread = new System.Threading.Thread(tmpRenderThreadStart);
				tmpRenderThread.Start();
			}
		}

		private void RenderFullVisual()
		{
			this.WriteToLog ( "*** Visually Hinted Image Computation Beginning ***" );
			this.WriteToLog ( this._RenderingEngine.Name + " " + this._RenderingEngine.ParameterSerialization );
			TimeSpanMetric tmpRenderTimer = new TimeSpanMetric();

			int tmpScreenXCounter, tmpScreenYCounter;
			double tmpChaosXCounter, tmpChaosYCounter;
			double tmpChaosXIncrement, tmpChaosYIncrement;
			int tmpTotalPixelCount, tmpCurrentPixelCount;

			//A thread-safe container for the display refresh event
			IdleHandler tmpIdleMessageRefreshContainer = new IdleHandler(this.RefreshDisplaySafe);

			this._RenderingEngine.RenderBegin();

			//Compute the Pixel to Chaos space ratio
			tmpChaosXIncrement = (double)((this._RenderingEngine.ChaosMaxX - this._RenderingEngine.ChaosMinX) / (double)_OutputPort.Width );
			//Chaos_Y_Incriment = (double)((this.Rendering_Engine.Chaos_Max_Y - this.Rendering_Engine.Chaos_Min_Y) / (double)Output_Port.Height );
			tmpChaosYIncrement = tmpChaosXIncrement;

			//Compute the pixel counter for the progress bar.
			tmpTotalPixelCount = _OutputPort.Width * _OutputPort.Height;
			tmpCurrentPixelCount = 0;

			_OutputPort.LastXOrigin = this._RenderingEngine.ChaosMinX;
			_OutputPort.LastYOrigin = this._RenderingEngine.ChaosMinY;
			_OutputPort.LastXRatio = tmpChaosXIncrement;
			_OutputPort.LastYRatio = tmpChaosYIncrement;
			this._RenderingEngine.CurrentRenderRatio = tmpChaosXIncrement;

			//Clear the render port
			for (tmpScreenXCounter = 0; tmpScreenXCounter < _OutputPort.Width; tmpScreenXCounter++)
			{
				for (tmpScreenYCounter = 0; tmpScreenYCounter < _OutputPort.Height; tmpScreenYCounter++)
					_OutputPort.MarkPixel ( tmpScreenXCounter, tmpScreenYCounter, -1 );
			}

			this.WriteToLog ( " --> Viewport Cleared" );
			//Refresh the display
			Gdk.Threads.Enter();
			GLib.Idle.Add( tmpIdleMessageRefreshContainer );
			Gdk.Threads.Leave();

			//Now render a quad fractal preview
			tmpChaosXCounter = this._RenderingEngine.ChaosMinX;
			for (tmpScreenXCounter = 0; tmpScreenXCounter < _OutputPort.Width; tmpScreenXCounter++)
			{
				tmpChaosYCounter = this._RenderingEngine.ChaosMinY;
				for (tmpScreenYCounter = 0; tmpScreenYCounter < _OutputPort.Height; tmpScreenYCounter++)
				{
					if ( (tmpScreenXCounter % 2 == 0) && (tmpScreenYCounter % 2 == 0) )
					{
						_OutputPort.MarkPixelQuad ( tmpScreenXCounter, tmpScreenYCounter, this._RenderingEngine.RenderPixel ( tmpChaosXCounter, tmpChaosYCounter ) );
						tmpCurrentPixelCount++;
					}

					//if ( Current_Pixel_Counter % (this.Output_Port.Width*4) == 1 )
					//{
					this._RenderProgressAmount = ((double)tmpCurrentPixelCount/(double)tmpTotalPixelCount);
					//}

					tmpChaosYCounter += tmpChaosXIncrement;
				}

				tmpChaosXCounter += tmpChaosYIncrement;
			}

			this.WriteToLog ( " --> Quad 0,0 Done [" + tmpCurrentPixelCount.ToString()+"/"+tmpTotalPixelCount.ToString()+"]" );
			//Refresh the display
			Gdk.Threads.Enter();
			GLib.Idle.Add( tmpIdleMessageRefreshContainer );
			Gdk.Threads.Leave();

			//Now render the 1,1 fractal set
			tmpChaosXCounter = this._RenderingEngine.ChaosMinX;
			for (tmpScreenXCounter = 0; tmpScreenXCounter < _OutputPort.Width; tmpScreenXCounter++)
			{
				tmpChaosYCounter = this._RenderingEngine.ChaosMinY;
				for (tmpScreenYCounter = 0; tmpScreenYCounter < _OutputPort.Height; tmpScreenYCounter++)
				{
					if ( (tmpScreenXCounter % 2 == 1) && (tmpScreenYCounter % 2 == 1) )
					{
						_OutputPort.MarkPixel ( tmpScreenXCounter, tmpScreenYCounter, this._RenderingEngine.RenderPixel ( tmpChaosXCounter, tmpChaosYCounter ) );
						tmpCurrentPixelCount++;
					}

					this._RenderProgressAmount = ((double)tmpCurrentPixelCount/(double)tmpTotalPixelCount);

					tmpChaosYCounter += tmpChaosXIncrement;
				}

				tmpChaosXCounter += tmpChaosYIncrement;
			}

			this.WriteToLog ( " --> Subquad 1,1 Done [" + tmpCurrentPixelCount.ToString()+"/"+tmpTotalPixelCount.ToString()+"]" );

			//Refresh the display
			Gdk.Threads.Enter();
			GLib.Idle.Add( tmpIdleMessageRefreshContainer );
			Gdk.Threads.Leave();

			//Now render the 1,0 fractal set
			tmpChaosXCounter = this._RenderingEngine.ChaosMinX;
			for (tmpScreenXCounter = 0; tmpScreenXCounter < _OutputPort.Width; tmpScreenXCounter++)
			{
				tmpChaosYCounter = this._RenderingEngine.ChaosMinY;
				for (tmpScreenYCounter = 0; tmpScreenYCounter < _OutputPort.Height; tmpScreenYCounter++)
				{
					if ( (tmpScreenXCounter % 2 == 1) && (tmpScreenYCounter % 2 == 0) )
					{
						_OutputPort.MarkPixel ( tmpScreenXCounter, tmpScreenYCounter, this._RenderingEngine.RenderPixel ( tmpChaosXCounter, tmpChaosYCounter ) );
						tmpCurrentPixelCount++;
					}

					this._RenderProgressAmount = ((double)tmpCurrentPixelCount/(double)tmpTotalPixelCount);

					tmpChaosYCounter += tmpChaosXIncrement;
				}

				tmpChaosXCounter += tmpChaosYIncrement;
			}

			this.WriteToLog ( " --> Subquad 1,0 Done [" + tmpCurrentPixelCount.ToString()+"/"+tmpTotalPixelCount.ToString()+"]" );
			//Refresh the display
			Gdk.Threads.Enter();
			GLib.Idle.Add( tmpIdleMessageRefreshContainer );
			Gdk.Threads.Leave();

			//Now render the 0,1 fractal set
			tmpChaosXCounter = this._RenderingEngine.ChaosMinX;
			for (tmpScreenXCounter = 0; tmpScreenXCounter < _OutputPort.Width; tmpScreenXCounter++)
			{
				tmpChaosYCounter = this._RenderingEngine.ChaosMinY;
				for (tmpScreenYCounter = 0; tmpScreenYCounter < _OutputPort.Height; tmpScreenYCounter++)
				{
					if ( (tmpScreenXCounter % 2 == 0) && (tmpScreenYCounter % 2 == 1) )
					{
						_OutputPort.MarkPixel ( tmpScreenXCounter, tmpScreenYCounter, this._RenderingEngine.RenderPixel ( tmpChaosXCounter, tmpChaosYCounter ) );
						tmpCurrentPixelCount++;
					}

					//if ( Current_Pixel_Counter % (this.Output_Port.Width*4) == 1 )
					//{
					this._RenderProgressAmount = ((double)tmpCurrentPixelCount/(double)tmpTotalPixelCount);
					//}

					tmpChaosYCounter += tmpChaosXIncrement;
				}

				tmpChaosXCounter += tmpChaosYIncrement;
			}

			this.WriteToLog ( " --> Subquad 0,1 Done [" + tmpCurrentPixelCount.ToString()+"/"+tmpTotalPixelCount.ToString()+"]" );

			//Refresh the display
			Gdk.Threads.Enter();
			GLib.Idle.Add( tmpIdleMessageRefreshContainer );
			Gdk.Threads.Leave();


			this._RenderingEngine.Render_End();

			//Update the progress at 100%
			this._RenderProgressAmount = 1;

			tmpRenderTimer.Time_Stamp();
			this.WriteToLog ( "*** Image Computation Complete (" + tmpRenderTimer.TimeDifference.ToString() + "ms) ***" );
			this._CurrentlyRendering = false;
		}

		private void RenderFull()
		{
			this.WriteToLog ( "*** Silent Image Computation Beginning ***" );
			this.WriteToLog ( this._RenderingEngine.Name + " " + this._RenderingEngine.ParameterSerialization );
			TimeSpanMetric tmpComputationTimer = new TimeSpanMetric();

			int tmpScreenXCounter, tmpScreenYCounter;
			double tmpChaosXCounter, tmpChaosYCounter;
			double tmpChaosXIncrement, tmpChaosYIncrement;
			int tmpTotalPixelCount, tmpCurrentPixelCounter;

			//A thread-safe container for the display refresh event
			IdleHandler tmpIdleMessageRefreshContainer = new IdleHandler(this.RefreshDisplaySafe);

			this._RenderingEngine.RenderBegin();

			//Compute the Pixel to Chaos space ratio
			tmpChaosXIncrement = (double)((this._RenderingEngine.ChaosMaxX - this._RenderingEngine.ChaosMinX) / (double)_OutputPort.Width );
			//Chaos_Y_Incriment = (double)((this.Rendering_Engine.Chaos_Max_Y - this.Rendering_Engine.Chaos_Min_Y) / (double)Output_Port.Height );
			tmpChaosYIncrement = tmpChaosXIncrement;

			//Compute the pixel counter for the progress bar.
			tmpTotalPixelCount = _OutputPort.Width * _OutputPort.Height;
			tmpCurrentPixelCounter = 0;

			_OutputPort.LastXOrigin = this._RenderingEngine.ChaosMinX;
			_OutputPort.LastYOrigin = this._RenderingEngine.ChaosMinY;
			_OutputPort.LastXRatio = tmpChaosXIncrement;
			_OutputPort.LastYRatio = tmpChaosYIncrement;
			this._RenderingEngine.CurrentRenderRatio = tmpChaosXIncrement;

			tmpChaosXCounter = this._RenderingEngine.ChaosMinX;
			for (tmpScreenXCounter = 0; tmpScreenXCounter < _OutputPort.Width; tmpScreenXCounter++)
			{
				tmpChaosYCounter = this._RenderingEngine.ChaosMinY;
				for (tmpScreenYCounter = 0; tmpScreenYCounter < _OutputPort.Height; tmpScreenYCounter++)
				{
 					if ( this._QuadSampling  )
					{
						if ( (tmpScreenXCounter % 2 == 0) && (tmpScreenYCounter % 2 == 0) )
						{
							_OutputPort.MarkPixelQuad ( tmpScreenXCounter, tmpScreenYCounter, this._RenderingEngine.RenderPixel ( tmpChaosXCounter, tmpChaosYCounter ) );
						}
						tmpCurrentPixelCounter++;
					}
					else
					{
						_OutputPort.MarkPixel ( tmpScreenXCounter, tmpScreenYCounter, this._RenderingEngine.RenderPixel ( tmpChaosXCounter, tmpChaosYCounter ) );

						tmpCurrentPixelCounter++;
					}

					//if ( Current_Pixel_Counter % (this.Output_Port.Width*4) == 1 )
					//{
					this._RenderProgressAmount = ((double)tmpCurrentPixelCounter/(double)tmpTotalPixelCount);
					//}

					tmpChaosYCounter += tmpChaosXIncrement;
				}

				tmpChaosXCounter += tmpChaosYIncrement;
			}

			this._RenderingEngine.Render_End();

			//It's now safe to refresh the display and such.
			Gdk.Threads.Enter();
			GLib.Idle.Add( tmpIdleMessageRefreshContainer );
			Gdk.Threads.Leave();


			//Update the progress at 100%
			this._RenderProgressAmount = 1;

			tmpComputationTimer.Time_Stamp();
			this.WriteToLog ( "*** Image Computation Complete (" + tmpComputationTimer.TimeDifference.ToString() + "ms) ***" );
			this._CurrentlyRendering = false;
		}

		//Progress Bar Data Access
		public double Progress
		{
			get { return this._RenderProgressAmount; }
		}

		public bool Rendering
		{
			get { return this._CurrentlyRendering; }
		}

		public int RenderQuality
		{
			set
			{
				if ( value > 0 )
				{
					this.WriteToLog ( "Quality Set To: " + value.ToString() );

					this._RenderingEngine.CurrentRenderQuality = value;
					this._QuadSampling = false;
				}
				else
				{
					this.WriteToLog ( "Quality Set To: 0 (preview)" );

					//Zero or subzero means a special quality where the renderer is on 1 and we only do 1/4 pixels
					this._RenderingEngine.CurrentRenderQuality = 1;
					this._QuadSampling = true;
				}

			}
		}

		public bool RenderVisual
		{
			set
			{
				if (value)
					this.WriteToLog ( "Visually Hinted Render Mode Activated" );
				else
					this.WriteToLog ( "Silent Render Mode Activated" );

				this._VisualRender = value;
			}
		}

		public ChaosEngine Engine
		{
			set { this._RenderingEngine = value; }
		}

		#region Display Refresh Event
		public event EventHandler RefreshDisplay;

		public bool RefreshDisplaySafe()
		{
			EventArgs tmpArguments = new EventArgs();

			this.RefreshDisplay( this, tmpArguments );

			return false;
		}
		#endregion
	}
}
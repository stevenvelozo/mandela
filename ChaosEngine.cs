using MasterLogFile;

//Todo: Add last rendered timestamps?
namespace MasterChaosDisplay
{
	//This will be the base for Mandlebrot, Julia, Sierpenski, Buddhabrot, etc.
	class ChaosEngine : PassThroughLoggedClass
	{
		//The total number of rendered points
		private long _TotalPointsRendered;

		//This is TRUE when Rendering.  Stops a lot of things from working when set.
		private bool _Rendering;

		//This is a list of parameters for this engine.
		protected ChaosEngineParameterList _EngineParameters;

		//The default parameters using Param_PARAMETERNAME as the nomenclature
		private double _Param_XMin;
		private double _Param_YMin;
		private double _Param_XMax;
		private double _Param_YMax;
		private int _Param_ScreenColorCount;	//The color count for the current Render.

		//This stuff is used in the dynamically recompiling rendering logic
		private double _RenderingRatio;			//Screen to chaos space ratio
		private double _RenderingQuality;		//The "quality" of the render.  1 is lowest, or in general a 1 to 1 mapping of points

		public ChaosEngine()
		{
			//Make sure the rendering subsystem is explicitly off
			this._Rendering = false;

			//Reset the count of rendered points.
			this._TotalPointsRendered = 0;

			this._EngineParameters = new ChaosEngineParameterList();

			//There are a few parameters that ALWAYS are required in a paramlist
			this._EngineParameters.AddParameter ( "XMin", "X Minimum",
				"The left edge X Value in Chaos space, which is translated to the origin of the screen render device.",
				"Chaos Space X Minimum", "Float", "-400" );
			this._EngineParameters.AddParameter ( "XMax", "X Maximum",
				"The right edge X Value in Chaos space, which is translated to the right edge screen render device..",
				"Chaos Space X Maximum", "Float", "400" );
			this._EngineParameters.AddParameter ( "YMin", "Y Minimum",
				"The top edge Y Value in Chaos space, which is translated to the origin of the screen render device.",
				"Chaos Space Y Minimum", "Float", "-300" );
			this._EngineParameters.AddParameter ( "YMax", "Y Maximum",
				"The bottom edge Y Value in Chaos space, which is translated to the bottom edge screen render device.",
				"Chaos Space Y Maximum", "Float", "300" );
			this._EngineParameters.AddParameter ( "Screen_Color_Count", "Screen Color Count",
				"The number of colors to use in the onscreen image palette.",
				"Screen Color Count", "Integer", "255" );

			this.SyncInternalParameters();

			this.InitializeEngineParameters ();

			this._RenderingQuality = 1;
		}

		public string GetParameter ( string pParameterKey )
		{
			return this._EngineParameters.GetParameter( pParameterKey );
		}

		public bool SetParameter ( string pParameterKey, string pParameterValue )
		{
			return this._EngineParameters.SetParameter( pParameterKey, pParameterValue );
		}

		public string ParameterSerialization
		{
			get {return this._EngineParameters.Serialize_Parameters; }
		}

		protected void SetDefaultLimits ( double pXMin, double pXMax, double pYMin, double pYMax )
		{
			this._EngineParameters.SetParameter( "XMin", pXMin.ToString() );
			this._EngineParameters.SetParameter( "XMax", pXMax.ToString() );
			this._EngineParameters.SetParameter( "YMin", pYMin.ToString() );
			this._EngineParameters.SetParameter( "YMax", pYMax.ToString() );
		}

		//Add any engine-specific parameters
		public virtual void InitializeEngineParameters ()
		{
			//This will hold any engine parameters that are necessary
		}

		//This is the overridden rendering system initialization
		public virtual void InitializeRenderSubsystem ()
		{
			//This would initialize any variables that are used.
		}

		//The overridden render value (without changing this we have no engine)
		public virtual int EngineRenderValue ( double pChaosSpaceX, double pChaosSpaceY )
		{
			//Default to always returning infinity.
			return -1;
		}

#region Render Subsystem (Untouchable)
		//Begin the render
		public bool RenderBegin()
		{
			if ( !this._Rendering )
			{
				//Set the built-in parameters in the cache
				this.SyncInternalParameters();

				//Set all user parameters.  Use overloadable functions JUST LIKE IN THE MOOBIES
				this.InitializeRenderSubsystem();

				this._Rendering = true;

				return true;
			}
			else
			{
				return false;
			}
		}

		private void SyncInternalParameters()
		{
			//Set the default parameters from the list
			this._Param_XMin = double.Parse( this._EngineParameters.GetParameter( "XMin" ) );
			this._Param_YMin = double.Parse( this._EngineParameters.GetParameter( "XMax" ) );
			this._Param_XMax = double.Parse( this._EngineParameters.GetParameter( "YMin" ) );
			this._Param_YMax = double.Parse( this._EngineParameters.GetParameter( "YMax" ) );
			this._Param_ScreenColorCount = int.Parse( this._EngineParameters.GetParameter( "Screen_Color_Count" ) );
		}

		//Get a pixel rendering value.  -1 will be Infinite, otherwise it will ALWAYS have to fall inside the range of 0 to color_count - 1
		//TODO: We will have to test after we put the REAL code in here whether this function
		//      should be virtualized or it should wrap one for nicer outside calls.
		public int RenderPixel( double pChaosSpaceX, double pChaosSpaceY )
		{
			if (this._Rendering)
			{
				this._TotalPointsRendered++;
				return this.EngineRenderQualityValue (pChaosSpaceX, pChaosSpaceY);
			}
			else
			{
				return -2;
			}
		}

		private int EngineRenderQualityValue ( double pChaosSpaceX, double pChaosSpaceY )
		{
			if ( this.CurrentRenderQuality <= 1 )
			{
				return this.EngineRenderValue ( pChaosSpaceX, pChaosSpaceY );
			}
			else
			{
				//Do a subgrid-based averaging system.  We could also Weight them by distance from pixel but I think that will be freaky.
				int tmpRenderAverager = 0;
				int tmpSubpixelValue;
				bool tmpInfinitePoint = true;

				//This is the spacing gap that we walk between pixels
				double tmpCurrentRatioSubpixelGap = (this.CurrentRenderRatio / this.CurrentRenderQuality);

				//This is the distance from the "center" we go for the origin and boundaries of the subpixel grouping
				double tmpCurrentRatioSubpixelHalf = ((this.CurrentRenderQuality-1)/2)*tmpCurrentRatioSubpixelGap;

				for ( double tmpChaosSpaceX = pChaosSpaceX - tmpCurrentRatioSubpixelHalf;
					tmpChaosSpaceX <= pChaosSpaceX + tmpCurrentRatioSubpixelHalf;
					tmpChaosSpaceX = tmpChaosSpaceX + tmpCurrentRatioSubpixelGap )
				{
					for ( double tmpChaosSpaceY = pChaosSpaceY - tmpCurrentRatioSubpixelHalf;
						tmpChaosSpaceY <= pChaosSpaceY + tmpCurrentRatioSubpixelHalf;
						tmpChaosSpaceY = tmpChaosSpaceY + tmpCurrentRatioSubpixelGap )
					{
						tmpSubpixelValue = this.EngineRenderValue ( tmpChaosSpaceX, tmpChaosSpaceY );

						if ( tmpSubpixelValue >= 0 )
							tmpInfinitePoint = false;
						else
						    tmpSubpixelValue = this.ColorCount - 1;

						tmpRenderAverager += tmpSubpixelValue;
					}
				}

				if ( tmpInfinitePoint )
					//Return the infinity
					return -1;
				else
					return tmpRenderAverager / (int)(this.CurrentRenderQuality*this.CurrentRenderQuality);
			}
		}

		//End the render
		public bool Render_End()
		{
			this._Rendering = false;
			return true;
		}
#endregion

#region Data Access
		public double CurrentRenderQuality
		{
			set
			{
				this._RenderingQuality = value;

				//This will require only odd numbers for render values.
				if ( this.CurrentRenderQuality % 2 == 0 )
					this.CurrentRenderQuality -= 1;
			}
			get {return this._RenderingQuality; }
		}

		public double CurrentRenderRatio
		{
			set {this._RenderingRatio = value; }
			get {return this._RenderingRatio; }
		}

		public int ColorCount
		{
			get {return this._Param_ScreenColorCount; }
		}

		public double ChaosMinX
		{
			get {return this._Param_XMin; }
		}

		public double ChaosMaxX
		{
			get {return this._Param_YMin; }
		}

		public double ChaosMinY
		{
			get {return this._Param_XMax; }
		}

		public double ChaosMaxY
		{
			get {return this._Param_YMax; }
		}

		public double ChaosWidth
		{
			get {return this._Param_YMin - this._Param_XMin; }
		}

		public double ChaosHeight
		{
			get {return this._Param_YMax - this._Param_XMax; }
		}

		public long TotalPointsRendered
		{
			get {return this._TotalPointsRendered; }
		}

		public virtual string Name
		{
			get {return "None"; }
		}
#endregion
	}
}

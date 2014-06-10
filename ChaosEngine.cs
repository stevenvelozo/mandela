using Master_Log_File;

//Todo: Add last rendered timestamps?
namespace Master_Chaos_Display
{
	//This will be the base for Mandlebrot, Julia, Sierpenski, Buddhabrot, etc.
	class Chaos_Engine : Pass_Through_Logged_Class
	{
		//The total number of rendered points
		private long Total_Points_Rendered;

		//This is TRUE when Rendering.  Stops a lot of things from working when set.
		private bool Rendering;

		//This is a list of parameters for this engine.
		protected Chaos_Engine_Parameter_List Engine_Parameters;

		//The default parameters using Param_PARAMETERNAME as the nomenclature
		private double Param_XMin;
		private double Param_XMax;
		private double Param_YMin;
		private double Param_YMax;
		private int Param_Screen_Color_Count;	//The color count for the current Render.

		//This stuff is used in the dynamically recompiling rendering logic
		private double Rendering_Ratio;			//Screen to chaos space ratio
		private double Rendering_Quality;		//The "quality" of the render.  1 is lowest, or in general a 1 to 1 mapping of points

		public Chaos_Engine()
		{
			//Make sure the rendering subsystem is explicitly off
			this.Rendering = false;

			//Reset the count of rendered points.
			this.Total_Points_Rendered = 0;

			this.Engine_Parameters = new Chaos_Engine_Parameter_List();

			//There are a few parameters that ALWAYS are required in a paramlist
			this.Engine_Parameters.Add_Parameter ( "XMin", "X Minimum",
				"The left edge X Value in Chaos space, which is translated to the origin of the screen render device.",
				"Chaos Space X Minimum", "Float", "-400" );
			this.Engine_Parameters.Add_Parameter ( "XMax", "X Maximum",
				"The right edge X Value in Chaos space, which is translated to the right edge screen render device..",
				"Chaos Space X Maximum", "Float", "400" );
			this.Engine_Parameters.Add_Parameter ( "YMin", "Y Minimum",
				"The top edge Y Value in Chaos space, which is translated to the origin of the screen render device.",
				"Chaos Space Y Minimum", "Float", "-300" );
			this.Engine_Parameters.Add_Parameter ( "YMax", "Y Maximum",
				"The bottom edge Y Value in Chaos space, which is translated to the bottom edge screen render device.",
				"Chaos Space Y Maximum", "Float", "300" );
			this.Engine_Parameters.Add_Parameter ( "Screen_Color_Count", "Screen Color Count",
				"The number of colors to use in the onscreen image palette.",
				"Screen Color Count", "Integer", "255" );

			this.Sync_Internal_Parameters();

			this.Initialize_Engine_Parameters ();

			this.Rendering_Quality = 1;
		}

		public string Get_Parameter ( string Parameter_Name )
		{
			return this.Engine_Parameters.Get_Parameter( Parameter_Name );
		}

		public bool Set_Parameter ( string Parameter_Name, string Parameter_Value )
		{
			return this.Engine_Parameters.Set_Parameter( Parameter_Name, Parameter_Value );
		}

		public string Parameter_Serialization
		{
			get {return this.Engine_Parameters.Serialize_Parameters; }
		}

		protected void Set_Default_Limits ( double X_Min, double X_Max, double Y_Min, double Y_Max )
		{
			this.Engine_Parameters.Set_Parameter( "XMin", X_Min.ToString() );
			this.Engine_Parameters.Set_Parameter( "XMax", X_Max.ToString() );
			this.Engine_Parameters.Set_Parameter( "YMin", Y_Min.ToString() );
			this.Engine_Parameters.Set_Parameter( "YMax", Y_Max.ToString() );
		}

		//Add any engine-specific parameters
		public virtual void Initialize_Engine_Parameters ()
		{
			//This will hold any engine parameters that are necessary
		}

		//This is the overridden rendering system initialization
		public virtual void Initialize_Render_Subsystem ()
		{
			//This would initialize any variables that are used.
		}

		//The overridden render value (without changing this we have no engine)
		public virtual int Engine_Render_Value ( double Chaos_Space_X, double Chaos_Space_Y )
		{
			//Default to always returning infinity.
			return -1;
		}

#region Render Subsystem (Untouchable)
		//Begin the render
		public bool Render_Begin()
		{
			if ( !this.Rendering )
			{
				//Set the built-in parameters in the cache
				this.Sync_Internal_Parameters();

				//Set all user parameters.  Use overloadable functions JUST LIKE IN THE MOOBIES
				this.Initialize_Render_Subsystem();

				this.Rendering = true;

				return true;
			}
			else
			{
				return false;
			}
		}

		private void Sync_Internal_Parameters()
		{
			//Set the default parameters from the list
			this.Param_XMin = double.Parse( this.Engine_Parameters.Get_Parameter( "XMin" ) );
			this.Param_XMax = double.Parse( this.Engine_Parameters.Get_Parameter( "XMax" ) );
			this.Param_YMin = double.Parse( this.Engine_Parameters.Get_Parameter( "YMin" ) );
			this.Param_YMax = double.Parse( this.Engine_Parameters.Get_Parameter( "YMax" ) );
			this.Param_Screen_Color_Count = int.Parse( this.Engine_Parameters.Get_Parameter( "Screen_Color_Count" ) );
		}

		//Get a pixel rendering value.  -1 will be Infinite, otherwise it will ALWAYS have to fall inside the range of 0 to color_count - 1
		//TODO: We will have to test after we put the REAL code in here whether this function
		//      should be virtualized or it should wrap one for nicer outside calls.
		public int Render_Pixel( double Chaos_Space_X, double Chaos_Space_Y )
		{
			if ( this.Rendering )
			{
				this.Total_Points_Rendered++;
				return this.Engine_Render_Quality_Value ( Chaos_Space_X, Chaos_Space_Y );
			}
			else
				return -2;
		}

		private int Engine_Render_Quality_Value ( double Chaos_Space_X, double Chaos_Space_Y )
		{
			if ( this.Current_Render_Quality <= 1 )
			{
				return this.Engine_Render_Value ( Chaos_Space_X, Chaos_Space_Y );
			}
			else
			{
				//Do a subgrid-based averaging system.  We could also Weight them by distance from pixel but I think that will be freaky.
				int Render_Averager = 0;
				int Subpixel_Value;
				bool Infinite_Point = true;

				//This is the spacing gap that we walk between pixels
				double Current_Ratio_Subpixel_Gap = (this.Current_Render_Ratio / this.Current_Render_Quality);

				//This is the distance from the "center" we go for the origin and boundaries of the subpixel grouping
				double Current_Ratio_Subpixel_Half = ((this.Current_Render_Quality-1)/2)*Current_Ratio_Subpixel_Gap;

				for ( double Working_Chaos_X = Chaos_Space_X - Current_Ratio_Subpixel_Half;
					Working_Chaos_X <= Chaos_Space_X + Current_Ratio_Subpixel_Half;
					Working_Chaos_X = Working_Chaos_X + Current_Ratio_Subpixel_Gap )
				{
					for ( double Working_Chaos_Y = Chaos_Space_Y - Current_Ratio_Subpixel_Half;
						Working_Chaos_Y <= Chaos_Space_Y + Current_Ratio_Subpixel_Half;
						Working_Chaos_Y = Working_Chaos_Y + Current_Ratio_Subpixel_Gap )
					{
						Subpixel_Value = this.Engine_Render_Value ( Working_Chaos_X, Working_Chaos_Y );

						if ( Subpixel_Value >= 0 )
							Infinite_Point = false;
						else
						    Subpixel_Value = this.Color_Count - 1;

						Render_Averager += Subpixel_Value;
					}
				}

				if ( Infinite_Point )
				{
					//Return the infinity
					return -1;
				}
				else
				{
					return Render_Averager / (int)(this.Current_Render_Quality*this.Current_Render_Quality);
				}
			}
		}

		//End the render
		public bool Render_End()
		{
			this.Rendering = false;
			return true;
		}
#endregion

#region Data Access
		public double Current_Render_Quality
		{
			set
			{
				this.Rendering_Quality = value;

				//This will require only odd numbers for render values.
				if ( this.Current_Render_Quality % 2 == 0 )
				{
					this.Current_Render_Quality -= 1;
				}
			}
			get {return this.Rendering_Quality; }
		}

		public double Current_Render_Ratio
		{
			set {this.Rendering_Ratio = value; }
			get {return this.Rendering_Ratio; }
		}

		public int Color_Count
		{
			get {return this.Param_Screen_Color_Count; }
		}

		public double Chaos_Min_X
		{
			get {return this.Param_XMin; }
		}

		public double Chaos_Max_X
		{
			get {return this.Param_XMax; }
		}

		public double Chaos_Min_Y
		{
			get {return this.Param_YMin; }
		}

		public double Chaos_Max_Y
		{
			get {return this.Param_YMax; }
		}

		public double Chaos_Width
		{
			get {return this.Param_XMax - this.Param_XMin; }
		}

		public double Chaos_Height
		{
			get {return this.Param_YMax - this.Param_YMin; }
		}

		public long Total_Rendered_Points
		{
			get {return this.Total_Points_Rendered; }
		}

		public virtual string Name
		{
			get {return "None"; }
		}
#endregion
	}
}

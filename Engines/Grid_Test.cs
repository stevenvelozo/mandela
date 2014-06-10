// A test engine that will print a gradiented grid.
// Our engines could be turned into DLL's at some point
namespace Master_Chaos_Display
{
	class Grid_Test_Engine : Chaos_Engine
	{
		public override string Name
		{
			get
			{
				return "Grid Debug";
			}
		}

		public override int Engine_Render_Value ( double Chaos_Space_X, double Chaos_Space_Y )
		{
			//Lots of math on iteration
			//	Chaos_Space_X - this.Chaos_Min_X = position
			double Relative_X_Position = Chaos_Space_X - this.Chaos_Min_X;
			double Relative_Y_Position = Chaos_Space_Y - this.Chaos_Min_Y;


			double X_Percentage = Relative_X_Position / this.Chaos_Width;
			double Y_Percentage = Relative_Y_Position / this.Chaos_Height;

			double Total_Percentage = (X_Percentage + Y_Percentage) / 2;

			/*
			if ( this.Total_Rendered_Points < 50 )
			{
				this.Write_To_Log ( "#PIXEL " + this.Total_Rendered_Points.ToString() );
				this.Write_To_Log ( " E: [XMin "+this.Chaos_Min_X.ToString()+"] [YMin "+this.Chaos_Min_Y.ToString()+"] [W "+this.Chaos_Width.ToString()+"] [H "+this.Chaos_Height.ToString()+"]");
				this.Write_To_Log ( " C: [X "+Chaos_Space_X.ToString()+"] [Y "+Chaos_Space_Y.ToString()+"]" );
				this.Write_To_Log ( " X: [Rel "+Relative_X_Position.ToString()+"] [Min "+Chaos_Min_X.ToString()+"] [Per "+X_Percentage.ToString()+"]" );
				this.Write_To_Log ( " Y: [Rel "+Relative_Y_Position.ToString()+"] [Min "+Chaos_Min_Y.ToString()+"] [Per "+Y_Percentage.ToString()+"]" );
				this.Write_To_Log ( " P: " + Total_Percentage.ToString() +"%");
			}
			*/


			if ( ((int)(X_Percentage*1000) % 5 == 0) || ((int)(Y_Percentage*1000) % 5 == 0) )
			{
				return this.Color_Count - (int)(Total_Percentage * this.Color_Count);
			}
			else
			{
				return (int)(Total_Percentage * this.Color_Count);
			}
		}
	}
}
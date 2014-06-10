using Gtk;
using Master_Super_Image;
using Master_Log_File;

namespace Master_Chaos_Display
{
	//A "Rendering Machine" on which a "Chaos Engine" renders
	//Todo: Move to the Origin X, Y and then Width
	class Chaos_Rendering_Machine : Pass_Through_Logged_Class
	{
		//The (definable on creation) screen name.  This is useful for logging prefixes and such.
		private string Render_Machine_Name;

		//The Width and Height of the viewport
		private int Pixel_Width;
		private int Pixel_Height;

		//The pixel ratios of the last rendered image.
		private double Last_Rendered_X_Pixel_Ratio;
		private double Last_Rendered_Y_Pixel_Ratio;

		//The origins of the last rendered image.  I.E. the display's absolute zero values.
		private double Last_Rendered_X_Origin;
		private double Last_Rendered_Y_Origin;

		//The last clicked cartesian centerpoint.
		private double Last_Clicked_Centerpoint_X;
		private double Last_Clicked_Centerpoint_Y;

		//The data used for the zoom on this viewport.
		//Todo: Hide these behind properties
		public int Zoom_Centerpoint_X;
		public int Zoom_Centerpoint_Y;

		public int Zoom_Top_Left_X;
		public int Zoom_Top_Left_Y;

		public int Zoom_Width;
		public int Zoom_Height;

		//The actual Render Machine output image
		private Super_Image Render_Machine_Output;

		//The color information
		private Super_Color Infinite_Color;
		private Super_Image_Pallete Render_Machine_Pallete;

		//The simplest Constructor
		public Chaos_Rendering_Machine ( string Machine_Name )
		{
			//Set the machine name
			this.Render_Machine_Name = Machine_Name;

			//Set the machines viewport size
			this.Pixel_Width = 400;
			this.Pixel_Height = 300;

			//Define a color to represent "infinite"
			this.Infinite_Color = new Super_Color( 0, 15, 0 );

			//Create a simple 100 color pallette
			Super_Color Start_Color = new Super_Color( 0, 0, 0 );
			Super_Color End_Color = new Super_Color( 255, 255, 255 );
			this.Render_Machine_Pallete = new Super_Image_Pallete( Start_Color, End_Color, 100 );

			//Create a simple image; later this could end up being a "subsuper" image?  Only buffers no gtk
			this.Render_Machine_Output = new Super_Image ( this.Pixel_Width, this.Pixel_Height );
		}

		public Chaos_Rendering_Machine ( string Machine_Name, int Display_Width, int Display_Height, Super_Color Infinite_Color, Super_Color Start_Color, Super_Color End_Color, int Color_Count )
		{
			//Set the machine name
			this.Render_Machine_Name = Machine_Name;

			//Set the machines viewport size
			this.Pixel_Width = Display_Width;
			this.Pixel_Height = Display_Height;

			//Define a color to represent "infinite"
			this.Infinite_Color = Infinite_Color;

			//Create a simple Gradient Pallete
			this.Render_Machine_Pallete = new Super_Image_Pallete( Start_Color, End_Color, Color_Count );

			//Create a simple image; later this could end up being a "subsuper" image?  Only buffers no gtk
			this.Render_Machine_Output = new Super_Image ( this.Pixel_Width, this.Pixel_Height, 2 );
		}

		public void Hide_Selection ()
		{
			this.Render_Machine_Output.Hide_Selection();
		}

		public void Selection_Set ()
		{
			//this.Write_To_Log ( "Image Set Selection X["+this.Zoom_Top_Left_X.ToString()+"] Y["+this.Zoom_Top_Left_Y.ToString()+"] W["+this.Zoom_Width.ToString()+"] H["+this.Zoom_Height.ToString()+"]");
			this.Render_Machine_Output.Set_Selection ( this.Zoom_Top_Left_X, this.Zoom_Top_Left_Y, this.Zoom_Width, this.Zoom_Height );
		}

#region Data Access Functions
		public string Name
		{
			get {return this.Render_Machine_Name; }
		}

		public int Width
		{
			get {return this.Pixel_Width; }
			set
			{
				try
				{
					if ( value > 0 )
						this.Pixel_Width = value;
				}
				catch
				{
					this.Write_To_Log ( "Error setting [Pixel_Width]." );
				}
			}
		}

		public int Height
		{
			get {return this.Pixel_Height; }
			set
			{
				try
				{
					if ( value > 0 )
						this.Pixel_Height = value;
				}
				catch
				{
					this.Write_To_Log ( "Error setting [Pixel_Height]." );
				}
			}
		}

		public double Last_X_Ratio
		{
			get {return this.Last_Rendered_X_Pixel_Ratio; }
			set {this.Last_Rendered_X_Pixel_Ratio = value; }
		}

		public double Last_Y_Ratio
		{
			get {return this.Last_Rendered_Y_Pixel_Ratio; }
			set {this.Last_Rendered_Y_Pixel_Ratio = value; }
		}

		public double Last_X_Origin
		{
			get {return this.Last_Rendered_X_Origin; }
			set {this.Last_Rendered_X_Origin = value; }
		}

		public double Last_Y_Origin
		{
			get {return this.Last_Rendered_Y_Origin; }
			set {this.Last_Rendered_Y_Origin = value; }
		}

		public double Last_Clicked_X_Centerpoint
		{
			get {return this.Last_Clicked_Centerpoint_X; }
			set {this.Last_Clicked_Centerpoint_X = value; }
		}

		public double Last_Clicked_Y_Centerpoint
		{
			get {return this.Last_Clicked_Centerpoint_Y; }
			set {this.Last_Clicked_Centerpoint_Y = value; }
		}

		public void Mark_Pixel ( int Pixel_X, int Pixel_Y, int Color_Index )
		{
			if ( Color_Index == -1 )
				this.Render_Machine_Output.Mark_Pixel ( Pixel_X, Pixel_Y, this.Infinite_Color );
			else
				this.Render_Machine_Output.Mark_Pixel ( Pixel_X, Pixel_Y, this.Render_Machine_Pallete.Get_Color ( Color_Index ) );
		}

		public void Mark_Pixel_Quad ( int Pixel_X, int Pixel_Y, int Color_Index )
		{
			if ( Color_Index == -1 )
				this.Render_Machine_Output.Mark_Pixel_Quad ( Pixel_X, Pixel_Y, this.Infinite_Color );
			else
				this.Render_Machine_Output.Mark_Pixel_Quad ( Pixel_X, Pixel_Y, this.Render_Machine_Pallete.Get_Color ( Color_Index ) );
		}

		public void Refresh ()
		{
			this.Render_Machine_Output.Blit_Buffer();
			this.Render_Machine_Output.Refresh();
		}

		public Super_Image_Pallete Pallete
		{
			get {return this.Render_Machine_Pallete; }
			set {this.Render_Machine_Pallete = value; }
		}

		public Super_Color Infinite
		{
			get {return this.Infinite_Color; }
		}

		public Gtk.Image Image
		{
			get {return this.Render_Machine_Output.Image; }
		}
#endregion
	}
}
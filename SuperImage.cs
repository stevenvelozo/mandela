// created on 8/16/2005 at 8:04 AM
// My wrapper for the GTK Image to allow me per-pixel editing without using unsafe code
// This also allows me to move to different image output systems without having to overly
// mangle my code by using discreet pixel manipulation functions.
//
// Possibly try to get this working on windows using another toolkit like MSFT
// through an image abstraction layer.
using System;
using Gtk;
using Gdk;
using Master_Log_File;
using Master_Data_Structure_Base;
using Master_Time_Span;

namespace Master_Super_Image
{
	public class Super_Image
	{
		private int Width;
		private int Height;


		private int Image_Buffer_Count;
		private int Current_Image_Buffer;
		private bool Locked_Display_Image;
		private Gdk.Pixbuf Image_Display_Buffer;

		//This adds a little overhead memory but is way faster and more efficient in the longrun than
		//instantiating a buffer every time we want to do a draw
		private Gdk.Pixbuf Scratch_Display_Buffer;
		private Gdk.Pixmap Scratch_Render_Buffer;
		private bool Scratch_Buffer_Allocated;
		private Gdk.GC Scratch_Pen_Context;
		private Gdk.Colormap Scratch_Color_Map;

		//This stuff pertains to the "selection" stuff
		private Gdk.Color Selection_Color;
		private bool Selection_Last_Set;
		private int Selection_Last_X;
		private int Selection_Last_Y;
		private int Selection_Last_Width;
		private int Selection_Last_Height;


		//If this guy is true, we can select something that isn't currently displayed.
//		private bool Allow_Out_Of_Bounds_Selections;

		private Super_Image_Buffer[] Image_Buffers;

		private Gtk.Image Image_Widget;

		public Super_Image ( int Image_Width, int Image_Height )
		{
			this.Width = Image_Width;
			this.Height = Image_Height;

			this.Image_Buffer_Count = 1;
			this.Current_Image_Buffer = 0;

			this.Image_Buffers = new Super_Image_Buffer[this.Image_Buffer_Count];

			for (int tmpCounter = 0; tmpCounter < this.Image_Buffer_Count; tmpCounter++ )
			{
				this.Image_Buffers[tmpCounter] = new Super_Image_Buffer( this.Width, this.Height );
			}

			//Constructor: Pixbuf (Colorspace colorspace, bool has_alpha, int bits_per_sample, int width, int height)
			this.Image_Display_Buffer = new Gdk.Pixbuf ( Gdk.Colorspace.Rgb, false, 8, this.Width, this.Height );
			this.Image_Widget = new Gtk.Image( this.Image_Display_Buffer );
		}

		public Super_Image ( int Image_Width, int Image_Height, int Buffer_Count )
		{
			this.Width = Image_Width;
			this.Height = Image_Height;

			this.Image_Buffer_Count = Buffer_Count;
			this.Current_Image_Buffer = 0;

			this.Image_Buffers = new Super_Image_Buffer[this.Image_Buffer_Count];

			for (int tmpCounter = 0; tmpCounter < this.Image_Buffer_Count; tmpCounter++ )
			{
				this.Image_Buffers[tmpCounter] = new Super_Image_Buffer( this.Width, this.Height );
			}

			//Constructor: Pixbuf (Colorspace colorspace, bool has_alpha, int bits_per_sample, int width, int height)
			this.Image_Display_Buffer = new Gdk.Pixbuf ( Gdk.Colorspace.Rgb, false, 8, this.Width, this.Height );
			this.Image_Widget = new Gtk.Image( this.Image_Display_Buffer );
		}

		public Gtk.Image Image
		{
			get
			{
				return this.Image_Widget;
			}
		}

		public bool Main_Image_Lock
		{
			get
			{
				return this.Locked_Display_Image;
			}
			set
			{
				this.Locked_Display_Image = value;
			}
		}

		public void Blit_Buffer ()
		{
			this.Blit_Buffer( this.Current_Image_Buffer );
		}

		public void Blit_Buffer ( int Buffer_Number )
		{
			this.Blit_Buffer( this.Current_Image_Buffer, 0, 0, this.Width, this.Height );
		}

		public void Blit_Buffer ( int Buffer_Number, int X, int Y, int Width, int Height )
		{
			//Composite (Pixbuf dest, int dest_x, int dest_y, int dest_width, int dest_height, double offset_x, double offset_y, double scale_x, double scale_y, InterpType interp_type, int overall_alpha)
			//			Gdk.InterpType.Nearest, Tiles, Bilinear, Hyper (worst to best)
			//this.Image_Buffers[Buffer_Number].Buffer.Composite( this.Image_Display_Buffer, 0, 0, this.Width, this.Height, 0.0, 0.0, 1.0, 1.0, Gdk.InterpType.Nearest, 255 );

			//public void CopyArea (int src_x, int src_y, int width, int height, Pixbuf dest_pixbuf, int dest_x, int dest_y)
			if ( !this.Locked_Display_Image )
				this.Image_Buffers[Buffer_Number].Buffer.CopyArea( X, Y, Width, Height, this.Image_Display_Buffer, X, Y);
		}

		public void Refresh ()
		{
			this.Image_Widget.QueueDraw();
		}

		public void Refresh ( int X, int Y, int Width, int Height )
		{
			this.Image_Widget.QueueDrawArea( X, Y, Width, Height );
		}

		public void Mark_Pixel_Quad ( int X, int Y, Super_Color Color )
		{
			this.Image_Buffers[this.Current_Image_Buffer].Mark_Pixel ( X, Y, Color.Red, Color.Green, Color.Blue );
			if ( X < Width-1 )
				this.Image_Buffers[this.Current_Image_Buffer].Mark_Pixel ( X+1, Y, Color.Red, Color.Green, Color.Blue );
			if ( Y < Height-1 )
				this.Image_Buffers[this.Current_Image_Buffer].Mark_Pixel ( X, Y+1, Color.Red, Color.Green, Color.Blue );

			if ( X < Width-1 && Y < Height-1 )
				this.Image_Buffers[this.Current_Image_Buffer].Mark_Pixel ( X+1, Y+1, Color.Red, Color.Green, Color.Blue );
		}

		public void Mark_Pixel ( int X, int Y, Super_Color Color )
		{
			this.Image_Buffers[this.Current_Image_Buffer].Mark_Pixel ( X, Y, Color.Red, Color.Green, Color.Blue );
		}

		public void Mark_Pixel ( int X, int Y, byte Red, byte Green, byte Blue )
		{
			this.Image_Buffers[this.Current_Image_Buffer].Mark_Pixel ( X, Y, Red, Green, Blue );
		}

		private void Check_Scratch_Buffers ()
		{
			if ( !this.Scratch_Buffer_Allocated )
			{
				//Allocate a pixmap to be used for draw operations
				this.Scratch_Render_Buffer = new Gdk.Pixmap ( null, this.Width, this.Height, Gdk.Visual.BestDepth );
				this.Scratch_Display_Buffer = new Gdk.Pixbuf ( Gdk.Colorspace.Rgb, false, 8, this.Width, this.Height );
				this.Scratch_Buffer_Allocated = true;

				this.Scratch_Pen_Context = new Gdk.GC ( this.Scratch_Render_Buffer );

				//Todo: This should be a property
				this.Selection_Color = new Gdk.Color ( 0xff, 0, 0 );

				this.Scratch_Color_Map = Gdk.Colormap.System;

				this.Scratch_Color_Map.AllocColor (ref this.Selection_Color, true, true);

				this.Scratch_Pen_Context.Foreground = this.Selection_Color;
			}
		}

		public void Set_Selection ( int X, int Y, int Width, int Height )
		{
			//if ( !this.Allow_Out_Of_Bounds_Selections || ((X > 0) && (Y > 0) && ((Y+Height) < this.Height) && ((X+Width) < this.Width) ) )
			if ((X > 0) && (Y > 0) && ((Y+Height) < this.Height) && ((X+Width) < this.Width))
			{
				this.Check_Scratch_Buffers();

				this.Clear_Last_Selection();
				int Working_X = Math.Max(X, 0);
				int Working_Y = Math.Max(Y, 0);
				int Working_Width, Working_Height;

				if ( Working_X + Width > this.Width )
				{
					Working_Width = this.Width - Working_X;
				}
				else
				{
					Working_Width = Width;
				}

				if ( Working_Y + Height > this.Height )
				{
					Working_Height = this.Height - Working_Y;
				}
				else
				{
					Working_Height = Height;
				}

				int Invalid_Top_X = Math.Min(this.Selection_Last_X, X);
				int Invalid_Top_Y = Math.Min( this.Selection_Last_Y, Y);
				int Invalid_Bottom_X = Math.Max( this.Selection_Last_X+this.Selection_Last_Width, X+Width);
				int Invalid_Bottom_Y = Math.Max( this.Selection_Last_Y+this.Selection_Last_Height, Y+Width);

				this.Selection_Last_X = Working_X;
				this.Selection_Last_Y = Working_Y;
				this.Selection_Last_Width = Working_Width;
				this.Selection_Last_Height = Working_Height;
				this.Selection_Last_Set = true;

				// RenderToDrawable (Drawable drawable, GC gc, int src_x, int src_y, int dest_x, int dest_y, int width, int height, RgbDither dither, int x_dither, int y_dither)
				this.Image_Buffers[this.Current_Image_Buffer].Buffer.RenderToDrawable( Scratch_Render_Buffer, this.Scratch_Pen_Context, Working_X, Working_Y, Working_X, Working_Y, Working_Width, Working_Height, Gdk.RgbDither.None, 0, 0 );

				this.Scratch_Render_Buffer.DrawRectangle (this.Scratch_Pen_Context, false, X, Y, Width - 1, Height - 1);

				//FromDrawable (Drawable src, Colormap cmap, int src_x, int src_y, int dest_x, int dest_y, int width, int height)
				//GetFromDrawable (Drawable drawable, Colormap cmap, int src_x, int src_y, int dest_x, int dest_y, int width, int height)
				this.Scratch_Display_Buffer.GetFromDrawable ( this.Scratch_Render_Buffer, this.Scratch_Color_Map, Working_X, Working_Y, Working_X, Working_Y, Working_Width, Working_Height );
				this.Scratch_Display_Buffer.CopyArea(  Working_X, Working_Y, Working_Width, Working_Height, this.Image_Display_Buffer, Working_X, Working_Y );

				this.Refresh( Invalid_Top_X, Invalid_Top_Y, Invalid_Bottom_X-Invalid_Top_X, Invalid_Bottom_Y-Invalid_Top_Y );
			}
		}

		private void Clear_Last_Selection ()
		{
			if ( this.Selection_Last_Set )
			{
				this.Blit_Buffer( this.Current_Image_Buffer, this.Selection_Last_X, this.Selection_Last_Y, this.Selection_Last_Width, this.Selection_Last_Height );
				//this.Blit_Buffer();
			}
		}

		public void Hide_Selection()
		{
			this.Clear_Last_Selection();

			this.Refresh ( this.Selection_Last_X, this.Selection_Last_Y, this.Selection_Last_Width, this.Selection_Last_Height );
		}
	}

	//Would it not be neat if this had a masking layer as well?  Also support for an Alpha Channel maybe
	public class Super_Image_Buffer
	{
		//Width and Height
		private int Width;
		private int Height;
		private int Row_Size;

		//Each buffer has a Rendering byte array and a Pixbuf buffer
		private byte[] Image_Buffer_Renderer;
		private Gdk.Pixbuf Image_Buffer;

		public Super_Image_Buffer ( int Image_Width, int Image_Height )
		{
			this.Width = Image_Width;
			this.Height = Image_Height;

			//Row_Size is 3*Width (R, G, B)
			//Eventually we could do Images with more than 24 bits, although we'd have to downsample the colors to display them.
			this.Row_Size = Image_Width * 3;

			//The byte buffer needs to be Row_Size * Height bytes long
			this.Image_Buffer_Renderer = new byte[this.Row_Size*this.Height];

			//And now to create the buffer.
			//Constructor:  Pixbuf (byte[] data, Colorspace colorspace, bool has_alpha, int bits_per_sample, int width, int height, int rowstride, PixbufDestroyNotify destroy_fn)
			this.Image_Buffer = new Gdk.Pixbuf( this.Image_Buffer_Renderer, Gdk.Colorspace.Rgb, false, 8, this.Width, this.Height, this.Row_Size, null);
		}

		public Gdk.Pixbuf Buffer
		{
			get
			{
				return this.Image_Buffer;
			}
		}

		//Others: MarkRow, MarkColumn, ModifyRow (using a modifier function type! yeah), ModifyPixel, MarkBlock (with setable size?)
		public void Mark_Pixel ( int X, int Y, Super_Color Color )
		{
			this.Mark_Pixel ( X, Y, Color.Red, Color.Green, Color.Blue );
		}

		public void Mark_Pixel ( int X, int Y, byte Red, byte Green, byte Blue )
		{
			//The simple marking strategy
			//This may at some point be changed to having a "last written pixels" buffer deal
			//No validation yet for X and Y
			//		Valid values: 0 .. Size-1
			this.Image_Buffer_Renderer[(Y*this.Row_Size)+(X*3)] = Red;
			this.Image_Buffer_Renderer[(Y*this.Row_Size)+(X*3)+1] = Green;
			this.Image_Buffer_Renderer[(Y*this.Row_Size)+(X*3)+2] = Blue;
		}
	}
}
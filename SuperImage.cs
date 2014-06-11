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
using MasterLogFile;
using MasterDataStructureBase;
using MasterTimeSpan;

namespace MasterSuperImage
{
	public class SuperImage
	{
		private int _Width;
		private int _Height;


		private int _ImageBufferCount;
		private int _CurrentImageBuffer;
		private bool _LockedDisplayImage;
		private Gdk.Pixbuf _ImageDisplayBuffer;

		//This adds a little overhead memory but is way faster and more efficient in the longrun than
		//instantiating a buffer every time we want to do a draw
		private Gdk.Pixbuf _ScratchDisplayBuffer;
		private Gdk.Pixmap _ScratchRenderBuffer;
		private bool _ScratchBufferAllocated;
		private Gdk.GC _ScratchPenContext;
		private Gdk.Colormap _ScratchColorMap;

		//This stuff pertains to the "selection" stuff
		private Gdk.Color _SelectionColor;
		private bool _SelectionLastSet;
		private int _SelectionLastX;
		private int _SelectionLastY;
		private int _SelectionLastWidth;
		private int _SelectionLastHeight;


		//If this guy is true, we can select something that isn't currently displayed.
//		private bool Allow_Out_Of_Bounds_Selections;

		private SuperImageBuffer[] _ImageBuffers;

		private Gtk.Image _ImageWidget;

		public SuperImage ( int pImageWidth, int Image_Height )
		{
			this._Width = pImageWidth;
			this._Height = Image_Height;

			this._ImageBufferCount = 1;
			this._CurrentImageBuffer = 0;

			this._ImageBuffers = new SuperImageBuffer[this._ImageBufferCount];

			for (int tmpCounter = 0; tmpCounter < this._ImageBufferCount; tmpCounter++ )
			{
				this._ImageBuffers[tmpCounter] = new SuperImageBuffer( this._Width, this._Height );
			}

			//Constructor: Pixbuf (Colorspace colorspace, bool has_alpha, int bits_per_sample, int width, int height)
			this._ImageDisplayBuffer = new Gdk.Pixbuf ( Gdk.Colorspace.Rgb, false, 8, this._Width, this._Height );
			this._ImageWidget = new Gtk.Image( this._ImageDisplayBuffer );
		}

		public SuperImage ( int pImageWidth, int pImageHeight, int pBufferCount )
		{
			this._Width = pImageWidth;
			this._Height = pImageHeight;

			this._ImageBufferCount = pBufferCount;
			this._CurrentImageBuffer = 0;

			this._ImageBuffers = new SuperImageBuffer[this._ImageBufferCount];

			for (int tmpCounter = 0; tmpCounter < this._ImageBufferCount; tmpCounter++ )
			{
				this._ImageBuffers[tmpCounter] = new SuperImageBuffer( this._Width, this._Height );
			}

			//Constructor: Pixbuf (Colorspace colorspace, bool has_alpha, int bits_per_sample, int width, int height)
			this._ImageDisplayBuffer = new Gdk.Pixbuf ( Gdk.Colorspace.Rgb, false, 8, this._Width, this._Height );
			this._ImageWidget = new Gtk.Image( this._ImageDisplayBuffer );
		}

		public Gtk.Image Image
		{
			get { return this._ImageWidget; }
		}

		public bool MainImageLock
		{
			get { return this._LockedDisplayImage; }
			set { this._LockedDisplayImage = value; }
		}

		public void BlitBuffer ()
		{
			this.BlitBuffer( this._CurrentImageBuffer );
		}

		public void BlitBuffer ( int pBufferNumber )
		{
			this.BlitBuffer( this._CurrentImageBuffer, 0, 0, this._Width, this._Height );
		}

		public void BlitBuffer ( int pBufferNumber, int pX, int pY, int pWidth, int pHeight )
		{
			//Composite (Pixbuf dest, int dest_x, int dest_y, int dest_width, int dest_height, double offset_x, double offset_y, double scale_x, double scale_y, InterpType interp_type, int overall_alpha)
			//			Gdk.InterpType.Nearest, Tiles, Bilinear, Hyper (worst to best)
			//this.Image_Buffers[Buffer_Number].Buffer.Composite( this.Image_Display_Buffer, 0, 0, this._Width, this.Height, 0.0, 0.0, 1.0, 1.0, Gdk.InterpType.Nearest, 255 );

			//public void CopyArea (int src_x, int src_y, int width, int height, Pixbuf dest_pixbuf, int dest_x, int dest_y)
			if ( !this._LockedDisplayImage )
				this._ImageBuffers[pBufferNumber].Buffer.CopyArea( pX, pY, pWidth, pHeight, this._ImageDisplayBuffer, pX, pY);
		}

		public void Refresh ()
		{
			this._ImageWidget.QueueDraw();
		}

		public void Refresh ( int pX, int pY, int pWidth, int pHeight )
		{
			this._ImageWidget.QueueDrawArea( pX, pY, pWidth, pHeight );
		}

		public void MarkPixelQuad ( int pX, int pY, SuperColor pColor )
		{
			this._ImageBuffers[this._CurrentImageBuffer].MarkPixel ( pX, pY, pColor.Red, pColor.Green, pColor.Blue );
			if ( pX < _Width-1 )
				this._ImageBuffers[this._CurrentImageBuffer].MarkPixel ( pX+1, pY, pColor.Red, pColor.Green, pColor.Blue );
			if ( pY < _Height-1 )
				this._ImageBuffers[this._CurrentImageBuffer].MarkPixel ( pX, pY+1, pColor.Red, pColor.Green, pColor.Blue );

			if ( pX < _Width-1 && pY < _Height-1 )
				this._ImageBuffers[this._CurrentImageBuffer].MarkPixel ( pX+1, pY+1, pColor.Red, pColor.Green, pColor.Blue );
		}

		public void MarkPixel ( int pX, int pY, SuperColor pColor )
		{
			this._ImageBuffers[this._CurrentImageBuffer].MarkPixel ( pX, pY, pColor.Red, pColor.Green, pColor.Blue );
		}

		public void MarkPixel ( int pX, int pY, byte pRed, byte pGreen, byte pBlue )
		{
			this._ImageBuffers[this._CurrentImageBuffer].MarkPixel ( pX, pY, pRed, pGreen, pBlue );
		}

		private void CheckScratchBuffers ()
		{
			if ( !this._ScratchBufferAllocated )
			{
				//Allocate a pixmap to be used for draw operations
				this._ScratchRenderBuffer = new Gdk.Pixmap ( null, this._Width, this._Height, Gdk.Visual.BestDepth );
				this._ScratchDisplayBuffer = new Gdk.Pixbuf ( Gdk.Colorspace.Rgb, false, 8, this._Width, this._Height );
				this._ScratchBufferAllocated = true;

				this._ScratchPenContext = new Gdk.GC ( this._ScratchRenderBuffer );

				//Todo: This should be a property
				this._SelectionColor = new Gdk.Color ( 0xff, 0, 0 );

				this._ScratchColorMap = Gdk.Colormap.System;

				this._ScratchColorMap.AllocColor (ref this._SelectionColor, true, true);

				this._ScratchPenContext.Foreground = this._SelectionColor;
			}
		}

		public void SetSelection ( int pX, int pY, int pWidth, int pHeight )
		{
			//if ( !this.Allow_Out_Of_Bounds_Selections || ((X > 0) && (Y > 0) && ((Y+Height) < this.Height) && ((X+_Width) < this._Width) ) )
			if ((pX > 0) && (pY > 0) && ((pY+pHeight) < this._Height) && ((pX+pWidth) < this._Width))
			{
				this.CheckScratchBuffers();

				this.ClearLastSelection();
				int tmpX = Math.Max(pX, 0);
				int tmpY = Math.Max(pY, 0);
				int tmpWidth, tmpHeight;

				if ( tmpX + pWidth > this._Width )
					tmpWidth = this._Width - tmpX;
				else
					tmpWidth = pWidth;

				if ( tmpY + pHeight > this._Height )
					tmpHeight = this._Height - tmpY;
				else
					tmpHeight = pHeight;

				int tmpInvalidTopX = Math.Min(this._SelectionLastX, pX);
				int tmpInvalidTopY = Math.Min( this._SelectionLastY, pY);
				int tmpInvalidBottomX = Math.Max( this._SelectionLastX+this._SelectionLastWidth, pX+pWidth);
				int tmpInvalidBottomY = Math.Max( this._SelectionLastY+this._SelectionLastHeight, pY+pWidth);

				this._SelectionLastX = tmpX;
				this._SelectionLastY = tmpY;
				this._SelectionLastWidth = tmpWidth;
				this._SelectionLastHeight = tmpHeight;
				this._SelectionLastSet = true;

				// RenderToDrawable (Drawable drawable, GC gc, int src_x, int src_y, int dest_x, int dest_y, int width, int height, RgbDither dither, int x_dither, int y_dither)
				this._ImageBuffers[this._CurrentImageBuffer].Buffer.RenderToDrawable (_ScratchRenderBuffer, this._ScratchPenContext, tmpX, tmpY, tmpX, tmpY, tmpWidth, tmpHeight, Gdk.RgbDither.None, 0, 0);

				this._ScratchRenderBuffer.DrawRectangle (this._ScratchPenContext, false, pX, pY, pWidth - 1, pHeight - 1);

				//FromDrawable (Drawable src, Colormap cmap, int src_x, int src_y, int dest_x, int dest_y, int width, int height)
				//GetFromDrawable (Drawable drawable, Colormap cmap, int src_x, int src_y, int dest_x, int dest_y, int width, int height)
				this._ScratchDisplayBuffer.GetFromDrawable (this._ScratchRenderBuffer, this._ScratchColorMap, tmpX, tmpY, tmpX, tmpY, tmpWidth, tmpHeight);
				this._ScratchDisplayBuffer.CopyArea (tmpX, tmpY, tmpWidth, tmpHeight, this._ImageDisplayBuffer, tmpX, tmpY);

				this.Refresh (tmpInvalidTopX, tmpInvalidTopY, tmpInvalidBottomX-tmpInvalidTopX, tmpInvalidBottomY-tmpInvalidTopY);
			}
		}

		private void ClearLastSelection ()
		{
			if ( this._SelectionLastSet )
			{
				this.BlitBuffer( this._CurrentImageBuffer, this._SelectionLastX, this._SelectionLastY, this._SelectionLastWidth, this._SelectionLastHeight );
				//this.Blit_Buffer();
			}
		}

		public void HideSelection()
		{
			this.ClearLastSelection();

			this.Refresh (this._SelectionLastX, this._SelectionLastY, this._SelectionLastWidth, this._SelectionLastHeight);
		}
	}

	//Would it not be neat if this had a masking layer as well?  Also support for an Alpha Channel maybe
	public class SuperImageBuffer
	{
		//_Width and Height
		private int _Width;
		private int _Height;
		private int _RowSize;

		//Each buffer has a Rendering byte array and a Pixbuf buffer
		private byte[] _ImageBufferRenderer;
		private Gdk.Pixbuf _ImageBuffer;

		public SuperImageBuffer ( int pImageWidth, int pImageHeight )
		{
			this._Width = pImageWidth;
			this._Height = pImageHeight;

			//Row_Size is 3*_Width (R, G, B)
			//Eventually we could do Images with more than 24 bits, although we'd have to downsample the colors to display them.
			this._RowSize = pImageWidth * 3;

			//The byte buffer needs to be Row_Size * Height bytes long
			this._ImageBufferRenderer = new byte[this._RowSize*this._Height];

			//And now to create the buffer.
			//Constructor:  Pixbuf (byte[] data, Colorspace colorspace, bool has_alpha, int bits_per_sample, int width, int height, int rowstride, PixbufDestroyNotify destroy_fn)
			this._ImageBuffer = new Gdk.Pixbuf( this._ImageBufferRenderer, Gdk.Colorspace.Rgb, false, 8, this._Width, this._Height, this._RowSize, null);
		}

		public Gdk.Pixbuf Buffer
		{
			get { return this._ImageBuffer; }
		}

		//Others: MarkRow, MarkColumn, ModifyRow (using a modifier function type! yeah), ModifyPixel, MarkBlock (with setable size?)
		public void MarkPixel (int pX, int pY, SuperColor pColor)
		{
			this.MarkPixel (pX, pY, pColor.Red, pColor.Green, pColor.Blue );
		}

		public void MarkPixel (int pX, int pY, byte pRed, byte pGreen, byte pBlue )
		{
			//The simple marking strategy
			//This may at some point be changed to having a "last written pixels" buffer deal
			//No validation yet for X and Y
			//		Valid values: 0 .. Size-1
			this._ImageBufferRenderer[(pY*this._RowSize)+(pX*3)] = pRed;
			this._ImageBufferRenderer[(pY*this._RowSize)+(pX*3)+1] = pGreen;
			this._ImageBufferRenderer[(pY*this._RowSize)+(pX*3)+2] = pBlue;
		}
	}
}
using Gtk;
using MasterSuperImage;
using MasterLogFile;

namespace MasterChaosDisplay
{
	//A "Rendering Machine" on which a "Chaos Engine" renders
	//Todo: Move to the Origin X, Y and then Width
	class ChaosRenderingMachine : PassThroughLoggedClass
	{
		//The (definable on creation) screen name.  This is useful for logging prefixes and such.
		private string _MachineName;

		//The Width and Height of the viewport
		private int _PixelWidth;
		private int _PixelHeight;

		//The pixel ratios of the last rendered image.
		private double _LastRenderedXPixelRatio;
		private double _LastRenderedYPixelRatio;

		//The origins of the last rendered image.  I.E. the display's absolute zero values.
		private double _LastRenderedXOrigin;
		private double _LastRenderedYOrigin;

		//The last clicked cartesian centerpoint.
		private double _LastClickedCenterpointX;
		private double _LastClickedCenterpointY;

		//The data used for the zoom on this viewport.
		//Todo: Hide these behind properties
		public int _ZoomCenterpointX;
		public int _ZoomCenterpointY;

		public int _ZoomTopLeftX;
		public int _ZoomTopLeftY;

		public int _ZoomWidth;
		public int _ZoomHeight;

		//The actual Render Machine output image
		private SuperImage _MachineOutputImage;

		//The color information
		private SuperColor _InfiniteColor;
		private SuperImagePalette _Palette;

		//The simplest Constructor
		public ChaosRenderingMachine ( string pMachineName )
		{
			//Set the machine name
			this._MachineName = pMachineName;

			//Set the machines viewport size
			this._PixelWidth = 400;
			this._PixelHeight = 300;

			//Define a color to represent "infinite"
			this._InfiniteColor = new SuperColor( 0, 15, 0 );

			//Create a simple 100 color pallette
			SuperColor tmpStartColor = new SuperColor( 0, 0, 0 );
			SuperColor tmpEndColor = new SuperColor( 255, 255, 255 );
			this._Palette = new SuperImagePalette( tmpStartColor, tmpEndColor, 100 );

			//Create a simple image; later this could end up being a "subsuper" image?  Only buffers no gtk
			this._MachineOutputImage = new SuperImage ( this._PixelWidth, this._PixelHeight );
		}

		public ChaosRenderingMachine ( string pMachineName, int pDisplayWidth, int pDisplayHeight, SuperColor pInfiniteColor, SuperColor pStartColor, SuperColor pEndColor, int pColorCount )
		{
			//Set the machine name
			this._MachineName = pMachineName;

			//Set the machines viewport size
			this._PixelWidth = pDisplayWidth;
			this._PixelHeight = pDisplayHeight;

			//Define a color to represent "infinite"
			this._InfiniteColor = pInfiniteColor;

			//Create a simple Gradient Pallete
			this._Palette = new SuperImagePalette( pStartColor, pEndColor, pColorCount );

			//Create a simple image; later this could end up being a "subsuper" image?  Only buffers no gtk
			this._MachineOutputImage = new SuperImage ( this._PixelWidth, this._PixelHeight, 2 );
		}

		public void HideSelection ()
		{
			this._MachineOutputImage.HideSelection();
		}

		public void SelectionSet ()
		{
			//this.Write_To_Log ( "Image Set Selection X["+this.Zoom_Top_Left_X.ToString()+"] Y["+this.Zoom_Top_Left_Y.ToString()+"] W["+this.Zoom_Width.ToString()+"] H["+this.Zoom_Height.ToString()+"]");
			this._MachineOutputImage.SetSelection ( this._ZoomTopLeftX, this._ZoomTopLeftY, this._ZoomWidth, this._ZoomHeight );
		}

#region Data Access Functions
		public string Name
		{
			get { return this._MachineName; }
		}

		public int Width
		{
			get { return this._PixelWidth; }
			set
			{
				try
				{
					if ( value > 0 )
						this._PixelWidth = value;
				}
				catch
				{
					this.WriteToLog ( "Error setting [Pixel_Width]." );
				}
			}
		}

		public int Height
		{
			get { return this._PixelHeight; }
			set
			{
				try
				{
					if ( value > 0 )
						this._PixelHeight = value;
				}
				catch
				{
					this.WriteToLog ( "Error setting [Pixel_Height]." );
				}
			}
		}

		public double LastXRatio
		{
			get {return this._LastRenderedXPixelRatio; }
			set {this._LastRenderedXPixelRatio = value; }
		}

		public double LastYRatio
		{
			get {return this._LastRenderedYPixelRatio; }
			set {this._LastRenderedYPixelRatio = value; }
		}

		public double LastXOrigin
		{
			get {return this._LastRenderedXOrigin; }
			set {this._LastRenderedXOrigin = value; }
		}

		public double LastYOrigin
		{
			get {return this._LastRenderedYOrigin; }
			set {this._LastRenderedYOrigin = value; }
		}

		public double LastClickedXCenterpoint
		{
			get {return this._LastClickedCenterpointX; }
			set {this._LastClickedCenterpointX = value; }
		}

		public double LastClickedYCenterpoint
		{
			get {return this._LastClickedCenterpointY; }
			set {this._LastClickedCenterpointY = value; }
		}

		public void MarkPixel ( int pX, int pY, int pColorIndex )
		{
			if ( pColorIndex == -1 )
				this._MachineOutputImage.MarkPixel ( pX, pY, this._InfiniteColor );
			else
				this._MachineOutputImage.MarkPixel ( pX, pY, this._Palette.GetColor ( pColorIndex ) );
		}

		public void MarkPixelQuad ( int pX, int pY, int pColorIndex )
		{
			if ( pColorIndex == -1 )
				this._MachineOutputImage.MarkPixelQuad ( pX, pY, this._InfiniteColor );
			else
				this._MachineOutputImage.MarkPixelQuad ( pX, pY, this._Palette.GetColor ( pColorIndex ) );
		}

		public void Refresh ()
		{
			this._MachineOutputImage.BlitBuffer();
			this._MachineOutputImage.Refresh();
		}

		public SuperImagePalette Pallete
		{
			get {return this._Palette; }
			set {this._Palette = value; }
		}

		public SuperColor Infinite
		{
			get {return this._InfiniteColor; }
		}

		public Gtk.Image Image
		{
			get {return this._MachineOutputImage.Image; }
		}
#endregion
	}
}
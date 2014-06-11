using MasterLogFile;

namespace MasterSuperImage
{
	public class SuperColor
	{
		private byte _Red;
		private byte _Green;
		private byte _Blue;

		private byte _Alpha;

		public SuperColor ()
		{
			this.SetColor ( 0, 0, 0 );
		}

		public SuperColor ( byte pRed, byte pGreen, byte pBlue )
		{
			this.SetColor ( pRed, pGreen, pBlue );
		}

		public SuperColor ( int pRed, int pGreen, int pBlue )
		{
			this.SetColor ( pRed, pGreen, pBlue );
		}

		public void SetColor ( byte pRed, byte pGreen, byte pBlue )
		{
			this.Red = 	pRed;
			this.Green = pGreen;
			this.Blue = pBlue;
		}

		public void SetColor (int New_Red, int New_Green, int New_Blue )
		{
			this.Red = 	this.ParseIntCustom( New_Red );
			this.Green = this.ParseIntCustom( New_Green );
			this.Blue = this.ParseIntCustom( New_Blue );
		}

		private byte ParseIntCustom ( int pValue )
		{
			if ( (pValue < 255) && (pValue >= 0) )
				return (byte)pValue;
			else if (pValue < 0)
				return (byte)0;
			else
				return (byte)255;
		}

		#region Data Access Functions
		public byte Red
		{
			get {return this._Red; }
			set {this._Red = value; }
		}

		public byte Green
		{
			get {return this._Green; }
			set {this._Green = value; }
		}

		public byte Blue
		{
			get {return this._Blue; }
			set {this._Blue = value; }
		}


		public byte Alpha
		{
			get {return this._Alpha; }
			set {this._Alpha = value; }
		}

		//Return string-based values for logging and such.
		public string DebugDisplayValue()
		{
			return "[c.R"+this._Red.ToString()+".G"+this._Green.ToString()+".B"+this._Blue.ToString()+"]";
		}
#endregion
	}


	public class SuperImagePalette : PassThroughLoggedClass
	{
		private SuperColor[] _ColorList;

		private int _ColorListCount;

		//Create a gradient pallete
		public SuperImagePalette ( SuperColor pFromColor, SuperColor pToColor, int pSteps )
		{
			this._ColorListCount = pSteps;

			this._ColorList = new SuperColor[this._ColorListCount];
			for (int tmpCounter = 0; tmpCounter < this._ColorListCount; tmpCounter++ )
				this._ColorList[tmpCounter] = new SuperColor();

			this.ComputeGradient ( 0, this._ColorListCount - 1, pFromColor, pToColor );
		}

		//Compute a gradient
		public void ComputeGradient ( int pFromColorIndex, int pThroughColorIndex, SuperColor pFromColor, SuperColor pThroughColor )
		{
			//TODO: deal with From_Color_Index > or = Thru_Color_Index and Negative Color Indices
			double tmpRed = (double)pFromColor.Red;
			double tmpGreen = (double)pFromColor.Green;
			double tmpBlue = (double)pFromColor.Blue;

			// Right now this is a linear walk through color space, but there could be a third point given to create interesting textures across the "arc".

			//Next we need to figure the step values, which will be Color Variance / Distance
			double tmpRedStep = (double)((double)(pThroughColor.Red - pFromColor.Red) / (double)(pThroughColorIndex - pFromColorIndex));
			double tmpGreenStep = (double)((double)(pThroughColor.Green - pFromColor.Green) / (double)(pThroughColorIndex - pFromColorIndex));
			double tmpBlueStep = (double)((double)(pThroughColor.Blue - pFromColor.Blue) / (double)(pThroughColorIndex - pFromColorIndex));

			this.WriteToLog ( "Creating Gradient From #"+pFromColorIndex.ToString()+pFromColor.DebugDisplayValue()+" To #"+pThroughColorIndex.ToString()+pThroughColor.DebugDisplayValue() );



			//Hopefully this cast isn't super heavy on processing power!
			for (int tmpCounter = 0; tmpCounter <= (pThroughColorIndex - pFromColorIndex); tmpCounter++ )
			{
				this._ColorList[tmpCounter + pFromColorIndex].Red = (byte)tmpRed;
				this._ColorList[tmpCounter + pFromColorIndex].Green = (byte)tmpGreen;
				this._ColorList[tmpCounter + pFromColorIndex].Blue = (byte)tmpBlue;

				tmpRed += tmpRedStep;
				tmpGreen += tmpGreenStep;
				tmpBlue += tmpBlueStep;
			}
		}

		public int Count
		{
			get {return this._ColorListCount; }
		}

		public SuperColor GetColor( int pIndex )
		{
			if ( (pIndex < this._ColorListCount) && (pIndex >= 0) )
			{
				return this._ColorList[pIndex];
			}
			else
			{
				this.WriteToLog("Color list out of bounds: " + pIndex.ToString() + " (list of " + this.Count.ToString() + ")");
				return this._ColorList[0];
			}
		}
	}
}
using Master_Log_File;

namespace Master_Super_Image
{
	public class Super_Color
	{
		private byte Red_Value;
		private byte Green_Value;
		private byte Blue_Value;

		private byte Alpha_Value;

		public Super_Color ()
		{
			this.Set_Color ( 0, 0, 0 );
		}

		public Super_Color ( byte Red, byte Green, byte Blue )
		{
			this.Set_Color ( Red, Green, Blue );
		}

		public Super_Color ( int Red, int Green, int Blue )
		{
			this.Set_Color ( Red, Green, Blue );
		}

		public void Set_Color ( byte New_Red, byte New_Green, byte New_Blue )
		{
			this.Red = 		New_Red;
			this.Green = 	New_Green;
			this.Blue = 	New_Blue;
		}

		public void Set_Color (int New_Red, int New_Green, int New_Blue )
		{
			this.Red = 		this.Parse_Int( New_Red );
			this.Green = 	this.Parse_Int( New_Green );
			this.Blue = 	this.Parse_Int( New_Blue );
		}

		private byte Parse_Int ( int Color_Value )
		{
			if ( (Color_Value < 255) && (Color_Value >= 0) )
				return (byte)Color_Value;
			else if (Color_Value < 0)
				return (byte)0;
			else
				return (byte)255;
		}


#region Overloaded Operators
		//public static Super_Color operator + (Value First_Color, Value Second_Color)
		//{return First_Color}
#endregion

#region Data Access Functions
		public byte Red
		{
			get {return this.Red_Value; }
			set {this.Red_Value = value; }
		}

		public byte Green
		{
			get {return this.Green_Value; }
			set {this.Green_Value = value; }
		}

		public byte Blue
		{
			get {return this.Blue_Value; }
			set {this.Blue_Value = value; }
		}


		public byte Alpha
		{
			get {return this.Alpha_Value; }
			set {this.Alpha_Value = value; }
		}

		//Return string-based values for logging and such.
		public string Debug_Value()
		{
			return "[c.R"+this.Red_Value.ToString()+".G"+this.Green_Value.ToString()+".B"+this.Blue_Value.ToString()+"]";
		}
#endregion
	}


	public class Super_Image_Pallete : Pass_Through_Logged_Class
	{
		private Super_Color[] Color_List;

		private int Color_List_Count;

		//Create a gradient pallete
		public Super_Image_Pallete ( Super_Color From_Color, Super_Color To_Color, int Steps )
		{
			this.Color_List_Count = Steps;

			this.Color_List = new Super_Color[this.Color_List_Count];
			for (int tmpCounter = 0; tmpCounter < this.Color_List_Count; tmpCounter++ )
				this.Color_List[tmpCounter] = new Super_Color();

			this.Compute_Gradient ( 0, this.Color_List_Count - 1, From_Color, To_Color );
		}

		//Compute a gradient
		public void Compute_Gradient ( int From_Color_Index, int Thru_Color_Index, Super_Color From_Color, Super_Color Thru_Color )
		{
			//TODO: deal with From_Color_Index > or = Thru_Color_Index and Negative Color Indices
			double Red_Pigment = (double)From_Color.Red;
			double Green_Pigment = (double)From_Color.Green;
			double Blue_Pigment = (double)From_Color.Blue;

			//Next we need to figure the step values, which will be Color Variance / Distance
			double Red_Step_Value = (double)((double)(Thru_Color.Red - From_Color.Red) / (double)(Thru_Color_Index - From_Color_Index));
			double Green_Step_Value = (double)((double)(Thru_Color.Green - From_Color.Green) / (double)(Thru_Color_Index - From_Color_Index));
			double Blue_Step_Value = (double)((double)(Thru_Color.Blue - From_Color.Blue) / (double)(Thru_Color_Index - From_Color_Index));

			this.Write_To_Log ( "Creating Gradient From #"+From_Color_Index.ToString()+From_Color.Debug_Value()+" To #"+Thru_Color_Index.ToString()+Thru_Color.Debug_Value() );



			//Hopefully this cast isn't super heavy on processing power!
			for (int tmpCounter = 0; tmpCounter <= (Thru_Color_Index - From_Color_Index); tmpCounter++ )
			{
				this.Color_List[tmpCounter + From_Color_Index].Red = (byte)Red_Pigment;
				this.Color_List[tmpCounter + From_Color_Index].Green = (byte)Green_Pigment;
				this.Color_List[tmpCounter + From_Color_Index].Blue = (byte)Blue_Pigment;

				Red_Pigment += Red_Step_Value;
				Green_Pigment += Green_Step_Value;
				Blue_Pigment += Blue_Step_Value;
			}
		}

		public int Count
		{
			get {return this.Color_List_Count; }
		}

		public Super_Color Get_Color( int Color_Index )
		{
			if ( (Color_Index < this.Color_List_Count) && (Color_Index >= 0) )
			{
				return this.Color_List[Color_Index];
			}
			else
			{
				this.Write_To_Log("Color list out of bounds: " + Color_Index.ToString() + " (list of " + this.Count.ToString() + ")");
				return this.Color_List[0];
			}
		}
	}
}
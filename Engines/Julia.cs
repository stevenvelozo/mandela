using System;

// The Julia set Fractal
// This engine is special in that it takes 2 parameters from another engine, although they are just coordinates.
// Our engines could be turned into DLL's at some point
namespace Master_Chaos_Display
{
	class Julia_Engine : Chaos_Engine
	{
		//The number of iterations for each point
		private int Param_Iterations;

		//The bailout distance from 0.  Set to3 2.0 as default.
		private double Param_Bailout;

		//The location on the Mandelbrot set to compute the Julia set from
		private double Param_Mandelbrot_Real;
		private double Param_Mandelbrot_Imaginary;

		//Cached internal coefficients
		private double Working_Real, Working_Imaginary;
		private double Working_Distance, Iterated_Real;
		private int Iteration_Counter;

		public override string Name
		{
			get
			{
				return "Julia";
			}
		}

		//Add any engine-specific parameters
		public override void Initialize_Engine_Parameters ()
		{
			//Normal limits
			this.Set_Default_Limits ( -1.5, 1.5, -1.125, 1.125 );

			//There are a few parameters that ALWAYS are required in a paramlist
			this.Engine_Parameters.Add_Parameter ( "Iterations", "Iterations",
				"The number of times to iterate each rendered point in the fractal.  Higher numbers take more time to render and generate more complex images.  Negligible results after 2000.",
				"Iteration Max", "Integer", "1000" );
			this.Engine_Parameters.Add_Parameter ( "Bailout", "Bailout",
				"The point at which an intersection is considered infinite.  The standard mandelbrot fractal stops at 2.0 but changing this can make interesting results",
				"Bailout", "Float", "4.0" );
			this.Engine_Parameters.Add_Parameter ( "MandelbrotReal", "Mandelbrot Real",
				"The real number on the Mandelbrot space from which to compute the Julia set.",
				"Mandelbrot space Real", "Float", "-0.502875" );
			this.Engine_Parameters.Add_Parameter ( "MandelbrotImaginary", "Mandelbrot Imaginary",
				"The imaginary number on the Mandelbrot space from which to compute the Julia set.",
				"Mandelbrot space Imaginary", "Float", "0.518925" );
		}

		//This is the overridden rendering system initialization
		public override void Initialize_Render_Subsystem ()
		{
			//Pure fractals only have one parameter.
			this.Param_Iterations = int.Parse( this.Engine_Parameters.Get_Parameter( "Iterations" ) );
			this.Param_Bailout = double.Parse( this.Engine_Parameters.Get_Parameter( "Bailout" ) );

			this.Param_Mandelbrot_Real = double.Parse( this.Engine_Parameters.Get_Parameter( "MandelbrotReal" ) );
			this.Param_Mandelbrot_Imaginary = double.Parse( this.Engine_Parameters.Get_Parameter( "MandelbrotImaginary" ) );
		}

		public override int Engine_Render_Value ( double Chaos_Space_X, double Chaos_Space_Y )
		{
			//Reset the Coefficients
			this.Working_Real = Chaos_Space_X;
			this.Working_Imaginary = Chaos_Space_Y;

			Iteration_Counter = 0;

			//Compute the initial coefficient
			this.Iterated_Real = (this.Working_Real * this.Working_Real) + (this.Working_Imaginary * this.Working_Imaginary);

			while ( (Iteration_Counter < this.Param_Iterations) && (this.Iterated_Real < this.Param_Bailout) )
			{
				this.Working_Distance = (this.Working_Real + this.Working_Real) * this.Working_Imaginary + this.Param_Mandelbrot_Imaginary;
				this.Working_Real = (this.Working_Real * this.Working_Real - this.Working_Imaginary * this.Working_Imaginary + this.Param_Mandelbrot_Real);
				this.Working_Imaginary = this.Working_Distance;

				this.Iterated_Real = (this.Working_Real * this.Working_Real) + (this.Working_Imaginary * this.Working_Imaginary);
				Iteration_Counter++;
			}


			if ( Iteration_Counter == this.Param_Iterations )
			{
				//Infinite
				return -1;
			}
			else
			{
				double Percentage = (double)Iteration_Counter / (double)(this.Param_Iterations);
				return (int)(Percentage * this.Color_Count);
			}
		}
	}
}
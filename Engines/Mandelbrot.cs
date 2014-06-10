using System;

// The Mandlebrot Fractal
// Our engines could be turned into DLL's at some point
namespace Master_Chaos_Display
{
	class Mandelbrot_Engine : Chaos_Engine
	{
		//The number of iterations for each point
		private int Param_Iterations;

		//The bailout distance from 0.  Set to 2.0 as default.
		private double Param_Bailout;

		//Cached internal coefficients
		//private double Working_Real, Working_Imaginary;
		//private double Working_Distance, Iterated_Real;
		private int Iteration_Counter;

		public override string Name
		{
			get
			{
				return "Mandelbrot";
			}
		}

		//Add any engine-specific parameters
		public override void Initialize_Engine_Parameters ()
		{
			//Normal limits
			this.Set_Default_Limits ( -2.0, 1.0, -1.0, 1.0 );

			//There are a few parameters that ALWAYS are required in a paramlist
			this.Engine_Parameters.Add_Parameter ( "Iterations", "Iterations",
				"The number of times to iterate each rendered point in the fractal.  Higher numbers take more time to render and generate more complex images.  Negligible results after 2000.",
				"Iteration Max", "Integer", "100" );
			this.Engine_Parameters.Add_Parameter ( "Bailout", "Bailout",
				"The point at which an intersection is considered infinite.  The standard mandelbrot fractal stops at 2.0 but changing this can make interesting results",
				"Bailout", "Float", "2.0" );
		}

		//This is the overridden rendering system initialization
		public override void Initialize_Render_Subsystem ()
		{
			//Pure fractals only have one parameter.
			this.Param_Iterations = int.Parse( this.Engine_Parameters.Get_Parameter( "Iterations" ) );
			this.Param_Bailout = double.Parse( this.Engine_Parameters.Get_Parameter( "Bailout" ) );
		}

		public override int Engine_Render_Value ( double Chaos_Space_X, double Chaos_Space_Y )
		{
			//Reset the Coefficients
			//this.Working_Real = 0.0;
			//this.Working_Imaginary = 0.0;
			//this.Working_Distance = 0.0;

			Iteration_Counter = 0;

			double Max_Normalization = Param_Bailout * Param_Bailout;

			// Initialize our complex numbers
			Complex Current_Point = new Complex(Chaos_Space_X, Chaos_Space_Y);
			Complex z = new Complex();

			while
				(
					(Iteration_Counter < Param_Iterations) &&
					//If z ever goes over  our max normalization, which is the equivelant of infinity in fractal space, we can bail out.
					(z.Normalize() < Max_Normalization)
				)
			{
				// Trying out structs in C# for kicks...
				// Used to track this all in the internal variables.  This will do a lot more allocation of objects but
				// greatly simplifies writing out the iterative formula in code.
				z = z * z + Current_Point;
				Iteration_Counter++;
			}

			if ( Iteration_Counter == Param_Iterations )
			{
				//Infinite
				return -1;
			}
			else
			{
				double Percentage = (double)Iteration_Counter / (double)Param_Iterations;

				return (int)((double)Percentage * (double)Color_Count);
			}
		}

		// Later on we should convert this to use an arbitrary precision library
		struct Complex
		{
			public double r; // the real number
			public double i; // the imaginary unit

			public Complex(double pReal, double pImaginary)
			{
				r = pReal;
				i = pImaginary;
			}

			// Add two complex numbers together
			public static Complex
				operator +
					(Complex m, Complex n)
			{
				return new Complex(m.r + n.r, m.i + n.i);
			}

			// Multiply two complex numbers
			public static Complex
				operator *
					(Complex m, Complex n)
			{
				return new Complex((m.r * n.r) - (m.i * n.i), (m.r * n.i) + (n.r * m.i));
			}

			public double Normalize()
			{
				return r * r + i * i;
			}
		}
	}
}

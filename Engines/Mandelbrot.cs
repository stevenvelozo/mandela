using System;

// The Mandlebrot Fractal
// Our engines could be turned into DLL's at some point
namespace MasterChaosDisplay
{
	class Mandelbrot_Engine : ChaosEngine
	{
		//The number of iterations for each point
		private int _Param_Iterations;

		//The bailout distance from 0.  Set to 2.0 as default.
		private double _ParamBailout;

		double _MaxNormalization;

		//Cached internal coefficients
		//private double Working_Real, Working_Imaginary;
		//private double Working_Distance, Iterated_Real;
		private int _IterationCounter;

		public override string Name
		{
			get { return "Mandelbrot"; }
		}

		//Add any engine-specific parameters
		public override void InitializeEngineParameters ()
		{
			//Normal limits
			this.SetDefaultLimits ( -2.0, 1.0, -1.0, 1.0 );

			//There are a few parameters that ALWAYS are required in a paramlist
			this._EngineParameters.AddParameter ( "Iterations", "Iterations",
				"The number of times to iterate each rendered point in the fractal.  Higher numbers take more time to render and generate more complex images.  Negligible results after 2000.",
				"Iteration Max", "Integer", "100" );
			this._EngineParameters.AddParameter ( "Bailout", "Bailout",
				"The point at which an intersection is considered infinite.  The standard mandelbrot fractal stops at 2.0 but changing this can make interesting results",
				"Bailout", "Float", "2.0" );
		}

		//This is the overridden rendering system initialization
		public override void InitializeRenderSubsystem ()
		{
			//Pure fractals only have one parameter.
			this._Param_Iterations = int.Parse( this._EngineParameters.GetParameter( "Iterations" ) );
			this._ParamBailout = double.Parse( this._EngineParameters.GetParameter( "Bailout" ) );
			_MaxNormalization = _ParamBailout * _ParamBailout;
		}

		public override int EngineRenderValue ( double pChaosSpaceX, double pChaosSpaceY )
		{
			_IterationCounter = 0;

			// Initialize our complex numbers
			Complex tmpCurrentPoint = new Complex(pChaosSpaceX, pChaosSpaceY);
			Complex tmpZ = new Complex();

			//If z ever goes over  our max normalization, which is the equivelant of infinity in fractal space, we can bail out.
			while ( (_IterationCounter < _Param_Iterations) && (tmpZ.Normalize() < _MaxNormalization) )
			{
				// Trying out structs in C# for kicks...
				// Used to track this all in the internal variables.  This will do a lot more allocation of objects but
				// greatly simplifies writing out the iterative formula in code.
				tmpZ = tmpZ * tmpZ + tmpCurrentPoint;
				_IterationCounter++;
			}

			if ( _IterationCounter == _Param_Iterations )
			{
				//Infinite
				return -1;
			}
			else
			{
				double tmpPercentage = (double)_IterationCounter / (double)_Param_Iterations;
				return (int)((double)tmpPercentage * (double)ColorCount);
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

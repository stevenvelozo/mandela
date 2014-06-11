using System;

// The Julia set Fractal
// This engine is special in that it takes 2 parameters from another engine, although they are just coordinates.
// Our engines could be turned into DLL's at some point
namespace MasterChaosDisplay
{
	class Julia_Engine : ChaosEngine
	{
		//The number of iterations for each point
		private int _Param_Iterations;

		//The bailout distance from 0.  Set to3 2.0 as default.
		private double _Param_Bailout;

		//The location on the Mandelbrot set to compute the Julia set from
		private double _Param_Mandelbrot_Real;
		private double _Param_Mandelbrot_Imaginary;

		//Cached internal coefficients
		private double tmpWorkingReal, tmpWorkingImaginary;
		private double tmpWorkingDistance, tmpIteratedReal;
		private int tmpIterationCounter;

		public override string Name
		{
			get { return "Julia"; }
		}

		//Add any engine-specific parameters
		public override void InitializeEngineParameters ()
		{
			//Normal limits
			this.SetDefaultLimits ( -1.5, 1.5, -1.125, 1.125 );

			//There are a few parameters that ALWAYS are required in a paramlist
			this._EngineParameters.AddParameter ( "Iterations", "Iterations",
				"The number of times to iterate each rendered point in the fractal.  Higher numbers take more time to render and generate more complex images.  Negligible results after 2000.",
				"Iteration Max", "Integer", "1000" );
			this._EngineParameters.AddParameter ( "Bailout", "Bailout",
				"The point at which an intersection is considered infinite.  The standard mandelbrot fractal stops at 2.0 but changing this can make interesting results",
				"Bailout", "Float", "4.0" );
			this._EngineParameters.AddParameter ( "MandelbrotReal", "Mandelbrot Real",
				"The real number on the Mandelbrot space from which to compute the Julia set.",
				"Mandelbrot space Real", "Float", "-0.502875" );
			this._EngineParameters.AddParameter ( "MandelbrotImaginary", "Mandelbrot Imaginary",
				"The imaginary number on the Mandelbrot space from which to compute the Julia set.",
				"Mandelbrot space Imaginary", "Float", "0.518925" );
		}

		//This is the overridden rendering system initialization
		public override void InitializeRenderSubsystem ()
		{
			//Pure fractals only have one parameter.
			this._Param_Iterations = int.Parse( this._EngineParameters.GetParameter( "Iterations" ) );
			this._Param_Bailout = double.Parse( this._EngineParameters.GetParameter( "Bailout" ) );

			this._Param_Mandelbrot_Real = double.Parse( this._EngineParameters.GetParameter( "MandelbrotReal" ) );
			this._Param_Mandelbrot_Imaginary = double.Parse( this._EngineParameters.GetParameter( "MandelbrotImaginary" ) );
		}

		public override int EngineRenderValue ( double pChaosSpaceX, double pChaosSpaceY )
		{
			//Reset the Coefficients
			this.tmpWorkingReal = pChaosSpaceX;
			this.tmpWorkingImaginary = pChaosSpaceY;

			tmpIterationCounter = 0;

			//Compute the initial coefficient
			this.tmpIteratedReal = (this.tmpWorkingReal * this.tmpWorkingReal) + (this.tmpWorkingImaginary * this.tmpWorkingImaginary);

			while ( (tmpIterationCounter < this._Param_Iterations) && (this.tmpIteratedReal < this._Param_Bailout) )
			{
				this.tmpWorkingDistance = (this.tmpWorkingReal + this.tmpWorkingReal) * this.tmpWorkingImaginary + this._Param_Mandelbrot_Imaginary;
				this.tmpWorkingReal = (this.tmpWorkingReal * this.tmpWorkingReal - this.tmpWorkingImaginary * this.tmpWorkingImaginary + this._Param_Mandelbrot_Real);
				this.tmpWorkingImaginary = this.tmpWorkingDistance;

				this.tmpIteratedReal = (this.tmpWorkingReal * this.tmpWorkingReal) + (this.tmpWorkingImaginary * this.tmpWorkingImaginary);
				tmpIterationCounter++;
			}


			if ( tmpIterationCounter == this._Param_Iterations )
			{
				//Infinite
				return -1;
			}
			else
			{
				double tmpPercentage = (double)tmpIterationCounter / (double)(this._Param_Iterations);
				return (int)(tmpPercentage * this.ColorCount);
			}
		}
	}
}
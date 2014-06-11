// A test engine that will print a gradiented grid.
// Our engines could be turned into DLL's at some point
namespace MasterChaosDisplay
{
	class Grid_Test_Engine : ChaosEngine
	{
		public override string Name
		{
			get { return "Grid Debug"; }
		}

		public override int EngineRenderValue ( double pChaosSpaceX, double pChaosSpaceY )
		{
			//Lots of math on iteration
			//	Chaos_Space_X - this.Chaos_Min_X = position
			double tmpRelativeX = pChaosSpaceX - this.ChaosMinX;
			double tmpRelativeY = pChaosSpaceY - this.ChaosMinY;


			double tmpXPercentage = tmpRelativeX / this.ChaosWidth;
			double tmpYPercentage = tmpRelativeY / this.ChaosHeight;

			double tmpTotalPercentage = (tmpXPercentage + tmpYPercentage) / 2;

			if ( ((int)(tmpXPercentage*1000) % 5 == 0) || ((int)(tmpYPercentage*1000) % 5 == 0) )
				return this.ColorCount - (int)(tmpTotalPercentage * this.ColorCount);
			else
				return (int)(tmpTotalPercentage * this.ColorCount);
		}
	}
}
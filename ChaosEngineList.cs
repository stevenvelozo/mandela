using System.Text.RegularExpressions;
using MasterDataStructureBase;

namespace MasterChaosDisplay
{
	class ChaosEngineList : LinkedList
	{
		public bool AddEngine ( string pEngineName, ChaosEngine pEngine )
		{
			ChaosEngineNode tmpEngine;

			tmpEngine = new ChaosEngineNode ( pEngineName );

			tmpEngine._Engine = pEngine;

			this.AddNodeTolist( tmpEngine );

			return true;
		}

		public bool FindFirstByName ( string pNameToFind )
		{
			Engine_Name_Match pSearchCriteria = new Engine_Name_Match ( pNameToFind );

			if ( this.LogAllSearches )
				this.WriteToLog( "Find Parameter: " + pSearchCriteria.Name );

			return this.FindFirst( pSearchCriteria );
		}

		public ChaosEngine Engine
		{
			get { return (CurrentNode.NodeData as ChaosEngineNode)._Engine; }
		}

		public string CurrentEngineName
		{
			get
			{
				if ( this.Count > 0 )
					return (CurrentNode.NodeData as ChaosEngineNode).Name;
				else
					return "";
			}
		}
	}

	public class Engine_Name_Match : NodeMatch
	{
		private string _NameToMatch;

		private Regex _MatchFunction;

		public Engine_Name_Match ( string pNameToMatch )
		{
			//Our names have no spaces
			this._NameToMatch = pNameToMatch.Trim();

			//Now build the regular expression out of it
			this._MatchFunction = new Regex( "\\b"+this._NameToMatch.Replace( "*", ".*" )+"\\b" );
		}

		#region Data Access Functions
		public string Name
		{
			get { return this._NameToMatch; }
		}
		#endregion

		public override bool Match ( DataNode pNodeToVerify )
		{
			//See if it matches.  This is extremely straightforward.
			if ( this._MatchFunction.IsMatch( (pNodeToVerify.NodeData as ChaosEngineNode).Name ) )
				return true;
			else
				return false;
		}
	}

	class ChaosEngineNode
	{
		//The paramater node for the list of Engines.
		private string _Name;

		public ChaosEngine _Engine;

		public ChaosEngineNode ( string pName )
		{
			this._Name = pName;
		}

		public string Name
		{
			get { return this._Name; }
		}
 	}
}
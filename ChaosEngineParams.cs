using System.Text.RegularExpressions;
using MasterDataStructureBase;

namespace MasterChaosDisplay
{
	class ChaosEngineParameterList : LinkedList
	{
		public bool AddParameter ( string pParameterKey, string pName, string pDescription, string pTooltip, string pType, string pDefaultValue )
		{
			Chaos_Engine_Parameter tmpChaosEngineParameter;

			tmpChaosEngineParameter = new Chaos_Engine_Parameter ( pParameterKey );

			tmpChaosEngineParameter.Name = pName;
			tmpChaosEngineParameter.Description = pDescription;
			tmpChaosEngineParameter.Tooltip = pTooltip;
			tmpChaosEngineParameter.Type = pType;
			tmpChaosEngineParameter.Value = pDefaultValue;
			tmpChaosEngineParameter.DefaultValue = pDefaultValue;

			this.AddNodeTolist( tmpChaosEngineParameter );

			return true;
		}

		public string GetParameter ( string pParameterKey )
		{
			if ( this.FindFirstByKey ( pParameterKey ) )
				return (CurrentNode.NodeData as Chaos_Engine_Parameter).Value;
			else
				return "";
		}

		public bool SetParameter ( string pParameterKey, string pParameterValue )
		{
			if ( this.FindFirstByKey ( pParameterKey ) )
			{
				(CurrentNode.NodeData as Chaos_Engine_Parameter).Value = pParameterValue;
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool FindFirstByKey ( string pKeyToFind )
		{
			KeyMatch Search_Criteria = new KeyMatch ( pKeyToFind );

			if ( this.LogAllSearches )
				this.WriteToLog( "Find Parameter: " + Search_Criteria.Key );

			return this.FindFirst( Search_Criteria );
		}

		public string CurrentParameterKey
		{
			get
			{
				if ( this.Count > 0 )
					return (CurrentNode.NodeData as Chaos_Engine_Parameter).ParameterKey;
				else
					return "";
			}
		}

		//Serialize the list of parameters as PARAM[VALUE] PARAM2[VALUE2]
		public string Serialize_Parameters
		{
			get
			{
				string tmpParameterSet = "";

				this.MoveFirst();
				if ( this.Count > 0 )
				{
					//Walk through the list.
					while ( !this.EOL )
					{
						tmpParameterSet += (CurrentNode.NodeData as Chaos_Engine_Parameter).ParameterKey + "[" + (CurrentNode.NodeData as Chaos_Engine_Parameter).Value + "] ";
						this.MoveNext();
					}
				}

				return tmpParameterSet;
			}
		}
	}

	public class KeyMatch : NodeMatch
	{
		private string _KeyToMatch;

		private Regex MatchFunction;

		public KeyMatch ( string pKeyToMatch )
		{
			//Our names have no spaces
			this._KeyToMatch = pKeyToMatch.Trim();

			//Now build the regular expression out of it
			this.MatchFunction = new Regex( "\\b"+this._KeyToMatch.Replace( "*", ".*" )+"\\b" );
		}

		#region Data Access Functions
		public string Key
		{
			get { return this._KeyToMatch; }
		}
		#endregion

		public override bool Match ( DataNode NodeToVerify )
		{
			//See if it matches.  This is extremely straightforward.
			if ( this.MatchFunction.IsMatch( (NodeToVerify.NodeData as Chaos_Engine_Parameter).ParameterKey ) )
				return true;
			else
				return false;
		}
	}

	class Chaos_Engine_Parameter
	{
		//The paramater node for the list of Engine Parameters.
		private string _ParameterKey;

		public string Name;

		public string Description;

		public string Tooltip;

		public string Type;

		public string Value;

		public string DefaultValue;

		public Chaos_Engine_Parameter ( string pKey )
		{
			this._ParameterKey = pKey;
		}

		public string ParameterKey
		{
			get { return this._ParameterKey; }
		}
 	}
}
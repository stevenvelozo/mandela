using System.Text.RegularExpressions;
using Master_Data_Structure_Base;

namespace Master_Chaos_Display
{
	class Chaos_Engine_List : Linked_List
	{
		public bool Add_Engine ( string Engine_Name, Chaos_Engine Engine )
		{
			Chaos_Engine_Node New_Engine;

			New_Engine = new Chaos_Engine_Node ( Engine_Name );

			New_Engine.Engine = Engine;

			this.Add_Node_To_List( New_Engine );

			return true;
		}

		public bool Find_First_By_Name ( string Name_To_Find )
		{
			Engine_Name_Match Search_Criteria = new Engine_Name_Match ( Name_To_Find );

			if ( this.Log_All_Searches )
			{
				this.Write_To_Log( "Find Parameter: " + Search_Criteria.Name );
			}

			return this.Find_First( Search_Criteria );
		}

		public Chaos_Engine Engine
		{
			get
			{
				return (Current_Node.Node_Data as Chaos_Engine_Node).Engine;
			}
		}

		public string Current_Engine_Name
		{
			get
			{
				if ( this.Count > 0 )
				{
					return (Current_Node.Node_Data as Chaos_Engine_Node).Name;
				}
				else
				{
					return "";
				}
			}
		}
	}

	public class Engine_Name_Match : Node_Match
	{
		private string Name_To_Match;

		private Regex Match_Function;

		public Engine_Name_Match ( string New_Name_To_Match )
		{
			//Our names have no spaces
			this.Name_To_Match = New_Name_To_Match.Trim();

			//Now build the regular expression out of it
			this.Match_Function = new Regex( "\\b"+this.Name_To_Match.Replace( "*", ".*" )+"\\b" );
		}

		#region Data Access Functions
		public string Name
		{
			get
			{
				return this.Name_To_Match;
			}
		}
		#endregion

		public override bool Match ( Data_Node Node_To_Verify )
		{
			//See if it matches.  This is extremely straightforward.
			if ( this.Match_Function.IsMatch( (Node_To_Verify.Node_Data as Chaos_Engine_Node).Name ) )
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	class Chaos_Engine_Node
	{
		//The paramater node for the list of Engines.
		private string Engine_Internal_Name;

		public Chaos_Engine Engine;

		public Chaos_Engine_Node ( string Engine_Name )
		{
			this.Engine_Internal_Name = Engine_Name;
		}

		public string Name
		{
			get
			{
				return this.Engine_Internal_Name;
			}
		}
 	}
}
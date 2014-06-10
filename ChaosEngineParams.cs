using System.Text.RegularExpressions;
using Master_Data_Structure_Base;

namespace Master_Chaos_Display
{
	class Chaos_Engine_Parameter_List : Linked_List
	{
		public bool Add_Parameter ( string Parameter_Internal_Name, string Parameter_Name, string Parameter_Description,
									string Parameter_Tooltip, string Parameter_Type, string Parameter_Default_Value )
		{
			Chaos_Engine_Parameter New_Engine_Parameter;

			New_Engine_Parameter = new Chaos_Engine_Parameter ( Parameter_Internal_Name );

			New_Engine_Parameter.Parameter_Name = Parameter_Name;
			New_Engine_Parameter.Parameter_Description = Parameter_Description;
			New_Engine_Parameter.Parameter_Tooltip = Parameter_Tooltip;
			New_Engine_Parameter.Parameter_Type = Parameter_Type;
			New_Engine_Parameter.Parameter_Value = Parameter_Default_Value;
			New_Engine_Parameter.Parameter_Default_Value = Parameter_Default_Value;

			this.Add_Node_To_List( New_Engine_Parameter );

			return true;
		}

		public string Get_Parameter ( string Parameter_Name )
		{
			if ( this.Find_First_By_Name ( Parameter_Name ) )
			{
				return (Current_Node.Node_Data as Chaos_Engine_Parameter).Parameter_Value;
			}
			else
			{
				return "";
			}
		}

		public bool Set_Parameter ( string Parameter_Name, string New_Value )
		{
			if ( this.Find_First_By_Name ( Parameter_Name ) )
			{
				(Current_Node.Node_Data as Chaos_Engine_Parameter).Parameter_Value = New_Value;
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool Find_First_By_Name ( string Name_To_Find )
		{
			Name_Match Search_Criteria = new Name_Match ( Name_To_Find );

			if ( this.Log_All_Searches )
			{
				this.Write_To_Log( "Find Parameter: " + Search_Criteria.Name );
			}

			return this.Find_First( Search_Criteria );
		}

		public string Current_Parameter_Name
		{
			get
			{
				if ( this.Count > 0 )
				{
					return (Current_Node.Node_Data as Chaos_Engine_Parameter).Name;
				}
				else
				{
					return "";
				}
			}
		}

		//Serialize the list of parameters as PARAM[VALUE] PARAM2[VALUE2]
		public string Serialize_Parameters
		{
			get
			{
				string Parameter_Set = "";

				this.Move_First();
				if ( this.Count > 0 )
				{
					//Walk through the list.
					while ( !this.EOL )
					{
						Parameter_Set += (Current_Node.Node_Data as Chaos_Engine_Parameter).Name + "[" + (Current_Node.Node_Data as Chaos_Engine_Parameter).Parameter_Value + "] ";
						this.Move_Next();
					}
				}

				return Parameter_Set;
			}
		}
	}

	public class Name_Match : Node_Match
	{
		private string Name_To_Match;

		private Regex Match_Function;

		public Name_Match ( string New_Name_To_Match )
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
			if ( this.Match_Function.IsMatch( (Node_To_Verify.Node_Data as Chaos_Engine_Parameter).Name ) )
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	class Chaos_Engine_Parameter
	{
		//The paramater node for the list of Engine Parameters.
		private string Parameter_Internal_Name;

		public string Parameter_Name;

		public string Parameter_Description;

		public string Parameter_Tooltip;

		public string Parameter_Type;

		public string Parameter_Value;

		public string Parameter_Default_Value;

		public Chaos_Engine_Parameter ( string Internal_Name )
		{
			this.Parameter_Internal_Name = Internal_Name;
		}

		public string Name
		{
			get
			{
				return this.Parameter_Internal_Name;
			}
		}
 	}
}
/*
 * A base class for all List memory data structures.
 * 
 * Prototype and logging information that would be considered "standard" will start
 * here.
 * 
 * This should allow us to reuse comparators for multiple data structures.
 * 
  * TODO: Consider putting saving and loading crap into this
 */

using System;
using Master_Log_File;

namespace Master_Data_Structure_Base
{
	public class Data_Structure_Base : Pass_Through_Logged_Class
	{
		//This is so we can override the current node business
		protected Data_Node Internal_Current_Node;

		//The overridable property
		public virtual Data_Node Current_Node
		{
			get
			{
				return this.Internal_Current_Node;
			}
			set
			{
				//Nothing now, but we want to be able to override it.
			}
		}

		public long Current_Item_Key
		{
			get
			{
				if (this.Node_Count > 0)
				{
					return this.Current_Node.Index;
				}
				else
				{
					return 0;
				}
			}
		}
		
		
		// The number of items in the data structure
		protected long Node_Count;
		
		//Default statistics for a data structure
		protected long Search_Count;            //Number of searches
		protected long Search_Match_Count;      //Number of matches resulting from searches
		protected long Navigation_Count;		//Number of navigation calls made (movefirst, etc)
		//Extended statistics
		protected long Add_Count;				//Number of inserts
		protected long Delete_Count;			//Number of deletes
		
		//Logging options
		protected bool Log_Searches;			//Time and log each search
		protected bool Log_Navigation;		    //Log each navigation function (WAY HUGE log files)
		//Extended logging options
		protected bool Log_Adds;				//Log each add (can make log files HUGE)
		protected bool Log_Deletes;				//Log each delete operation
		

		// The Auto Incriment key
		protected long Last_Given_Key;

		public Data_Structure_Base ()
		{
			//Initialize the list count
			this.Node_Count = 0;

			//Reset the statistics
			this.Search_Count = 0;
			this.Search_Match_Count = 0;
			this.Navigation_Count = 0;
			this.Add_Count = 0;
			this.Delete_Count = 0;
			
			this.Log_Searches = false;
			this.Log_Navigation = false;
			this.Log_Adds = false;
			this.Log_Deletes = false;
			
			//The list is empty.  Zero everything out!
			this.Last_Given_Key = 0;
		}
		
		#region Data Access Functions
		/// <summary>
		/// The list count
		/// </summary>
		public long Count
		{
			get
			{
				return this.Node_Count;
			}
		}
		
		public long Statistic_Search_Count
		{
			get
			{
				return this.Search_Count;
			}
		}
		
		public long Statistic_Search_Match_Count
		{
			get
			{
				return this.Search_Match_Count;
			}
		}
		
		public long Statistic_Navigation_Count
		{
			get
			{
				return this.Navigation_Count;
			}
		}
		public long Statistic_Add_Count
		{
			get
			{
				return this.Add_Count;
			}
		}
		
		public long Statistic_Delete_Count
		{
			get
			{
				return this.Delete_Count;
			}
		}
		
		
		public bool Log_All_Searches
		{
			get
			{
				return this.Log_Searches;
			}
			set
			{
				this.Log_Searches = value;
			}
		}
		
		public bool Log_All_Navigation
		{
			get
			{
				return this.Log_Navigation;
			}
			set
			{
				this.Log_Navigation = value;
			}
		}
		
		public bool Log_All_Adds
		{
			get
			{
				return this.Log_Adds;
			}
			set
			{
				this.Log_Adds = value;
			}
		}
		
		public bool Log_All_Deletes
		{
			get
			{
				return this.Log_Deletes;
			}
			set
			{
				this.Log_Deletes = value;
			}
		}
		#endregion
	}

	public class Data_Node
    {
        //The primary key
        private long Data_Node_ID;
        
        //The data gram
        private object Data_Gram;
        
        //The constructor
        public Data_Node ( object New_Data_Gram )
        {
        	this.Data_Gram = New_Data_Gram;
        }

        #region Data Access Functions
        /// <summary>
        /// The primary key for the list node.
        /// </summary>
        public long Index
        {
        	get
        	{
        		return this.Data_Node_ID;
        	}
        	set
        	{
        		this.Data_Node_ID = value;
        	}
        }
        
        public object Node_Data
        {
        	get
        	{
        		return this.Data_Gram;
        	}
        	set
        	{
        		this.Data_Gram = value;
        	}
        }
        #endregion
    }

		
	public class Node_Match
	{
		//The key to match
		private long Node_Key;
		
		public Node_Match ()
		{
			//Do nothing
			this.Node_Key = 0;
		}
		
		public Node_Match ( long Node_Key_To_Find )
		{
			this.Node_Key = Node_Key_To_Find;
		}

		/// <summary>
		/// Compare two nodes.
		/// Return -1 for Left < Right
		/// Return 0 for  Left = Right
		/// Return 1 for  Left > Right
		/// </summary>
		/// <param name="Left_Node_Value">Left Comparator</param>
		/// <param name="Right_Node_Value">Right Comparator</param>
		/// <returns>-1 for L less than R, 0 for L equal to R, 1 for L greater than R</returns>
		public virtual int Compare ( Data_Node Left_Node_Value, Data_Node Right_Node_Value )
		{
			if ( Left_Node_Value.Index < Right_Node_Value.Index )
			{
				return -1;
			}
			else if ( Left_Node_Value.Index == Right_Node_Value.Index )
			{
				return 0;
			}
			else
			{
				return 1;
			}
		}
		
		
		public virtual bool Match ( Data_Node Node_To_Verify )
		{
			//See if it matches.  This is extremely straightforward.
			if ( Node_To_Verify.Index == this.Node_Key )
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}

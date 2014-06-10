/*
 * A derivable linked list implimented to test out inheritance and speed theirin.
 *
 * TODO: Thread safe iterators
 * TODO: Auto Sorted lists by key, may require comparison stuff in case search doesn't use Sorted parameter
 * TODO: Dynamic searching methods, taking into account list sort status
 */

using System;
using Master_Log_File;
using Master_Time_Span;
using Master_Data_Structure_Base;

namespace Master_Data_Structure_Base
{
	public class Linked_List_Base : Data_Structure_Base
	{
		// The list head and tail
		protected List_Node List_Head;
		protected List_Node List_Tail;

		#region Data Access Functions
		public bool EOL
		{
			get
			{
				if ( (this.Node_Count > 0) && (this.Current_Node.Index == this.List_Tail.Index) )
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		#endregion

		#region Linked List Navigation Functions
		public bool Move_First ()
		{
			try
			{
				if ( Node_Count > 0 )
				{
					//Set the current node to the list head
					this.Current_Node = this.List_Head;

					//Update the statistics
					this.Navigation_Count++;

					if ( this.Log_Navigation )
					{
						this.Write_To_Log ( "Nav: [Move First] in "+this.Count.ToString()+" items" );
					}

					return true;
				}
				else
				{
					return false;
				}
			}
			catch
			{
				//Eventually we may want to set up a log file chain for errors
				return false;
			}
		}

		public bool Move_Previous ()
		{
			try
			{
				if ( (Node_Count > 0) && (this.Current_Node.Index != this.List_Head.Index) )
				{
					//Set the current node to the previous node
					this.Current_Node = (this.Current_Node as List_Node).Previous_Node;

					//Update the statistics
					this.Navigation_Count++;

					if ( this.Log_Navigation )
					{
						this.Write_To_Log ( "Nav: [Move Previous] in "+this.Count.ToString()+" items" );
					}

					return true;
				}
				else
				{
					return false;
				}
			}
			catch
			{
				//Eventually we may want to set up a log file chain for errors
				return false;
			}
		}

		public bool Move_Next ()
		{
			try
			{
				if ( (Node_Count > 0) && (this.Current_Node.Index != this.List_Tail.Index) )
				{
					//Set the current node to the next node in the list
					this.Current_Node = (this.Current_Node as List_Node).Next_Node;

					//Update the statistics
					this.Navigation_Count++;

					if ( this.Log_Navigation )
					{
						this.Write_To_Log ( "Nav: [Move Next] in "+this.Count.ToString()+" items" );
					}

					return true;
				}
				else
				{
					return false;
				}
			}
			catch
			{
				//Eventually we may want to set up a log file chain for errors
				return false;
			}
		}

		public bool Move_Last ()
		{
			try
			{
				if ( Node_Count > 0 )
				{
					//Set the current node to the list tail
					this.Current_Node = this.List_Tail;

					//Update the statistics
					this.Navigation_Count++;

					if ( this.Log_Navigation )
					{
						this.Write_To_Log ( "Nav: [Move Last] in "+this.Count.ToString()+" items" );
					}

					return true;
				}
				else
				{
					return false;
				}
			}
			catch
			{
				//Eventually we may want to set up a log file chain for errors
				return false;
			}
		}
		#endregion

		#region Linked List Search Functions
		/// <summary>
		/// A linear search
		/// </summary>
		/// <param name="Key_To_Search_For">The index # to find in the list</param>
		/// <returns></returns>
		public bool Find_First_By_Index ( long Index_To_Search_For )
		{
			try
			{
				//Create the default match criteron (a key)
				Node_Match Find_Key = new Node_Match ( Index_To_Search_For );

				//Now find it
				return this.Find_First ( Find_Key );
			}
			catch
			{
				this.Write_To_Log ( "Error finding key " + Index_To_Search_For.ToString() );

				return false;
			}
		}

		public bool Find_First ( Node_Match Matching_Method )
		{
			try
			{
				return this.Linear_Search_Forward_From_Beginning ( Matching_Method );
			}
			catch
			{
				this.Write_To_Log ( "Error finding first item" );

				return false;
			}
		}

		public bool Find_Next ( Node_Match Matching_Method )
		{
			try
			{
				if ( (this.Node_Count > 1) && (!this.EOL) )
				{
					this.Move_Next();

					return this.Linear_Search_Forward( Matching_Method );
				}
				else
				{
					return false;
				}
			}
			catch
			{
				this.Write_To_Log ( "Error finding next item" );

				return false;
			}
		}

		/// <summary>
		/// A forward linear search from the beginning of the list
		/// </summary>
		/// <param name="Matching_Method">A derivative of Node_Match that returns Match() as true on hits</param>
		/// <returns>True if anything was found</returns>
		public bool Linear_Search_Forward_From_Beginning ( Node_Match Matching_Method )
		{
			bool Matched_Item = false;

			try
			{
				if ( Node_Count > 0 )
				{
					//Update the statistics
					this.Search_Count++;

					//First see if we are already at the matching node
					if ( Matching_Method.Match( this.Current_Node ) )
					{
						this.Search_Match_Count++;
						Matched_Item = true;
					}
					else
					{
						//Set the current node to the list head
						this.Move_First();

						//Now search forward and see if there are any hits
						Matched_Item = this.Linear_Search_Forward ( Matching_Method );
					}
				}
			}
			catch
			{
				this.Write_To_Log( "Error searching list of ("+this.Count.ToString()+") items" );
			}

			return Matched_Item;
		}

		public bool Linear_Search_Forward ( Node_Match Matching_Method )
		{
			bool Matched_Item = false;
			Time_Span Search_Timer = new Time_Span();

			if ( this.Log_Searches )
			{
				Search_Timer.Start();
			}

			try
			{
				if ( this.Count > 0 )
				{
					//Now walk through the list until we either find it or hit the end.
					while ( ( !Matching_Method.Match( this.Current_Node ) ) && (!this.EOL) )
					{
						this.Move_Next();
					}

					//See if we found it or not
					if ( Matching_Method.Match( this.Current_Node ) )
					{
						this.Search_Match_Count++;
						Matched_Item = true;
					}
				}
			}
			catch
			{
				this.Write_To_Log( "Error during forward linear searching list of ("+this.Count.ToString()+") items" );
			}
			finally
			{
				if ( this.Log_Searches )
				{
					Search_Timer.Time_Stamp();
					if ( Matched_Item )
					{
						this.Write_To_Log( "Searched Forward in a list of ("+this.Count.ToString()+") items and found item #"+this.Current_Item_Key.ToString()+" taking "+Search_Timer.Time_Difference.ToString()+"ms." );
					}
					else
					{
						this.Write_To_Log( "Searched Forward in a list of ("+this.Count.ToString()+") items and did not get a match taking "+Search_Timer.Time_Difference.ToString()+"ms." );
					}
				}
			}

			return Matched_Item;
		}
		#endregion
	}

	/// <summary>
	/// A linked list class; this will be inherited obviously.
	/// </summary>
	public class Linked_List : Linked_List_Base
	{
		/// <summary>
		/// The current node.
		/// In the full list it is writable and readable.
		/// </summary>
		public override Data_Node Current_Node
		{
			get
			{
				return this.Internal_Current_Node;
			}
			set
			{
				this.Internal_Current_Node = value;
			}
		}

		// The class initializer
		public Linked_List():base()
		{
			//Nothing needs to be done for a base linked list
		}

		#region Linked List Insert Functions
		/// <summary>
		/// Add a node to the linked list.  This will in most cases stay the way it is.
		/// It does magic number generation and stuff.
		/// </summary>
		/// <param name="Node_Data_Gram">An object to be the datagram in the node</param>
		protected void Add_Node_To_List ( object Node_Data_Gram )
		{
			// TODO: Later the add method style should be dynamic
			//       i.e. Beginning would add new nodes to the top, End would be the bottom,
			//            Before_Current before the Current_Node, After_Current after
			List_Node Node_To_Add;

			try
			{
				Node_To_Add = new List_Node( Node_Data_Gram );

				if ( this.Count > 0 )
				{
					//Add it to the end of the list
					this.Add_Node_After ( Node_To_Add, this.List_Tail );
				}
				else
				{
					//Add it to an empty list
					this.Add_First_Node ( Node_To_Add );
				}

				this.Node_Count++;

				// We handle the key on inserts, as well.
				this.Last_Given_Key++;
				Node_To_Add.Index = this.Last_Given_Key;

				// Update the statistics
				this.Add_Count++;
			}
			catch
			{
				this.Write_To_Log( "Error adding a node to the list during the decision tree phase." );
			}
		}

		/// <summary>
		/// Add a node to an empty list
		/// </summary>
		/// <param name="Node_To_Add">The node to add to the list</param>
		protected void Add_First_Node ( List_Node Node_To_Add )
		{
			try
			{
				// Add a node to the list of no nodes.
				if ( this.Count == 0 )
				{
					this.List_Head = Node_To_Add;
					this.Current_Node = Node_To_Add;
					this.List_Tail = Node_To_Add;
					if ( this.Log_Adds )
					{
						this.Write_To_Log( "Added node # "+Node_To_Add.Index.ToString()+" successfully to the empty list." );
					}
				}
			}
			catch
			{
				this.Write_To_Log( "Error adding the first node to the list." );
			}
		}

		/// <summary>
		/// Add a node after the node Reference_Point
		/// </summary>
		/// <param name="Node_To_Add">The node to add to the list</param>
		/// <param name="Reference_Point">The node after which this node is to come</param>
		protected void Add_Node_After ( List_Node Node_To_Add, List_Node Reference_Point )
		{
			try
			{
				if ( Reference_Point.Index != this.List_Tail.Index )
				{
					//This is being inserted after a normal node
					Node_To_Add.Previous_Node = Reference_Point;
					Node_To_Add.Next_Node = Reference_Point.Next_Node;

					Reference_Point.Next_Node = Node_To_Add;

					Node_To_Add.Next_Node.Previous_Node = Node_To_Add;
				}
				else
				{
					//This is coming after the list tail
					Node_To_Add.Previous_Node = Reference_Point;

					Reference_Point.Next_Node = Node_To_Add;

					this.List_Tail = Node_To_Add;
				}

				if ( this.Log_Adds )
				{
					this.Write_To_Log( "Added node # "+Node_To_Add.Index.ToString()+" after node # "+Reference_Point.Index.ToString()+"." );
				}
			}
			catch
			{
				this.Write_To_Log( "Error adding node #"+Node_To_Add.Index+" after Node #"+Reference_Point.Index+"." );
			}
		}

		/// <summary>
		/// Add a node before the node Reference_Point
		/// </summary>
		/// <param name="Node_To_Add">The node to add to the list</param>
		/// <param name="Reference_Point">The node before which this node is to come</param>
		protected void Add_Node_Before ( List_Node Node_To_Add, List_Node Reference_Point )
		{
			try
			{
				if ( Reference_Point.Index != this.List_Head.Index )
				{
					//This is being inserted after a normal node
					Node_To_Add.Next_Node = Reference_Point;
					Node_To_Add.Previous_Node = Reference_Point.Previous_Node;

					Reference_Point.Previous_Node = Node_To_Add;

					Node_To_Add.Previous_Node.Next_Node = Node_To_Add;
				}
				else
				{
					//This is coming before the list head
					Node_To_Add.Next_Node = Reference_Point;

					Reference_Point.Previous_Node = Node_To_Add;

					this.List_Head = Node_To_Add;
				}

				if ( this.Log_Adds )
				{
					this.Write_To_Log( "Added node # "+Node_To_Add.Index.ToString()+" before node # "+Reference_Point.Index.ToString()+"." );
				}
			}
			catch
			{
				this.Write_To_Log( "Error adding node #"+Node_To_Add.Index+" after Node #"+Reference_Point.Index+"." );
			}
		}
		#endregion
	}

	/// <summary>
	/// The linked list node.
	/// </summary>
	public class List_Node : Data_Node
    {
		//The next and previous node
        private List_Node Next_List_Node;
        private List_Node Previous_List_Node;

        public List_Node ( object New_Data_Gram ):base(New_Data_Gram)
        {
        	//This is just a pass-through
        }

        #region List Node Data Access Functions
        public List_Node Next_Node
        {
        	get
        	{
        		return this.Next_List_Node;
        	}
			set
			{
				this.Next_List_Node = value;
			}
        }

        public List_Node Previous_Node
        {
        	get
        	{
        		return this.Previous_List_Node;
        	}
        	set
        	{
        		this.Previous_List_Node = value;
        	}
        }
        #endregion
    }
}

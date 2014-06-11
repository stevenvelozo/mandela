/*
 * A derivable linked list implimented to test out inheritance and speed theirin.
 *
 * TODO: Thread safe iterators
 * TODO: Auto Sorted lists by key, may require comparison stuff in case search doesn't use Sorted parameter
 * TODO: Dynamic searching methods, taking into account list sort status
 */

using System;
using MasterLogFile;
using MasterTimeSpan;
using MasterDataStructureBase;

namespace MasterDataStructureBase
{
	public class LinkedListBase : DataStructureBase
	{
		// The list head and tail
		protected ListNode _Head;
		protected ListNode _Tail;

		#region Data Access Functions
		public bool EOL
		{
			get
			{
				if ( (this._NodeCount > 0) && (this.CurrentNode.Index == this._Tail.Index) )
					return true;
				else
					return false;
			}
		}
		#endregion

		#region Linked List Navigation Functions
		public bool MoveFirst ()
		{
			try
			{
				if ( _NodeCount > 0 )
				{
					//Set the current node to the list head
					this.CurrentNode = this._Head;

					//Update the statistics
					this._NavigationCount++;

					if ( this._LogNavigation )
						this.WriteToLog ( "Nav: [Move First] in "+this.Count.ToString()+" items" );

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

		public bool MovePrevious ()
		{
			try
			{
				if ( (_NodeCount > 0) && (this.CurrentNode.Index != this._Head.Index) )
				{
					//Set the current node to the previous node
					this.CurrentNode = (this.CurrentNode as ListNode).PreviousNode;

					//Update the statistics
					this._NavigationCount++;

					if ( this._LogNavigation )
						this.WriteToLog ( "Nav: [Move Previous] in "+this.Count.ToString()+" items" );

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

		public bool MoveNext ()
		{
			try
			{
				if ( (_NodeCount > 0) && (this.CurrentNode.Index != this._Tail.Index) )
				{
					//Set the current node to the next node in the list
					this.CurrentNode = (this.CurrentNode as ListNode).NextNode;

					//Update the statistics
					this._NavigationCount++;

					if ( this._LogNavigation )
						this.WriteToLog ( "Nav: [Move Next] in "+this.Count.ToString()+" items" );

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

		public bool MoveLast ()
		{
			try
			{
				if ( _NodeCount > 0 )
				{
					//Set the current node to the list tail
					this.CurrentNode = this._Tail;

					//Update the statistics
					this._NavigationCount++;

					if ( this._LogNavigation )
						this.WriteToLog ( "Nav: [Move Last] in "+this.Count.ToString()+" items" );

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
		public bool FindFirstByIndex ( long pIndexToSearchFor )
		{
			try
			{
				//Create the default match criteron (a key)
				NodeMatch tmpSearchingNode = new NodeMatch ( pIndexToSearchFor );

				//Now find it
				return this.FindFirst ( tmpSearchingNode );
			}
			catch
			{
				this.WriteToLog ( "Error finding key " + pIndexToSearchFor.ToString() );

				return false;
			}
		}

		public bool FindFirst ( NodeMatch pMatchMethod )
		{
			try
			{
				return this.LinearSearchForwardFromBeginning ( pMatchMethod );
			}
			catch
			{
				this.WriteToLog ( "Error finding first item" );

				return false;
			}
		}

		public bool FindNext ( NodeMatch pMatchMethod )
		{
			try
			{
				if ( (this._NodeCount > 1) && (!this.EOL) )
				{
					this.MoveNext();

					return this.LinearSearchForward( pMatchMethod );
				}
				else
				{
					return false;
				}
			}
			catch
			{
				this.WriteToLog ( "Error finding next item" );

				return false;
			}
		}

		/// <summary>
		/// A forward linear search from the beginning of the list
		/// </summary>
		/// <param name="Matching_Method">A derivative of Node_Match that returns Match() as true on hits</param>
		/// <returns>True if anything was found</returns>
		public bool LinearSearchForwardFromBeginning ( NodeMatch pMatchMethod )
		{
			bool tmpMatchFound = false;

			try
			{
				if ( _NodeCount > 0 )
				{
					//Update the statistics
					this._SearchCount++;

					//First see if we are already at the matching node
					if ( pMatchMethod.Match( this.CurrentNode ) )
					{
						this._SearchMatchCount++;
						tmpMatchFound = true;
					}
					else
					{
						//Set the current node to the list head
						this.MoveFirst();

						//Now search forward and see if there are any hits
						tmpMatchFound = this.LinearSearchForward ( pMatchMethod );
					}
				}
			}
			catch
			{
				this.WriteToLog( "Error searching list of ("+this.Count.ToString()+") items" );
			}

			return tmpMatchFound;
		}

		public bool LinearSearchForward ( NodeMatch pMatchMethod )
		{
			bool tmpMatchFound = false;
			TimeSpanMetric tmpSearchTimer = new TimeSpanMetric();

			if ( this._LogSearch )
				tmpSearchTimer.Start();

			try
			{
				if ( this.Count > 0 )
				{
					//Now walk through the list until we either find it or hit the end.
					while ( ( !pMatchMethod.Match( this.CurrentNode ) ) && (!this.EOL) )
						this.MoveNext();

					//See if we found it or not
					if ( pMatchMethod.Match( this.CurrentNode ) )
					{
						this._SearchMatchCount++;
						tmpMatchFound = true;
					}
				}
			}
			catch
			{
				this.WriteToLog( "Error during forward linear searching list of ("+this.Count.ToString()+") items" );
			}
			finally
			{
				if ( this._LogSearch )
				{
					tmpSearchTimer.Time_Stamp();
					if ( tmpMatchFound )
						this.WriteToLog( "Searched Forward in a list of ("+this.Count.ToString()+") items and found item #"+this.CurrentNodeIndex.ToString()+" taking "+tmpSearchTimer.TimeDifference.ToString()+"ms." );
					else
						this.WriteToLog( "Searched Forward in a list of ("+this.Count.ToString()+") items and did not get a match taking "+tmpSearchTimer.TimeDifference.ToString()+"ms." );
				}
			}

			return tmpMatchFound;
		}
		#endregion
	}

	/// <summary>
	/// A linked list class; this will be inherited obviously.
	/// </summary>
	public class LinkedList : LinkedListBase
	{
		/// <summary>
		/// The current node.
		/// In the full list it is writable and readable.
		/// </summary>
		public override DataNode CurrentNode
		{
			get { return this._CurrentNode; }
			set { this._CurrentNode = value; }
		}

		// The class initializer
		public LinkedList():base()
		{
			//Nothing needs to be done for a base linked list
		}

		#region Linked List Insert Functions
		/// <summary>
		/// Add a node to the linked list.  This will in most cases stay the way it is.
		/// It does magic number generation and stuff.
		/// </summary>
		/// <param name="Node_Data_Gram">An object to be the datagram in the node</param>
		protected void AddNodeTolist ( object pNodeData )
		{
			// TODO: Later the add method style should be dynamic
			//       i.e. Beginning would add new nodes to the top, End would be the bottom,
			//            Before_Current before the Current_Node, After_Current after
			ListNode tmpNodeToAdd;

			try
			{
				tmpNodeToAdd = new ListNode( pNodeData );

				if ( this.Count > 0 )
				{
					//Add it to the end of the list
					this.AddNodeAfter ( tmpNodeToAdd, this._Tail );
				}
				else
				{
					//Add it to an empty list
					this.AddFirstNode ( tmpNodeToAdd );
				}

				this._NodeCount++;

				// We handle the key on inserts, as well.
				this._LastGivenIndex++;
				tmpNodeToAdd.Index = this._LastGivenIndex;

				// Update the statistics
				this._AddCount++;
			}
			catch
			{
				this.WriteToLog( "Error adding a node to the list during the decision tree phase." );
			}
		}

		/// <summary>
		/// Add a node to an empty list
		/// </summary>
		/// <param name="Node_To_Add">The node to add to the list</param>
		protected void AddFirstNode ( ListNode pNodeToAdd )
		{
			try
			{
				// Add a node to the list of no nodes.
				if ( this.Count == 0 )
				{
					this._Head = pNodeToAdd;
					this.CurrentNode = pNodeToAdd;
					this._Tail = pNodeToAdd;
					if ( this._LogAdd )
						this.WriteToLog( "Added node # "+pNodeToAdd.Index.ToString()+" successfully to the empty list." );
				}
			}
			catch
			{
				this.WriteToLog( "Error adding the first node to the list." );
			}
		}

		/// <summary>
		/// Add a node after the node Reference_Point
		/// </summary>
		/// <param name="Node_To_Add">The node to add to the list</param>
		/// <param name="Reference_Point">The node after which this node is to come</param>
		protected void AddNodeAfter ( ListNode pNodeToAdd, ListNode pReferenceNode )
		{
			try
			{
				if ( pReferenceNode.Index != this._Tail.Index )
				{
					//This is being inserted after a normal node
					pNodeToAdd.PreviousNode = pReferenceNode;
					pNodeToAdd.NextNode = pReferenceNode.NextNode;

					pReferenceNode.NextNode = pNodeToAdd;

					pNodeToAdd.NextNode.PreviousNode = pNodeToAdd;
				}
				else
				{
					//This is coming after the list tail
					pNodeToAdd.PreviousNode = pReferenceNode;

					pReferenceNode.NextNode = pNodeToAdd;

					this._Tail = pNodeToAdd;
				}

				if ( this._LogAdd )
					this.WriteToLog( "Added node # "+pNodeToAdd.Index.ToString()+" after node # "+pReferenceNode.Index.ToString()+"." );
			}
			catch
			{
				this.WriteToLog( "Error adding node #"+pNodeToAdd.Index+" after Node #"+pReferenceNode.Index+"." );
			}
		}

		/// <summary>
		/// Add a node before the node Reference_Point
		/// </summary>
		/// <param name="Node_To_Add">The node to add to the list</param>
		/// <param name="Reference_Point">The node before which this node is to come</param>
		protected void AddNodeBefore ( ListNode pNodeToAdd, ListNode pReferenceNode )
		{
			try
			{
				if ( pReferenceNode.Index != this._Head.Index )
				{
					//This is being inserted after a normal node
					pNodeToAdd.NextNode = pReferenceNode;
					pNodeToAdd.PreviousNode = pReferenceNode.PreviousNode;

					pReferenceNode.PreviousNode = pNodeToAdd;

					pNodeToAdd.PreviousNode.NextNode = pNodeToAdd;
				}
				else
				{
					//This is coming before the list head
					pNodeToAdd.NextNode = pReferenceNode;

					pReferenceNode.PreviousNode = pNodeToAdd;

					this._Head = pNodeToAdd;
				}

				if ( this._LogAdd )
				{
					this.WriteToLog( "Added node # "+pNodeToAdd.Index.ToString()+" before node # "+pReferenceNode.Index.ToString()+"." );
				}
			}
			catch
			{
				this.WriteToLog( "Error adding node #"+pNodeToAdd.Index+" after Node #"+pReferenceNode.Index+"." );
			}
		}
		#endregion
	}

	/// <summary>
	/// The linked list node.
	/// </summary>
	public class ListNode : DataNode
    {
		//The next and previous node
		private ListNode _NextNode;
		private ListNode _PreviousNode;

        public ListNode ( object pNodeData ):base(pNodeData)
        {
        	//This is just a pass-through
        }

        #region List Node Data Access Functions
        public ListNode NextNode
        {
        	get { return this._NextNode; }
			set { this._NextNode = value; }
        }

        public ListNode PreviousNode
        {
        	get { return this._PreviousNode; }
        	set { this._PreviousNode = value; }
        }
        #endregion
    }
}

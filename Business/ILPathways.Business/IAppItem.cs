using System;
using System.Collections.Generic;
using System.Text;

namespace ILPathways.Business
{
	/// <summary>
	/// Public interface for all entities acting as an application item
	/// </summary>
	public interface IAppItem : IBaseObject
	{

		/// <summary>
		/// Gets/Sets VersionNbr
		/// </summary>
		int VersionNbr{ get; set;}


		/// <summary>
		/// Gets/Sets the RowId of the parent AppItem
		/// </summary>
		Guid ParentRowId{ get; set;}
		

		/// <summary>
		/// Gets/Sets SequenceNbr
		/// </summary>
		int SequenceNbr{ get; set;}

		/// <summary>
		/// Gets/Sets TypeId
		/// </summary>
		int TypeId{ get; set;}
		

		/// <summary>
		/// Gets/Sets Type
		/// </summary>
		string AppItemCode { get; set;}


		/// <summary>
		/// Gets/Sets Title
		/// </summary>
		//string Title { get; set;}

		/// <summary>
		/// Gets/Sets Description
		/// </summary>
		//string Description { get; set;}

		/// <summary>
		/// Gets/Sets Category
		/// </summary>
		string Category { get; set;}

		/// <summary>
		/// Gets/Sets Subcategory
		/// </summary>
		string Subcategory { get; set;}

		/// <summary>
		/// Gets/Sets IsActive
		/// </summary>
		bool IsActive { get; set;}

		/// <summary>
		/// Gets/Sets Status
		/// </summary>
		string Status { get; set;}

		/// <summary>
		/// Gets/Sets StartDate
		/// </summary>
		DateTime StartDate { get; set;}

		/// <summary>
		/// Gets/Sets EndDate
		/// </summary>
		DateTime EndDate { get; set;}

		/// <summary>
		/// Gets/Sets ExpiryDate
		/// </summary>
		DateTime ExpiryDate { get; set;}

		/// <summary>
		/// Gets/Sets Approved
		/// </summary>
		DateTime Approved { get; set;}

		/// <summary>
		/// Gets/Sets ApprovedById
		/// </summary>
		int ApprovedById { get; set;}

		/// <summary>
		/// Gets/Sets the RowId of the parent AppItem
		/// </summary>
		Guid RelatedObjectRowId { get; set;}

		/// <summary>
		/// Gets/Sets HistoryTitle
		/// </summary>
		//string HistoryTitle();
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ILPathways.Business
{
	public interface IBaseObject
	{
		/// <summary>
		/// Gets/Sets Id
		/// </summary>
		int Id { get; set;}


		/// <summary>
		/// Gets/Sets the RowId
		/// </summary>
		Guid RowId { get; set;}

		/// <summary>
		/// Gets/Sets Title
		/// </summary>
		string Title { get; set;}

		/// <summary>
		/// Gets/Sets Description
		/// </summary>
		string Description { get; set;}

        /// <summary>
        /// Gets/Sets CreatedById
        /// </summary>
        int CreatedById { get; set; }

		/// <summary>
		/// Gets/Sets HistoryTitle
		/// </summary>
		string HistoryTitle();

		/// <summary>
		/// Gets/Sets CreatedByTitle
		/// </summary>
		string CreatedByTitle();

		/// <summary>
		/// Gets/Sets UpdatedByTitle
		/// </summary>
		string UpdatedByTitle();
	}
}

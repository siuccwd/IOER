using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{/// <summary>
    /// Represents an object that describes a GenericMessage
    /// </summary>
    [Serializable]
    public class GenericMessage : BaseBusinessDataEntity
    {
        /// <summary>
        /// Initializes a new instance of the ILPathways.Business.GenericMessage class
        /// </summary>
        public GenericMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ILPathways.Business.GenericMessage class
        /// while specifying the text of the message
        /// </summary>
        /// <param name="text"></param>
        public GenericMessage( string text )
        {
            this.text = text;
        }

        private string text = string.Empty;
        /// <summary>
        /// Gets/Sets the GenericMessage's associated Text
        /// </summary>
        public virtual string Text
        {
            get { return this.text; }
            set { this.text = value; }
        }
    }
}

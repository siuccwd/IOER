namespace ILPathways.Utilities
{
	/// <summary>
	/// Summary description for Constants.
	/// </summary>
	public class Constants
	{
		public Constants()
		{		}
		/// <summary>
		///  Key to store user information in the aspnet session memory
		/// </summary>
		public const string USER_REGISTER				= "user"; // stored in the session
		/// <summary>
		/// Key to store MCMS user information in the aspnet session memory
		/// </summary>
		public const string USER_MCMS					= "mcmsUser"; // stored in the session

		/// <summary>
		/// Roles
		/// </summary>
		public const long USER_ROLE_RESIDENT = 1;
		public const long USER_ROLE_ADVISOR = 2;
		public const long USER_ROLE_EMPLOYER = 4;
		public const long USER_ROLE_ADMINISTRATOR = 99;

		/// <summary>
		/// Security
		/// </summary>
		public const string MCMS_SECURITYLEVEL_REGIONAL = "1";
		public const string MCMS_SECURITYLEVEL_ADMIN = "2";
		public const string MCMS_SPECIAL_STATEWIDE_ACCESS = "3";

	}
}

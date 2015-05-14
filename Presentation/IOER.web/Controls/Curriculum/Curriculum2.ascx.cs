using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using ILPathways.Library;

namespace ILPathways.Controls.Curriculum
{
    public partial class Curriculum2 : BaseUserControl
    {
        public List<Common.CodeItem> k12Subjects { get; set; }
        public List<Common.CodeItem> gradeLevels { get; set; }
        public List<Common.CodeItem> orgs { get; set; }

        protected void Page_Load( object sender, EventArgs e )
        {
            LoadFormsData();
        }

        protected void LoadFormsData()
        {
            //K-12 Subjects
            k12Subjects = CodeTableBizService.Resource_CodeTableSelectList( "Codes.Subject" );
            gradeLevels = CodeTableBizService.Resource_CodeTableSelectList( "Codes.GradeLevel" );

            orgs = OrganizationBizService.OrganizationMembersCodes_WithContentPrivileges( WebUser.Id );
            if ( orgs == null || orgs.Count == 0 )
            {
                //hide the org list
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using IOER.Library;
using ILPathways.Common;

namespace IOER.Controls.Curriculum
{
    public partial class Curriculum2 : BaseUserControl
    {
        public List<CodeItem> k12Subjects { get; set; }
        public List<CodeItem> gradeLevels { get; set; }
        public List<CodeItem> orgs { get; set; }

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
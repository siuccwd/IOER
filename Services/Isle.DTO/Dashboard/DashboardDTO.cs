using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
  public class DashboardDTO
  {
    public DashboardDTO()
    {
      library = new ResourcesBox();
      orgLibraries = new ResourcesBox();
      orgLibrariesResources = new ResourcesBox();

      myResources = new ResourcesBox();

      followedLibraries = new ResourcesBox();
      followedLibraryResources = new ResourcesBox();
        //set default, allow setting later
      maxResources = 6;

      includeMyFollowedLibraries = 2;
      includeMyFollowedLibrariesRecentResources = 0;

      includeMyMemberLibraries = 2;
      includeMyMemberLibrariesRecentResources = 0;
      message = "";
    }
      //configuration
      //0 - none, 1 - Private, 2 - public (visible to public)

    public int includeMyFollowedLibraries { get; set; }
    public int includeMyFollowedLibrariesRecentResources { get; set; }

    /// <summary>
    /// Whether to show personal library plus all libraries where a member
    /// </summary>
    public int includeMyMemberLibraries { get; set; }
    public int includeMyMemberLibrariesRecentResources { get; set; }

    public string message { get; set; }
    //user
    public int userId { get; set; }
    public bool isMyDashboard { get; set; }
    public string name { get; set; }
    public string avatarUrl { get; set; }
    public string description { get; set; }
    public string role { get; set; }
    public string jobTitle { get; set; }
    public string organization { get; set; }

    //Library
    public int libraryId { get; set; }
    public string libraryUrl { get; set; }
    public int libraryPublicAccessLevel { get; set; }
    public int libraryOrgAccessLevel { get; set; }
    

    /// <summary>
    /// Set to the max nbr of resources to include with a library
    /// </summary>
    public int maxResources { get; set; }
    /// <summary>
    /// Extra = collection the resource is in
    /// </summary>
    public ResourcesBox library { get; set; } //

    public ResourcesBox myResources { get; set; } //Extra = empty or something useful

    public ResourcesBox orgLibraries { get; set; } //Extra = the organization library name (or org name) this came from    
    public ResourcesBox orgLibrariesResources { get; set; } //Extra = name of the Library this came from

    public ResourcesBox followedLibraries { get; set; }
    public ResourcesBox followedLibraryResources { get; set; } //Extra = name of the Library this came from



  }
}

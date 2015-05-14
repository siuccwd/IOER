<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ubersearch.ascx.cs" Inherits="ILPathways.Controls.SearchV6.Themes.ubersearch" %>

<script type="text/javascript">
  var keywordSchemas = ["gradeLevel", "learningResourceType", "mediaType", "k12Subject"];
</script>
<style type="text/css">
  /* Search Header */
  #btnToggleFilters { background-color: <%=MainColorHex %>; color: #FFF; }
  #btnToggleFilters.expanded { background-color: #9984BD; }

  /* Filters */
  #filters #categories input { background-color: rgba(<%=MainColor.R %>,<%=MainColor.G %>,<%=MainColor.B %>, 0.9); color: #FFF; }
  #filters #categories input.selected { background-color: #9984BD; }
  #tags .tagList h2 { background-color: #4AA394; color: #FFF; }

  /* Paginators */
  .paginator input { background-color: <%=MainColorHex %>; color: #FFF; }
  .paginator input.current { background-color: #9984BD; }

  /* Results */
  .result.list .expandCollapseBox input { background-color: transparent; color: <%=MainColorHex %>; border: none; font-style: italic; }
  .result.list .expandCollapseBox input:hover, .result.list .expandCollapseBox input:focus { color: #4C98CC; }
  .theme .result.grid .paradata { background-image: linear-gradient(90deg, transparent 10%, rgba(<%=MainColor.R %>,<%=MainColor.G %>,<%=MainColor.B %>,0.8)); }
</style>
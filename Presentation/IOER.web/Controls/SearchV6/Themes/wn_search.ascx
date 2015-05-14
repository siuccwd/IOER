<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="wn_search.ascx.cs" Inherits="ILPathways.Controls.SearchV6.Themes.wn_search" %>

<script type="text/javascript">
  var keywordSchemas = ["educationalRole", "learningResourceType", "mediaType", "k12Subject"];
</script>
<style type="text/css">
  /* Search Header */
  #btnToggleFilters { background-color: <%=MainColorHex %>; color: #FFF; }
  #btnToggleFilters.expanded { background-color: #4D4D4D; }

  /* Filters */
  #filters #categories input { background-color: rgba(<%=MainColor.R %>,<%=MainColor.G %>,<%=MainColor.B %>, 0.9); color: #FFF; }
  #filters #categories input.selected { background-color: #4D4D4D; }
  #tags .tagList h2 { background-color: #4D4D4D; color: #FFF; }

  /* Paginators */
  .paginator input { background-color: <%=MainColorHex %>; color: #FFF; }
  .paginator input.current { background-color: #4D4D4D; }

  /* Results */
  .result.list .expandCollapseBox input { background-color: transparent; color: <%=MainColorHex %>; border: none; font-style: italic; }
  .result.list .expandCollapseBox input:hover, .result.list .expandCollapseBox input:focus { color: #D97B22; }
  .theme .result.grid .paradata { background-image: linear-gradient(90deg, transparent 10%, rgba(<%=MainColor.R %>,<%=MainColor.G %>,<%=MainColor.B %>, 0.8)); }
</style>
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace ILPathways.Utilities
{
	/// <summary>
	/// TBD
	/// </summary>
  public class HttpPostRequest
  {
		#region Properties
    private string _url = "";
    public string Url {
      get {return _url;}
      set {_url = value;}
    }

    private string _parameters = "";
    public string Parameters {
      get {return _parameters;}
      set {_parameters=value;}
    }
		#endregion

		#region Constructors
    public HttpPostRequest()
    {
    }

    public HttpPostRequest(string url)
    {
      _url = url;
    }
		#endregion

		/// <summary>
		/// Send an HTTP Post command
		/// </summary>
		/// <param name="postParameters"></param>
		/// <param name="message">Used to return error messages</param>
		/// <returns>String returned from the post</returns>
		public string DoPost( string postParameters, ref string message )
		{
			string result = "";

			StreamWriter myWriter = null;
			StreamReader sr = null;

			if (Url.Length ==0) 
			{
				message = "<p>ERROR: No URL was provided for class: HttpPostRequest</p>";
				return "";
			}

			HttpWebRequest webRequest = ( HttpWebRequest ) WebRequest.Create( Url );
			webRequest.Method = "POST";
			webRequest.ContentLength = postParameters.Length;
			webRequest.ContentType = "application/x-www-form-urlencoded";

			try
			{
				myWriter = new StreamWriter( webRequest.GetRequestStream() );
				myWriter.Write( postParameters );
			} catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "HttpPostRequest.DoPost - HttpWebRequest" );
				message = "ERROR: HttpPostRequest.DoPost<p>" + ex.ToString() + "</p>";
			} finally
			{
				myWriter.Close();
			}

			try
			{
				HttpWebResponse objResponse = ( HttpWebResponse ) webRequest.GetResponse();
				using ( sr = new StreamReader( objResponse.GetResponseStream() ) )
				{
					result = sr.ReadToEnd();

				}
			} catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "HttpPostRequest.DoPost - HttpWebResponse" );
				message = "ERROR: HttpPostRequest.DoPost - HttpWebResponse<p>" + ex.ToString() + "</p>";
			} finally
			{
				if ( sr != null )
				{
					// Close and clean up the StreamReader
					sr.Close();
				}
			}

			return result;

		} // end method


    /// <summary>
    /// Does an HTTP "POST" request to the URL specified with the specified parameters.
    /// Returns the results of the POST request.
    /// </summary>
    /// <returns></returns>
    public string Submit()
    {
      WebRequest webRequest = WebRequest.Create(Url);

      webRequest.Method = "POST";
      webRequest.ContentType = "application/x-www-form-urlencoded";

      byte[] bytes = Encoding.ASCII.GetBytes(Parameters);
      Stream os = null;

      try
      { //Send the post
        webRequest.ContentLength = bytes.Length; //Count bytes to send
        os = webRequest.GetRequestStream();
        os.Write(bytes, 0, bytes.Length); //Send request
      }
      catch (Exception ex)
      {
        // Do nothing
      }
      finally
      {
        if (os != null)
        {
          os.Close();
        }
      }

      try
      { //Get the response
        WebResponse webResponse = webRequest.GetResponse();
        if (webResponse == null)
        {
          return null;
        }

        StreamReader sr = new StreamReader(webResponse.GetResponseStream());
        return sr.ReadToEnd().Trim();
      }
      catch (Exception ex)
      {
        //do nothing
      }

      return null;
    }

    /// <summary>
    /// URL encodes and adds key/value pair to string for HTTP post
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void AddPostParameter(string key, string value)
    {
      if (_parameters == null || _parameters.Length == 0)
      {
        _parameters = key + "=" + HttpUtility.UrlEncode(value);
      }
      else
      {
        _parameters += "&" + key + "=" + HttpUtility.UrlEncode(value);
      }
      return;
    }

  }
}

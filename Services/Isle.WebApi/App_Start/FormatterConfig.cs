using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using WebApiContrib.Formatting.Jsonp;

namespace Isle.WebApi
{
    public class FormatterConfig
    {
        public static void RegisterFormatters( MediaTypeFormatterCollection formatters )
        {
            var jsonFormatter = formatters.JsonFormatter;
            jsonFormatter.SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // Insert the JSONP formatter in front of the standard JSON formatter.
            var jsonpFormatter = new JsonpMediaTypeFormatter( formatters.JsonFormatter );
            formatters.Insert( 0, jsonpFormatter );
        }
    }
}
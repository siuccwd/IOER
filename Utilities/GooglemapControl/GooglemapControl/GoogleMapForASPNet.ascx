<%--//   Google Maps User Control for ASP.Net version 1.0:
//   ========================
//   Copyright (C) 2008  Shabdar Ghata 
//   Email : ghata2002@gmail.com
//   URL : http://www.shabdar.org

//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.

//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.

//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.

//   This program comes with ABSOLUTELY NO WARRANTY.
--%>
<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GoogleMapForASPNet.ascx.cs" Inherits="GoogleMapForASPNet" %>
<%@ Register Assembly="System.Web.Extensions, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>
<asp:ScriptManager ID="ScriptManager1" runat="server">
 <Services>
    <asp:ServiceReference Path="~/GService.asmx" />
  </Services>
</asp:ScriptManager>

<div id="GoogleMap_Div" style="width:<%=GoogleMapObject.Width %>;height:<%=GoogleMapObject.Height %>;">

</div>
<div id="directions_canvas">

</div>
<script src="http://maps.google.com/maps?file=api&amp;v=<%=GoogleMapObject.APIVersion %>&amp;key=<%=GoogleMapObject.APIKey %>"  type="text/javascript"></script>
<script language="JavaScript">
var map;
var trafficInfo = null;

//function fListeners()
//{
//    this.listeners = new Array();
//    this.getLength = function() { return this.listeners.length; };
//    this.pushValue = function(v) { this.listeners.push(v); }
//    this.getValue = function(i)  { return this.listeners[i]; }
//}
function fMarkers()
{
    this.markers = new Array();
    this.getLength = function() { return this.markers.length; };
    this.pushValue = function(v) { this.markers.push(v); }
    this.getValue = function(i)  { return this.markers[i]; }
    this.getLastValue = function()  { return this.markers[this.markers.length-1]; }
    this.getValueById = function(ID)  {
                                        var i;
                                        for(i=0;i<this.markers.length;i++)
                                        {
                                            if(this.markers[i].value==ID)
                                            {
                                               // alert('marker found : '+this.markers[i].value);
                                                return this.markers[i];
                                            }
                                        } 
                                        return null; 
                                      }
    this.removeValueById = function(ID)  {
                                        var i;
                                        for(i=0;i<this.markers.length;i++)
                                        {
                                            if(this.markers[i].value==ID)
                                            {
                                               // alert('marker found : '+this.markers[i].value);
                                                this.markers.splice(i,1);
                                                //alert('changed marker removed');

                                            }
                                        } 
                                        return null; 
                                      }
}

function fPolylines()
{
    this.polylines = new Array();
    this.polylinesID = new Array();
    this.getLength = function() { return this.polylines.length; };
    this.pushValue = function(v,ID) {  this.polylines.push(v); this.polylinesID.push(ID); }
    this.getValue = function(i)  { return this.polylines[i]; }
    this.getLastValue = function()  { return this.polylines[this.polylines.length-1]; }
    this.getValueById = function(ID)  {
                                        var i;
                                        for(i=0;i<this.polylinesID.length;i++)
                                        {
                                            if(this.polylinesID[i]==ID)
                                            {
                                               // alert('polyline found : '+this.polylines[i].value);
                                                return this.polylines[i];
                                            }
                                        }
                                        return null;
                                      }
  this.removeValueById = function(ID) {
                                        var i;
                                        for(i=0;i<this.polylinesID.length;i++)
                                        {
                                            if(this.polylinesID[i]==ID)
                                            {
                                                this.polylines.splice(i,1);
                                                this.polylinesID.splice(i,1);
                                            }
                                        }
                                        return null;
                                      }
}

function fPolygons()
{
    this.polygons = new Array();
    this.polygonsID = new Array();
    this.getLength = function() { return this.polygons.length; };
    this.pushValue = function(v,ID) {  this.polygons.push(v); this.polygonsID.push(ID); }
    this.getValue = function(i)  { return this.polygons[i]; }
    this.getLastValue = function()  { return this.polygons[this.polygons.length-1]; }
    this.getValueById = function(ID)  {
                                        var i;
                                        for(i=0;i<this.polygonsID.length;i++)
                                        {
                                            if(this.polygonsID[i]==ID)
                                            {
                                                return this.polygons[i];
                                            }
                                        } 
                                        return null;
                                      }
    this.removeValueById = function(ID)  {
                                        var i;
                                        for(i=0;i<this.polygonsID.length;i++)
                                        {
                                            if(this.polygonsID[i]==ID)
                                            {
                                                this.polygons.splice(i,1);
                                                this.polygonsID.splice(i,1);
                                            }
                                        } 
                                        return null; 
                                      }
}

if (GBrowserIsCompatible())
{
map = new GMap2(document.getElementById("GoogleMap_Div"));

var markers = new fMarkers();
var polylines = new fPolylines();
var polygons = new fPolygons();
//var myEventListeners = new fListeners();
                
function CreateMarker(point,icon,InfoHTML)
{
    var marker;
    if(icon!=null)
    {
        marker = new GMarker(point,icon);
    }
    else
    {
        marker = new GMarker(point);
    }
    if(InfoHTML!='')
    {
       // vlisteners.pushValue(GEvent.addListener(marker, "click", function() { map.openInfoWindowHtml(point,InfoHTML); }));
        GEvent.addListener(marker, "click", function() { map.openInfoWindowHtml(point,InfoHTML); });
    }
    return marker;
}

function CreatePolyline(points,color,width,isgeodesic)
{
    var polyline;
    if(!isgeodesic)
    {
        polyline = new GPolyline(points,color,width);
    }
    else
    {
        var polyOptions = {geodesic:true};
        polyline = new GPolyline(points,color,width,1,polyOptions);
    }
    return polyline;
}

function CreatePolygon(points,strokecolor,strokeweight,strokeopacity,fillcolor,fillopacity)
{
    var polygon;
    
    var polygon = new GPolygon(points,strokecolor,strokeweight,strokeopacity,fillcolor,fillopacity);
    return polygon;
}

function fGetGoogleObject(result, userContext)
{
    map.setCenter(new GLatLng(result.CenterPoint.Latitude, result.CenterPoint.Longitude), result.ZoomLevel);
    
    if(result.ShowMapTypesControl)
    {
        map.addControl(new GMapTypeControl());
    }

    if(result.ShowZoomControl)
    {
        map.addControl(new GLargeMapControl());
    }
    

    
    var i;
    if(markers!=null)
    {
        for(i=0;i<markers.getLength();i++)
        {
            var cmark = markers.getValue(i);
            if(cmark !=null)
            {
                    map.removeOverlay(cmark);
            }
        }
    }
//    if(myEventListeners!=null)
//    {
//        for(i=0;i<myEventListeners.getLength();i++)
//        {
//            var lisnr = myEventListeners.getValue(i);
//            if(lisnr!=null)
//            {
//                GEvent.removeListener(lisnr);
//            }
//        }
//    }  
    markers = new fMarkers();
//    myEventListeners = new fListeners();

    for(i=0;i<result.Points.length;i++)
    {
        var myIcon_google;

        var myPoint = new GLatLng(result.Points[i].Latitude, result.Points[i].Longitude);
        
        myIcon_google = null;
        if(result.Points[i].IconImage!='')
        {
            myIcon_google = new GIcon(G_DEFAULT_ICON);
            markerOptions = { icon:myIcon_google };

            myIcon_google.iconSize = new GSize(result.Points[i].IconImageWidth,result.Points[i].IconImageHeight);
            myIcon_google.image = result.Points[i].IconImage;
        }
        //with listener  var marker = CreateMarker(myPoint,myIcon_google,result.Points[i].InfoHTML,myEventListeners);
        var marker = CreateMarker(myPoint,myIcon_google,result.Points[i].InfoHTML);
        marker.value = result.Points[i].ID;
        markers.pushValue(marker);
        map.addOverlay(markers.getLastValue());
    }
    //Add polylines
   // alert('adding polyline');

    polylines = new fPolylines();
    for(i=0;i<result.Polylines.length;i++)
    {
	 var polypoints = new Array();
	 var j;
	 for(j=0;j<result.Polylines[i].Points.length;j++)
 	 {
	 	polypoints.push(new GLatLng(result.Polylines[i].Points[j].Latitude, result.Polylines[i].Points[j].Longitude));
	 }
        var polyline = CreatePolyline(polypoints,result.Polylines[i].ColorCode,result.Polylines[i].Width,result.Polylines[i].Geodesic);
        polylines.pushValue(polyline,result.Polylines[i].ID);
        map.addOverlay(polylines.getLastValue());
    }
// var polypoints = new Array();
// polypoints.push(new GLatLng(43.65669, -79.44268));
// polypoints.push(new GLatLng(43.66619, -79.44268));
// var poly = CreatePolyline(polypoints,"#66FF00",10,true);
// map.addOverlay(poly);

// var polypoints = new Array();
// polypoints.push(new GLatLng(43.65669, -79.44268));
// polypoints.push(new GLatLng(43.66619, -79.44268));
// polypoints.push(new GLatLng(43.67619, -79.44268));
// var directions = new GDirections(map,document.getElementById("directions_canvas")); 
//Clear the mapa nd directions of any old information
//directions.clear();

//Load the map and directions from the specified waypoints
//directions.loadFromWaypoints(polypoints);


    polygons = new fPolygons();
    for(i=0;i<result.Polygons.length;i++)
    {
	 var polypoints = new Array();
	 var j;
	 for(j=0;j<result.Polygons[i].Points.length;j++)
 	 {
	 	polypoints.push(new GLatLng(result.Polygons[i].Points[j].Latitude, result.Polygons[i].Points[j].Longitude));
	 }
        var polygon = CreatePolygon(polypoints,result.Polygons[i].StrokeColor,result.Polygons[i].StrokeWeight,result.Polygons[i].StrokeOpacity,result.Polygons[i].FillColor,result.Polygons[i].FillOpacity);
        polygons.pushValue(polygon,result.Polygons[i].ID);
        map.addOverlay(polygons.getLastValue());
    }

    
    if(result.ShowTraffic)
    {
        trafficInfo = new GTrafficOverlay();
        map.addOverlay(trafficInfo);
    }

}
 GService.GetGoogleObject(fGetGoogleObject);
 
function fGetGoogleObjectOptimized(result, userContext)
{
    
    if(result.ShowTraffic)
    {
        trafficInfo = new GTrafficOverlay();
        map.addOverlay(trafficInfo);
    }
    else
    {
        if(trafficInfo!=null)
        {
            map.removeOverlay(trafficInfo);
            trafficInfo = null;
        }
    }

    var i;
    for(i=0;i<result.Points.length;i++)
    {
        //Create icon
        var myIcon_google;

        var myPoint = new GLatLng(result.Points[i].Latitude, result.Points[i].Longitude);
        
        myIcon_google = null;
        if(result.Points[i].IconImage!='')
        {
            myIcon_google = new GIcon(G_DEFAULT_ICON);
            markerOptions = { icon:myIcon_google };

            myIcon_google.iconSize = new GSize(result.Points[i].IconImageWidth,result.Points[i].IconImageHeight);
            myIcon_google.image = result.Points[i].IconImage;
        }
        //Existing marker, but changed.
        if(result.Points[i].PointStatus=='C')
        {
            var marker = markers.getValueById(result.Points[i].ID);
            if(marker!=null)
            {
                markers.removeValueById(result.Points[i].ID);
                map.removeOverlay(marker);
            }
            marker =  CreateMarker(myPoint,myIcon_google,result.Points[i].InfoHTML);
            marker.value = result.Points[i].ID;
            markers.pushValue(marker);
            map.addOverlay(markers.getLastValue());
        }
        //New Marker
        if(result.Points[i].PointStatus=='N')
        {
            var marker =  CreateMarker(myPoint,myIcon_google,result.Points[i].InfoHTML);
            marker.value = result.Points[i].ID;
            markers.pushValue(marker);
            map.addOverlay(markers.getLastValue());
        }
        //Existing marker, but deleted.
        if(result.Points[i].PointStatus=='D')
        {
            var marker = markers.getValueById(result.Points[i].ID);
            if(marker!=null)
            {
                markers.removeValueById(result.Points[i].ID);
                map.removeOverlay(marker);
            }
        }
    }
    
    //Get Polylines
    for(i=0;i<result.Polylines.length;i++)
    {
        //Existing marker, but changed.
        
        if(result.Polylines[i].LineStatus=='C')
        {
        
            var polyline = polylines.getValueById(result.Polylines[i].ID);
            if(polyline!=null)
            {
                polylines.removeValueById(result.Polylines[i].ID);
                map.removeOverlay(polyline);
            }
	        var polypoints = new Array();
	        var j;
	        for(j=0;j<result.Polylines[i].Points.length;j++)
 	        {
	 	        polypoints.push(new GLatLng(result.Polylines[i].Points[j].Latitude, result.Polylines[i].Points[j].Longitude));
	        }
            var polyline = CreatePolyline(polypoints,result.Polylines[i].ColorCode,result.Polylines[i].Width,result.Polylines[i].Geodesic);
            polylines.pushValue(polyline,result.Polylines[i].ID);
            map.addOverlay(polylines.getLastValue());
        }
        //New Marker

        if(result.Polylines[i].LineStatus=='N')
        {
	        var polypoints = new Array();
	        var j;
	        for(j=0;j<result.Polylines[i].Points.length;j++)
 	        {
	 	        polypoints.push(new GLatLng(result.Polylines[i].Points[j].Latitude, result.Polylines[i].Points[j].Longitude));
	        }
            var polyline = CreatePolyline(polypoints,result.Polylines[i].ColorCode,result.Polylines[i].Width,result.Polylines[i].Geodesic);
            polylines.pushValue(polyline,result.Polylines[i].ID);
            map.addOverlay(polylines.getLastValue());
        }
        //Existing marker, but deleted.
        if(result.Polylines[i].LineStatus=='D')
        {
            var polyline = polylines.getValueById(result.Polylines[i].ID);
            if(polyline!=null)
            {
                polylines.removeValueById(result.Polylines[i].ID);
                map.removeOverlay(polyline);
            }
        }
    }
    
        //Get Polygons
    for(i=0;i<result.Polygons.length;i++)
    {
        //Existing marker, but changed.

        if(result.Polygons[i].Status=='C')
        {
        
            var polygon = polygons.getValueById(result.Polygons[i].ID);
            if(polygon!=null)
            {
                polygons.removeValueById(result.Polygons[i].ID);
                map.removeOverlay(polygon);
            }
	        var polypoints = new Array();
	        var j;
	        for(j=0;j<result.Polygons[i].Points.length;j++)
 	        {
	 	        polypoints.push(new GLatLng(result.Polygons[i].Points[j].Latitude, result.Polygons[i].Points[j].Longitude));
	        }
            var polygon = CreatePolygon(polypoints,result.Polygons[i].StrokeColor,result.Polygons[i].StrokeWeight,result.Polygons[i].StrokeOpacity,result.Polygons[i].FillColor,result.Polygons[i].FillOpacity);
            polygons.pushValue(polygon,result.Polygons[i].ID);
            map.addOverlay(polygons.getLastValue());
        }
        //New Marker

        if(result.Polygons[i].Status=='N')
        {
	        var polypoints = new Array();
	        var j;
	        for(j=0;j<result.Polygons[i].Points.length;j++)
 	        {
	 	        polypoints.push(new GLatLng(result.Polygons[i].Points[j].Latitude, result.Polygons[i].Points[j].Longitude));
	        }
            var polygon = CreatePolygon(polypoints,result.Polygons[i].StrokeColor,result.Polygons[i].StrokeWeight,result.Polygons[i].StrokeOpacity,result.Polygons[i].FillColor,result.Polygons[i].FillOpacity);
            polygons.pushValue(polygon,result.Polygons[i].ID);
            map.addOverlay(polygons.getLastValue());
        }
        //Existing marker, but deleted.
        if(result.Polygons[i].Status=='D')
        {
            var polygon = polygons.getValueById(result.Polygons[i].ID);
            if(polygon!=null)
            {
                polygons.removeValueById(result.Polygons[i].ID);
                map.removeOverlay(polygon);
            }
        }
    }
}

}
function endRequestHandler(sender, args)
    {
    //alert('hi');
        GService.GetOptimizedGoogleObject(fGetGoogleObjectOptimized);
    }
   function pageLoad()
    {
        if(!Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack())
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequestHandler);
    }    
</script>


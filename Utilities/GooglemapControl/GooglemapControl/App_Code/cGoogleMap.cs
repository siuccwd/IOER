//   Google Maps User Control for ASP.Net version 1.0:
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

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Drawing;

/// <summary>
/// Summary description for cGoogleMap
/// </summary>
/// 

public class GoogleObject
{
    public GoogleObject()
    {
    }

    public GoogleObject(GoogleObject prev)
    {
        Points = GooglePoints.CloneMe(prev.Points);
        Polylines = GooglePolylines.CloneMe(prev.Polylines);
        Polygons = GooglePolygons.CloneMe(prev.Polygons);
        ZoomLevel = prev.ZoomLevel;
        ShowZoomControl = prev.ShowZoomControl;
        ShowMapTypesControl = prev.ShowMapTypesControl;
        Width = prev.Width;
        Height = prev.Height;
        MapType = prev.MapType;
        APIKey = prev.APIKey;
        ShowTraffic = prev.ShowTraffic;
    }

    GooglePoints _gpoints = new GooglePoints();
    public GooglePoints Points
    {
        get { return _gpoints; }
        set { _gpoints = value; }
    }

    GooglePolylines _gpolylines = new GooglePolylines();
    public GooglePolylines Polylines
    {
        get { return _gpolylines; }
        set { _gpolylines = value; }
    }

    GooglePolygons _gpolygons = new GooglePolygons();
    public GooglePolygons Polygons
    {
        get { return _gpolygons; }
        set { _gpolygons = value; }
    }

    GooglePoint _centerpoint = new GooglePoint();
    public GooglePoint CenterPoint
    {
        get { return _centerpoint; }
        set { _centerpoint = value; }
    }

    int _zoomlevel = 3;
    public int ZoomLevel
    {
        get { return _zoomlevel; }
        set { _zoomlevel = value; }
    }

    bool _showzoomcontrol = true;
    public bool ShowZoomControl
    {
        get { return _showzoomcontrol; }
        set { _showzoomcontrol = value; }
    }

    bool _showtraffic = false;
    public bool ShowTraffic
    {
        get { return _showtraffic; }
        set { _showtraffic = value; }
    }

    bool _showmaptypescontrol = true;
    public bool ShowMapTypesControl
    {
        get { return _showmaptypescontrol; }
        set { _showmaptypescontrol = value; }
    }

    string _width = "500px";
    public string Width
    {
        get
        {
            return _width;
        }
        set
        {
            _width = value;
        }
    }

    string _height = "400px";
    public string Height
    {
        get
        {
            return _height;
        }
        set
        {
            _height = value;
        }
    }


    string _maptype = "";
    public string MapType
    {
        get
        {
            return _maptype;
        }
        set
        {
            _maptype = value;
        }
    }

    string _apikey = "";
    public string APIKey
    {
        get
        {
            return _apikey;
        }
        set
        {
            _apikey = value;
        }
    }

    string _apiversion = "2";
    public string APIVersion
    {
        get
        {
            return _apiversion;
        }
        set
        {
            _apiversion = value;
        }
    }

}

public class GooglePoint
{
    public GooglePoint()
    {
    }

    string _pointstatus = ""; //N-New, D-Deleted, C-Changed, ''-No Action
    public string PointStatus
    {
        get { return _pointstatus; }
        set { _pointstatus = value; }
    }

    public GooglePoint(string pID,double plat, double plon, string picon, string pinfohtml)
    {
        ID = pID;
        Latitude = plat;
        Longitude = plon;
        IconImage = picon;
        InfoHTML = pinfohtml;
    }

    public GooglePoint(string pID, double plat, double plon, string picon)
    {
        ID = pID;
        Latitude = plat;
        Longitude = plon;
        IconImage = picon;
    }

    public GooglePoint(string pID, double plat, double plon)
    {
        ID = pID;
        Latitude = plat;
        Longitude = plon;
    }

    string _id = "";
    public string ID
    {
        get
        {
            return _id;
        }
        set
        {
            _id = value;
        }
    }

    string _icon = "";
    public string IconImage
    {
        get
        {
            return _icon;
        }
        set
        {
            
            //Get physical path of icon image. Necessary for Bitmap object.
            string sIconImage = value;
            if (sIconImage == "")
                return;
            string ImageIconPhysicalPath = cCommon.GetLocalPath() + sIconImage.Replace("/", "\\");
            //Find width and height of icon using Bitmap image.


            using (System.Drawing.Image img = System.Drawing.Image.FromFile(ImageIconPhysicalPath))
            {
                IconImageWidth = img.Width;
                IconImageHeight = img.Height;
            }
            _icon = cCommon.GetHttpURL() + sIconImage;


            _icon = value;
        }
    }

    int _iconimagewidth = 32;
    public int IconImageWidth
    {
        get
        {
            return _iconimagewidth;
        }
        set
        {
            _iconimagewidth = value;
        }
    }

    int _iconimageheight = 32;
    public int IconImageHeight
    {
        get
        {
            return _iconimageheight;
        }
        set
        {
            _iconimageheight = value;
        }
    }

    double _lat = 0.0;
    public double Latitude
    {
        get
        {
            return _lat;
        }
        set
        {
            _lat = value;
        }
    }

    double _lon = 0.0;
    public double Longitude
    {
        get
        {
            return _lon;
        }
        set
        {
            _lon = value;
        }
    }

    string _infohtml = "";
    public string InfoHTML
    {
        get
        {
            return _infohtml;
        }
        set
        {
            _infohtml = value;
        }
    }
    public override bool Equals(System.Object obj)
    {
        // If parameter is null return false.
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        GooglePoint p = obj as GooglePoint;
        if ((System.Object)p == null)
        {
            return false;
        }

        // Return true if the fields match:
        return (InfoHTML == p.InfoHTML) && (IconImage == p.IconImage) && (p.ID==ID) && (p.Latitude==Latitude) && (p.Longitude==Longitude);
    }
}

public class GooglePoints : CollectionBase
{

    public GooglePoints()
    {
    }

    public static GooglePoints CloneMe(GooglePoints prev)
    {
        GooglePoints p = new GooglePoints();
        for (int i = 0; i < prev.Count; i++)
        {
            p.Add(new GooglePoint(prev[i].ID, prev[i].Latitude, prev[i].Longitude, prev[i].IconImage, prev[i].InfoHTML));
        }
        return p;
    }
 

    public GooglePoint this[int pIndex]
    {
        get
        {
            return (GooglePoint)this.List[pIndex];
        }
        set
        {
            this.List[pIndex] = value;
        }
    }

    public GooglePoint this[string pID]
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].ID == pID)
                {
                    return (GooglePoint)this.List[i];
                }
            }
            return null;
        }
        set
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].ID == pID)
                {
                    this.List[i] = value;
                }
            }
        }

    }

    public void Add(GooglePoint pPoint)
    {
        this.List.Add(pPoint);
    }
    public void Remove(int pIndex)
    {
        this.RemoveAt(pIndex);
    }
    public void Remove(string pID)
    {
        for (int i = 0; i < Count; i++)
        {
            if (this[i].ID == pID)
            {
                this.List.RemoveAt(i);
                return;
            }
        }
    }
    public void RemoveAll()
    {
        this.List.Clear();
    }
    public override bool Equals(System.Object obj)
    {
        // If parameter is null return false.
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        GooglePoints p = obj as GooglePoints;
        if ((System.Object)p == null)
        {
            return false;
        }

        if(p.Count!=Count)
            return false;


        for(int i=0;i<p.Count;i++)
        {
            if(!this[i].Equals(p[i]))
                return false;
        }
        // Return true if the fields match:
        return true;
    }
}

public class GooglePolyline
{
    string _linestatus = ""; //N-New, D-Deleted, C-Changed, ''-No Action
    public string LineStatus
    {
        get { return _linestatus; }
        set { _linestatus = value; }
    }

    string _id = "";
    public string ID
    {
        get { return _id; }
        set { _id = value; } 
    }

    GooglePoints _gpoints=new GooglePoints();
    public GooglePoints Points
    {
        get { return _gpoints; }
        set { _gpoints = value; }
    }

    string _colorcode = "#66FF00";
    public string ColorCode
    {
        get { return _colorcode; }
        set { _colorcode = value; }
    }

    int _width = 10;
    public int Width
    {
        get { return _width; }
        set { _width = value; }
    }

    bool _geodesic = false;
    public bool Geodesic
    {
        get { return _geodesic; }
        set { _geodesic = value; }
    }

    public override bool Equals(System.Object obj)
    {
        // If parameter is null return false.
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        GooglePolyline p = obj as GooglePolyline;
        if ((System.Object)p == null)
        {
            return false;
        }

        // Return true if the fields match:
        return (Geodesic == p.Geodesic) && (Width == p.Width) && (p.ID == ID) && (p.ColorCode == ColorCode) && (p.Points.Equals(Points));
    }

}

public class GooglePolylines : CollectionBase
{

    public GooglePolylines()
    {
    }

    public static GooglePolylines CloneMe(GooglePolylines prev)
    {
        GooglePolylines p = new GooglePolylines();
        for (int i = 0; i < prev.Count; i++)
        {
            GooglePolyline GPL = new GooglePolyline();
            GPL.ColorCode = prev[i].ColorCode;
            GPL.Geodesic = prev[i].Geodesic;
            GPL.ID = prev[i].ID;
            GPL.Points = GooglePoints.CloneMe(prev[i].Points);
            GPL.Width = prev[i].Width;
            p.Add(GPL);
        }
        return p;
    }

    public GooglePolyline this[int pIndex]
    {
        get
        {
            return (GooglePolyline)this.List[pIndex];
        }
        set
        {
            this.List[pIndex] = value;
        }
    }

    public GooglePolyline this[string pID]
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].ID == pID)
                {
                    return (GooglePolyline)this.List[i];
                }
            }
            return null;
        }
        set
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].ID == pID)
                {
                    this.List[i] = value;
                }
            }
        }
    }

    public void Add(GooglePolyline pPolyline)
    {
        this.List.Add(pPolyline);
    }
    public void Remove(int pIndex)
    {
        this.RemoveAt(pIndex);
    }
    public void Remove(string pID)
    {
        for (int i = 0; i < Count; i++)
        {
            if (this[i].ID == pID)
            {
                this.List.RemoveAt(i);
                return;
            }
        }
    }

}


public sealed class GoogleMapType
{
    public const string NORMAL_MAP = "G_NORMAL_MAP";
    public const string SATELLITE_MAP = "G_SATELLITE_MAP";
    public const string HYBRID_MAP = "G_HYBRID_MAP";
}

public class cCommon
{
    public cCommon()
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public static Random random = new Random();

    public static string GetHttpURL()
    {
        string[] s = System.Web.HttpContext.Current.Request.Url.AbsoluteUri.Split(new char[] { '/' });
        string path = s[0] + "/";
        for (int i = 1; i < s.Length - 1; i++)
        {
            path = path + s[i] + "/";
        }
        return path;
    }

    public static string GetLocalPath()
    {
        string[] s = System.Web.HttpContext.Current.Request.Url.AbsoluteUri.Split(new char[] { '/' });
        string PageName = s[s.Length - 1];
        s = System.Web.HttpContext.Current.Request.MapPath(PageName).Split(new char[] { '\\' });
        string path = s[0] + "\\";
        for (int i = 1; i < s.Length - 1; i++)
        {
            path = path + s[i] + "\\";
        }
        return path;
    }

    public static decimal RandomNumber(decimal min, decimal max)
    {
        decimal Fractions = 10000000;
        int iMin = (int)GetIntegerPart(min * Fractions);
        int iMax = (int)GetIntegerPart(max * Fractions);
        int iRand = random.Next(iMin, iMax);

        decimal dRand = (decimal)iRand;
        dRand = dRand / Fractions;

        return dRand;
    }


    public static decimal GetFractional(decimal source)
    {
        return source % 1.0m;
    }

    public static decimal GetIntegerPart(decimal source)
    {
        return decimal.Parse(source.ToString("#.00"));
    }

}

public class GooglePolygon
{
    string _status = ""; //N-New, D-Deleted, C-Changed, ''-No Action
    public string Status
    {
        get { return _status; }
        set { _status = value; }
    }

    string _id = "";
    public string ID
    {
        get { return _id; }
        set { _id = value; }
    }

    GooglePoints _gpoints = new GooglePoints();
    public GooglePoints Points
    {
        get { return _gpoints; }
        set { _gpoints = value; }
    }

    string _strokecolor = "#0000FF";
    public string StrokeColor
    {
        get { return _strokecolor; }
        set { _strokecolor = value; }
    }

    string _fillcolor = "#66FF00";
    public string FillColor
    {
        get { return _fillcolor; }
        set { _fillcolor = value; }
    }

    int _strokeweight = 10;
    public int StrokeWeight
    {
        get { return _strokeweight; }
        set { _strokeweight = value; }
    }

    double _strokeopacity = 1;
    public double StrokeOpacity
    {
        get { return _strokeopacity; }
        set { _strokeopacity = value; }
    }

    double _fillopacity = 0.2;
    public double FillOpacity
    {
        get { return _fillopacity; }
        set { _fillopacity = value; }
    }

    public override bool Equals(System.Object obj)
    {
        // If parameter is null return false.
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        GooglePolygon p = obj as GooglePolygon;
        if ((System.Object)p == null)
        {
            return false;
        }

        // Return true if the fields match:
        return (FillColor == p.FillColor) && (FillOpacity == p.FillOpacity) && (p.ID == ID) && (p.Status == Status) && (p.StrokeColor == StrokeColor) && (p.StrokeOpacity == StrokeOpacity) && (p.StrokeWeight == StrokeWeight) && (p.Points.Equals(Points));
    }

}

public class GooglePolygons : CollectionBase
{

    public GooglePolygons()
    {
    }

    public static GooglePolygons CloneMe(GooglePolygons prev)
    {
        GooglePolygons p = new GooglePolygons();
        for (int i = 0; i < prev.Count; i++)
        {
            GooglePolygon GPL = new GooglePolygon();
            GPL.FillColor = prev[i].FillColor;
            GPL.FillOpacity = prev[i].FillOpacity;
            GPL.ID = prev[i].ID;
            GPL.Status = prev[i].Status;
            GPL.StrokeColor = prev[i].StrokeColor;
            GPL.StrokeOpacity = prev[i].StrokeOpacity;
            GPL.StrokeWeight = prev[i].StrokeWeight;
            GPL.Points = GooglePoints.CloneMe(prev[i].Points);
            p.Add(GPL);
        }
        return p;
    }

    public GooglePolygon this[int pIndex]
    {
        get
        {
            return (GooglePolygon)this.List[pIndex];
        }
        set
        {
            this.List[pIndex] = value;
        }
    }

    public GooglePolygon this[string pID]
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].ID == pID)
                {
                    return (GooglePolygon)this.List[i];
                }
            }
            return null;
        }
        set
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].ID == pID)
                {
                    this.List[i] = value;
                }
            }
        }
    }

    public void Add(GooglePolygon pPolygon)
    {
        this.List.Add(pPolygon);
    }
    public void Remove(int pIndex)
    {
        this.RemoveAt(pIndex);
    }
    public void Remove(string pID)
    {
        for (int i = 0; i < Count; i++)
        {
            if (this[i].ID == pID)
            {
                this.List.RemoveAt(i);
                return;
            }
        }
    }

}

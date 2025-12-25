using CoordinateSharp;

namespace Base.Common.Gis;


public static class LatLonConversions
{
    public static (double? lat, double? lng) ToCoordinate(double? X, double? Y, int Zone)
    {
        try
        {
            UniversalTransverseMercator utm = new UniversalTransverseMercator("N", Zone, X.Value, Y.Value);
            Coordinate c = UniversalTransverseMercator.ConvertUTMtoLatLong(utm);
            var lat = c.Latitude.DecimalDegree;
            var lng = c.Longitude.DecimalDegree;
            return (lat, lng);
        }
        catch (System.Exception)
        {
            return (null, null);
        }

    }
}


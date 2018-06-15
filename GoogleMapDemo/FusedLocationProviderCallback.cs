using System;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Util;
using Android.Widget;

namespace GoogleMapDemo
{   
    public class FusedLocationProviderCallback : LocationCallback
    {
        readonly MainActivity activity;

        public FusedLocationProviderCallback(MainActivity activity)
        {
            this.activity = activity;
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Log.Debug("FusedLocationProviderSample","IsLocationAvailable: {0}",locationAvailability.IsLocationAvailable);

        }

        public override void OnLocationResult(LocationResult result)
        {
            if(result.Locations != null)
            {
                var location = result.Locations[0];
                activity.latitude2.Text = activity.Resources.GetString(Resource.String.latitude_string, location.Latitude);
                activity.longitude2.Text = activity.Resources.GetString(Resource.String.longitude_string, location.Longitude);
                activity.provider2.Text = activity.Resources.GetString(Resource.String.requesting_updates_provider_string, location.Provider);

                UpdateLocation(location);

                Log.Debug("FusedLocationProviderSample", location.Latitude + " , " + location.Longitude);
            }
            else
            {
                activity.latitude2.SetText(Resource.String.location_unavailable);
                activity.longitude2.SetText(Resource.String.location_unavailable);
                activity.provider2.SetText(Resource.String.could_not_get_last_location);
                activity.requestLocationUpdatesButton.SetText(Resource.String.request_location_button_text);
            }

        }

        private void UpdateLocation(Location location)
        {
            if (MainActivity.Instance.Map != null)
            {
                //MarkerOptions markerOpt1 = new MarkerOptions();
                //markerOpt1.SetPosition(new LatLng(location.Latitude, location.Longitude));
                //markerOpt1.SetTitle("ICM Inc");

                CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
                builder.Target(new LatLng(location.Latitude, location.Longitude));
                builder.Zoom(18);
                builder.Bearing(155);

                CameraPosition cameraPosition = builder.Build();
                CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

                MainActivity.Instance.Map.MoveCamera(cameraUpdate);

                //_map.MoveCamera(cameraUpdate);
            }
        }
    }
}

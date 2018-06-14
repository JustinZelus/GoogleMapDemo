
using Android.OS;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.Util;
using System.Threading.Tasks;
using Java.Lang;
using System;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Support.V4.View;
using Android.Widget;
using Android.Gms.Maps;
using Android.Locations;
using Android.Gms.Maps.Model;


namespace GoogleMapDemo
{
    [Activity(Label = "GoogleMapDemo", MainLauncher = true, Icon = "@mipmap/icon",ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity,IOnMapReadyCallback
    {
        //UI
        private DrawerLayout mDrawerLayout;

        const long ONE_MINUTE = 60 * 1000;
        int count = 1;
        bool isRequestingLocationUpdates = false;
        bool isGooglePlayServicesInstalled = false;
        FusedLocationProviderClient fusedLocationProviderClient;

        LocationRequest locationRequest;

        Button getLastLocationButton;
        internal Button requestLocationUpdatesButton;
        TextView longitude;
        TextView latitude;
        TextView provider;
        internal TextView longitude2;
        internal TextView latitude2;
        internal TextView provider2;
        LocationCallback locationCallback;

        public static MainActivity Instance;


        private void init()
        {
            //runtime permission check
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                //StartRequestingLocationUpdates();
                isRequestingLocationUpdates = true;
            }
            else
            {

            }
            isGooglePlayServicesInstalled = IsGooglePlayServicesInstalled();

            // UI to display last location
            getLastLocationButton = FindViewById<Button>(Resource.Id.get_last_location_button);
            latitude = FindViewById<TextView>(Resource.Id.latitude);
            longitude = FindViewById<TextView>(Resource.Id.longitude);
            provider = FindViewById<TextView>(Resource.Id.provider);

            // UI to display location updates
            requestLocationUpdatesButton = FindViewById<Button>(Resource.Id.request_location_updates_button);
            latitude2 = FindViewById<TextView>(Resource.Id.latitude2);
            longitude2 = FindViewById<TextView>(Resource.Id.longitude2);
            provider2 = FindViewById<TextView>(Resource.Id.provider2);



            if (isGooglePlayServicesInstalled)
            {
                locationRequest = new LocationRequest()
                    .SetPriority(LocationRequest.PriorityHighAccuracy)
                    .SetInterval(ONE_MINUTE / 10)
                    .SetFastestInterval(ONE_MINUTE / 30);

                locationCallback = new FusedLocationProviderCallback(this);

                fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
                getLastLocationButton.Click += GetLastLocationButtonOnClick;
                requestLocationUpdatesButton.Click += RequestLocationUpdatesButtonOnClick;
            }
            else
            {

            }
        }




        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Android.Resource.Id.Home:
                    mDrawerLayout.OpenDrawer(GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            Instance = this;

            //UI
            //Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            //SetSupportActionBar(toolbar);
            //toolbar.SetTitle(Resource.String.toolbar_home);
            //Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            //actionBar.SetDisplayHomeAsUpEnabled(true);
            //actionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);

            //mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            //NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            //navigationView.NavigationItemSelected += (object sender, NavigationView.NavigationItemSelectedEventArgs e) => {
            //    // set item as selected to persist highlight
            //    e.MenuItem.SetChecked(true);
            //    mDrawerLayout.CloseDrawer((int)GravityFlags.Left,true);

            //};

            init();
            _mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;
            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeNormal)
                    .InvokeZoomControlsEnabled(false)
                    .InvokeCompassEnabled(true);

                FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                _mapFragment = MapFragment.NewInstance(mapOptions);
                fragTx.Add(Resource.Id.map, _mapFragment, "map");
                fragTx.Commit();
            }
            _mapFragment.GetMapAsync(this);
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _map = googleMap;
          
        }

        public GoogleMap _map;
        MapFragment _mapFragment;

        //protected override void OnResume()
        protected override async void OnResume()
        {
            base.OnResume();

            await StartRequestingLocationUpdates();
            await GetLastLocationFromDevice();

            LatLng location = new LatLng(fusedLocation.Latitude, fusedLocation.Longitude);
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(location);
            builder.Zoom(18);
            builder.Bearing(155);
            //builder.Tilt(65);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);


            if (_map != null)
            {
                Log.Debug("Map", "map is ready now !");
                //_map.MapType = GoogleMap.MapTypeNormal;
                MarkerOptions markerOpt1 = new MarkerOptions();
                markerOpt1.SetPosition(new LatLng(fusedLocation.Latitude,fusedLocation.Longitude));
                markerOpt1.SetTitle("ICM Inc");


                _map.UiSettings.ZoomControlsEnabled = true;
                _map.UiSettings.CompassEnabled = true;
                _map.AnimateCamera(cameraUpdate);
                _map.MyLocationEnabled = true;
                _map.AddMarker(markerOpt1);
                //_map.MoveCamera(cameraUpdate);
            }
        }

        async Task StartRequestingLocationUpdates()
        {
            requestLocationUpdatesButton.SetText(Resource.String.request_location_in_progress_button_text);
            await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);
        }

        async void StopRequestionLocationUpdates()
        {
            latitude2.Text = string.Empty;
            longitude2.Text = string.Empty;
            provider2.Text = string.Empty;

            requestLocationUpdatesButton.SetText(Resource.String.request_location_button_text);

            if (isRequestingLocationUpdates)
            {
                await fusedLocationProviderClient.RemoveLocationUpdatesAsync(locationCallback);
            }

        }

        async void RequestLocationUpdatesButtonOnClick(object sender, EventArgs e)
        {
            // No need to request location updates if we're already doing so.
            if (isRequestingLocationUpdates)
            {
                StopRequestionLocationUpdates();
                isRequestingLocationUpdates = false;
            }
            else
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
                {
                    await StartRequestingLocationUpdates();
                    isRequestingLocationUpdates = true;
                }
                else
                {

                }
            }
        }

        async void GetLastLocationButtonOnClick(object sender, EventArgs e)
        {
            if(ContextCompat.CheckSelfPermission(this,Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                await GetLastLocationFromDevice();
            }
            else
            {
                
            }
        }

        Location fusedLocation;

        async Task GetLastLocationFromDevice()
        {
            getLastLocationButton.SetText(Resource.String.getting_last_location);
            fusedLocation = await fusedLocationProviderClient.GetLastLocationAsync();

            if(fusedLocation == null)
            {
                latitude.SetText(Resource.String.location_unavailable);
                longitude.SetText(Resource.String.location_unavailable);
                provider.SetText(Resource.String.could_not_get_last_location);
            }
            else
            {
                latitude.Text = Resources.GetString(Resource.String.latitude_string, fusedLocation.Latitude);
                longitude.Text = Resources.GetString(Resource.String.longitude_string, fusedLocation.Longitude);
                provider.Text = Resources.GetString(Resource.String.provider_string, fusedLocation.Provider);
                getLastLocationButton.SetText(Resource.String.get_last_location_button_text);
            }
        }

        bool IsGooglePlayServicesInstalled()
        {
            var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if(queryResult == ConnectionResult.Success)
            {
                Log.Info("MainActivity","Google Play Services is installed on this device.");
                return true;
            }

            if(GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                // Check if there is a way the user can resolve the issue
                var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error("MainActivity","There is a problem with Google Play Services on this device: {0} - {1}",
                          queryResult,errorString);

                // Alternately, display the error to the user.
            }
            return false;
        }

       
    }
}


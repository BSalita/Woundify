using System;

#if WINDOWS_UWP
using Windows.UI.Core;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
#endif

namespace WoundifyShared
{
    class GeoLocation
    {
        public static double longitude = Options.options.geolocaton.longitude;
        public static double latitude = Options.options.geolocaton.latitude;
        public static int desiredAccuracyInMeters = Options.options.geolocaton.accuracyInMeters;
        public static string town = Options.options.geolocaton.town;
        public static string region = Options.options.geolocaton.region;
        public static string regionCode = Options.options.geolocaton.regionCode;
        public static string country = Options.options.geolocaton.country;
        public static string countryCode = Options.options.geolocaton.countryCode;

#if WINDOWS_UWP
        private static Geoposition geoPosition = null;

        static GeoLocation() // note static constructor - always instantiated upon startup
        {
            GetGeoLocationAsync(); // todo: safer if awaited
        }

 
        private static async System.Threading.Tasks.Task reverseGeocodeAsync()
        {
            // The location to reverse geocode.
            BasicGeoposition location = new BasicGeoposition();
            location.Latitude = latitude;
            location.Longitude = longitude;
            Geopoint pointToReverseGeocode = new Geopoint(location);

            // Reverse geocode the specified geographic location.
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAtAsync(pointToReverseGeocode);

            // If the query returns results, display the name of the town
            // contained in the address of the first result.
            if (result.Status == MapLocationFinderStatus.Success)
            {
                Log.WriteLine("Town:" + result.Locations[0].Address.Town);
                Log.WriteLine("Region:" + result.Locations[0].Address.Region);
                Log.WriteLine("RegionCode:" + result.Locations[0].Address.Region);
                Log.WriteLine("Country:" + result.Locations[0].Address.Country);
                Log.WriteLine("CountryCode:" + result.Locations[0].Address.CountryCode);
                town = result.Locations[0].Address.Town;
                region = result.Locations[0].Address.Region;
                regionCode = result.Locations[0].Address.RegionCode;
                country = result.Locations[0].Address.Country;
                countryCode = result.Locations[0].Address.CountryCode;
            }
        }

        private static async System.Threading.Tasks.Task UpdateLocationDataAsync(Geoposition pos)
        {
            geoPosition = pos;
            if (pos == null)
            {
                Log.WriteLine("Geolocation updated: using defaults.");
                longitude = Options.options.geolocaton.longitude;
                latitude = Options.options.geolocaton.latitude;
            }
            else
            {
                Log.WriteLine("Geolocation updated: longitude:" + pos.Coordinate.Longitude + " latitude:" + pos.Coordinate.Latitude);
                longitude = pos.Coordinate.Longitude;
                latitude = pos.Coordinate.Latitude;
            }
            await reverseGeocodeAsync();
        }

        private static async void OnPositionChangedAsync(Geolocator sender, PositionChangedEventArgs e)
        {
            await UpdateLocationDataAsync(e.Position);
        }

        private static async void OnStatusChangedAsync(Geolocator sender, StatusChangedEventArgs e)
        {
#if false // status change not implemented
            // Show the location setting message only if status is disabled.
            LocationDisabledMessage.Visibility = Visibility.Collapsed;

            switch (e.Status)
            {
                case PositionStatus.Ready:
                    // Location platform is providing valid data.
                    ScenarioOutput_Status.Text = "Ready";
                    _rootPage.NotifyUser("Location platform is ready.", NotifyType.StatusMessage);
                    break;

                case PositionStatus.Initializing:
                    // Location platform is attempting to acquire a fix. 
                    ScenarioOutput_Status.Text = "Initializing";
                    _rootPage.NotifyUser("Location platform is attempting to obtain a position.", NotifyType.StatusMessage);
                    break;

                case PositionStatus.NoData:
                    // Location platform could not obtain location data.
                    ScenarioOutput_Status.Text = "No data";
                    _rootPage.NotifyUser("Not able to determine the location.", NotifyType.ErrorMessage);
                    break;

                case PositionStatus.Disabled:
                    // The permission to access location data is denied by the user or other policies.
                    ScenarioOutput_Status.Text = "Disabled";
                    _rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);

                    // Show message to the user to go to location settings.
                    LocationDisabledMessage.Visibility = Visibility.Visible;

                    // Clear any cached location data.
                    UpdateLocationData(null);
                    break;

                case PositionStatus.NotInitialized:
                    // The location platform is not initialized. This indicates that the application 
                    // has not made a request for location data.
                    ScenarioOutput_Status.Text = "Not initialized";
                    _rootPage.NotifyUser("No request for location is made yet.", NotifyType.StatusMessage);
                    break;

                case PositionStatus.NotAvailable:
                    // The location platform is not available on this version of the OS.
                    ScenarioOutput_Status.Text = "Not available";
                    _rootPage.NotifyUser("Location is not available on this version of the OS.", NotifyType.ErrorMessage);
                    break;

                default:
                    ScenarioOutput_Status.Text = "Unknown";
                    _rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                    break;
            }
#endif
        }

        private static async System.Threading.Tasks.Task GetGeoLocationAsync()
        {
            GeolocationAccessStatus accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    Log.WriteLine("Getting Geolocation data");

                    Geolocator _geolocator = new Geolocator { DesiredAccuracyInMeters = (uint)desiredAccuracyInMeters };

                    // Subscribe to the StatusChanged event to get updates of location status changes.
#if false
                    // todo: disable updating due to very frequenty updating (per second).
                    _geolocator.PositionChanged += OnPositionChanged;
#endif
                    _geolocator.StatusChanged += OnStatusChangedAsync;

                    // Carry out the operation.
                    Geoposition pos = await _geolocator.GetGeopositionAsync();
                    longitude = pos.Coordinate.Longitude;
                    latitude = pos.Coordinate.Latitude;

                    await UpdateLocationDataAsync(pos);
                    break;

                case GeolocationAccessStatus.Denied:
                    Log.WriteLine("Geolocation: Access to location is denied.");
                    //LocationDisabledMessage.Visibility = Visibility.Visible;
                    await UpdateLocationDataAsync(null);
                    break;

                case GeolocationAccessStatus.Unspecified:
                default:
                    Log.WriteLine("Unspecified error.");
                    await UpdateLocationDataAsync(null);
                    break;
            }

        }
#else
        static GeoLocation()
        {
            // todo: not implemented in non-Windows 10. Need to call web service?
        }
#endif
    }
}

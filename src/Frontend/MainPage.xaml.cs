namespace Frontend;

using System;
using System.IO;

using Microsoft.Maui.Devices.Sensors;

#if ANDROID || IOS
using Shiny.Push;
#endif

public partial class MainPage : ContentPage
{
    FileInfo nonEventsFile = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "nonEvents.json"));
    FileInfo nonEventsFileBackup = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "nonEvents.json.bak"));

    Location location = null;

#if ANDROID || IOS
    IPushManager pushManager;
#endif

    public MainPage()
    {
        InitializeComponent();
        //Application.Current.RequestedThemeChanged += (s, a) => {
        //    Application.Current.UserAppTheme = Application.Current.RequestedTheme;
        //    this.DisplayAlert("Alert", Application.Current.RequestedTheme.ToString(), "cancel");
        //    var currentPage = Application.Current.MainPage;
        //    Application.Current.MainPage = null;
        //    Application.Current.MainPage = currentPage;
        //    //var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        //    //activity.FinishAffinity();
        //    //activity.StartActivity(activity.Intent);
        //};
        MainThread.BeginInvokeOnMainThread(Setup);
    }

    async void Setup()
    {
        if (!await CheckIfLocationPermissionGranted())
            return;

        var _location = await GatherLocation();
    }

    protected override void OnAppearing()
    {
        Task.Run(UpdateCount);
        base.OnAppearing();
    }

    private void UpdateCount()
    {
        lock (nonEventsFile)
        {
            var nonEvents = LoadNonEvents();

            lock (App.EventsFile)
            {
                var events = App.LoadEvents();
                var action = String.Empty;
                if (!(events.Length == 0 && nonEvents.Length == 0))
                {
                    string percentage =
                        (((decimal)events.Length / ((decimal)events.Length + (decimal)nonEvents.Length)) * 100m).ToString("0.##");
                    action = $" Action: {percentage}%";
                }
                MainThread.BeginInvokeOnMainThread(() => {
                    eventsInfoLabel.Text =
                        $"Non-events: {nonEvents.Length}. Events: {events.Length}.{action}";

                    SemanticScreenReader.Announce(eventsInfoLabel.Text);
                });
            }
        }
    }


    /* no push notifs yet
    int GetUserID()
    {
        #if ANDROID || IOS
                    await SetupPushNotifications(newUserId.ToString());
        #endif

        // no userId yet
        return 0;
    }
    */

    int? lastTapCount = null;
    string[] motivationalTexts = new string[] {
        "Fear? How about going to a medieval war as a warrior!",
        "No repercussions: society is vastly immense and there's no desertion concept nowadays.",
        "Ego: remove that stupid shield that we built as 'sociological protection' which we were not born with.",
        "Belief: picture yourself having success, getting what you want. You've had it before, you are worthy of love.",
        "Mindset: you're like a billionaire handing out cash; if they don't want it, not your fault.",
        "Carpe diem: add a little adventure to your life, you might die tomorrow.",
        "Nothing matters anyway: you will die some day.",
        "The universe doesn't care about your AA, why should you?",
        "She seems in a hurry? Stop that know-it-all inner voice and nod to this other one: 'let's find out'",
        "What they are attracted more is our 6th sense / empathy / capabilities for reading situations; so, does she seem shy at approach? tell her she looks shy, ask if she's shy.",
        "The goal is not to get laid / to get a gf; the goal is to become a more socially calibrated individual.",
        "'I have a boyfriend' ...Oh yeah? How long have you had that problem?",
        "'I have a boyfriend' ...oh then we have something in common for sure, I'm also in a relationship",
        "'Let's go for a coffee/drink...'; 'I have a boyfriend'; ...well he's not invited, this'd be just for the two of us.",
        "Sense of humor: most ppl have it. But their social anxiety, fear of judgment and self-imposed mental filters suppress it. Worried what they say may not be good enough, or they will be judged. Once these barriers (ego) are removed, their humor will often reveal itself naturally.",
        "“It is better to be a fool with the world than to be wise alone.” –Baltasar Gracian",
        "“DG is hard”... WTF is the alternative?",
        "Make experiments to relieve the pressure? e.g. 'I'm not actually hitting on you, I have a gf/married, but thought you look kind of hot'",
        "Most people play not to lose instead of playing to win. Be the 1%.",
    };

    void TapHereTapped(object sender, TappedEventArgs _evArgs)
    {
        var tappedLabel = (Label)sender;
        if (currentTexts is null || currentTexts.Count == 1)
        {
            currentTexts = new List<string>(this.motivationalTexts);
        }
        else if (lastTapCount.HasValue)
        {
            currentTexts.Remove(currentTexts[lastTapCount.Value]);
        }
        lastTapCount = new Random().Next(0, currentTexts.Count - 1);
        tappedLabel.Text = currentTexts[lastTapCount.Value];
    }

    List<string> currentTexts = null;

    private async Task<DataModel.GpsLocation> GatherLocation()
    {
        try
        {
            var req = new GeolocationRequest(GeolocationAccuracy.Low);
            location = await Geolocation.GetLocationAsync(req);

            MainThread.BeginInvokeOnMainThread(() => {
                addEventButton.IsEnabled = true;
                addNonEventButton.IsEnabled = true;
            });
        }
        catch (FeatureNotEnabledException)
        {
            FallbackLabel.Text = Texts.GpsLocationFeatureIsNeededText;
            MainLayout.IsVisible = false;
            return null;
        }

        var gpsLocation =
            new DataModel.GpsLocation(location.Latitude, location.Longitude);
        return gpsLocation;
    }

    #region Permissions
    private async Task<PermissionStatus> RequestAndGetLocationPermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            return status;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            await DisplayAlert("Warning", Texts.BackgroundLocationPermissionIsNeededText, "Ok");
            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        status = await Permissions.RequestAsync<Permissions.LocationAlways>();

        return status;
    }

    private async Task<bool> CheckIfLocationPermissionGranted()
    {
        var locationPermissionStatus = await RequestAndGetLocationPermission();
        if (locationPermissionStatus != PermissionStatus.Granted)
        {
            MainLayout.IsVisible = false;
            return false;
        }

        return true;
    }
    #endregion

    void NavigateToAddEventClicked(object sender, EventArgs evArgs)
    {
        Navigation.PushAsync(new EventPage(location, !this.soloSwitch.IsToggled));
    }

    void NavigateToEventsClicked(object sender, EventArgs evArgs)
    {
        Navigation.PushAsync(new EventsPage());
    }

    DataModel.NonEvent[] LoadNonEvents()
    {
        if (!Monitor.IsEntered(nonEventsFile))
            throw new Exception("Access to LoadNonEvents() without lock");
        if (!nonEventsFile.Exists)
            return Array.Empty<DataModel.NonEvent>();

        var nonEventsJson = File.ReadAllText(nonEventsFile.FullName);
        if (nonEventsJson is null)
            throw new Exception("Reading nonEvents file returned null");
        if (nonEventsJson.Trim() == string.Empty)
            throw new Exception("The nonEvents file had no content");

        DataModel.NonEvent[] persistedNonEvents =
            DataModel.Marshaller.Deserialize<DataModel.NonEvent[]>(nonEventsJson);
        nonEventsFile.CopyTo(nonEventsFileBackup.FullName, true);
        return persistedNonEvents;
    }

    void SaveNonEvents(DataModel.NonEvent[] nonEvents)
    {
        if (!Monitor.IsEntered(nonEventsFile))
            throw new Exception("Access to SaveNonEvents() without lock");
        var json = DataModel.Marshaller.Serialize(nonEvents);
        File.WriteAllText(nonEventsFile.FullName, json);
        nonEventsFile.Refresh();
    }

    async void AddNonEventClicked(object sender, EventArgs evArgs)
    {
        bool solo = !soloSwitch.IsToggled;
        string soloModeName = solo ? "solo" : "wing(s)";
        string question = $"Are you sure to add a {soloModeName} non-event?";
        bool answer = await DisplayAlert("Confirmation", question, "Yes", "No");

        if (!answer)
            return;

        var location = await GatherLocation();
        if (location is not null)
        {
            var nonEvent =
                new DataModel.NonEvent(DateTime.UtcNow, DateTime.Now, location, !soloSwitch.IsToggled);
            lock (nonEventsFile)
            {
                var nonEvents = LoadNonEvents();
                var newNonEventsList = new List<DataModel.NonEvent>(nonEvents);
                newNonEventsList.Add(nonEvent);
                SaveNonEvents(newNonEventsList.ToArray());

                Task.Run(UpdateCount);
            }
        }
    }

#if ANDROID || IOS
    async Task SetupPushNotifications(string userID)
    {
        pushManager = this.Handler.MauiContext.Services.GetService<IPushManager>();
        var result = await this.pushManager.RequestAccess();
        if (result.Status == AccessState.Available)
        {
            await pushManager.Tags.AddTag(userID);
        }
    }
#endif
}


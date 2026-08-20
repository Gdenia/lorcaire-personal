using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Calendar.CreateCalendarEvent;
using Lorcaire.Application.Calendar.GetCalendarEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Lorcaire.Desktop.Views;

public partial class CalendarView : UserControl
{
    private readonly CreateCalendarEventHandler _createEventHandler;
    private readonly GetCalendarEventsHandler _getEventsHandler;
    private readonly WorkspaceContext _workspaceContext;

    public CalendarView()
        : this(
            App.Services.GetRequiredService<CreateCalendarEventHandler>(),
            App.Services.GetRequiredService<GetCalendarEventsHandler>(),
            App.Services.GetRequiredService<WorkspaceContext>())
    {
    }

    public CalendarView(
        CreateCalendarEventHandler createEventHandler,
        GetCalendarEventsHandler getEventsHandler,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createEventHandler);
        ArgumentNullException.ThrowIfNull(getEventsHandler);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createEventHandler = createEventHandler;
        _getEventsHandler = getEventsHandler;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        SetDefaultSchedule();
        Loaded += LoadEvents;
    }

    private async void LoadEvents(object? sender, RoutedEventArgs e)
    {
        try
        {
            await RefreshEventsAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to load events: {exception.Message}";
        }
    }

    private async void CreateEvent(object? sender, RoutedEventArgs e)
    {
        CreateEventButton.IsEnabled = false;
        OperationMessage.Text = string.Empty;

        try
        {
            var startAt = BuildDateTime(StartDate, StartTime);
            var endAt = HasEndTime.IsChecked == true
                ? BuildDateTime(EndDate, EndTime)
                : (DateTimeOffset?)null;

            await _createEventHandler.HandleAsync(
                new CreateCalendarEventCommand(
                    _workspaceContext.DefaultAreaId,
                    EventTitle.Text ?? string.Empty,
                    EventDescription.Text,
                    startAt,
                    endAt));

            EventTitle.Text = string.Empty;
            EventDescription.Text = string.Empty;
            HasEndTime.IsChecked = false;
            SetDefaultSchedule();
            OperationMessage.Text = "Event created.";
            await RefreshEventsAsync();
        }
        catch (Exception exception)
        {
            OperationMessage.Text =
                $"Unable to create event: {exception.Message}";
        }
        finally
        {
            CreateEventButton.IsEnabled = true;
        }
    }

    private static DateTimeOffset BuildDateTime(
        CalendarDatePicker datePicker,
        TimePicker timePicker)
    {
        var date = datePicker.SelectedDate
            ?? throw new InvalidOperationException("Select a date.");
        var time = timePicker.SelectedTime
            ?? throw new InvalidOperationException("Select a time.");

        return new DateTimeOffset(
            date.Year,
            date.Month,
            date.Day,
            time.Hours,
            time.Minutes,
            time.Seconds,
            DateTimeOffset.Now.Offset);
    }

    private void SetDefaultSchedule()
    {
        var start = DateTimeOffset.Now.AddHours(1);
        var end = start.AddHours(1);
        StartDate.SelectedDate = start.Date;
        StartTime.SelectedTime = start.TimeOfDay;
        EndDate.SelectedDate = end.Date;
        EndTime.SelectedTime = end.TimeOfDay;
    }

    private async Task RefreshEventsAsync()
    {
        EventsList.ItemsSource = await _getEventsHandler.HandleAsync();
    }
}

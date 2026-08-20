using Avalonia.Controls;
using Avalonia.Interactivity;
using Lorcaire.Application;
using Lorcaire.Application.Calendar.CreateCalendarEvent;
using Lorcaire.Application.Calendar.GetCalendarEvents;
using Lorcaire.Application.Calendar.UpdateCalendarEvent;
using Lorcaire.Application.Calendar.DeleteCalendarEvent;
using Lorcaire.Desktop.Time;

namespace Lorcaire.Desktop.Views;

public partial class CalendarView : UserControl
{
    private readonly CreateCalendarEventHandler _createEventHandler;
    private readonly GetCalendarEventsHandler _getEventsHandler;
    private readonly UpdateCalendarEventHandler _updateEventHandler;
    private readonly DeleteCalendarEventHandler _deleteEventHandler;
    private readonly TimeProvider _timeProvider;
    private readonly WorkspaceContext _workspaceContext;
    private IReadOnlyList<CalendarEventDisplayItem> _events=[];private Guid? _editingId;private Guid? _pendingDeleteId;

    public CalendarView(
        CreateCalendarEventHandler createEventHandler,
        GetCalendarEventsHandler getEventsHandler,
        UpdateCalendarEventHandler updateEventHandler,
        DeleteCalendarEventHandler deleteEventHandler,
        TimeProvider timeProvider,
        WorkspaceContext workspaceContext)
    {
        ArgumentNullException.ThrowIfNull(createEventHandler);
        ArgumentNullException.ThrowIfNull(getEventsHandler);
        ArgumentNullException.ThrowIfNull(updateEventHandler);
        ArgumentNullException.ThrowIfNull(deleteEventHandler);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(workspaceContext);

        _createEventHandler = createEventHandler;
        _getEventsHandler = getEventsHandler;
        _updateEventHandler = updateEventHandler;
        _deleteEventHandler = deleteEventHandler;
        _timeProvider = timeProvider;
        _workspaceContext = workspaceContext;

        InitializeComponent();
        SetDefaultSchedule();
        Loaded += LoadEvents;
    }
    private void BeginEdit(object? sender,RoutedEventArgs e){if(sender is not Button{Tag:Guid id})return;var x=_events.Single(i=>i.Id==id);_editingId=id;EventTitle.Text=x.Title;EventDescription.Text=x.Description;StartDate.SelectedDate=x.LocalStartAt.Date;StartTime.SelectedTime=x.LocalStartAt.TimeOfDay;HasEndTime.IsChecked=x.LocalEndAt is not null;if(x.LocalEndAt is DateTimeOffset end){EndDate.SelectedDate=end.Date;EndTime.SelectedTime=end.TimeOfDay;}FormTitle.Text="Edit event";CreateEventButton.IsVisible=false;SaveEventButton.IsVisible=true;CancelEventButton.IsVisible=true;}
    private async void SaveEvent(object? sender,RoutedEventArgs e){if(_editingId is not Guid id)return;try{var start=BuildDateTime(StartDate,StartTime);var end=HasEndTime.IsChecked==true?BuildDateTime(EndDate,EndTime):(DateTimeOffset?)null;await _updateEventHandler.HandleAsync(new(id,EventTitle.Text??"",EventDescription.Text,start,end));ResetForm();await RefreshEventsAsync();OperationMessage.Text="Event updated.";}catch(Exception ex){OperationMessage.Text=$"Unable to update event: {ex.Message}";}}
    private void CancelEdit(object? sender,RoutedEventArgs e)=>ResetForm();
    private void DeleteEvent(object? sender,RoutedEventArgs e){if(sender is not Button{Tag:Guid id})return;_pendingDeleteId=id;ConfirmDeleteButton.IsVisible=true;CancelDeleteButton.IsVisible=true;OperationMessage.Text="Confirm or cancel the deletion.";}
    private async void ConfirmDelete(object? sender,RoutedEventArgs e){if(_pendingDeleteId is not Guid id)return;try{await _deleteEventHandler.HandleAsync(id);if(_editingId==id)ResetForm();await RefreshEventsAsync();OperationMessage.Text="Event deleted.";}catch(Exception ex){OperationMessage.Text=$"Unable to delete event: {ex.Message}";}finally{ClearDelete();}}
    private void CancelDelete(object? sender,RoutedEventArgs e){ClearDelete();OperationMessage.Text="Deletion cancelled.";}private void ClearDelete(){_pendingDeleteId=null;ConfirmDeleteButton.IsVisible=false;CancelDeleteButton.IsVisible=false;}private void ResetForm(){_editingId=null;EventTitle.Text="";EventDescription.Text="";HasEndTime.IsChecked=false;SetDefaultSchedule();FormTitle.Text="Create an event";CreateEventButton.IsVisible=true;SaveEventButton.IsVisible=false;CancelEventButton.IsVisible=false;}

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

    private DateTimeOffset BuildDateTime(
        CalendarDatePicker datePicker,
        TimePicker timePicker)
    {
        var date = datePicker.SelectedDate
            ?? throw new InvalidOperationException("Select a date.");
        var time = timePicker.SelectedTime
            ?? throw new InvalidOperationException("Select a time.");

        return LocalDateTimeResolver.ResolveToUtc(
            date.Date,
            time,
            _timeProvider.LocalTimeZone);
    }

    private void SetDefaultSchedule()
    {
        var start = _timeProvider.GetLocalNow().AddHours(1);
        var end = start.AddHours(1);
        StartDate.SelectedDate = start.Date;
        StartTime.SelectedTime = start.TimeOfDay;
        EndDate.SelectedDate = end.Date;
        EndTime.SelectedTime = end.TimeOfDay;
    }

    private async Task RefreshEventsAsync()
    {
        var events = await _getEventsHandler.HandleAsync();
        _events = events
            .Select(calendarEvent => CalendarEventDisplayItem.Create(
                calendarEvent,
                _timeProvider.LocalTimeZone))
            .ToArray();
        EventsList.ItemsSource = _events;
    }
}

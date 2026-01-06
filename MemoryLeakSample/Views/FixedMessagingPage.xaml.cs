using MemoryLeakSample.Models;
using System.Collections.ObjectModel;

namespace MemoryLeakSample;

/// <summary>
/// Page demonstrating safe messaging/timer patterns:
/// maintains an item collection, manages a cancellable timer lifecycle,
/// and tracks ticks without leaking resources.
/// </summary>
public partial class FixedMessagingPage : ContentPage
{
    private ObservableCollection<Person>? _items;
    private CancellationTokenSource? _timerCts;
    private int _tick;

    /// <summary>
    /// Initializes the page components. Data and timers are started in lifecycle methods.
    /// </summary>
    public FixedMessagingPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Binds data and starts a cancellable timer only while the page is visible.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        _items = new ObservableCollection<Person>();
        for (int i = 1; i <= 50; i++)
        {
            _items.Add(new Person { Name = $"Person {i}", Email = $"person{i}@example.com", IsActive = i % 3 == 0 });
        }

        listView.ItemsSource = _items;

        _timerCts = new CancellationTokenSource();
        _ = RunTimerAsync(_timerCts.Token);
    }

    /// <summary>
    /// Cancels the timer and breaks bindings to allow GC to reclaim views.
    /// </summary>
    protected override void OnDisappearing()
    {
        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;

        listView.ItemsSource = null;
        _items?.Clear();
        _items = null;

        base.OnDisappearing();
    }

    /// <summary>
    /// Repeating task that updates the UI on the UI thread. CancellationToken ensures
    /// the loop stops when the page is hidden/disposed.
    /// </summary>
    private async Task RunTimerAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1000, token);
                _tick++;
                MainThread.BeginInvokeOnMainThread(() => TimerLabel.Text = $"Timer: {_tick}");
            }
        }
        catch (TaskCanceledException) { }
    }

    /// <summary>
    /// Simulates a refresh action without using MessagingCenter.
    /// </summary>
    private void OnSendMessage(object? sender, EventArgs e)
    {
        DisplayAlert("Info", "Data refreshed", "OK");
    }
}

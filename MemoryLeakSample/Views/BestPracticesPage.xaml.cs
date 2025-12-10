using MemoryLeakSample.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

// Alias Syncfusion ListView event args to avoid ambiguity on Windows
using SfItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;
using SfItemSelectionChangedEventArgs = Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs;

namespace MemoryLeakSample;

/// <summary>
/// Demonstrates MAUI page lifecycle best practices: bind/unbind data and events only while visible,
/// cancel background work when hidden, and clear references to aid GC.
/// </summary>
public partial class BestPracticesPage : ContentPage
{
    private ObservableCollection<Person>? _items;
    private CancellationTokenSource? _bgTaskCts;

    /// <summary>
    /// Initializes the page. Actual data binding occurs in OnAppearing.
    /// </summary>
    public BestPracticesPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Wires events, binds data, and starts background work only while visible.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Create data and bind only while visible
        _items = new ObservableCollection<Person>();
        for (int i = 1; i <= 100; i++)
            _items.Add(new Person { Name = $"Person {i}", Email = $"person{i}@example.com", IsActive = i % 2 == 0 });
        listView.ItemsSource = _items;

        listView.ItemTapped += OnItemTapped;
        listView.SelectionChanged += OnSelectionChanged;

        _bgTaskCts = new CancellationTokenSource();
        _ = BackgroundWorkAsync(_bgTaskCts.Token);
    }

    /// <summary>
    /// Unwires events, cancels background work, and breaks bindings to allow GC.
    /// </summary>
    protected override void OnDisappearing()
    {
        listView.ItemTapped -= OnItemTapped;
        listView.SelectionChanged -= OnSelectionChanged;

        _bgTaskCts?.Cancel();
        _bgTaskCts?.Dispose();
        _bgTaskCts = null;

        // Stop page animations and break bindings to help GC
        this.AbortAnimation("PageAnimation");
        listView.ItemsSource = null;
        _items?.Clear();
        _items = null;

        base.OnDisappearing();
    }

    /// <summary>
    /// Periodic background task that respects cancellation when the page is hidden.
    /// </summary>
    /// <param name="token">Cancellation token linked to page visibility.</param>
    private static async Task BackgroundWorkAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(3000, token);
                Console.WriteLine("Background work...");
            }
        }
        catch (TaskCanceledException) { }
    }

    /// <summary>
    /// Handles item tap to show a quick alert with the tapped person's name.
    /// </summary>
    private async void OnItemTapped(object? sender, SfItemTappedEventArgs e)
    {
        if (e.DataItem is Person p)
            await DisplayAlertAsync("Tapped", p.Name, "OK");
    }

    /// <summary>
    /// Logs selection changes for diagnostics.
    /// </summary>
    private void OnSelectionChanged(object? sender, SfItemSelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Person p)
            Console.WriteLine($"Selected: {p.Name}");
    }
}

/// <summary>
/// Lightweight, reusable item view that safely subscribes/unsubscribes to BindingContext changes
/// and updates UI without leaking handlers.
/// </summary>
public partial class BestPracticeItemView : ContentView
{
    private object? _oldContext;
    private readonly Label _name = new() { FontAttributes = FontAttributes.Bold };
    private readonly Label _email = new() { FontSize = 12, TextColor = Colors.Gray };
    private readonly Label _status = new();

    /// <summary>
    /// Builds the item template layout (avatar, labels, and status).
    /// </summary>
    public BestPracticeItemView()
    {
        var grid = new Grid
        {
            Padding = 10,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) }
        };
        grid.Add(_name, 0, 0);
        grid.Add(_email, 0, 1);
        grid.Add(_status, 1, 0); Grid.SetRowSpan(_status, 2);
        Content = grid;
    }

    /// <summary>
    /// Manages handler subscription on BindingContext changes and updates the UI.
    /// </summary>
    protected override void OnBindingContextChanged()
    {
        // Stop any running item animation before rebind
        this.AbortAnimation("ItemAnimation");

        if (_oldContext is INotifyPropertyChanged oldCtx)
            oldCtx.PropertyChanged -= OnItemPropertyChanged;

        base.OnBindingContextChanged();

        if (BindingContext is INotifyPropertyChanged newCtx)
        {
            newCtx.PropertyChanged += OnItemPropertyChanged;
            UpdateView(BindingContext as Person);
        }
        _oldContext = BindingContext;
    }

    /// <summary>
    /// Ensures event handlers are detached when the native handler is removed.
    /// </summary>
    protected override void OnHandlerChanged()
    {
        // When the native view is detached, finalize cleanup
        if (Handler == null && _oldContext is INotifyPropertyChanged oldCtx)
        {
            oldCtx.PropertyChanged -= OnItemPropertyChanged;
            _oldContext = null;
        }
        base.OnHandlerChanged();
    }

    /// <summary>
    /// Finalizer to defensively detach from PropertyChanged if still subscribed.
    /// </summary>
    ~BestPracticeItemView()
    {
        if (_oldContext is INotifyPropertyChanged oldCtx)
        {
            try { oldCtx.PropertyChanged -= OnItemPropertyChanged; } catch { }
        }
    }

    /// <summary>
    /// Responds to model changes and refreshes the UI.
    /// </summary>
    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (BindingContext is Person p)
            UpdateView(p);
    }

    /// <summary>
    /// Applies the data to the UI.
    /// </summary>
    private void UpdateView(Person? p)
    {
        if (p == null) return;
        _name.Text = p.Name;
        _email.Text = p.Email;
        _status.Text = p.IsActive ? "Active" : "Inactive";
        _status.TextColor = p.IsActive ? Colors.Green : Colors.Red;
    }

}

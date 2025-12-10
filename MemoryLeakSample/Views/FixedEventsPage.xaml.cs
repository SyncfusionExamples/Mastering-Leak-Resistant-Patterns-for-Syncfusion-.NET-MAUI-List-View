using MemoryLeakSample.Models;
using System.Collections.ObjectModel;

// Alias Syncfusion event args to avoid ambiguity with Microsoft.Maui.Controls
using SfItemTappedEventArgs = Syncfusion.Maui.ListView.ItemTappedEventArgs;
using SfItemSelectionChangedEventArgs = Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs;

namespace MemoryLeakSample;

/// <summary>
/// Shows leak-resistant event handling: handlers are attached on appearing and detached
/// on disappearing. ItemsSource is cleared to break bindings and speed GC.
/// </summary>
public partial class FixedEventsPage : ContentPage
{
    private ObservableCollection<Person>? _items;

    public FixedEventsPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Populates data, binds ItemsSource, and attaches event handlers.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Populate fresh data and bind only while visible
        _items = new ObservableCollection<Person>();
        for (int i = 1; i <= 100; i++)
            _items.Add(new Person { Name = $"Person {i}", Email = $"person{i}@example.com", IsActive = i % 2 == 0 });
        listView.ItemsSource = _items;

        listView.ItemTapped += OnItemTapped;
        listView.SelectionChanged += OnSelectionChanged;
    }

    /// <summary>
    /// Detaches handlers and breaks bindings so the page and item views can be collected.
    /// </summary>
    protected override void OnDisappearing()
    {
        // Detach events first
        listView.ItemTapped -= OnItemTapped;
        listView.SelectionChanged -= OnSelectionChanged;

        // Break bindings and release data
        listView.ItemsSource = null;
        _items?.Clear();
        _items = null;

        base.OnDisappearing();
    }

    /// <summary>
    /// Event that triggers when the item is tapped.
    /// </summary>
    private void OnItemTapped(object? sender, SfItemTappedEventArgs e)
    {
        if (e.DataItem is Person p)
            Console.WriteLine($"Tapped: {p.Name}");
    }

    /// <summary>
    /// Handles selection change. Selection-related handlers are attached and detached
    /// with the page lifecycle to avoid retaining the Page instance.
    /// </summary>
    private void OnSelectionChanged(object? sender, SfItemSelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Person p)
            Console.WriteLine($"Selected: {p.Name}");
    }
}

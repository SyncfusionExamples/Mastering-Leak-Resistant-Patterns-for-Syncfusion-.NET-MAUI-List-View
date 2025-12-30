using MemoryLeakSample.Models;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace MemoryLeakSample;

/// <summary>
/// Page that binds a static list of people to a list view using a leak-resistant item template.
/// </summary>
public partial class FixedTemplatePage : ContentPage
{
    /// <summary>
    /// Backing collection for the list; populated once in the constructor.
    /// </summary>
    private ObservableCollection<Person> _items = new();

    /// <summary>
    /// Initializes components, populates data, and assigns the ItemsSource.
    /// </summary>
    public FixedTemplatePage()
    {
        InitializeComponent();

        for (int i = 1; i <= 100; i++)
        {
            _items.Add(new Person { Name = $"Person {i}", Email = $"person{i}@example.com", IsActive = i % 2 == 0 });
        }

        listView.ItemsSource = _items;
    }
}

/// <summary>
/// Lightweight item view that subscribes/unsubscribes to BindingContext changes and updates UI efficiently without leaking event handlers.
/// </summary>
public class FixedItemView : ContentView
{
    /// <summary>
    /// Tracks the previous BindingContext instance for safe handler detachment.
    /// </summary>
    private object? _oldContext;

    /// <summary>
    /// Name label rendered in bold.
    /// </summary>
    private readonly Label _name = new() { FontAttributes = FontAttributes.Bold };

    /// <summary>
    /// Email label with smaller, gray text.
    /// </summary>
    private readonly Label _email = new() { FontSize = 12, TextColor = Colors.Gray };

    /// <summary>
    /// Status label showing Active/Inactive.
    /// </summary>
    private readonly Label _status = new();

    /// <summary>
    /// Builds the item layout with name, email, and status columns.
    /// </summary>
    public FixedItemView()
    {
        var grid = new Grid
        {
            Padding = 10,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) }
        };

        grid.Add(_name, 0, 0);
        grid.Add(_email, 0, 1);
        grid.Add(_status, 1, 0);
        Grid.SetRowSpan(_status, 2);

        Content = grid;
    }

    /// <summary>
    /// Manages PropertyChanged subscription across BindingContext changes and refreshes the view.
    /// </summary>
    protected override void OnBindingContextChanged()
    {
        if (_oldContext is INotifyPropertyChanged oldContext)
        {
            oldContext.PropertyChanged -= OnItemPropertyChanged;
        }

        base.OnBindingContextChanged();

        if (BindingContext is INotifyPropertyChanged newContext)
        {
            newContext.PropertyChanged += OnItemPropertyChanged;
            UpdateView(BindingContext as Person);
        }

        _oldContext = BindingContext;
    }

    /// <summary>
    /// Ensures handlers are detached when the native handler is removed (view disposed).
    /// </summary>
    protected override void OnHandlerChanged()
    {
        // When the native handler is removed (view disposed), ensure cleanup
        if (Handler == null && _oldContext is INotifyPropertyChanged oldContext)
        {
            oldContext.PropertyChanged -= OnItemPropertyChanged;
            _oldContext = null;
        }

        base.OnHandlerChanged();
    }

    /// <summary>
    /// Updates UI on item property changes.
    /// </summary>
    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (BindingContext is Person person)
        {
            UpdateView(person);
        }
    }

    /// <summary>
    /// Applies the bound values to the UI.
    /// </summary>
    private void UpdateView(Person? person)
    {
        if (person == null)
        {
            return;
        }

        _name.Text = person.Name;
        _email.Text = person.Email;
        _status.Text = person.IsActive ? "Active" : "Inactive";
        _status.TextColor = person.IsActive ? Colors.Green : Colors.Red;
    }
}

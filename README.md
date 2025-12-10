# Mastering-Leak-Resistant-Patterns-for-Syncfusion-.NET-MAUI-List-View
This demo shows How to Master Leak Resistant Patterns for Syncfusion® .NET MAUI List View

## Overview

Building high-performance mobile apps means keeping memory usage under control. When working with Syncfusion® .NET MAUI List View, improper handling of event subscriptions, heavy templates, or async operations can lead to memory leaks and sluggish performance over time.
In this article, we will explore practical patterns and best practices to make your List View leak-resistant, covering lifecycle-aware bindings, disposal strategies, and efficient virtualization techniques to ensure smooth scrolling and stable apps.

## This article will cover:
* Causes of memory leaks with  Syncfusion® .NET MAUI List View and How to prevent them.
* Diagnostics to detect and confirm fixes.
* A quick checklist you can run through before shipping.

## Common Causes of Memory Leaks in Syncfusion® .NET MAUI List View and How to Prevent Them

Memory leaks in Syncfusion®  .NET MAUI List View apps are rarely dramatic. They build slowly from small oversights that accumulate over time. Here are the typical culprits, how they trap objects in memory, and practical patterns to prevent them.

### 1. Event handlers not unsubscribed
The problem: When you wire up ItemTapped, SelectionChanged, PropertyChanged, or MessagingCenter handlers, those subscriptions create strong references. If you leave the page without unsubscribing, the event source keeps the handler alive, and the handler keeps your page or ViewModel alive. Anonymous lambdas are especially risky because they capture this and make cleanup harder to track. Timers that call methods on your page or ViewModel have the same effect.

The fix: Hook event handlers when the page appears and removes them when it hides. Prefer Command bindings over code-behind events when possible since bindings clean up automatically. If you must use events, always pair subscribe with unsubscribe.

```
protected override void OnAppearing()
{
    base.OnAppearing();
    listView.ItemTapped += OnItemTapped;
    listView.SelectionChanged += OnSelectionChanged;
}
protected override void OnDisappearing()
{
    listView.ItemTapped -= OnItemTapped;
    listView.SelectionChanged -= OnSelectionChanged;
    base.OnDisappearing();
}
```
This prevents the page from remaining in memory after you navigate away.

### 2. Long-lived references to UI elements
The problem: Singletons, services, or static fields that hold a reference to a Page, List View, or item view prevent the garbage collector from reclaiming them. Even storing a single control or image in a static cache can pin an entire object graph.

The fix: Always use weak references or clear caches when pages disappear. Avoid storing UI objects in ViewModels. If you need a link from ViewModel to View, use WeakReference or WeakEventManager so the page can collect normally.

```
// Instead of:
public static Page CurrentPage;

// Use:
public static WeakReference<Page> CurrentPageRef;

// Access with:
if (CurrentPageRef?.TryGetTarget(out var page) == true)
{
    // Use page
}
```

### 3. ItemTemplate issues during recycling
The problem: Custom views inside a DataTemplate often attach events in code-behind or via behaviors and effects. When List View recycles a cell and assigns a new BindingContext, those old subscriptions remain unless you explicitly detach them. Recycled cells end up holding references to multiple old contexts, multiplying memory use.

The fix: Override OnBindingContextChanged, unsubscribe from the old context, call base, then subscribe to the new one. These stops recycled cells from clinging to old view models.

```
public class ItemView : ContentView
{
    object _oldContext;
    protected override void OnBindingContextChanged()
    {
        // Unsubscribe from old context
        if (_oldContext is INotifyPropertyChanged oldCtx)
            oldCtx.PropertyChanged -= OnItemPropertyChanged;
        base.OnBindingContextChanged();

        // Subscribe to new context
        if (BindingContext is INotifyPropertyChanged newCtx)
            newCtx.PropertyChanged += OnItemPropertyChanged;
        _oldContext = BindingContext;
    }

    void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Handle property changes here
    }
}
```

For behaviors and effects: Always unsubscribe in OnDetachingFrom and clear any stored view references.

```
public class SafeBehavior : Behavior<View>
{
    View associated;
    protected override void OnAttachedTo(View bindable)
    {
        associated = bindable;
        associated.PropertyChanged += OnPropertyChanged;
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        associated.PropertyChanged -= OnPropertyChanged;
        associated = null;
        base.OnDetachingFrom(bindable);
    }

    void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Handle property changes
    }
}
```

### 4. Images and unmanaged resources
The problem: Creating an ImageSource from a stream is convenient, but if you do not dispose the stream, the image data stays in memory. Large bitmaps, animated GIFs, or looping animations keep running even after the page is gone unless you explicitly stop or clear them.

The fix: Always dispose streams you use to create ImageSource with using or explicit Dispose(). Clear Image.Source and any image caches when the page disappears so the garbage collector can reclaim memory.

```
protected override void OnDisappearing()
{
    base.OnDisappearing();

    // Clear all tracked images
    foreach (var img in _trackedImages)
        img.Source = null;
    _trackedImages.Clear();
}

// When creating ImageSource from stream:
using (var stream = await GetImageStreamAsync())
{
    image.Source = ImageSource.FromStream(() => stream);
}
```

### 5. Messaging and timers
The problem: MessagingCenter subscriptions and Device.StartTimer or long-running Task loops that reference your page or ViewModel keep them alive until you unsubscribe or cancel. If you forget, every new instance of the page adds another subscription, and memory grows with each navigation.

The fix: Subscribe to messages in OnAppearing and unsubscribe in OnDisappearing. Cancel timers and background tasks when you leave the page, so callbacks do not keep references. Keep all wiring in one place, avoid anonymous lambdas that capture this unless you explicitly remove them, and use CancellationToken for repeating work.

```
CancellationTokenSource _timerCts;
protected override void OnAppearing()
{
    base.OnAppearing();
    MessagingCenter.Subscribe<MyMessage>(this, "Refresh", OnRefresh);
}
protected override void OnDisappearing()
{
    MessagingCenter.Unsubscribe<MyMessage>(this, "Refresh");
    _timerCts?.Cancel(); // Cancel timer or task if active.
    base.OnDisappearing();
}

void OnRefresh(MyMessage msg)
{
    // Handle refresh logic
}
```

### 6. Grouping, swipe, and interactions
The problem: Swipe templates, group headers and footers, and context actions all attach events. When cells scroll off-screen or recycled, those events can remain subscribed if you do not clean them up. Interactive features are powerful but need careful lifecycle management.

The fix: For pages that are recreated often, clear ItemsSource in OnDisappearing to break bindings and help the garbage collector release views. If you need to preserve state like scroll position or selection, save and restore it instead of clearing. Keep ItemTemplate lean, use virtualization, avoid nested Scroll View in items, and set a fixed ItemSize when possible.

```
protected override void OnDisappearing()
{
    base.OnDisappearing();
    listView.ItemsSource = null; // Helps GC by breaking bindings
}
```

### Reduce memory footprint:
* Keep ItemTemplate lean.
* Use virtualization; avoid nested Scroll View in items.
* Set a fixed ItemSize when possible.
* Do not store large per-item objects in Resources.

### 7. Data and collection management
The problem: Old items left in ObservableCollection, caches, or dictionaries inside your ViewModel prevent garbage collection. If ItemsSource keeps growing without trimming or replacing stale references, memory climbs.

The fix: Regularly prune collections and clear references when data not needed. Implement paging or virtualization patterns for large data sets.

```
// Trim old items
if (Items.Count > MaxCacheSize)
{
    var toRemove = Items.Take(Items.Count - MaxCacheSize).ToList();
    foreach (var item in toRemove)
        Items.Remove(item);
}

// Or replace entirely
Items.Clear();
foreach (var newItem in freshData)
    Items.Add(newItem);
```

### 8. Animations and gestures
The problem: Animations running on item views continue unless you stop them explicitly. When views recycle or the page disappears, animations can keep references alive.

The fix: Abort any active animations when you leave the page. Use a consistent animation key and stop it in OnDisappearing. If the animation runs per item, also stop it when item views recycled.

```
protected override void OnDisappearing()
{
    base.OnDisappearing();
    this.AbortAnimation("MyPulseAnimation");
}

// In custom item view:
protected override void OnBindingContextChanged()
{
    this.AbortAnimation("ItemAnimation");
    base.OnBindingContextChanged();
    // Start new animation if needed
}
```

### 9. Navigation and lifecycle
The problem: When you recreate list pages on every navigation, failing to clear bindings or ItemsSource when the page disappears causes leaks. Even though the navigation system pops the page from the stack, active subscriptions and references keep it alive.

The fix: Clear or null out ItemsSource, detach events, and unsubscribe during OnDisappearing. Combine all cleanup patterns in a single, consistent teardown method.

```
protected override void OnDisappearing()
{
    base.OnDisappearing();
    // Unsubscribe events
    listView.ItemTapped -= OnItemTapped;
    
    // Cancel timers and tasks
    _timerCts?.Cancel();
    
    // Unsubscribe messaging
    MessagingCenter.Unsubscribe<MyMessage>(this, "Refresh");
    
    // Clear images
    foreach (var img in _trackedImages)
        img.Source = null;

    // Clear bindings
    listView.ItemsSource = null;

    // Stop animations
    this.AbortAnimation("MyAnimation");
}
```

## Diagnostic Tips

### 1.	Use Profilers:
Track retained Pages, List Views, item views, and ViewModels after navigation with Visual Studio Diagnostic Tools or JetBrains dot Memory. On Android/iOS, inspect large image allocations and follow reference paths with Android Studio Profiler or Xcode Instruments. Compare snapshots before and after navigation to confirm releases and identify GC roots.
### 2.	Forced GC in Development:
Navigate away from the page, call GC.Collect(), and verify through your profiler that the page and its objects have been released from memory.
### 3.	Look for Telltale References:
Ensure event subscriptions, MessagingCenter hooks, static caches, timers, and Task continuations do not keep a reference to the page or ViewModel.
### 4.	Create a Minimal Repro:
Start from a simple ListView page and add features step by step to pinpoint where a leak begins and validate each change removes it.

## Quick Checklist to Avoid Memory Leaks
* [ ] All event handlers detached on Disappearing/Dispose
* [ ] Behaviors/Effects properly detach.
* [ ] ItemTemplate safe across BindingContext changes
* [ ] No View references held in ViewModel/services.
* [ ] Streams disposed; large images cleared.
* [ ] MessagingCenter/timers unsubscribed/cancelled.
* [ ] Swipe/group templates detach handlers.
* [ ] ItemsSource cleared when appropriate.
* [ ] Animations aborted on teardown
* [ ] Verified with a profiler that pages and items are released after navigation.

## Troubleshooting
Path too long exception
If you are facing a path too long exception when building this example project, close Visual Studio and rename the repository to a shorter name before building the project.

For a step-by-step procedure, refer to the [AI-Powered Billionaire Wealth Dashboard Blog](https://www.syncfusion.com/blogs/post/ai-powered-winui-line-chart).
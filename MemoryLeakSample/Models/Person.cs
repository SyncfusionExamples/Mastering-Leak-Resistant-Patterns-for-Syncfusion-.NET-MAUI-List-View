using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemoryLeakSample.Models
{
    /// <summary>
    /// Plain data model used by the sample pages. Implements INotifyPropertyChanged to
    /// simulate real-world item view models and to exercise template subscription cleanup.
    /// </summary>
    public partial class Person : INotifyPropertyChanged
    {
        private string? _name;
        private string? _email;
        private bool _isActive;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string? Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// Gets or sets the email ID used in the list item.
        /// </summary>
        public string? Email
        {
            get => _email;
            set { if (_email != value) { _email = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the item is considered active.
        /// Used to demonstrate UI updates and animations.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set { if (_isActive != value) { _isActive = value; OnPropertyChanged(); } }
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="name">Caller member name.</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

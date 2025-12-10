namespace MemoryLeakSample
{
    /// <summary>
    /// Landing page that links to all leak‑prevention samples.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Initializes the main page.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens the Events cleanup sample.
        /// </summary>
        private void OpenFixedEvents(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FixedEventsPage());
        }

        /// <summary>
        /// Opens the ItemTemplate cleanup sample.
        /// </summary>
        private void OpenFixedTemplate(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FixedTemplatePage());
        }

        /// <summary>
        /// Opens the Messaging and timers sample (CTS-based).
        /// </summary>
        private void OpenFixedMessaging(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FixedMessagingPage());
        }

        /// <summary>
        /// Opens the combined best‑practices sample.
        /// </summary>
        private void OpenBestPractices(object sender, EventArgs e)
        {
            Navigation.PushAsync(new BestPracticesPage());
        }
    }
}

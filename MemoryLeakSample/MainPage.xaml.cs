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
        /// Opens the Events cleanup page.
        /// </summary>
        private void OpenFixedEvents(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FixedEventsPage());
        }

        /// <summary>
        /// Opens the ItemTemplate cleanup page.
        /// </summary>
        private void OpenFixedTemplate(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FixedTemplatePage());
        }

        /// <summary>
        /// Opens the Messaging and timers page.
        /// </summary>
        private void OpenFixedMessaging(object sender, EventArgs e)
        {
            Navigation.PushAsync(new FixedMessagingPage());
        }

        /// <summary>
        /// Opens the combined best‑practices page.
        /// </summary>
        private void OpenBestPractices(object sender, EventArgs e)
        {
            Navigation.PushAsync(new BestPracticesPage());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using Weingartner.ReactiveCompositeCollections;

namespace ReactiveUIBug
{

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; }

        public CollapsibleLogEntry(DateTime dateTime, string message, int index) : base( dateTime, message, index )
        {
        }
    }

    public class LogEntry 
    {
         public DateTime DateTime {get; }
         public int Index {get; }
         public string Message {get; }

        public LogEntry(DateTime dateTime, string message, int index)
        {
            DateTime = dateTime;
            Message = message;
            Index = index;
        }
    }
    /// <summary>
    /// A simple class for creating a log viewer. Call the static Log message.
    /// </summary>
    public partial class LogViewer : Window
    {
        private CompositeSourceList<LogEntry> LogEntries { get; set; }

        private LogViewer()
        {
            InitializeComponent();
            LogEntries = new CompositeSourceList<LogEntry>();

            this.LoadUnloadHandler(() => Init());
        }

        private IEnumerable<IDisposable> Init()
        {
            yield return ClearButton
                .Events()
                .PreviewMouseUp
                .Subscribe(_ => ClearAll());

            yield return this
                .WhenAnyValue(p => p.FilterText.Text)
                .Subscribe(Console.WriteLine);

            var filteredEntries =
                LogEntries
                    //.Where(this
                    //    .WhenAnyValue(p => p.FilterText.Text)
                    //    .ObserveOn(this)
                    //    .Select(str => fun((LogEntry v) => v.Message.Contains(str)))
                    //)
                    .CreateObservableCollection(EqualityComparer<LogEntry>.Default);
            yield return filteredEntries;

            MainPanel.DataContext = filteredEntries;
            yield return Disposable.Create(() => MainPanel.DataContext = null);
        }

        public static readonly Lazy<LogViewer> Window = new Lazy<LogViewer>(() => CreateAndShowStaWindow(() => new LogViewer()).Result);


        private void ClearAll()
        {
            LogEntries.RemoveRange(LogEntries.Source);
        }

        public static void Invoke(Action a)
        {
            Window.Value.Dispatcher.Invoke(a);
        }

        public static Task<T> CreateAndShowStaWindow<T>(Func<T> factory) where T : Window
        {

            var windowResult = new TaskCompletionSource<T>();

            var newWindowThread = new Thread(() =>
            {
                var window = factory();
                window.Show();
                windowResult.SetResult(window);
                window.Events().Closed.Subscribe(_ => window.Dispatcher.InvokeShutdown());
                System.Windows.Threading.Dispatcher.Run();

            });
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
            newWindowThread.Name = "STA WPF";

            return windowResult.Task;
        }

        private static int _CurrentIndex;

        public static void Log(LogEntry entry)
        {
            var window = Window.Value;
            window.Dispatcher.Invoke(() => window.LogEntries.Add(entry));
        }

        public static void LogWithIndent(int i, string message)
        {
            Log(new string(' ', i * 4) + message);


        }
        public static void Log(string message)
        {
            try
            {
                Log(new LogEntry(DateTime.Now, message, _CurrentIndex++));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

    }

    public static class LogViewerExtensions
    {

    }
}
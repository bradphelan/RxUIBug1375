using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using Splat;

namespace ReactiveUIBug
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup( e );

            Init();



            var window = new MainWindow();
            window.Show();
        }

        public static void Init()
        {


            Locator.CurrentMutable.InitializeReactiveUI();
        }


    }



    public static class ObservableExtensions
    {

        public static void LoadUnloadHandler(this FrameworkElement control, Func<IEnumerable<IDisposable>> action)
        {
            LoadUnloadHandler(control, () => (IDisposable)new CompositeDisposable(action()));
        }

        public static void LoadUnloadHandler(this FrameworkElement control, Func<IDisposable> action)
        {
            var state = false;
            var cleanup = new SerialDisposable();
            Observable.Merge
                (Observable.Return(control.IsLoaded)
                , control.Events().Loaded.Select(x => true)
                , control.Events().Unloaded.Select(x => false)
                )
                .Subscribe(isLoadEvent =>
                {
                    if (!state)
                    {
                        // unloaded state
                        if (isLoadEvent)
                        {
                            state = true;
                            cleanup.Disposable = new CompositeDisposable(action());
                        }
                    }
                    else
                    {
                        // loaded state
                        if (!isLoadEvent)
                        {
                            state = false;
                            cleanup.Disposable = Disposable.Empty;
                        }
                    }

                });
        }




    }
}

using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using TVShow.Model.Api;

namespace TVShow.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IService, Service>();

            SimpleIoc.Default.Register(() => new MainViewModel());
            SimpleIoc.Default.Register(() => new MoviesViewModel());
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        /// <summary>
        /// Gets the Movies property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MoviesViewModel Movies
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MoviesViewModel>();
            }
        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            if (SimpleIoc.Default.IsRegistered<MoviesViewModel>())
            {
                SimpleIoc.Default.Unregister<MoviesViewModel>();
            }

            if (SimpleIoc.Default.IsRegistered<MainViewModel>())
            {
                SimpleIoc.Default.Unregister<MainViewModel>();
            }
        }
    }
}
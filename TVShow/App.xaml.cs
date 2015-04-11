using System;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System.Threading.Tasks;
using Squirrel;

namespace TVShow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }
    }
}

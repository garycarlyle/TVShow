﻿#pragma checksum "..\..\..\..\UserControls\Movies.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "EFCFFCF4C0EF50D748020970D7C8AFF9"
//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.0
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using TVShow.Converters;
using TVShow.CustomPanels;
using TVShow.Resources.Styles;


namespace TVShow.UserControls {
    
    
    /// <summary>
    /// Movies
    /// </summary>
    public partial class Movies : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 36 "..\..\..\..\UserControls\Movies.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid Container;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\..\UserControls\Movies.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal MahApps.Metro.Controls.ProgressRing ProgressRing;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\UserControls\Movies.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock NoMouvieFound;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\..\UserControls\Movies.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer ScrollView;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\UserControls\Movies.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ItemsControl ItemsList;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TVShow;component/usercontrols/movies.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\UserControls\Movies.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.Container = ((System.Windows.Controls.Grid)(target));
            
            #line 36 "..\..\..\..\UserControls\Movies.xaml"
            this.Container.GotMouseCapture += new System.Windows.Input.MouseEventHandler(this.Container_GotMouseCapture);
            
            #line default
            #line hidden
            
            #line 36 "..\..\..\..\UserControls\Movies.xaml"
            this.Container.KeyDown += new System.Windows.Input.KeyEventHandler(this.Container_KeyDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ProgressRing = ((MahApps.Metro.Controls.ProgressRing)(target));
            return;
            case 3:
            this.NoMouvieFound = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.ScrollView = ((System.Windows.Controls.ScrollViewer)(target));
            
            #line 46 "..\..\..\..\UserControls\Movies.xaml"
            this.ScrollView.ScrollChanged += new System.Windows.Controls.ScrollChangedEventHandler(this.ScrollViewer_ScrollChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ItemsList = ((System.Windows.Controls.ItemsControl)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

﻿#pragma checksum "..\..\..\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "DB7C69CEB20C9159CB58ED6843EF093B51ACC241"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ETDataHub;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
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


namespace ETDataHub {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image imgStaticEnv;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tbETStatus;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel spButtons;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton btnToggleET;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnLabelGaze;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tbLabelGaze;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.11.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ETDataHub;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.11.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.imgStaticEnv = ((System.Windows.Controls.Image)(target));
            return;
            case 2:
            this.tbETStatus = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.spButtons = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 4:
            this.btnToggleET = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            
            #line 29 "..\..\..\MainWindow.xaml"
            this.btnToggleET.Checked += new System.Windows.RoutedEventHandler(this.btnToggleET_Checked);
            
            #line default
            #line hidden
            
            #line 30 "..\..\..\MainWindow.xaml"
            this.btnToggleET.Unchecked += new System.Windows.RoutedEventHandler(this.btnToggleET_Unchecked);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnLabelGaze = ((System.Windows.Controls.Button)(target));
            
            #line 34 "..\..\..\MainWindow.xaml"
            this.btnLabelGaze.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.btnLabelGaze_MouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 35 "..\..\..\MainWindow.xaml"
            this.btnLabelGaze.PreviewMouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.btnLabelGaze_MouseLeftButtonUp);
            
            #line default
            #line hidden
            
            #line 36 "..\..\..\MainWindow.xaml"
            this.btnLabelGaze.PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.btnLabelGaze_PreviewMouseRightButtonDown);
            
            #line default
            #line hidden
            
            #line 37 "..\..\..\MainWindow.xaml"
            this.btnLabelGaze.PreviewMouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.btnLabelGaze_PreviewMouseRightButtonUp);
            
            #line default
            #line hidden
            return;
            case 6:
            this.tbLabelGaze = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}


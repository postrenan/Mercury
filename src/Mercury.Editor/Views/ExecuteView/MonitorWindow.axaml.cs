using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mercury.Editor.ViewModels.Execute;

namespace Mercury.Editor.Views.ExecuteView;

public partial class MonitorWindow : BaseWindow<MonitorWindow, MonitorViewModel> {
    public MonitorWindow() {
        InitializeComponent();
        ViewModel.Screen = Screen;
    }
}
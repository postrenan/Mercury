using System;
using Avalonia;
using Avalonia.Controls;
using Mercury.Editor.ViewModels.Code;
using Microsoft.Extensions.DependencyInjection;

namespace Mercury.Editor.Views.CodeView;

public partial class CodeTabView : BaseControl<CodeTabView, CodeTabViewModel> {
    public CodeTabView() {
        InitializeComponent();
        Unloaded += (_,_) => ViewModel.OnUnload();
        Loaded += (_, _) => ViewModel.OnLoad();
    }
}
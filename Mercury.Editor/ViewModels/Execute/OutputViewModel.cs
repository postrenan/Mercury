using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Editor.Models.Messages;
using Mercury.Editor.Views.ExecuteView;
using Mercury.Engine.Common;
using Mercury.Engine.Common.Events;

namespace Mercury.Editor.ViewModels.Execute;

public partial class OutputViewModel : BaseViewModel<OutputViewModel, OutputView> {

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCommand))] private string inputText = string.Empty;

    // manually implemented text property
    private string? textCached;
    public string Text => textCached ??= sb.ToString();

    private void TriggerTextUpdate() {
        textCached = null;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Text)));
        ClearCommand.NotifyCanExecuteChanged();
        GetView()?.OutputScroller?.ScrollToEnd();
    }

    private readonly StringBuilder sb = new();
    private EventBus? eventBus;
    private readonly List<IDisposable> subscribers = [];
    private BufferedStdinModule? stdinModule;

    public OutputViewModel() {
        WeakReferenceMessenger.Default.Register<ProgramLoadMessage>(this, OnProgramLoaded);
    }

    private static void OnProgramLoaded(object receiver, ProgramLoadMessage msg) {
        OutputViewModel vm = (OutputViewModel)receiver;
        // dispose old objects
        vm.sb.Clear();

        foreach (IDisposable sub in vm.subscribers) {
            sub.Dispose();
        }
        vm.subscribers.Clear();
        vm.eventBus = msg.MipsMachine.EventBus;
        vm.subscribers.Add(vm.eventBus.Subscribe<StdOutWriteEvent>(vm.HandleStdOut));
        vm.subscribers.Add(vm.eventBus.Subscribe<StdErrWriteEvent>(vm.HandleStdErr));
        vm.stdinModule = msg.MipsMachine.StdIn;
        
        vm.TriggerTextUpdate();
    }

    private void HandleStdOut(StdOutWriteEvent e) {
        sb.Append(e.Data);
        TriggerTextUpdate();
    }

    private void HandleStdErr(StdErrWriteEvent e) {
        sb.Append(e.Data);
        TriggerTextUpdate();
    }
    
    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task Send() {
        if (stdinModule is null) {
            return;
        }
        sb.AppendLine(InputText);
        await stdinModule.Writer.WriteAsync(InputText + '\n');
        InputText = string.Empty;
        TriggerTextUpdate();
    }

    [RelayCommand(CanExecute = nameof(CanClear))]
    private void Clear() {
        sb.Clear();
        TriggerTextUpdate();
    }

    private bool CanSend() {
        return !string.IsNullOrEmpty(InputText) && stdinModule is not null;
    }

    private bool CanClear() {
        return sb.Length > 0;
    }
}
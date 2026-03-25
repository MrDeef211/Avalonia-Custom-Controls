using ReactiveUI;
using System.Reactive;

namespace Demonstrations.Desktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {


        public MainWindowViewModel() 
        {
            var canExecuteTest = this.WhenAnyValue(x => x.TestCommandIsActive);

            ClickCommand = ReactiveCommand.Create(() =>
            {
                TestCommandIsActive = !TestCommandIsActive;
            });

            TestCommand = ReactiveCommand.Create(() => { }, canExecuteTest);
        }

        private bool _testCommandIsActive = true;
        public bool TestCommandIsActive
        {
            get => _testCommandIsActive;
            set => this.RaiseAndSetIfChanged(ref _testCommandIsActive, value);
        }

        public ReactiveCommand<Unit,Unit> ClickCommand { get; }

        public ReactiveCommand<Unit, Unit> TestCommand { get; }
    }
}

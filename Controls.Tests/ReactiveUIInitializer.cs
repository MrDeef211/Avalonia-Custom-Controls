using ReactiveUI.Builder;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Controls.Tests
{
    public static class ReactiveUIInitializer
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            RxAppBuilder.CreateReactiveUIBuilder()
                .WithCoreServices()
                .BuildApp();
        }

        private class TestExceptionHandler : IObserver<Exception>
        {
            public void OnNext(Exception value) { }
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }
    }
}

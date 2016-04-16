using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WoundifyUWP
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ProcessArgs();
        }

        private async void ProcessArgs() // valid args include --ClientId "..."
        {
            await WoundifyShared.Commands.ProcessArgsAsync(ScriptTBx.Text);
        }

        private void NewScriptBtn_Click(object sender, RoutedEventArgs e)
        {
            WoundifyShared.Commands.ProcessArgsReset(ScriptTBx.Text);
        }

        private async void ExecuteBtn_Click(object sender, RoutedEventArgs e)
        {
            await WoundifyShared.Commands.SingleStepCommandsAsync();
        }

    }
}

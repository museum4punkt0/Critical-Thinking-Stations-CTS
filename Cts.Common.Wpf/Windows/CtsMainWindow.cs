using Gemelo.Components.Common.Localization;
using Gemelo.Components.Dah.Windows;
using System;
using System.Windows.Input;

namespace Gemelo.Components.Cts.Windows
{
    public abstract class CtsMainWindow : DahMainWindow
    {
        public virtual void ProcessNewUser() { }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.L) Languages.Next(LanguageChangeSource.User);
        }
    }
}

﻿

<<<<<<< HEAD
#pragma checksum "C:\Users\Mihai\Documents\GitHub\MediaPlayer\MediaPlayer\MediaPlayer\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "39237A289B0FF906EA25C85D98B27FA8"
=======
#pragma checksum "C:\Users\Arthur\Documents\GitHub\MediaPlayer\MediaPlayer\MediaPlayer\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "83F542A6BDEE6B43D4C0D4440079F41F"
>>>>>>> TopTrackByTag first implementation
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MediaPlayer
{
    partial class MainPage : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 25 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.Set_Click;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 20 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.PlayPause_Tapped;
                 #line default
                 #line hidden
                #line 20 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerEntered += this.PlayPause_PointerEntered;
                 #line default
                 #line hidden
                #line 20 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerExited += this.PlayPause_PointerExited;
                 #line default
                 #line hidden
                #line 20 "..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).PointerPressed += this.PlayPause_PointerPressed;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}



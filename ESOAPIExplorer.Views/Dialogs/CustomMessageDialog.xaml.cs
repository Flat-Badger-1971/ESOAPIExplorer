// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ESOAPIExplorer.Views.Dialogs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
#pragma warning disable CsWinRT1029 // Class not trimming / AOT compatible
public sealed partial class CustomMessageDialog : ContentDialog
#pragma warning restore CsWinRT1029 // Class not trimming / AOT compatible
{
    public CustomMessageDialog()
    {
        this.InitializeComponent();
    }
}

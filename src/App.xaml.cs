using Microsoft.AspNetCore.Components.WebView.Maui;
using System.Reflection;

namespace PveAdmin;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

        MainPage = new MainPage();
	}
}


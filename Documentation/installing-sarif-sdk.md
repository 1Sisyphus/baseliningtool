Installing Sarif-SDK

Way 1:

	To install Sarif Sdk on your machine, first download and install dotnet sdk: https://dotnet.microsoft.com/download/dotnet/2.1

	Then open command window and run this command: dotnet new console --framework netcoreapp2.1

	Then install the latest version of Sarif Sdk https://www.nuget.org/packages/Sarif.Sdk/. You can also use this command after installing dotnet: dotnet add package Sarif.Sdk --verison 2.4.10. You should choose the version that is available to you.

	Remark: Sarif Sdk is installed on Workspace level

Way 2:

	- create a visual studio project.
	- once it's created, right click on it and click on manage nuget packages. From that window go to browse tab and install Sarif sdk.




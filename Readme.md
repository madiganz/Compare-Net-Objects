[<img src="https://github.com/GregFinzer/comparenetobjects/blob/master/logo.png">](http://www.kellermansoftware.com)

[<img src="https://github.com/GregFinzer/comparenetobjects/blob/master/PoweredByNDepend.png">](http://www.ndepend.com)

# Project Description
What you have been waiting for. Perform a deep compare of any two .NET objects using reflection. Shows the differences between the two objects.

# Compatibility
* Compatible with .NET Framework 4.0 and higher.  
* .NET Standard 1.3 Build Compatible with .NET Core 1.0, Mono 4.6, Xamarin.iOS 10.0, Xamarin.Mac 3.0, Xamarin.Android 7.0, Universal Windows Platform 10.0
* .NET Standard 2.0 Build Compatible with .NET Core 2.0, Mono 5.4, Xamarin.iOS 10.14, Xamarin.Mac 3.8, Xamarin.Android 8.0, Universal Windows Platform 10.0.16299, Unity 2018.1
* .NET Standard 2.1 Build Compatible with .NET 5, .NET 6, Mono 6.4, Xamarin.iOS 12.16, Xamarin.Mac 5.16, Xamarin.Android 10.0

# NuGet Package

<a href="https://ci.appveyor.com/project/GregFinzer/compare-net-objects">
  <img src="https://ci.appveyor.com/api/projects/status/pi60wxnpsre5gu3f?svg=true" alt="AppVeyor Status" height="50">
</a>


<a href="https://www.nuget.org/packages/CompareNETObjects">
  <img src="http://img.shields.io/nuget/v/CompareNETObjects.svg" alt="NuGet Version" height="50">
</a>

<a href="https://www.nuget.org/packages/CompareNETObjects">
  <img src="https://img.shields.io/nuget/dt/CompareNETObjects.svg" alt="NuGet Downloads" height="50">
</a>

http://www.nuget.org/packages/CompareNETObjects

## Installation

Install with NuGet Package Manager Console
```
Install-Package CompareNETObjects
```

Install with .NET CLI
```
dotnet add package CompareNETObjects
```

# Features

## Feature Overview
* Compare Children (on by default)
* Handling for Trees with Children Pointing To Parents (Circular References)
* Compares Publicly Visible Class Fields and Properties
* Compares Private Fields and Properties (off by default)
* Source code in C#
* NUnit Test Project Included with over **275+** unit tests
* Ability to load settings from a config file for use with powershell
* Ability to pass in the configuration
* Ability to save and load the configuration as json
* Test Extensions .ShouldCompare and .ShouldNotCompare
* Several configuration options for comparing private elements, ignoring specific elements, including specific elements.
* Property and Field Info reflection caching for increased performance
* Rich Differences List or simple DifferencesString
* Difference Callback
* Supports custom comparison for types and properties
* ElapsedMilliseconds indicates how long the comparison took
* Thread Safe
* Beyond Compare Report
* WinMerge Report
* CSV Report
* User Friendly Report 
* HTML Report

## Options
* Ability to IgnoreCollectionOrder to compare lists of different lengths
* Ability to ignore indexer comparison
* Ability to ignore types
* Ability to ignore specific members by name or by wildcard
* Interface member filtering
* Ability to treat string.empty and null as equal
* Ability to ignore string leading and trailing whitespace
* Case insensitive option for strings
* Ignore millisecond differences between DateTime values or DateTimeOffset values
* Precision for double or decimal values

## Supported Types
* Classes
* Dynamic (Expando objects and Dynamic objects are supported)
* Anonymous Types
* Primitive Types (String, Int, Boolean, etc.)
* Structs
* IList Objects
* Collections
* Single and Multi-Dimensional Arrays
* Immutable Arrays
* IDictionary Objects
* Enums
* Timespans
* Guids
* Classes that Implement IList with Integer Indexers
* DataSet Data
* DataTable Data
* DataRow Data
* DataColumn Differences
* LinearGradient
* HashSet
* URI
* IPEndPoint (Supported for everything except .NET Standard 1.0)
* Types of Type (RuntimeType)
* StringBuilder
* SByte

# Limitations
* Custom Collections with Non-Integer Indexers cannot be compared.
* Private properties and fields cannot be compared for .NET Core 1.3.  They are allowed to be compared in .NET Core 2.0 and higher.
* When ignoring the collection order, the collection matching spec must be a property on the class.  It cannot be a field or a property  on a child or parent class.  The property has to be a simple type.
* COM Objects are not compared.  To compare COM objects wrap their properties in a .NET Object or create a <a href="https://github.com/GregFinzer/Compare-Net-Objects/wiki/Custom-Comparers">custom comparer</a>.  Also See:  https://stackoverflow.com/questions/9735394/reflection-on-com-interop-objects


# Getting Started
https://github.com/GregFinzer/Compare-Net-Objects/wiki/Getting-Started

# Help File
https://github.com/GregFinzer/Compare-Net-Objects/blob/master/Compare-NET-Objects-Help/Compare-NET-Objects-Help.chm?raw=true

# Licensing
https://github.com/GregFinzer/Compare-Net-Objects/wiki/Licensing

# MauiBlazorBridge

MauiBlazorBridge is a Helper Utitlity That Makes Easier for Maui Blazor Hybrid Developers to Seamlessly Detect Platform, FormFactor and Etc..

# Get Started

**1.** In order to use the **MauiBlazorHybrid** you need to call the extension method in your **Program.cs** file as follows: 

```csharp
builder.Services.AddMauiBlazorBridge();
```

**2.** In the `_imports.razor` file, you need to import the namespace as follows:

```razor
@using MauiBlazorBridge
```

**3.** In `MainLayout.razor` file, you need to add `BridgeProvider` to Initialize the Bridge. Additionally Setting `GlobalListener` Property to True Makes Bridge to Listen to FormFactor Changes Globaly Instead of Creating and Disposing Listener During the Usage of `BridgeFormFactor` Component

```html
@inherits LayoutComponentBase
<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <article class="content px-4">
            @Body
        </article>
        <!-- Add BridgeProvider -->
        <BridgeProvider/>
    </main>
</div>
```

**Disclaimer:** When it comes PreRendering Enabled Blazor Flavor, You don't need to add `BridgeProvider` in `MainLayout.razor` file, Instead you need to add `BridgeProvider` in all the different Blazor Components that Utilizes Bridge. Additionally you need to Enable Interactivity for that Specific Component or Page

# Usage

```html
@inject IBridge Bridge

<BridgeFormFactor>
    <Mobile>
        <h3>FormFactor : Mobile</h3>
    </Mobile>
    <Tablet>
        <h3>FormFactor : Tablet</h3>
    </Tablet>
    <Desktop>
        <h3>FormFactor : Desktop</h3>
    </Desktop>
    <Default>
        <h3>FormFactor : Unknown</h3>
    </Default>
</BridgeFormFactor>

<h3>Platform : @Bridge.Platform</h3>
<h3>Platform Version : @Bridge.PlatformVersion</h3>
<h3>FrameWork : @Bridge.Framework</h3>
```

# Components

### BridgeFormFactor

```html
<BridgeFormFactor>
    <Mobile>
        <h3>FormFactor : Mobile</h3>
    </Mobile>
    <Tablet>
        <h3>FormFactor : Tablet</h3>
    </Tablet>
    <Desktop>
        <h3>FormFactor : Desktop</h3>
    </Desktop>
    <Default>
        <h3>FormFactor : Unknown</h3>
    </Default>
</BridgeFormFactor>
```

### BridgePlatform

```html
<BridgePlatform>
	<Android>
		<h3>Platform : Android</h3>
	</Android>
	<IOS>
		<h3>Platform : iOS</h3>
	</IOS>
	<Windows>
		<h3>Platform : Windows</h3>
	</Windows>
	<Mac>
		<h3>Platform : MacCatalyst</h3>
	</Mac>
	<Default>
		<h3>Platform : Unknown</h3>
	</Default>
</BridgePlatform>
 ```

 ### BridgeFramework

 ```html
 <BridgeFramework>
   <Maui>
    <h3>Framework : Maui</h3>
   </Maui>
   <Blazor>
   <h3>Framework : Blazor</h3>
   </Blazor>
 </BridgeFramework>
 ```

 # Note

 The Documentation is Under Construction, More Features and Components will be Added Soon.

 # License

 MauiBlazorBridge is licensed under the **MIT** license


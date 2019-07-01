
# Getting Started with stonehenge3

## Create new Console Project
**VisualStudio 2017 / 2019**

    File - New Project - Visual C# - .NET Core - Console App (.NET Core)

    Under project properties choose .NET Core 2.1 or 2.2 as target framework

**JetBrains Rider 2019**

    File - New... - .NET Core - Console Application

    Select Framework .NETCoreApp v2.1 or .NETCoreApp v2.2 as target framework

## Add stonehenge3 Nuget package

`Install-Package IctBaden.Stonehenge3`

## Add code to Main()

We have to decide what client (JavaScript) framework to use.    
This description uses [Vue.js](https://vuejs.org/).    
The server deploys embedded resources to the client. So we need to create a resource provider.    
Immediately adding the Vue provider enables delivering automaatically generated Vue code.
``` csharp
    var vue = new VueResourceProvider();
    var provider = StonehengeResourceLoader.CreateDefaultLoader(vue);
```
Now we define application title and a start page name and some other as hosting options.
``` csharp
    var options = new StonehengeHostOptions
            {
                Title = "Sample",
                StartPage = "start",
                ServerPushMode = ServerPushModes.LongPolling,
                PollIntervalMs = 1000
            };
```
Next we have to decide what hosting environment to use.    
This description uses [Kestrel], the .NET Core default stack        
(https://docs.microsoft.com/de-de/aspnet/core/fundamentals/servers/kestrel).    
``` csharp
    var host = new KestrelHost(provider, options);
```
Finally we have to start the server giving a listening address and a TCP port.
``` csharp
    //           Title     ListenOn     Port
    host.Start("Sample", "localhost", 32000);
```

Adding some console logging, error handling and termination code we should end up something like this.
``` csharp
    static void Main(string[] args)
    {
        Console.WriteLine(@"Sample starting");

        // client framework (use Vue.js)
        var vue = new VueResourceProvider();
        var provider = StonehengeResourceLoader.CreateDefaultLoader(vue);

        // options
        var options = new StonehengeHostOptions
        {
            Title = "Sample",
            StartPage = "start",
            ServerPushMode = ServerPushModes.LongPolling,
            PollIntervalMs = 1000
        };

        // hosting
        var host = new KestrelHost(provider, options);
        if (!host.Start("localhost", 32000))
        {
            Console.WriteLine(@"Failed to start server on: " + host.BaseUrl);
            Environment.Exit(1);
        }

        // wait for user pressing Ctrl+C to terminate
        var terminate = new AutoResetEvent(false);
        Console.CancelKeyPress += (sender, eventArgs) => { terminate.Set(); };
        Console.WriteLine(@"Started server on: " + host.BaseUrl);
        terminate.WaitOne();
        Console.WriteLine(@"Server terminated.");
        host.Terminate();
    }
```

## Adding the initial content
By convention stonehenge looks for all content pages in a folder named ```app```. This is true for file and resource based content. This sample uses embedded resources to store all content.

Create a folder in your project named ```app```.

Given ```start``` as the entry point in the InitProvider method create a HTML file named ```start.html``` within the ```app``` folder.     
Mark the file as *Embedded Resource* in the file's properties pane.

Enter file content. As we use Vue the page has to be a div.
```html
<div>
    <p>Hello Client!</p>
</div>
```

## Adding the corresponding server side ViewModel
Every client side page or component needs to have a corresponding server side ViewModel.

Create a folder in your project named ```ViewModels```.

Create a class named ```StartVm``` in folder ```ViewModels```.    
By convention stonehenge looks for a class named as the html resource followed by "Vm" as postfix. This can be overwritten if desired.

The class must derive from the stonehenge class ActiveViewModel.
``` csharp
    public class StartVm : ActiveViewModel
    {
    }
```

## First run
This is it. We are ready for the first run!

Start the application. The output should be as follows:

        Sample starting
        Hosting evironment: Production
        Content root path: <Debug directory of this project>
        Now listening on: http://127.0.0.1:32000
        Started server on: http://localhost:32000

Now start your browser and navigate to http://localhost:32000

After a short splash screen "Sample loading..." you shold see the following SPA application:
![Sample first startup](Sample1.png)


## Add server side content
To add some server side content, let's add the computer's name.

Add the following property to StartVm:
``` csharp
    public string ComputerName => Environment.MachineName;
```

To display this in the view (start.html) add a binding in the page like this:
```html
<div>
    <p>Hello Client from computer {{ComputerName}}!</p>
</div>
```

Starting the application now will result in displaying

        Hello Client from computer WIN10PC! 

where "WIN10PC" will be replaced with your computer's name.

## Add client side actions
To do some interaction from the client side, let's add a button and count how often it is pressed.

Add the following property to StartVm:
``` csharp
    public int CountPressed { get; private set; }
```

And the following method marked as ActionMethod by using stonehenge's attribute:
``` csharp
    [ActionMethod]
    public void ButtonPressed()
    {
        CountPressed++;
    }
```

In start.html add the following two lines within the div.
```html
    <p>Button pressed {{CountPressed}} times.</p>
    <p><button v-on:click="ButtonPressed()">Click Me</button></p>
```
The ```v-on:click``` is the Vue way to bind a click handler.

Start the application to see it work.    
**Note:** You may need to clear the cache to reload the updated content !

![Sample with click counter](Sample2.png)
(after clicked the button five times)


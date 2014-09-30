unity-tools
===========

[![Build Status](https://travis-ci.org/NoxHarmonium/unity-tools.svg?branch=master)](https://travis-ci.org/NoxHarmonium/unity-tools)

Unity-tools is a toolkit for scripting in [Unity3D](http://unity3d.com/) which greatly streamlines asychronous tasks, particularly with HTTP requests. 

Unity-tools is made up of:
- UnityTask - Composable Asynchronous Tasks For Unity
- UnityDispatcher - A Unity Thread Dispatcher
- UnityAgent - A Unity REST API Client
- [litjson](http://lbv.github.io/litjson/) - A third party JSON parser used by the REST client

The project goals are:
- Code that is high quality and ready for professional environments
- Code that is thoroughly unit tested
- Code that is decoupled from Unity's (as much as possible)
- Code that is tested on all devices that Unity supports

UnityTask - Composable Asynchronous Tasks For Unity
---------------------------------------------------
In leue of the .NET 4.5 async methods and Task framework that are missing in Unity's version of Mono, I decided to implement a system to compose asynchronous tasks in a manageable way.

It is an implementation of the [Promises/A+](http://promises-aplus.github.io/promises-spec/) specification which is used by the [q](https://github.com/kriskowal/q) framework.

When an asynchronous method is run, it returns immediately with a promise (in this case a UnityTask object) that the task will either succeed or fail in the future. This future object allows callbacks to be added to handle these events. Promises can also be combined into other promises allowing powerful compositions.

#### Creating an asynchronous method
```c#
public UnityTask<string> DownloadFile()
{
    return new UnityTask<string>( (task) =>
    {
        // This code executes on a new thread
        string result = SomeBlockingDownloadMethod();
        // Uncaught exceptions will get caught internally and reject the task automatically
        task.Resolve(result);
    });
}
```
    

#### The same thing as above with less sugar
```c#
public UnityTask<string> DownloadFile()
{
    // Create promise
    UnityTask<string> task = new UnityTask<string>();

    // Run something asynchronously
    new Thread( () => { 
        // Need to try catch here as the thread is not wrapped by the task like the above example.
        try
        {
            string result = SomeSlowBlockingDownloadMethod();
            task.resolve(result);
        }
        catch(Exception ex)
        {
            task.Reject(ex);
        }
    }).Start();

    return task;
}
```

####  Using the asynchronous method, all callbacks are optional
```c#
DownloadFile().Then(
    onFulfilled: result  => Debug.Log("Download data: " + result),
    onFailure: ex        => Debug.Log("Oh No an exception occurred."),
    onProgress: p        => Debug.Log("Progress changed: " + p),
    onEnd:               => Debug.Log("Clean up temporary files.") // Run regardless of outcome
);
```
    
#### Notifying progress
```c#
public UnityTask<string> CopyFiles(string sourceDir, string destDir)
{
    return new UnityTask( (task) =>
    {
        int count = 0;
        for (var sourceFile = Directory.EnumerateFiles(sourceDir))
        {
            File.Copy(sourceFile, destDir + File.GetFileName(sourceFile));
            // Trigger the onProgress callback
            task.Notify(count / (float) files.length);
            count++;
        }
        task.Resolve();
    }
}
```

####  Forcing synchronicity
```c#
try
{
    // The Result property blocks until the task ends
    string result = DownloadFile().Result;
    Debug.Log("Download data: " + result),
}
catch (Exception e)
{
     // The Result property also propagates exceptions from UnityTask.Reject()
     Debug.Log("Oh No an exception occurred.");
}

Debug.Log("Clean up temporary files.");
```

#### Composing tasks in parallel
```c#
var DownloadAllFiles = UnityTask.All(
    DownloadFile1(),
    DownloadFile2(),
    DownloadFile3()
).Then(
    onFulfilled: results    => Debug.Log("All files successfully downloaded."),
    onFailure: ex           => Debug.Log("Oh No an exception occurred.").
    onEnd:                  => Debug.Log("Clean up temporary files.")
);
```

#### Getting the results of parallel tasks
```c#
var DownloadAllFiles = UnityTask<string>.All(
    DownloadFile1(),
    DownloadFile2(),
    DownloadFile3()
).Then(
    onFulfilled: results    => {
        foreach (var result in results)
        {
            Debug.Log("Result: " + result);
        }
    },
    onFailure: ex           => Debug.Log("Oh No an exception occurred.").
    onEnd:                  => Debug.Log("Clean up temporary files.")
);
```

#### Composing tasks sequentially
```c#
// AllSequential takes in lambda functions that return the task.
// The functions are executed in order
var DownloadAllFiles = UnityTask.AllSequential(
    () => DownloadFile1(), 
    () => DownloadFile2(),
    () => DownloadFile3()
).Then(
    onFulfilled: o  => Debug.Log("All files successfully downloaded."),
    onFailure: ex   => Debug.Log("Oh No an exception occurred.").
    onEnd:          => Debug.Log("Clean up temporary files.")
);
```

#### Nested chaining
```c#
var DownloadAllFiles = UnityTask.All(
    DownloadFile1().Then(() => Debug.Log("File 1 is done!")),
    DownloadFile2().Then(() => Debug.Log("File 2 is done!")),
    DownloadFile3().Then(() => Debug.Log("File 3 is done!"))
).Then(
    onFulfilled: results    => Debug.Log("All files successfully downloaded."),
    onFailure: ex           => Debug.Log("Oh No an exception occurred.").
    onEnd:                  => Debug.Log("Clean up temporary files.")
);
```

#### Nested Alls
```c#
var DownloadAllFiles = UnityTask.All(
    DownloadFile1(),
    UnityTask.AllSequential(
        () => DownloadFile2(),
        () => DownloadFile3()
    ).Then(() => Debug.Log("Files 2 and 3 are done!")),
    DownloadFile4(),
    DownloadFile5()
).Then(
    onFulfilled: results    => Debug.Log("All files successfully downloaded."),
    onFailure: ex           => Debug.Log("Oh No an exception occurred.").
    onEnd:                  => Debug.Log("Clean up temporary files.")
);
```

UnityDispatcher - A Unity Thread Dispatcher
-------------------------------------------

Unity is very fussy with the threads you call certain methods from. Method that call into the engine, and even properties such as Application.persistentDataPath need to be executed from the main Unity thread (i.e. from a MonoBehaviour.Update() ).
A dispatcher executes code on the main thread so that the Unity API can be called safely.

You have to make sure that the UnityDispatcher MonoBehavour is attached to an active GameObject in the scene, otherwise the dispatcher will not execute actions.

#### Basic dispatching
```c#
void MethodCalledFromOtherThread()
{
    Debug.Log("I will execute first.");

    UnityDispatcher.Instance.Dispatch( () => 
    {
        // Executed on next update cycle
        Debug.Log("I will execute third.");
        Texture2D.EncodeToPNG("./test.png");
    });

    Debug.Log("I will execute second.");
}
```

#### Blocking dispatching
```c#
void MethodCalledFromOtherThread()
{
    Debug.Log("I will execute first.");

    // DispatchWait blocks the current thread until the action has executed.
    // This creates a continuation.
    UnityDispatcher.Instance.DispatchWait( () => 
    {
        // Executed on next update cycle
        Debug.Log("I will execute second.");
        Texture2D.EncodeToPNG("./test.png");
    });

    Debug.Log("I will execute third.");
}
```

#### Integration with tasks
```c#
var task = new UnityTask<string>( (task) =>
    {
        string result = SomeBlockingDownloadMethod();
        task.Resolve(result);
    }, 
    // You can pass an object that implements IDispatcher and callbacks will be automatically dispatched
    UnityDispatcher.Instance
); 

task.Then( 
    onSucess: () => Debug.Log("This will be called on the dispatch thread"))
);
```

UnityAgent - A Unity REST API Client
------------------------------------

I really liked the style of the REST client that I use in Node.js called [SuperAgent](http://visionmedia.github.io/superagent/) so I took heavy inspiration from it.

I have used UnityTasks to implement all the functionality so it is easy to compose API tasks.

I have found a good JSON library that works well with Unity and I am integrating it seamlessly with UnityAgent. For now you can only send and receive string data.

#### Simple Gets
```c#
new UnityAgent()
    .Get("www.google.com")
    .Begin()
    .Then(
        (response)  => Debug.Log(response.Body),
        (ex)        => Debug.Log("An error occurred")
    );
```

#### JSON Gets
```c#
new UnityAgent()
    .Get("http://httpbin.org/get")
    .Begin()
    .Then(
        (response)  => Debug.Log((string)response.JSON["some_key"]),
        (ex)        => Debug.Log("An error occurred")
    );
```

#### Simple Posts
```c#
new UnityAgent()
    .Post("http://httpbin.org/post")
    .Send("Hello")
    .Begin()
    .Then(
        onFailure: (ex) => Debug.Log("An error occurred")
    );
```

#### Composing Calls
```c#
var agent = new UnityAgent();
UnityTask<string>.All(
    agent.Get("www.url1.com").Begin(),
    agent.Get("www.url2.com").Begin(),
    agent.Get("www.url3.com").Begin()
).Then(
    onFulfilled: (responses)  => {
       // The All method returns an array of results
       foreach (var response in responses)
       {
            Debug.Log(response.Body);            
       } 
    },
    onFailure: (ex) => Debug.Log("An error occurred")
);
```

Build Environment
-----------------

There are two seperate build environments that share the same code base. UnityProject is for testing the code with the Unity3D compiler and integrating it with Unity3D projects. XamarinProject can be opened in Xamarin Studio and will compile without Unity3D with the use of harness classes that pretend to be Unity but don't actually do anything (see FakeUnity.cs). As the project doesn't actually depend on Unity functionallity it is fine for testing. 

I recommend using Xamarin for most of the development and only switching to Unity to make sure the code compiles there.

I am  unit testing heavily, I haven't achieved 100% code coverage yet but I am working on it.

Contribute
----------
If you have essential building blocks that help you develop in Unity that you wish to share, please do!

Todo List
---------
- [   ] Write more unit tests
- [   ] Test on multiple devices
- [   ] Think of more essential Unity tools!

Licence
-------

MIT Licence





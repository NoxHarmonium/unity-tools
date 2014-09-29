unity-tools
===========

Where I work, we use [Unity3D](http://unity3d.com/) quite frequently and it constanly amazes how hard it is to find something simple such as a rest API client or JSON serializer library thats open source and maintained. It just adds to my frustration when I find a feature of the .NET framework that would be perfect for the job but is partially implemented, broken or missing in Unity's version of Mono. To remedy this I want to start a toolbox of essential tools that will be high quality, open source and tested that people can grab and throw into their Unity project and not have to waste development time building tools that should be already there.

Goals
- Code that is high quality and ready for professional environments
- Code that is thoroughly unit tested
- Code that is decoupled from Unity's (as much as possible)
- Code that is tested on all devices that Unity supports

UnityTask - Composable Asynchronous Tasks For Unity
---------------------------------------------------
In leue of the .NET 4.5 async methods and Task framework that are missing in Unity's version of Mono, I decided to implement a system to compose asynchronous tasks in a managable way.

It is an implementation of the [Promises/A+](http://promises-aplus.github.io/promises-spec/) specification which is used by the [q](https://github.com/kriskowal/q) framework.

When an asyncronous method is run, it returns immediately with a promise (in this case a UnityTask object) that the task will either suceed or fail in the future. This future object allows callbacks to be added to handle these events. Promises can also be combined into other promises allowing powerful compositions.

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
    onFailure: ex        => Debug.Log("Oh No an exeception occurred."),
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
     // The Result property also propogates exceptions from UnityTask.Reject()
     Debug.Log("Oh No an exeception occurred.");
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
    onFailure: ex           => Debug.Log("Oh No an exeception occurred.").
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
    onFailure: ex           => Debug.Log("Oh No an exeception occurred.").
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
    onFailure: ex   => Debug.Log("Oh No an exeception occurred.").
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
    onFailure: ex           => Debug.Log("Oh No an exeception occurred.").
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
    onFailure: ex           => Debug.Log("Oh No an exeception occurred.").
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

I really liked the style of the REST client that I use in Node.js called [SuperAgent](http://visionmedia.github.io/superagent/) so I took heavy insperation from it.

I have used UnityTasks to implement all the functionality so it is easy to compose API tasks.

I am still searching for a good JSON library that works well with Unity and when I do I'll integrate it seemlessly with UnityAgent. For now you can only send and recieve string data.

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

I like to be able to build my code separately to Unity as I can compile my code without constantly switching to Unity and back. To make sure that the code will work in Unity's version of Mono, I added a post build task that compiles the codebase in Unity's version of mcs. This fails the build if you write code that is not supported by Unity. At the moment it is not very flexible as it is hardcoded to my Unity installation location and assumes you are running OS X / can run bash scripts but I'm working on it. You can change the paths in 'unity-check.sh'. Remove the post build task from the solution property window if you are having issues.

I have also added the --aot-only switch to the compiler and added a post-build IL stripping task to try and pick up issues to do with Mono JITing code which is not supported on iOS.

I am also unit testing heavily, I haven't achieved 100% code coverage yet but I am working on it.

Contribute
----------
If you have essential building blocks that help you develop in Unity that you wish to share, please do!

Why Is The Solution File Called UPM?
------------------------------------
When I started developing UnityTools I wanted it to be part of a package system called Unity Package Manager which I was going to model off the [Node Package Manager](https://npmjs.org/) or the [Sublime Text Package Control](https://sublime.wbond.net/). I havn't completly given up on the idea, it would be nice to have a collection of high quality developer contributed open source libraries for Unity but it is less useful in the short term.

Todo List
---------
- [  ] Implement / use existing JSON library that works on all platforms
- [  ] Integrate JSON library with UnityEngine
- [  ] Integrate with [Travis CI](https://travis-ci.org/). (Yes it is possible!)
- [  ] Write more unit tests
- [  ] Test on multiple devices
- [  ] Think of more essential Unity tools!

Licence
-------

MIT Licence





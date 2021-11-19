`cl2j.DataStore` is a multi-providers CRUD repoository abstraction library written in C#. It's an open and extensible framework based on interfaces and Dependency Injection.

Providers supported:

- `JSON` - Data store that use a JSON file for CRUD

# Getting started

The setup is simple.

1. Add the nuget package to your project:

```powershell
Install-Package cl2j.DataStore.Json
```

2. Add the following lines in the appsettings.json to configure the IFileStoreProvider:

```json
"cl2j": {
    "FileStorage": {
        "Storages": {
            "DataStore": {
                "Type": "Disk",
                "Path": "<ThePathOfTheJSONFiles>"
            }
        }
    }
}
```

The settings configure a Disk FileStorage provider, which use `System.IO` classes.
Path is the local path, i.e. c:\dev, where the json file will be.

> cl2j.DataStore use [cl2j.FileStorage](https://github.com/jfblier/cl2j.FileStorage) library to abstract the access to the file system. This allow to use different providers like Disk, Azure, AWS, etc.

3. Configures the services by calling `AddFileStorage()` and then `AddDataStoreJson()`

4. Get the `IDataStore` configured and perform CRUD operatons.

`AddDataStoreJson` receive three parameters:

1. The name of the FileStore to use. That's the name declared in the appsettings.json.
2. The name of the file were the data are persisted.
3. A predeicate that tell the DataStore how to get a item by it's id.

## Web Application

```cs
public void ConfigureServices(IServiceCollection services)
{
  ...

  //Bootstrap the FileStorage to be available from DependencyInjection.
  //This will allow accessing IFileStorageProviderFactory instance
  services.AddFileStorage();

  //Configure the JSON DataStore.
  //The store will use the FileStorageProvider to access the file colors.json.
  services.AddDataStoreJson<string, Color>("DataStore", "colors.json", (predicate) =>
  {
      return predicate.Item1.Id == predicate.Item2;
  });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
  ...

  //Add the Disk FileStorage provider
  app.ApplicationServices.UseFileStorageDisk();
}
```

## Console Application

```cs
var services = new ServiceCollection();
...

//Bootstrap the FileStorage to be available from DependencyInjection.
//This will allow accessing IFileStorageProviderFactory instance
services.AddFileStorage();

//Configure the JSON DataStore.
//The store will use the FileStorageProvider to access the file colors.json.
services.AddDataStoreJson<string, Color>("DataStore", "colors.json", (predicate) =>
{
    return predicate.Item1.Id == predicate.Item2;
});

//Add the Disk FileStorage provider
var serviceProvider = services.BuildServiceProvider();
serviceProvider.UseFileStorageDisk();
```

# Sample code

Here's a class that execute common operations.

```cs
internal class DataStoreSample
{
    private readonly IDataStore<string, Color> dataStoreColorRead;

    public DataStoreSample(IDataStore<string, Color> dataStoreColorRead)
    {
        this.dataStoreColorRead = dataStoreColorRead;
    }

    public async Task ExecuteAsync()
    {
        //Create colors
        await dataStoreColorRead.InsertAsync(new Color { Id = "red", Value = "#f00" });
        await dataStoreColorRead.InsertAsync(new Color { Id = "green", Value = "#0f0" });
        await dataStoreColorRead.InsertAsync(new Color { Id = "blue", Value = "#00f" });
        await dataStoreColorRead.InsertAsync(new Color { Id = "cyan", Value = "#0ff" });
        await dataStoreColorRead.InsertAsync(new Color { Id = "magenta", Value = "#f0f" });
        await dataStoreColorRead.InsertAsync(new Color { Id = "yellow", Value = "#ff0" });
        await dataStoreColorRead.InsertAsync(new Color { Id = "black", Value = "#000" });

        //Retreive all the colors
        var colors = await dataStoreColorRead.GetAllAsync();
        foreach (var color in colors)
            Debug.WriteLine($"{color.Id} -> {color.Value}");

        //Retreive the color black and update it's value
        var black = await dataStoreColorRead.GetByIdAsync("black");
        if (black != null)
        {
            black.Value = "#111";
            await dataStoreColorRead.UpdateAsync(black.Id, black);
        }

        //Delete a color
        await dataStoreColorRead.DeleteAsync("magenta");
    }
}
```

The class receive, by Dependency Injection, the `IDataStore`. The ExecuteAsync method insert colors, retreive all the colors inserted, modify the black color value and remove the magenta color.

A file named `colors.json` was created:

```json
[
  {
    "Id": "red",
    "Value": "#f00"
  },
  {
    "Id": "green",
    "Value": "#0f0"
  },
  {
    "Id": "blue",
    "Value": "#00f"
  },
  {
    "Id": "cyan",
    "Value": "#0ff"
  },
  {
    "Id": "yellow",
    "Value": "#ff0"
  },
  {
    "Id": "black",
    "Value": "#111"
  }
]
```

# Operations

Here's the operations available from the `IDataStore` interface:

```cs
public interface IDataStore<TKey, TValue>
{
    //Retreive all the items
    Task<IEnumerable<TValue>> GetAllAsync();

    //Get an item by it's key (Id)
    Task<TValue> GetByIdAsync(TKey key);

    //Insert a new item. The key must not exists
    Task InsertAsync(TValue entity);

    //Update an item
    Task UpdateAsync(TKey key, TValue entity);

    //Delete an item
    Task DeleteAsync(TKey key);
}
```

# Feedback & Community

We look forward to hearing your comments.
Feel free to submit your opinion, any problems you have encountered as well as ideas for improvements, we would love to hear it.

If you have a technical question or issue, please either:

- Submit an issue
- Ask a question on StackOverflow
- Contact us directly

# Roadmap

- Add new implementation
  - `Database` implementation using reflection to create, modify and delete objects
- `CQRS` Use CQRS parttern to decouple the read and write. This will allow
- `Cache` Add a simple builtin caching mecanism to have performant read DataStore
- Helper to convert to Dictionary DataStore

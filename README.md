# Bolton Cup

## Intro

This is the repo for the software tools developed for the Bolton Cup hockey tournament. It contains the website and mobile app along with the scoresheet and draft tool.

## Website URL

[https://boltoncup.ca](https://boltoncup.ca)


## Using the shared library `BoltonCup.Shared`

Add to `_Imports.razor`:

```razor
@using BoltonCup.Shared.Data
@using BoltonCup.Shared.Components.Shared
```

Add to the relevant sections of `Program.cs`:

```c#
using BoltonCup.Shared.Data;
```

```c#
builder.Services.AddBoltonCupServices(builder.Configuration);
```
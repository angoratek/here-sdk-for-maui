---
name: Feature request
about: Request a new wrapper type, method, or platform support
title: "[feature] "
labels: ["enhancement", "triage"]
assignees: []
---

## What you'd like added

One-sentence description.

## Underlying HERE SDK API

Link to the [HERE SDK Explore reference](https://developer.here.com/) for the
type or method you want wrapped. This repo only re-exposes a subset of the
underlying SDK — features must already exist in the native API to be wrapped.

## Proposed C# surface

Sketch the C# API you'd like to see, e.g.:

```csharp
public interface INewService
{
    Task<NewResult> DoSomethingAsync(NewOptions options);
}
```

## Use case

What app are you building? Why do you need this wrapper?

## Alternatives considered

Have you considered workarounds with the existing API?

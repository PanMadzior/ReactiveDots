# ReactiveDots
ReactiveDots is a set of utilities for Unity Data Oriented Tech Stack (DOTS). Its purpose is to help developers write common types of systems with a minimum of boilerplate code. ReactiveDots use Roslyn source generators to achieve that.

Highly inspired by [Entitas](https://github.com/sschmid/Entitas-CSharp), ECS framework for Unity.

Current version is not battle-tested. It is not recommended to use it in production. Use at your own risk.

#### Note from developer
On daily basis I work with Entitas, where reactive systems are a core feature. From time to time, in my spare time, I experiment with DOTS. ReactiveDots is such an experiment. My goal here is to transfer my workflows from Entitas to DOTS.

## Table of contents
1. [Installation](#installation)
2. [Features](#features)
   1. [ReactiveSystems](#reactive-systems)
      1. [Boilerplate code](#boilerplate-code-for-reactive-system)
      2. [How reactive systems work under the hood?](#how-reactive-systems-work-under-the-hood)
      3. [Reactive update methods](#reactive-update-methods)
   2. [Event Listeners](#event-listeners)
      1. [Boilerplate code](#boilerplate-code-for-event-listener)
      2. [Self vs Any listeners](#self-vs-any-events)
      3. [Custom event system](#custom-event-system)
      4. [How event systems work under the hood?](#how-event-systems-work-under-the-hood)
3. [Known issues and limitations](#known-issues-and-limitations)
4. [Planned features](#planned-features)

## Installation
Simply import ReactiveDots unity package from releases to your project. If there are some compilation errors you may have to restart Unity, should work after.

For source generators to work you have to have a fairly updated version of Visual Studio or Rider.

Tested on **Unity 2022.2.0b10**, **Entities 1.0.0-exp.12** and **Rider 2022.1.1.**

For Entities 0.50.0-preview.24 use ReactiveDots v0.2.0-preview.1.

# Features
You can play around with the implemented features in a sample project.

## Reactive Systems
Add systems that iterate over the entities when their component is added/removed/changed or enabled/disabled since the last frame.

```csharp
// React when a MoveDirection component is added/removed/changed.
[ReactiveSystem( typeof(MoveDirection), typeof(MoveDirectionReactive) )]
public partial class BounceCountSystem : SystemBase
{
    // Cache reactive data in a separate system state component.
    public struct MoveDirectionReactive : ISystemStateComponentData
    {
        public ComponentReactiveData<MoveDirection> Value;
    }

    protected override void OnUpdate()
    {        
        Entities.ForEach( ( ref Bounces bounces, in MoveDirectionReactive moveDirReactive ) =>
        {
            // React only to the changes in the MoveDirection component
            // and ignore when it is added to an entity.
            if ( !moveDirReactive.Value.Added && moveDirReactive.Value.Changed )
                bounces.Value += 1;
        } ).ScheduleParallel();
        
        // Update reactive data with auto-generated extension method.
        Dependency = this.UpdateReactive( Dependency );
    }
}
```
### Boilerplate code for reactive system
In order to use reactive systems you have to add a minimal set of boilerplate code to your system.

You have to:
1. Add a `[ReactiveSystem(typeof(A), typeof(B))]` attribute.
   1. Specify a component you want to react to in first argument (*A*).
   2. Specify a reactive data component (*B*) from step 2.
2. Add a simple reactive data cleanup component with a field of type `ComponentReactiveData<T>`, where *T* is the type of the component you want to react to (*A*).
3. Add `Dependency = this.UpdateReactive( Dependency );` at the end of the `OnUpdate()`.

#### FieldNameToCompare
Optionally, you can specify a name of a component field you want to use when deciding whether the component changed its value or not. Set it with *FieldNameToCompare* argument: `[ReactiveSystem( typeof(A), typeof(B), FieldNameToCompare = "CustomField" )]`. Default is `"Value"`.

You can specify multiple fields by separating them with comma: `FieldNameToCompare = "Field_0,Field_1"`.

For tag components use empty string: `FieldNameToCompare = ""`.

### How reactive systems work under the hood?

Source generators adds extra code to manage systems. You can inspect it in your IDE. Easiest way is to go to the declaration of the auto-generated extension method `UpdateReactive()`. Overall idea works like this:
- User marks system to be reactive system.
- User adds `ICleanupComponentData` component that will cache main component reactive data.
- When the main component is added to any entity that hasn't got the cache component, the cache component is added to the entity in auto-generated job. Cache component `.Added` value is set to true and the actual value of the main component is cached in the `.PreviousValue` field.
- Auto-generated parallel job (`IJobChunk`) checks every update if the actual value of the main component is different than the cached `.PreviousValue`. If it is, `.Changed` field is set to true, otherwise to false. The actual value is yet again cached in the cache component.
- If other auto-generated job finds an entity with the cache component but without the main one, it means that the component was removed or the entity was destroyed. In this case `.Removed` field in the cached component is set to true.
- Enabling and disabling components also trigger `.Added` and `.Removed` states change.

Actual and cached values are compared only on one or more specified fields, `.Value` by default. If any other field is changed, the system does not take it as a change. For tag components, as they have no real value, nothing is compared and `.Changed` is always set to `false`.

You should be aware, that the cache component holds duplicate of the whole main component. If there are maaany reactive systems (and components they manage), it might have a bad impact on your game's memory usage.

Default way is to create a new reactive system when you want to know when the component changed. This way changes are tracked per entity per system. Such design may be memory expensive because you create a new reactive component per system. If you don't need that much precision, you can make one system for the given component and use its reactive data component in multiple other systems. It this case you can place the reactive system just before its ECB system. This way all the systems after the ECB one will have an actual info about the component.

### Reactive update methods

There are several update methods for reactive systems. Their differences are mainly how they manage structural changes when adding or removing the cache component.

```csharp
// Update reactive data in parallel jobs using ECB from EndSimulationEntityCommandBufferSystem.
// This is a default and recommended way of updating.
// IMPORTANT NOTE: .Added and .Removed will be accessible with a one frame delay in the reactive system.
// If you need to know if component was added without a one frame delay, use Entities.WithNone<ReactiveComp>.
// If you need to know if component was removed without a one frame delay, use Entities.WithNone<Comp>.WithAll<ReactiveComp>.
Dependency = this.UpdateReactive( Dependency );

// Update reactive data in parallel jobs using ECB from a system passed in the argument.
// This is similar to the previous one but lets you choose which system playbacks structural changes.
Dependency = this.UpdateReactive( Dependency, YourEntityCommandBufferSystem );

// Update reactive data in parallel jobs using ECBs from the arguments.
// Be aware that .Added and .Removed may also be available with a one frame delay.
// It depends on when you playback the structural changes.
Dependency = this.UpdateReactive( Dependency, EcbForAdded, EcbForRemoved );

// Update reactive data using a system's EntityManager on the main thread.
// This method adds a sync point and is strongly not recommended.
Dependency = this.UpdateReactiveNowWithEntityManager( Dependency );

// Update reactive data in parallel jobs using a temporary EntityCommandBuffer.
// This method also adds a sync point and is not recommended.
// This method should be bbetter than the previous when you have many adds and removes per frame.
Dependency = this.UpdateReactiveNowWithEcb( Dependency );
```

## Event Listeners
Event listeners allow types outside of the ECS to react to component changes. Events are build on top of the reactive systems. Each listener implements an auto generated interface and register itself to listen to the desired event by creating an entity with a special component with a reference to the instance of said listener. 
```csharp
[ReactiveEvent] // Mark component with ReactiveEvent attribute.
public struct Foo : IComponentData
{
    public int Value;
}

public class Bar : MonoBehaviour,
    IAnyFooAddedListener, // Implement added/changed/removed auto generated interface.
    IAnyFooChangedListener, 
    IAnyFooRemovedListener
{
    private void Awake()
    {
        var entityManager  = World.DefaultGameObjectInjectionWorld.EntityManager;
        // Create listener entity.
        var listenerEntity = entityManager.CreateEntity();
        // Add auto generated event components to register your listeners.
        entityManager.AddComponentData( listenerEntity, new AnyFooAddedListener() { Value = this } );
        entityManager.AddComponentData( listenerEntity, new AnyFooRemovedListener() { Value = this } );
        entityManager.AddComponentData( listenerEntity, new AnyFooChangedListener() { Value = this } );
    }
    
    // React when any entity got Foo component added...
    public void OnAnyFooAdded( Entity entity, Foo foo, World world )
    {
        Debug.Log( $"New foo component with value={foo.Value}" );
    }
    
    // ...or changed...
    public void OnAnyFooChanged( Entity entity, Foo foo, World world )
    {
        Debug.Log( $"Foo component updated value={foo.Value}" );
    }
    
    // ...or removed.
    public void OnAnyFooRemoved( Entity entity, World world )
    {
        Debug.Log( "Foo component removed!" );
    }    
}
```
### Boilerplate code for event listener
In order to create listeners you have to do following steps.
1. Mark your component with [ReactiveEvent] attribute.
   1. Optionally you can specify what event types you want to generate with an `EventType` parameter (all by default).
   2. Optionally you can change an event system that handles component changes and event fires. Default is `ReactiveDots.DefaultEventSystem` that updates in `LateSimulationSystemGroup`. Read more about event systems below.
2. Listener interfaces will be auto generated. Implement them on any class.
3. Create an entity for your listener.
4. Add auto generated components corresponding to the implemented listeners to your entity.

### Custom event system
By default all event listeners are managed by `ReactiveDots.DefaultEventSystem` that updates in `LateSimulationSystemGroup`. You can add custom event systems and control when they are updating and firing events. Copy the default one and adjust it to your needs.

Use an attribute constructor when marking event components to change which system should manage it: `[ReactiveEvent( EventType.All, typeof(Your.Custom.EventSystem) )]`.

### Events for components you can't alter directly
If you want to make event listeners for components which you can't alter directly with `[ReactiveEvent]` attribute, use `[ReactiveEventFor]` instead.
Place it on any type and source generators will find it and do the magic. Example below.
```csharp
// Foo component, that you can't alter directly.
public struct Foo : IComponentData
{
    public int Value;
}

// ReactiveEventFor for component Foo on a random class Bar.
[ReactiveEventFor( typeof(Foo) )]
public class Bar
{
    // ...
}
```

### Self vs Any events
*Feature not yet implemented.*

Difference is simple. `Any` event listeners react to changes in all entities, when `Self` event listeners react only to changes in the same entity the listener component is attached to. In other words, `Self` listeners react to changes in the specified entities.

### How event systems work under the hood?
Event systems are built on top of the reactive systems. Check [how reactive systems work](#how-reactive-systems-work-under-the-hood) first. After the update of the reactive component data, event systems loops over entities with auto-generated event components and invokes methods of the interfaces referenced in the components.

#### Main thread
Event listener components are managed components because they hold interface instance references. Therefore invoking interface methods have to be done on the main thread. Invoking them in jobs doesn't make much sense anyway. Bear in mind that many events, especially these which fires frequently, may have bad impact on your game performance. Also, be aware that event systems make sync points by default.

# Known issues and limitations
- You can only react to changes of simple unmanaged `IComponentData` components and maybe `ICleanupComponentData`. Managed components won't work. There is no support for buffers either.
- Reactive system's internal `IEntityBatchJob`, that checks for component changes, use some unity internal methods (like `InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr`) for getting pointers to the component arrays. This way the job's performance is the same (or almost the same) as unity generated foreaches. I'm not sure if I should use said methods or something different. It works tho.
- `EventType` in event attributes is not yet implemented. All events are generated with `EventType.All` for now.
- Reactive components have to be written manually. You cannot generate components if you want to use them in ecs foreaches. Unity source generators do some magic on the component types under the hood. This forces some extra boilerplate code.
- Some of the generated code are static singletons which hurt my heart. Would be great to make them more manageable and possibly without singletons.
- Some of the generated code needs refactor.
- Only systems deriving from `SystemBase` are supported. `ISystem`-based are not yet supported.
- ...and for sure there is some more problems I am not yet aware of.

# Planned features
There are some features that may come to the ReactiveDots in the future.
- **Self events** which react only to changes in the specified entities.
- **Cleanup** systems that will help destroying entities with selected components or removing these components.
- **Benchmarks** which will help calculate how efficient generated systems run compared to the ones written by hand.
- **More samples** (if really needed).

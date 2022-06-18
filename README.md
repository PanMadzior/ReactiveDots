# ReactiveDots
ReactiveDots is a set of utilities for Unity Data Oriented Tech Stack (DOTS). Its purpose is to help developers write common types of systems with a minimum of boilerplate code. ReactiveDots use Roslyn source generators to achieve that.

Highly inspired by [Entitas](https://github.com/sschmid/Entitas-CSharp), ECS framework for Unity.

Current version is not battle-tested. It is not recommended to use it in production. Use at your own risk.

#### Note from developer
On daily basis I work with Entitas, where reactive systems are a core feature. From time to time in my spare time I experiment with DOTS. ReactiveDots is such an experiment. My goal here was to transfer my workflows from Entitas to DOTS.

## Table of contents
1. [Features](#features).
   1. [ReactiveSystems](#reactive-systems)
      1. [Boilerplate code](#boilerplate-code-for-reactive-system)
   2. [Event Listeners](#event-listeners)
      1. [Boilerplate code](#boilerplate-code-for-event-listener)
      2. [Self vs Any listeners](#self-vs-any-events)
      3. [Custom event system](#custom-event-system)
      4. [Main thread](#main-thread)
2. [Known issues and limitations](#known-issues-and-limitations)
3. [Planned features](#planned-features)

# Features
You can play around with the implemented features in a sample project.

## Reactive Systems
Add systems which iterate over the entities when their component is added/removed/changed since the last frame.

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
        // Update reactive data with auto-generated extension method.
        Dependency = this.UpdateReactive( Dependency );
        
        Entities.ForEach( ( ref Bounces bounces, in MoveDirectionReactive moveDirReactive ) =>
        {
            // React only to the changes in the MoveDirection component
            // and ignore when it is added to an entity.
            if ( !moveDirReactive.Value.Added && moveDirReactive.Value.Changed )
                bounces.Value += 1;
        } ).ScheduleParallel();
    }
}
```
### Boilerplate code for reactive system
In order to use reactive systems you have to add a minimal set of boilerplate code to your system.

You have to:
1. Add a `[ReactiveSystem(typeof(A), typeof(B))]` attribute.
   1. Specify a component you want to react to in first argument (*A*).
   2. Specify a reactive data component (*B*) from step 2.
2. Add a simple reactive data system state component with a field of type `ComponentReactiveData<T>`, where *T* is the type of the component you want to react to (*A*).
3. Add `Dependency = this.UpdateReactive( Dependency );` at the beginning of the `OnUpdate()`.

Optionally specify a name of a component field you want to use when deciding whether the component changed its value or not. Set it with *FieldNameToCompare* argument: `[ReactiveSystem( typeof(A), typeof(B), FieldNameToCompare = "CustomField" )]`. Default is `"Value"`. 

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
    public void OnAnyBouncesAdded( Entity entity, Foo foo, World world )
    {
        Debug.Log( $"New foo component with value={foo.Value}" );
    }
    
    // ...or changed...
    public void OnAnyBouncesChanged( Entity entity, Foo foo, World world )
    {
        Debug.Log( $"Foo component updated value={foo.Value}" );
    }
    
    // ...or removed.
    public void OnAnyBouncesRemoved( Entity entity, World world )
    {
        Debug.Log( "Foo component removed!" );
    }    
}
```
### Boilerplate code for event listener
In order to create listeners you have to do following steps.
1. Mark your component with [ReactiveEvent] attribute.
   1. Optionally you can specify what event types you want to generate with an `EventType` parameter (all by default).
   2. Optionally you can change an event system which handles component changes and event fires. Default is `ReactiveDots.DefaultEventSystem` which updates in `LateSimulationSystemGroup`. Read more about event systems below.
2. Listener interfaces will be auto generated. Implement them on any class.
3. Create an entity for your listener.
4. Add auto generated components corresponding to the implemented listeners to your entity.

### Custom event system
By default all event listeners are managed by `ReactiveDots.DefaultEventSystem` which updates in `LateSimulationSystemGroup`. You can add custom event systems and control when they are updating and firing events. Copy the default one and adjust it to your needs.

Use attribute constructor when marking event components to change which system should manage it: `[ReactiveEvent( EventType.All, typeof(Your.Custom.EventSystem) )]`.

### Self vs Any events
*Feature not yet implemented.*

Difference is simple. `Any` event listeners react to changes in all entities, when `Self` event listeners react only to changes in the same entity the listener component is attached to. In other words, `Self` listeners react to changes in the specified entities.

**Very important note:** When setting a custom event event system in the attribute **use a class name with full namespace path**. Since event systems and event components may be in different assemblies it is needed during a source generation phase to use a full name of the system.

### Main thread
Event listener components are managed components because they hold interface instance references. Therefore invoking interface methods have to be on the main thread. Invoking them in jobs doesn't make much sense anyway. Bear in mind that many events, especially these which fires frequently, may have bad impact on your games' performance. 

# Known issues and limitations
- Reactive systems can react to changes of the components in the same assembly. It should be easy to fix. Event systems use some magic to overcome this limitation.
- You cannot make events for components which you can't mark with an attribute. Workaround would be to introduce other way to mark said components.
- Component add and remove checks are done in manual iterations in simple foreach loops with structural changes. It would be nice to rewrite them into jobs and maybe use command buffers for structural changes.
- Reactive system's internal `IEntityBatchJob`, which checks for component changes, use some unity internal methods (like `InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr`) for getting pointers to the component arrays. This way a job's performance is the same (or almost the same) as unity generated foreaches. I'm not sure if I should use said methods or something different. It works tho.
- Event systems have to do `Dependency.Complete()` between updating reactive components and firing events. It should be easy to fix if event fires was some kind of main thread job, instead of plain foreach. This way it could use dependency management.
- Reactive components have to be written manually. You cannot generate components if you want to use them in ecs foreaches. Unity source generators do some magic on the component types under the hood. This forces some extra boilerplate code.
- Some of the generated code are static singletons which hurt my heart. Would be great to make it more manageable and possibly without singletons.
- Some of the generated code needs some cleaning.
- ...and for sure some more I am not yet aware of.

# Planned features
There are some features that may come to the ReactiveDots in the future.
- **Multi-assemly reactive systems** which will allow reacting on components from different (than system) assemblies.
- **Self events** which react only to changes in the specified entities.
- **External events** - a way to mark components from other assemblies to be events without a need to mark them directly with an attribute.
- **Cleanup** systems that will help destroying entities with selected components or removing these components.
- **Benchmarks** which will help calculate how efficient generated systems run compared to the ones written by hand.
- **More samples** (if really needed).
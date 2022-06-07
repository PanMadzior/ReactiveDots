# ReactiveDots
ReactiveDots is a set of utilities for Unity Data-Oriented Tech Stack (DOTS). Its purpose is to help developers write common types of systems with a minimum of boilerplate code. ReactiveDots use Roslyn source generators to achieve that.

Highly inspired by [Entitas](https://github.com/sschmid/Entitas-CSharp), ECS framework for Unity.

Current version is not battle-tested. It is not recommended to use it in production. Use at your own risk.

## Features
You can play around with the implemented features in a sample project.

### Reactive Systems
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
#### Boilerplate code
In order to use reactive systems you have to add a minimal set of boilerplate code to your system.

You have to:
1. Add a `[ReactiveSystem(typeof(A), typeof(B))]` attribute.
   1. Specify a component you want to react to in first argument (*A*).
   2. Specify a reactive data component (*B*) from step 2.
2. Add a simple reactive data system state component with a field of type `ComponentReactiveData<T>`, where *T* is the type of the component you want to react to (*A*).
3. Add `Dependency = this.UpdateReactive( Dependency );` at the beginning of the `OnUpdate()`.

Optionally specify a name of a component field you want to use when deciding whether the component changed its value or not. Set it with *FieldNameToCompare* argument: `[ReactiveSystem( typeof(A), typeof(B), FieldNameToCompare = "CustomField" )]`. Default is `"Value"`. 

## Planned features
There are some features that may come to the ReactiveDots in the future.
- **Events** based on reactive systems. Auto-generated systems that will fire events about component changes for listeners outside of DOTS. Example use is to update a non-DOTS user interface.
- **Cleanup** systems that will help destroying entities with selected components.
- **Set of tests** which will help ensure that ReactiveDots works as intended.
- **Benchmarks** which will help calculate how efficient generated systems run compared to the ones written by hand.
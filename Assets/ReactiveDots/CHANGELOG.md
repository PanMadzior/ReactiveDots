# ReactiveDots changelog

## v0.3.0
05 Nov 2022

This update focuses on adding support for a new Entities version and component types (tags and enableable).  

- Updated Entities supported version to 1.0.0-exp.12.
- `ReactiveSystemAttribute` no longer derives from `AlwaysUpdateSystemAttribute`. 
- Added support for enableable components.
  - Enabling and disabling components triggers .Added and .Removed state updates.
  - Reactive components are now removed from entities only when they are destroyed.
- Added support for tag component.
- Added support for multiple fields to compare when checking if component has changed.
- Added `ReactiveEntityTag` component which is added to all entities with reactive components.
- Added new tests for new features and updated existing ones.
- Updated internal jobs and reactive system update methods.
- Updated template strings replacement code in source generators.

## v0.2.0-preview.1
23 Jun 2022

This update focuses on removing sync points from reactive systems.

- Added new methods for updating reactive systems.
- Added new tests.
- Updated sample project with some improvements.
- Other minor fixes and improvements.

## v0.1.0-preview.1
19 Jun 2022

Initial version with:

- source generators
- reactive systems
- any event listeners
- unit tests

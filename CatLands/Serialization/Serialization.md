# Serialization

## Overview

| Member Type | Visibility | Serialized | Modifier           |
|-------------|------------|------------|--------------------|
| property    | public     | yes        | `[DontSerialize]`  |
| property    | private    | yes        | `[DontSerialize]`  |
| field       | public     | no         | `[SerializeField]` |
| field       | private    | no         | `[SerializeField]` |

Only writeable members are serialized.


## Renaming members

The serializer is case-insensitive, this means:
- Renaming `myProp` to `MyProp` will not break serialization.
- Classes may not have serialized members that differ only by case.

To rename a property (beyond case change), use the `[RenamedFrom]` attribute.


## Polymorphic type hierarchies

By default, classes are serialized by value. 
This means:
- No type information is stored.
- If the same class instance is saved multiple times, it will result in multiple different instances after deserialization.
- If a concrete instance is assigned to a variable of a base type, the concrete type information is lost.

To retain polymorphic type information and data sharing, use the `[SerializeTypeHierarchy]` attribute on the base type.


## Renaming or moving classes

This only applies to polymorphic type hierarchies which use the `[SerializeTypeHierarchy]` attribute.

A polymorphic class reference is serialized by its full name (namespace + type name).
Add the `[RenamedFrom]` attribute to a concrete class in the following situations:
- The class is renamed.
- The class is moved to a different namespace.

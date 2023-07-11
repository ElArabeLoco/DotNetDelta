C# Port of Quill Delta
======================

This is a C# port of the [Quill Delta](https://github.com/quilljs/delta). It is a library for creating and applying deltas to rich text. 
It is useful for creating collaborative editors like Google Docs or Microsoft Word Online.


## Quick Example
```csharp
// Document with text "Gandalf the Grey"
// with "Gandalf" bolded, and "Grey" in grey
var delta = new Delta()
    .Insert("Gandalf", new AttributeMap {{"bold", true}})
    .Insert(" the ")
    .Insert("Grey", new AttributeMap {{"color", "#ccc"}});
    
    
// Change intended to be applied to above:
// Keep the first 12 characters, insert a white 'White'
// and delete the next four characters ('Grey')
var death = new Delta()
    .Retain(12)
    .Insert("White", new AttributeMap {{ color: '#fff' }})
    .Delete(4);
    
// {
//   "ops": [
//     { "retain": 12 },
//     { "insert": "White", "attributes": { "color": "#fff" } },
//     { "delete": 4 }
//   ]
// }

// Applying the above:
var restored = delta.Compose(death);
// {
//   "ops": [
//     { "insert": "Gandalf", "attributes": { "bold": true } },
//     { "insert": " the " },
//     { "insert": "White", "attributes": { "color": "#fff" } }
//   ]
// }

```

The following is a brief description of the supported operations on Delta objects. 

## Contents

#### Operations

- [`Insert`](#insert-operation)
- [`Delete`](#delete-operation)
- [`Retain`](#retain-operation)

#### Construction

- [`constructor`](#constructor)
- [`Insert`](#insert)
- [`Delete`](#delete)
- [`Retain`](#retain)

#### Documents

Deltas can be used to represent both content (aka Documents) as well as individual changes. A Document is defined as a 
list of Insert operations. The following methods should be applied only to Documents. These methods called on or with 
non-document Deltas will result in undefined behavior.

- [`Concat`](#concat)
- [`Diff`](#diff)
- [`EachLine`](#eachline)
- [`Invert`](#invert)

#### Utility

- [`Filter`](#filter)
- [`ForEach`](#foreach)
- [`Length`](#length)
- [`Map`](#map)
- [`Partition`](#partition)
- [`Reduce`](#reduce)
- [`Slice`](#slice)

#### Operational Transform
These are the core methods for OT (Operational Transform). These are useful for implementating collaborative software,
as individual Deltas could be applied to a Document being edited, and that individual Delta could be the only piece
of information being shared among collaborating parties.
- [`Compose`](#compose)
- [`Transform`](#transform)
- [`TransformPosition`](#transformposition)


## Operations

### Insert Operation

Insert operations have an `insert` key defined. A String value represents inserting text. Any other type represents inserting an embed (however only one level of object comparison will be performed for equality).

In both cases of text and embeds, an optional `attributes` key can be defined with an Object to describe additonal formatting information. Formats can be changed by the [retain](#retain) operation.

```js
// Insert a bolded "Text"
{ insert: "Text", attributes: { bold: true } }

// Insert a link
{ insert: "Google", attributes: { link: 'https://www.google.com' } }

// Insert an embed
{
  insert: { image: 'https://octodex.github.com/images/labtocat.png' },
  attributes: { alt: "Lab Octocat" }
}

// Insert another embed
{
  insert: { video: 'https://www.youtube.com/watch?v=dMH0bHeiRNg' },
  attributes: {
    width: 420,
    height: 315
  }
}
```

### Delete Operation

Delete operations have a Number `delete` key defined representing the number of characters to delete. All embeds have a 
length of 1.

```js
// Delete the next 10 characters
{ delete: 10 }
```

### Retain Operation

Retain operations have a Number `retain` key defined representing the number of characters to keep (other libraries 
might use the name keep or skip). An optional `attributes` key can be defined with an Object to describe formatting 
changes to the character range. A value of `null` in the `attributes` Object represents removal of that key.

*Note: It is not necessary to retain the last characters of a document as this is implied.*

```js
// Keep the next 5 characters
{ retain: 5 }

// Keep and bold the next 5 characters
{ retain: 5, attributes: { bold: true } }

// Keep and unbold the next 5 characters
// More specifically, remove the bold key in the attributes Object
// in the next 5 characters
{ retain: 5, attributes: { bold: null } }
```


## Construction

### constructor

Creates a new Delta object.

#### Methods

- `new Delta()`
- `new Delta(ops)`
- `new Delta(delta)`
- `Delta.FromJson(deltaAsJson)`

#### Parameters

- `ops` - List of operations
- `delta` - Another Delta object
- `deltaAsJson` - JSON representation of a Delta object

*Note: No validity/sanity check is performed when constructed with ops or delta. The new delta's internal ops array will also be assigned from ops or delta.ops without deep copying.*

#### Example

```csharp
var delta = Delta.fromJson(@"[
        { ""insert"": ""Hello World"" },
        { ""insert"": ""!"", ""attributes"": { ""bold"": true }}
    ]");

var json = delta.toJson();

var chained = new Delta().Insert("Hello World").Insert("!", new AttributeMap {{ "bold", true }});
```

---

### Insert()

Appends an insert operation. Returns `this` for chainability.

#### Methods

- `Insert(string text, AttributeMap? attributes = null)`
- `Insert(object embed, AttributeMap? attributes = null)`

#### Parameters

- `text` - String representing text to insert
- `embed` - Object representing embed type to insert
- `attributes` - Optional attributes to apply

#### Example

```csharp
delta.Insert("Text", new AttributeMap {{ "bold", true }, { "color", "#ccc" }});
delta.Insert(new AttributeMap {{ "image", "https://octodex.github.com/images/labtocat.png" }});
```

---

### Delete()

Appends a delete operation. Returns `this` for chainability.

#### Methods

- `Delete(int length)`

#### Parameters

- `length` - Number of characters to delete

#### Example

```csharp
delta.Delete(5);
```

---

### Retain()

Appends a retain operation. Returns `this` for chainability.

#### Methods

- `Retain(int length, AttributeMap? attributes = null)`
- `Retain(object retainObject, AttributeMap? attributes = null)`

#### Parameters

- `length` - Number of characters to retain
- `attributes` - Optional attributes to apply
- `retainObject` - Embed type to retain, to which some attributes are applied

#### Example

```csharp
delta.Retain(4).Retain(5, new AttributeMap {{ "color", "#0c6" }});
```

## Documents

### Concat()

Returns a new Delta representing the concatenation of this and another document Delta's operations.

#### Methods

- `Concat(Delta other)`

#### Parameters

- `other` - Document Delta to concatenate

#### Returns

- `Delta` - Concatenated document Delta

#### Example

```csharp
Delta a = new Delta().Insert("Hello");
Delta b = new Delta().Insert("!", new AttributeMap {{ "bold", true }});


// {
//   ops: [
//     { "insert": "Hello" },
//     { "insert": "!", "attributes": { "bold": true } }
//   ]
// }
Delta concat = a.Concat(b);
```

---

### Diff()
Not implemented yet.

---

### ForEach()

Iterates through document Delta, calling a given function with a Delta and attributes object, representing the line segment.

#### Methods

- `ForEach(Action<Op, int> action)`

#### Parameters

- `predicate` - function to call on each line group
- `newline` - newline character, defaults to `\n`

#### Example

```csharp
var delta = new Delta().Insert("Hello\n\n")
                         .Insert("World")
                         .Insert(new AttributeMap {{ "image": "octocat.png" }})
                         .Insert("\n", new AttributeMap {{ "align": "right" })
                         .Insert("!");

delta.ForEach((op, i) => {
  Console.WriteLine(op.ToString() + ", " + i);
  // Can return false to exit loop early
});
```

---

### Invert()

Not implemented yet.


## Utility

### Filter()

Returns an array of operations that passes a given function.

#### Methods

- `Filter(Func<Op, int, bool> predicate)`

#### Parameters

- `predicate` - Function to test each operation against. Return `true` to keep the operation, `false` otherwise.

#### Returns

- `List<Op>` - Filtered resulting List of operations

#### Example

```csharp
var delta = new Delta().Insert("Hello", new AttributeMap {{ "bold", true }})
                         .Insert(new AttributeMap {{ "image", "https://octodex.github.com/images/labtocat.png" }})
                         .Insert("World!");

string textOnly = string.Join("", delta
                                    .Filter((op, i) => op.insert is string)
                                    .Map((op) => op.insert)));
```


---

### Length()

Returns length of a Delta, which is the sum of the lengths of its operations.

#### Methods

- `Length()`

#### Example

```csharp
new Delta().Insert("Hello").Length();  // Returns 5

new Delta().Insert("A").Retain(2).Delete(1).Length(); // Returns 4
```

---

### Map()

Returns a new List with the results of calling the provided function on each operation.

#### Methods

- `Map<T>(Func<Op, int, T> mappingFunction)`

#### Parameters

- `mappingFunction` - Function to call, passing in the current operation, returning an element of the new List to be returned

#### Returns

- `List<T>` - A new List with each element being the result of the given function.

#### Example

```csharp
var delta = new Delta().Insert("Hello", new AttributeMap {{ "bold", true }})
                         .Insert(new AttributeMap {{ "image", "https://octodex.github.com/images/labtocat.png" }})
                         .Insert("World!");

string text = string.Join("", delta.Map<string>((op, i) => (string)(op.insert is string ? op.insert : "")));
```

---

### Partition()

Returns a two Lists, the first with operations that pass the given function, the other that failed.

#### Methods

- `Partition(Func<Op, bool> predicate)`

#### Parameters

- `predicate` - Function to call, passing in the current operation, returning whether that operation passed

#### Returns

- `(List<Op>, List<Op>)` - A tuple of two lists, the first with passed operations, the other with failed operations

#### Example

```csharp
var delta = new Delta().Insert("Hello", new AttributeMap {{ "bold", true }})
                         .Insert(new AttributeMap {{ "image", "https://octodex.github.com/images/labtocat.png" }})
                         .Insert("World!");

var (passed, failed) = delta.Partition(op => op.insert is string);
// passed is [{ insert: 'Hello', attributes: { bold: true }}, { insert: 'World'}]
// failed [{ insert: { image: 'https://octodex.github.com/images/labtocat.png' }}]
```

---

### Reduce()

Applies given function against an accumulator and each operation to reduce to a single value.

#### Methods

- `Reduce<T>(Func<T, Op, int, T> reducer, T initialValue)`

#### Parameters

- `reducer` - Function to call per iteration, returning an accumulated value
- `initialValue` - Initial value to pass to first call to predicate

#### Returns

- `T` - the accumulated value

#### Example

```csharp
var delta = new Delta().Insert("Hello", new AttributeMap {{ "bold", true }})
                         .Insert(new AttributeMap {{ "image", "https://octodex.github.com/images/labtocat.png" }})
                         .Insert("World!");

long totalLength = delta.reduce((length, op) => length + Op.Length(op), 0);
```

---

### Slice()

Returns copy of delta with subset of operations.

#### Methods

- `Slice(int start = 0, int end = int.MaxValue)`

#### Parameters

- `start` - Start index of subset, defaults to 0
- `end` - End index of subset, defaults to rest of operations

#### Example

```csharp
var delta = new Delta().Insert("Hello", new AttributeMap {{ "bold", true }).Insert(" World");

// {
//   ops: [
//     { insert: 'Hello', attributes: { bold: true } },
//     { insert: ' World' }
//   ]
// }
var copy = delta.Slice();

// { ops: [{ insert: 'World' }] }
var world = delta.Slice(6);

// { ops: [{ insert: ' ' }] }
var space = delta.Slice(5, 6);
```


## Operational Transform

### compose()

Returns a Delta that is equivalent to applying the operations of own Delta, followed by another Delta.

#### Methods

- `Compose(Delta other)`

#### Parameters

- `other` - Delta to compose

#### Example

```csharp
var a = new Delta().Insert("abc");
var b = new Delta().Retain(1).Delete(1);

var composed = a.Compose(b);  // composed == new Delta().Insert('ac');

```

---

### Transform()

Not implemented yet.

---

### transformPosition()

Not implemented yet.

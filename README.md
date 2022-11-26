# VYaml

Extra fast, GC free YAML parser for C# and Unity.

The reason VYaml is fast is it handles utf8 byte sequences directly and uses a new API in C# (System.Buffers.* etc).
In parsing, scalar values are pooled and no allocation occurs until `Scalar.ToString()`. This works with very low overhead in environments such as Unity.

Comparison with YamlDotNet, 4x faster in .NET environment, More than 20x faster in Unity IL2CPP environment.


## Roadmaps

- [x] Unity Support
- [x] YAML 1.2 parser
- [ ] YAML Emitter
- [ ] Handy List/Dictionary/primitive type loader
- [ ] Deserizlize
- [ ] Serialize

## Parsing Raw API

wip

## Deserialization

wip


## YAML 1.2 spec support status

### Implicit primitive type conversion of scalar

- https://yaml.org/type/bool.html
    - :white_check_mark: true|True|TRUE|false|False|FALSE
    - :white_check_mark: y|Y|yes|Yes|YES|n|N|no|No|NO
    - :white_check_mark: on|On|ON|off|Off|OFF
- https://yaml.org/type/float.html
    - :white_check_mark: [-+]?([0-9][0-9_]*)?\.[0-9.]*([eE][-+][0-9]+)? (base 10)
    - :x: [-+]?[0-9][0-9_]*(:[0-5]?[0-9])+\.[0-9_]* (base 60)
    - :white_check_mark: [-+]?\.(inf|Inf|INF) # (infinity)
    - :white_check_mark: \.(nan|NaN|NAN) # (not a number)     
- https://yaml.org/type/int.
    - :white_check_mark: [-+]?(0|[1-9][0-9_]*) # (base 10)
    - :x: [-+]?0b[0-1_]+ # (base 2)
    - :x: [-+]?0[0-7_]+ # (base 8)
    - :white_check_mark: [-+]?0x[0-9a-fA-F_]+ # (base 16)
    - :x: [-+]?[1-9][0-9_]*(:[0-5]?[0-9])+ # (base 60)
- https://yaml.org/type/null.html
    - :white_check_mark: ~ # (canonical)
    - :white_check_mark: null|Null|NULL # (English)
    - :white_check_mark:  # (Empty)

### https://yaml.org/spec/1.2.2

- 2.1. Collections
  - :white_check_mark: Example 2.1 Sequence of Scalars (ball players)
  - :white_check_mark: Example 2.2 Mapping Scalars to Scalars (player statistics)
  - :white_check_mark: Example 2.3 Mapping Scalars to Sequences (ball clubs in each league)
  - :white_check_mark: Example 2.4 Sequence of Mappings (players’ statistics)
  - :white_check_mark: Example 2.5 Sequence of Sequences
  - :white_check_mark: Example 2.6 Mapping of Mappings
- 2.2. Structures
  - :white_check_mark: Example 2.7 Two Documents in a Stream (each with a leading comment)
  - :white_check_mark: Example 2.8 Play by Play Feed from a Game
  - :white_check_mark: Example 2.9 Single Document with Two Comments
  - :white_check_mark: Example 2.10 Node for “Sammy Sosa” appears twice in this document
  - :white_check_mark: Example 2.11 Mapping between Sequences
  - :white_check_mark: Example 2.12 Compact Nested Mapping
- 2.3. Scalars
  - :white_check_mark: Example 2.13 In literals, newlines are preserved
  - :white_check_mark: Example 2.14 In the folded scalars, newlines become spaces
  - :white_check_mark: Example 2.15 Folded newlines are preserved for “more indented” and blank lines
  - :white_check_mark: Example 2.16 Indentation determines scope
  - :white_check_mark: Example 2.17 Quoted Scalars
  - :white_check_mark: Example 2.18 Multi-line Flow Scalars
- 2.4. Tags
  - :warning: Example 2.19 Integers
  - :white_check_mark: Example 2.20 Floating Point
  - :white_check_mark: Example 2.21 Miscellaneous
  - :white_check_mark: Example 2.22 Timestamps
  - :white_check_mark: Example 2.23 Various Explicit Tags
  - :white_check_mark: Example 2.24 Global Tags
  - :white_check_mark: Example 2.25 Unordered Sets
  - :white_check_mark: Example 2.26 Ordered Mappings
- 2.5. Full Length Example
  - :white_check_mark: Example 2.27 Invoice
  - :white_check_mark: Example 2.28 Log File
- 5.2. Character Encodings
  - :white_check_mark: Example 5.1 Byte Order Mark
  - :white_check_mark: Example 5.2 Invalid Byte Order Mark
- 5.3. Indicator Characters
  - :white_check_mark: Example 5.3 Block Structure Indicators
  - :white_check_mark: Example 5.4 Flow Collection Indicators
  - :white_check_mark: Example 5.5 Comment Indicator
  - :white_check_mark: Example 5.6 Node Property Indicators
  - :white_check_mark: Example 5.7 Block Scalar Indicators
  - :white_check_mark: Example 5.8 Quoted Scalar Indicators
  - :white_check_mark: Example 5.9 Directive Indicator
  - :white_check_mark: Example 5.10 Invalid use of Reserved Indicators
- 5.4. Line Break Characters
  - :white_check_mark: Example 5.11 Line Break Characters
  - :white_check_mark: Example 5.12 Tabs and Spaces
  - :white_check_mark: Example 5.13 Escaped Characters
  - :white_check_mark: Example 5.14 Invalid Escaped Characters
- 6.1. Indentation Spaces
  - :white_check_mark: Example 6.1 Indentation Spaces
  - :white_check_mark: Example 6.2 Indentation Indicators
- 6.2. Separation Spaces
  - :white_check_mark: Example 6.3 Separation Spaces
- 6.3. Line Prefixes
  - :white_check_mark: Example 6.4 Line Prefixes
- 6.4. Empty Lines
  - :white_check_mark: Example 6.5 Empty Lines
- 6.5. Line Folding
  - :white_check_mark: Example 6.6 Line Folding
  - :white_check_mark: Example 6.7 Block Folding
  - :white_check_mark: Example 6.8 Flow Folding
- 6.6. Comments
  - :white_check_mark: Example 6.9 Separated Comment
  - :white_check_mark: Example 6.10 Comment Lines
  - :white_check_mark: Example 6.11 Multi-Line Comments
- 6.7. Separation Lines
  - :white_check_mark: Example 6.12 Separation Spaces
- 6.8. Directives
  - :white_check_mark: Example 6.13 Reserved Directives
  - :white_check_mark: Example 6.14 “YAML” directive
  - :white_check_mark: Example 6.15 Invalid Repeated YAML directive
  - :white_check_mark: Example 6.16 “TAG” directive
  - :white_check_mark: Example 6.17 Invalid Repeated TAG directive
  - :white_check_mark: Example 6.18 Primary Tag Handle
  - :white_check_mark: Example 6.19 Secondary Tag Handle
  - :white_check_mark: Example 6.20 Tag Handles
  - :white_check_mark: Example 6.21 Local Tag Prefix
  - :white_check_mark: Example 6.22 Global Tag Prefix
- 6.9. Node Properties
  - :white_check_mark: Example 6.23 Node Properties
  - :white_check_mark: Example 6.24 Verbatim Tags
  - :white_check_mark: Example 6.25 Invalid Verbatim Tags
  - :white_check_mark: Example 6.26 Tag Shorthands
  - :white_check_mark: Example 6.27 Invalid Tag Shorthands
  - :white_check_mark: Example 6.28 Non-Specific Tags
  - :white_check_mark: Example 6.29 Node Anchors
- 7.1. Alias Nodes
  - :white_check_mark: Example 7.1 Alias Nodes
- 7.2. Empty Nodes
  - :x: Example 7.2 Empty Content
  - :white_check_mark: Example 7.3 Completely Empty Flow Nodes
- 7.3. Flow Scalar Styles
  - :white_check_mark: Example 7.4 Double Quoted Implicit Keys
  - :white_check_mark: Example 7.5 Double Quoted Line Breaks
  - :white_check_mark: Example 7.6 Double Quoted Lines
  - :white_check_mark: Example 7.7 Single Quoted Characters
  - :white_check_mark: Example 7.8 Single Quoted Implicit Keys
  - :white_check_mark: Example 7.9 Single Quoted Lines
  - :white_check_mark: Example 7.10 Plain Characters
  - :white_check_mark: Example 7.11 Plain Implicit Keys
  - :white_check_mark: Example 7.12 Plain Lines
- 7.4. Flow Collection Styles
  - :white_check_mark: Example 7.13 Flow Sequence
  - :white_check_mark: Example 7.14 Flow Sequence Entries
  - :white_check_mark: Example 7.15 Flow Mappings
  - :white_check_mark: Example 7.16 Flow Mapping Entries
  - :white_check_mark: Example 7.17 Flow Mapping Separate Values
  - :white_check_mark: Example 7.18 Flow Mapping Adjacent Values
  - :white_check_mark: Example 7.20 Single Pair Explicit Entry
  - :x: Example 7.21 Single Pair Implicit Entries
  - :white_check_mark: Example 7.22 Invalid Implicit Keys
  - :white_check_mark: Example 7.23 Flow Content
  - :white_check_mark: Example 7.24 Flow Nodes
- 8.1. Block Scalar Styles
  - :white_check_mark: Example 8.1 Block Scalar Header
  - :x: Example 8.2 Block Indentation Indicator
  - :white_check_mark: Example 8.3 Invalid Block Scalar Indentation Indicators
  - :white_check_mark: Example 8.4 Chomping Final Line Break
  - :white_check_mark: Example 8.5 Chomping Trailing Lines
  - :white_check_mark: Example 8.6 Empty Scalar Chomping
  - :white_check_mark: Example 8.7 Literal Scalar
  - :white_check_mark: Example 8.8 Literal Content
  - :white_check_mark: Example 8.9 Folded Scalar
  - :white_check_mark: Example 8.10 Folded Lines
  - :white_check_mark: Example 8.11 More Indented Lines
  - :white_check_mark: Example 8.12 Empty Separation Lines
  - :white_check_mark: Example 8.13 Final Empty Lines
  - :white_check_mark: Example 8.14 Block Sequence
  - :white_check_mark: Example 8.15 Block Sequence Entry Types
  - :white_check_mark: Example 8.16 Block Mappings
  - :white_check_mark: Example 8.17 Explicit Block Mapping Entries
  - :white_check_mark: Example 8.18 Implicit Block Mapping Entries
  - :white_check_mark: Example 8.19 Compact Block Mappings
  - :white_check_mark: Example 8.20 Block Node Types
  - :white_check_mark: Example 8.21 Block Scalar Nodes
  - :white_check_mark: Example 8.22 Block Collection Nodes




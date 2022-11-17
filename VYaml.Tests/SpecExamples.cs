namespace VYaml.Tests;

/// <summary>
/// </summary>
/// <see href="https://yaml.org/spec/1.2.2/" />
public static class SpecExamples
{
    public const string Ex2_1 =
        "- Mark McGwire\n" +
        "- Sammy Sosa\n" +
        "- Ken Griffey";

    public const string Ex2_2 =
        "hr:  65    # Home runs\n" +
        "avg: 0.278 # Batting average\n" +
        "rbi: 147   # Runs Batted In";

    public const string Ex2_3 =
        "american:\n" +
        "- Boston Red Sox\n" +
        "- Detroit Tigers\n" +
        "- New York Yankees\n" +
        "national:\n" +
        "- New York Mets\n" +
        "- Chicago Cubs\n" +
        "- Atlanta Braves";

    public const string Ex2_4 =
        "-\n" +
        "  name: Mark McGwire\n" +
        "  hr:   65\n" +
        "  avg:  0.278\n" +
        "-\n" +
        "  name: Sammy Sosa\n" +
        "  hr:   63\n" +
        "  avg:  0.288";

    public const string Ex2_5 =
        "- [name        , hr, avg  ]\n" +
        "- [Mark McGwire, 65, 0.278]\n" +
        "- [Sammy Sosa  , 63, 0.288]";

    public const string Ex2_6 =
        "Mark McGwire: {hr: 65, avg: 0.278}\n" +
        "Sammy Sosa: {\n" +
        "    hr: 63,\n" +
        "    avg: 0.288\n" +
        "  }";

    public const string Ex2_7 =
        "# Ranking of 1998 home runs\n" +
        "---\n" +
        "- Mark McGwire\n" +
        "- Sammy Sosa\n" +
        "- Ken Griffey\n" +
        "\n" +
        "# Team ranking\n" +
        "---\n" +
        "- Chicago Cubs\n" +
        "- St Louis Cardinals";

    public const string Ex2_8 =
        "---\n" +
        "time: 20:03:20\n" +
        "player: Sammy Sosa\n" +
        "action: strike (miss)\n" +
        "...\n" +
        "---\n" +
        "time: 20:03:47\n" +
        "player: Sammy Sosa\n" +
        "action: grand slam\n" +
        "...";

    public const string Ex2_9 =
        "---\n" +
        "hr: # 1998 hr ranking\n" +
        "  - Mark McGwire\n" +
        "  - Sammy Sosa\n" +
        "rbi:\n" +
        "  # 1998 rbi ranking\n" +
        "  - Sammy Sosa\n" +
        "  - Ken Griffey";

    public const string Ex2_10 =
        "---\n" +
        "hr:\n" +
        "  - Mark McGwire\n" +
        "  # Following node labeled SS\n" +
        "  - &SS Sammy Sosa\n" +
        "rbi:\n" +
        "  - *SS # Subsequent occurrence\n" +
        "  - Ken Griffey";

    public const string Ex2_11 =
        "? - Detroit Tigers\n" +
        "  - Chicago cubs\n" +
        ":\n" +
        "  - 2001-07-23\n" +
        "\n" +
        "? [ New York Yankees,\n" +
        "    Atlanta Braves ]\n" +
        ": [ 2001-07-02, 2001-08-12,\n" +
        "    2001-08-14 ]";

    public const string Ex2_12 =
        "---\n" +
        "# Products purchased\n" +
        "- item    : Super Hoop\n" +
        "  quantity: 1\n" +
        "- item    : Basketball\n" +
        "  quantity: 4\n" +
        "- item    : Big Shoes\n" +
        "  quantity: 1";

    public const string Ex2_13 =
        "# ASCII Art\n" +
        "--- |\n" +
        "  \\//||\\/||\n" +
        "  // ||  ||__";

    public const string Ex2_14 =
        "--- >\n" +
        "  Mark McGwire's\n" +
        "  year was crippled\n" +
        "  by a knee injury.";

    public const string Ex2_15 =
        ">\n" +
        " Sammy Sosa completed another\n" +
        " fine season with great stats.\n" +
        " \n" +
        "   63 Home Runs\n" +
        "   0.288 Batting Average\n" +
        " \n" +
        " What a year!";

    public const string Ex2_16 =
        "name: Mark McGwire\n" +
        "accomplishment: >\n" +
        "  Mark set a major league\n" +
        "  home run record in 1998.\n" +
        "stats: |\n" +
        "  65 Home Runs\n" +
        "  0.278 Batting Average\n";

    public const string Ex2_17 =
        "unicode: \"Sosa did fine.\\u263A\"\n" +
        "control: \"\\b1998\\t1999\\t2000\\n\"\n" +
        "hex esc: \"\\x0d\\x0a is \\r\\n\"\n" +
        "\n" +
        "single: '\"Howdy!\" he cried.'\n" +
        "quoted: ' # Not a ''comment''.'\n" +
        "tie-fighter: '|\\-*-/|'";

    public const string Ex2_18 =
        "plain:\n" +
        "  This unquoted scalar\n" +
        "  spans many lines.\n" +
        "\n" +
        "quoted: \"So does this\n" +
        "  quoted scalar.\\n\"";

    public const string Ex2_19 =
        "canonical: 12345\n" +
        "decimal: +12345\n" +
        "octal: 0o14\n" +
        "hexadecimal: 0xC\n";

    public const string Ex2_20 =
        "canonical: 1.23015e+3\n" +
        "exponential: 12.3015e+02\n" +
        "fixed: 1230.15\n" +
        "negative infinity: -.inf\n" +
        "not a number: .NaN\n";

    public const string Ex2_21 =
        "null:\n" +
        "booleans: [ true, false ]\n" +
        "string: '012345'\n";

    public const string Ex2_22 =
        "canonical: 2001-12-15T02:59:43.1Z\n" +
        "iso8601: 2001-12-14t21:59:43.10-05:00\n" +
        "spaced: 2001-12-14 21:59:43.10 -5\n" +
        "date: 2002-12-14\n";

    public const string Ex2_23 =
        "---\n" +
        "not-date: !!str 2002-04-28\n" +
        "\n" +
        "picture: !!binary |\n" +
        " R0lGODlhDAAMAIQAAP//9/X\n" +
        " 17unp5WZmZgAAAOfn515eXv\n" +
        " Pz7Y6OjuDg4J+fn5OTk6enp\n" +
        " 56enmleECcgggoBADs=\n" +
        "\n" +
        "application specific tag: !something |\n" +
        " The semantics of the tag\n" +
        " above may be different for\n" +
        " different documents.";

    public const string Ex2_24 =
        "%TAG ! tag:clarkevans.com,2002:\n" +
        "--- !shape\n" +
        "  # Use the ! handle for presenting\n" +
        "  # tag:clarkevans.com,2002:circle\n" +
        "- !circle\n" +
        "  center: &ORIGIN {x: 73, y: 129}\n" +
        "  radius: 7\n" +
        "- !line\n" +
        "  start: *ORIGIN\n" +
        "  finish: { x: 89, y: 102 }\n" +
        "- !label\n" +
        "  start: *ORIGIN\n" +
        "  color: 0xFFEEBB\n" +
        "  text: Pretty vector drawing.";

    public const string Ex2_25 =
        "# Sets are represented as a\n" +
        "# Mapping where each key is\n" +
        "# associated with a null value\n" +
        "--- !!set\n" +
        "? Mark McGwire\n" +
        "? Sammy Sosa\n" +
        "? Ken Griffey";

    public const string Ex2_26 =
        "# Ordered maps are represented as\n" +
        "# A sequence of mappings, with\n" +
        "# each mapping having one key\n" +
        "--- !!omap\n" +
        "- Mark McGwire: 65\n" +
        "- Sammy Sosa: 63\n" +
        "- Ken Griffey: 58";

    public const string Ex2_27 =
        "--- !<tag:clarkevans.com,2002:invoice>\n" +
        "invoice: 34843\n" +
        "date   : 2001-01-23\n" +
        "bill-to: &id001\n" +
        "    given  : Chris\n" +
        "    family : Dumars\n" +
        "    address:\n" +
        "        lines: |\n" +
        "            458 Walkman Dr.\n" +
        "            Suite #292\n" +
        "        city    : Royal Oak\n" +
        "        state   : MI\n" +
        "        postal  : 48046\n" +
        "ship-to: *id001\n" +
        "product:\n" +
        "    - sku         : BL394D\n" +
        "      quantity    : 4\n" +
        "      description : Basketball\n" +
        "      price       : 450.00\n" +
        "    - sku         : BL4438H\n" +
        "      quantity    : 1\n" +
        "      description : Super Hoop\n" +
        "      price       : 2392.00\n" +
        "tax  : 251.42\n" +
        "total: 4443.52\n" +
        "comments:\n" +
        "    Late afternoon is best.\n" +
        "    Backup contact is Nancy\n" +
        "    Billsmer @ 338-4338.";

    public const string Ex2_28 =
        "---\n" +
        "Time: 2001-11-23 15:01:42 -5\n" +
        "User: ed\n" +
        "Warning:\n" +
        "  This is an error message\n" +
        "  for the log file\n" +
        "---\n" +
        "Time: 2001-11-23 15:02:31 -5\n" +
        "User: ed\n" +
        "Warning:\n" +
        "  A slightly different error\n" +
        "  message.\n" +
        "---\n" +
        "Date: 2001-11-23 15:03:17 -5\n" +
        "User: ed\n" +
        "Fatal:\n" +
        "  Unknown variable \"bar\"\n" +
        "Stack:\n" +
        "  - file: TopClass.py\n" +
        "    line: 23\n" +
        "    code: |\n" +
        "      x = MoreObject(\"345\\n\")\n" +
        "  - file: MoreClass.py\n" +
        "    line: 58\n" +
        "    code: |-\n" +
        "      foo = bar";

    // TODO: 5.1 - 5.2 BOM

    public const string Ex5_3 =
        "sequence:\n" +
        "- one\n" +
        "- two\n" +
        "mapping:\n" +
        "  ? sky\n" +
        "  : blue\n" +
        "  sea : green";

    public const string Ex5_4 =
        "sequence: [ one, two, ]\n" +
        "mapping: { sky: blue, sea: green }";

    public const string Ex5_5 = "# Comment only.";

    public const string Ex5_6 =
        "anchored: !local &anchor value\n" +
        "alias: *anchor";

    public const string Ex5_7 =
        "literal: |\n" +
        "  some\n" +
        "  text\n" +
        "folded: >\n" +
        "  some\n" +
        "  text\n";

    public const string Ex5_8 =
        "single: 'text'\n" +
        "double: \"text\"";

    // TODO: 5.9 directive
    // TODO: 5.10 reserved indicator

    public const string Ex5_11 =
        "|\n" +
        "  Line break (no glyph)\n" +
        "  Line break (glyphed)\n";

    public const string Ex5_12 =
        "# Tabs and spaces\n" +
        "quoted: \"Quoted \t\"\n" +
        "block:	|\n" +
        "  void main() {\n" +
        "  \tprintf(\"Hello, world!\\n\");\n" +
        "  }";

    public const string Ex5_13 =
        "- \"Fun with \\\\\"\n" +
        "- \"\\\" \\a \\b \\e \\f\"\n" +
        "- \"\\n \\r \\t \\v \\0\"\n" +
        "- \"\\  \\_ \\N \\L \\P \\\n" +
        "  \\x41 \\u0041 \\U00000041\"";

    public const string Ex5_14 =
        "Bad escapes:\n" +
        "  \"\\c\n" +
        "  \\xq-\"";

    public const string Ex6_1 =
        "  # Leading comment line spaces are\n" +
        "   # neither content nor indentation.\n" +
        "    \n" +
        "Not indented:\n" +
        " By one space: |\n" +
        "    By four\n" +
        "      spaces\n" +
        " Flow style: [    # Leading spaces\n" +
        "   By two,        # in flow style\n" +
        "  Also by two,    # are neither\n" +
        "  \tStill by two   # content nor\n" +
        "    ]             # indentation.";

    public const string Ex6_2 =
        "? a\n" +
        ": -  b\n" +
        "  -  -  c\n" +
        "     - d";

    public const string Ex6_3 =
        "- foo:   bar\n" +
        "- - baz\n" +
        "  -    baz";

    public const string Ex6_4 =
        "plain: text\n" +
        "  lines\n" +
        "quoted: \"text\n" +
        "  \tlines\"\n" +
        "block: |\n" +
        "  text\n" +
        "   \tlines\n";

    public const string Ex6_5 =
        "Folding:\n" +
        "  \"Empty line\n" +
        "   \t\n" +
        "  as a line feed\"\n" +
        "Chomping: |\n" +
        "  Clipped empty lines\n" +
        " ";

    public const string Ex6_6 =
        ">-\n" +
        "  trimmed\n" +
        "  \n" +
        " \n" +
        "\n" +
        "  as\n" +
        "  space";

    public const string Ex6_7 =
        ">\n" +
        "  foo \n" +
        " \n" +
        "  \t bar\n" +
        "\n" +
        "  baz\n";

    public const string Ex6_8 =
        "\"\n" +
        "  foo \n" +
        " \n" +
        "  \t bar\n" +
        "\n" +
        "  baz\n" +
        "\"";

    public const string Ex6_9 =
        "key:    # Comment\n" +
        "  value";

    public const string Ex6_10 =
        "  # Comment\n" +
        "   \n" +
        "\n";

    public const string Ex6_11 =
        "key:    # Comment\n" +
        "        # lines\n" +
        "  value\n" +
        "\n";

    public const string Ex6_12 =
        "{ first: Sammy, last: Sosa }:\n" +
        "# Statistics:\n" +
        "  hr:  # Home runs\n" +
        "     65\n" +
        "  avg: # Average\n" +
        "   0.278";

    public const string Ex6_13 =
        "%FOO  bar baz # Should be ignored\n" +
        "               # with a warning.\n" +
        "--- \"foo\"";

    public const string Ex6_14 =
        "%YAML 1.3 # Attempt parsing\n" +
        "           # with a warning\n" +
        "---\n" +
        "\"foo\"";

    public const string Ex6_15 =
        "%YAML 1.2\n" +
        "%YAML 1.1\n" +
        "foo";

    public const string Ex6_16 =
        "%TAG !yaml! tag:yaml.org,2002:\n" +
        "---\n" +
        "!yaml!str \"foo\"";

    public const string Ex6_17 =
        "%TAG ! !foo\n" +
        "%TAG ! !foo\n" +
        "bar";

    public const string Ex6_18 =
        "# Private\n" +
        "!foo \"bar\"\n" +
        "...\n" +
        "# Global\n" +
        "%TAG ! tag:example.com,2000:app/\n" +
        "---\n" +
        "!foo \"bar\"";

    public const string Ex6_19 =
        "%TAG !! tag:example.com,2000:app/\n" +
        "---\n" +
        "!!int 1 - 3 # Interval, not integer";

    public const string Ex6_20 =
        "%TAG !e! tag:example.com,2000:app/\n" +
        "---\n" +
        "!e!foo \"bar\"";

    public const string Ex6_21 =
        "%TAG !m! !my-\n" +
        "--- # Bulb here\n" +
        "!m!light fluorescent\n" +
        "...\n" +
        "%TAG !m! !my-\n" +
        "--- # Color here\n" +
        "!m!light green";

    public const string Ex6_22 =
        "%TAG !e! tag:example.com,2000:app/\n" +
        "---\n" +
        "- !e!foo \"bar\"";

    public const string Ex6_23 =
        "!!str &a1 \"foo\":\n" +
        "  !!str bar\n" +
        "&a2 baz : *a1";

    public const string Ex6_24 =
        "!<tag:yaml.org,2002:str> foo :\n" +
        "  !<!bar> baz";

    public const string Ex6_25 =
        "- !<!> foo\n" +
        "- !<$:?> bar\n";

    public const string Ex6_26 =
        "%TAG !e! tag:example.com,2000:app/\n" +
        "---\n" +
        "- !local foo\n" +
        "- !!str bar\n" +
        "- !e!tag%21 baz\n";

    public const string Ex6_27a =
        "%TAG !e! tag:example,2000:app/\n" +
        "---\n" +
        "- !e! foo";

    public const string Ex6_27b =
        "%TAG !e! tag:example,2000:app/\n" +
        "---\n" +
        "- !h!bar baz";

    public const string Ex6_28 =
        "# Assuming conventional resolution:\n" +
        "- \"12\"\n" +
        "- 12\n" +
        "- ! 12";

    public const string Ex6_29 =
        "First occurrence: &anchor Value\n" +
        "Second occurrence: *anchor";

    public const string Ex7_1 =
        "First occurrence: &anchor Foo\n" +
        "Second occurrence: *anchor\n" +
        "Override anchor: &anchor Bar\n" +
        "Reuse anchor: *anchor";

    public const string Ex7_2 =
        "{\n" +
        "  foo : !!str,\n" +
        "  !!str : bar,\n" +
        "}";

    public const string Ex7_3 =
        "{\n" +
        "  ? foo :,\n" +
        "  : bar,\n" +
        "}\n";

    public const string Ex7_4 =
        "\"implicit block key\" : [\n" +
        "  \"implicit flow key\" : value,\n" +
        " ]";

    public const string Ex7_5 =
        "\"folded \n" +
        "to a space,\t\n" +
        " \n" +
        "to a line feed, or \t\\\n" +
        " \\ \tnon-content\"";

    public const string Ex7_6 =
        "\" 1st non-empty\n" +
        "\n" +
        " 2nd non-empty \n" +
        "\t3rd non-empty \"";

    public const string Ex7_7 = " 'here''s to \"quotes\"'";

    public const string Ex7_8 =
        "'implicit block key' : [\n" +
        "  'implicit flow key' : value,\n" +
        " ]";

    public const string Ex7_9 =
        "' 1st non-empty\n" +
        "\n" +
        " 2nd non-empty \n" +
        "\t3rd non-empty '";

    public const string Ex7_10 =
        "# Outside flow collection:\n" +
        "- ::vector\n" +
        "- \": - ()\"\n" +
        "- Up, up, and away!\n" +
        "- -123\n" +
        "- http://example.com/foo#bar\n" +
        "# Inside flow collection:\n" +
        "- [ ::vector,\n" +
        "  \": - ()\",\n" +
        "  \"Up, up, and away!\",\n" +
        "  -123,\n" +
        "  http://example.com/foo#bar ]";

    public const string Ex7_11 =
        "implicit block key : [\n" +
        "  implicit flow key : value,\n" +
        " ]";

    public const string Ex7_12 =
        "1st non-empty\n" +
        "\n" +
        " 2nd non-empty \n" +
        "\t3rd non-empty";

    public const string Ex7_13 =
        "- [ one, two, ]\n" +
        "- [three ,four]";

    public const string Ex7_14 =
        "[\n" +
        "\"double\n" +
        " quoted\", 'single\n" +
        "           quoted',\n" +
        "plain\n" +
        " text, [ nested ],\n" +
        "single: pair,\n" +
        "]";

    public const string Ex7_15 =
        "- { one : two , three: four , }\n" +
        "- {five: six,seven : eight}";

    public const string Ex7_16 =
        "{\n" +
        "? explicit: entry,\n" +
        "implicit: entry,\n" +
        "?\n" +
        "}";

    public const string Ex7_17 =
        "{\n" +
        "unquoted : \"separate\",\n" +
        "http://foo.com,\n" +
        "omitted value:,\n" +
        ": omitted key,\n" +
        "}";

    public const string Ex7_18 =
        "{\n" +
        "\"adjacent\":value,\n" +
        "\"readable\":value,\n" +
        "\"empty\": \n" +
        "}";

    public const string Ex7_19 =
        "[\n" +
        "foo: bar\n" +
        "]";

    public const string Ex7_20 =
        "[\n" +
        "? foo\n" +
        " bar : baz\n" +
        "]";

    public const string Ex7_21 =
        "- [ YAML : separate ]\n" +
        "- [ : empty key entry ]\n" +
        "- [ {JSON: like}:adjacent ]";

    public const string Ex7_22 =
        "[ foo\n" +
        " bar: invalid,";

    public const string Ex7_23 =
        "- [ a, b ]\n" +
        "- { a: b }\n" +
        "- \"a\"\n" +
        "- 'b'\n" +
        "- c";

    public const string Ex7_24 =
        "- !!str \"a\"\n" +
        "- 'b'\n" +
        "- &anchor \"c\"\n" +
        "- *anchor\n" +
        "- !!str";

    public const string Ex8_1 =
        "- | # Empty header\n" +
        " literal\n" +
        "- >1 # Indentation indicator\n" +
        "  folded\n" +
        "- |+ # Chomping indicator\n" +
        " keep\n" +
        "\n" +
        "- >1- # Both indicators\n" +
        "  strip\n";

    public const string Ex8_2 =
        "- |\n" +
        " detected\n" +
        "- >\n" +
        " \n" +
        "  \n" +
        "  # detected\n" +
        "- |1\n" +
        "  explicit\n" +
        "- >\n" +
        " \t\n" +
        " detected\n";

    public const string Ex8_3a =
        "- |\n" +
        "  \n" +
        " text";

    public const string Ex8_3b =
        "- >\n" +
        "  text\n" +
        " text";

    public const string Ex8_3c =
        "- |2\n" +
        " text";

    public const string Ex8_4 =
        "strip: |-\n" +
        "  text\n" +
        "clip: |\n" +
        "  text\n" +
        "keep: |+\n" +
        "  text\n";

    public const string Ex8_5 =
        " # Strip\n" +
        "  # Comments:\n" +
        "strip: |-\n" +
        "  # text\n" +
        "  \n" +
        " # Clip\n" +
        "  # comments:\n" +
        "\n" +
        "clip: |\n" +
        "  # text\n" +
        " \n" +
        " # Keep\n" +
        "  # comments:\n" +
        "\n" +
        "keep: |+\n" +
        "  # text\n" +
        "\n" +
        " # Trail\n" +
        "  # Comments\n";

    public const string Ex8_6 =
        "strip: >-\n" +
        "\n" +
        "clip: >\n" +
        "\n" +
        "keep: |+\n" +
        "\n";

    public const string Ex8_7 =
        "|\n" +
        " literal\n" +
        " \ttext\n" +
        "\n";

    public const string Ex8_8 =
        "|\n" +
        " \n" +
        "  \n" +
        "  literal\n" +
        "   \n" +
        "  \n" +
        "  text\n" +
        "\n" +
        " # Comment\n";

    public const string Ex8_9 =
        ">\n" +
        " folded\n" +
        " text\n" +
        "\n";

    public const string Ex8_10 =
        ">\n" +
        "\n" +
        " folded\n" +
        " line\n" +
        "\n" +
        " next\n" +
        " line\n" +
        "   * bullet\n" +
        "\n" +
        "   * list\n" +
        "   * lines\n" +
        "\n" +
        " last\n" +
        " line\n" +
        "\n" +
        "# Comment\n";

    public const string Ex8_11 = Ex8_10;
    public const string Ex8_12 = Ex8_10;
    public const string Ex8_13 = Ex8_10;

    public const string Ex8_14 =
        "block sequence:\n" +
        "  - one\n" +
        "  - two : three\n";

    public const string Ex8_15 =
        "- # Empty\n" +
        "- |\n" +
        " block node\n" +
        "- - one # Compact\n" +
        "  - two # sequence\n" +
        "- one: two # Compact mapping\n";

    public const string Ex8_16 =
        "block mapping:\n" +
        " key: value\n";

    public const string Ex8_17 =
        "? explicit key # Empty value\n" +
        "? |\n" +
        "  block key\n" +
        ": - one # Explicit compact\n" +
        "  - two # block value\n";

    public const string Ex8_18 =
        "plain key: in-line value\n" +
        ":  # Both empty\n" +
        "\"quoted key\":\n" +
        "- entry\n";

    public const string Ex8_19 =
        "- sun: yellow\n" +
        "- ? earth: blue\n" +
        "  : moon: white\n";

    public const string Ex8_20 =
        "-\n" +
        "  \"flow in block\"\n" +
        "- >\n" +
        " Block scalar\n" +
        "- !!map # Block collection\n" +
        "  foo : bar\n";

    public const string Ex8_21 =
        "literal: |2\n" +
        "  value\n" +
        "folded:\n" +
        "   !foo\n" +
        "  >1\n" +
        " value\n";

    public const string Ex8_22 =
        "sequence: !!seq\n" +
        "- entry\n" +
        "- !!seq\n" +
        " - nested\n" +
        "mapping: !!map\n" +
        " foo: bar\n";
}

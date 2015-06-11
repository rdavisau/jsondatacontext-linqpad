# JSON Data Context Driver for LINQPad

A dynamic data context driver for querying JSON data with LINQPad. See [here](http://ryandavis.io/a-json-data-context-driver-for-linqpad/) for an overview, and check the [releases](https://github.com/rdavisau/jsondatacontext-linqpad/releases) for binaries.

This driver allows you to select a set of JSON inputs from which a LINQPad data context will be generated for strongly typed access. A context can currently be built from any combination of: 

- individual file paths
- directories to search, with a filemask (e.g. \*.json, \*.\*) and the option for recursive enumeration.

For each input, you can also control the number of rows the driver samples when attempting to determine types.

####Screenshots

![](http://ryandavis.io/content/images/2015/06/cxn_dialog.png)
![](http://ryandavis.io/content/images/2015/06/json_context-1.png)

####Planned Features:

* support for grabbing JSON from the world wide web (GET or POST with parameters and/or request body)
* support for caching of deserialised data, as well as programmatic invalidation of cached data
* support for persisting new data to the context (for example, written to path in the context's search definitions), allowing you to build out your context as you go
* better support wrapped JSON, as outlined below
* fixes for JSON data that it chokes on
* better error handling and no modal error dialogs (please forgive me, gods of UX).

####Considerations:
#####'Wrapped' JSON sources
The driver currently expects each input file to hold a single JSON array, of one type of object. This generally works OK but from time to time you might come across JSON items wrapped in a top level object. 

For example, one set of objects contained in a root object:
```
{
  "data": [
    { .. item 1 .. },
    { .. item 2 .. },
    { .. item 3 .. }
  ]
}
```
or many sets of different objects, contained in a root object:
```
{
  "cats": [ { ... }, { ... } ],
  "dogs": [ { ... }, { ... } ],
  "trucks": [ { ... }, { ... } ],
}
```
In the future, this should be optionally detected and unwrapped (in the first case, a single list of items, in the second case, several lists of items). As a short term workaround, the next release will wrap such sources in an array, resulting in a collection with the single root item that you can then unwrap yourself. Until then, you can wrap such a source in an array manually (i.e. enclose it in '[' and ']') to get the same workaround. 

#####Errors:

Errors encountered when processing individual sources will not typically prevent the construction of a full context; that is, 'bad' inputs will be ignored and the context is generated with 'good ones. If the driver fails on inputs that you are able to share, please include them when [filing an issue](https://github.com/rdavisau/jsondatacontext-linqpad/issues).

####Contributing:

Very welcome - fork, branch, PR :star2:

####Attributions:
* JSON-to-CSharp provided by the [jsonclassgenerator project](http://jsonclassgenerator.codeplex.com/).
* Dropdown button from [Extended WPF Toolkit](http://wpftoolkit.codeplex.com/)
* JSON.NET, of course

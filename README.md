# JSON Data Context Driver for LINQPad

A dynamic data context driver for querying JSON data with LINQPad. See [here](http://ryandavis.io/a-json-data-context-driver-for-linqpad/) for an overview, and check the [releases](https://github.com/rdavisau/jsondatacontext-linqpad/releases) for binaries.

This driver allows you to select a set of JSON inputs from which a LINQPad data context will be generated for strongly typed access. A context can currently be built from any combination of: 

- plain text inputs (for example, pasted in or typed by hand)
- individual file paths
- directories to search, with a filemask (e.g. \*.json, \*.\*) and the option for recursive enumeration
- GET calls to urls that return json, exposed either as properties on the context, or methods with parameters mapped to querystring components in the url

####Screenshots

![](http://ryandavis.io/content/images/2015/06/json_connection_dialog_updated.png)
![](http://ryandavis.io/content/images/2015/06/json_query-1.png)

####Planned Features:

* enhanced support for grabbing JSON from the world wide web (basic GET with headers is currently supported, support for POST planned)
* support for caching of deserialised data, as well as programmatic invalidation of cached data
* support for persisting new data to the context (for example, written to path in the context's search definitions), allowing you to build out your context as you go
* better support wrapped JSON
* fixes for JSON data that it chokes on
* better error handling and no modal error dialogs (please forgive me, gods of UX).

####Considerations:
#####Errors:

Errors encountered when processing individual sources will not typically prevent the construction of a full context; that is, 'bad' inputs will be ignored and the context is generated with 'good' ones. If the driver fails on inputs that you are able to share, please include them when [filing an issue](https://github.com/rdavisau/jsondatacontext-linqpad/issues).

####Contributing:

Very welcome - fork, branch, PR :star2:

####Attributions:
* JSON-to-CSharp provided by the [jsonclassgenerator project](http://jsonclassgenerator.codeplex.com/).
* Dropdown button from [Extended WPF Toolkit](http://wpftoolkit.codeplex.com/)
* JSON.NET

# JSON Data Context Driver for LINQPad

A LINQPad dynamic data context driver for JSON data. See [here](http://ryandavis.io/a-json-data-context-driver-for-linqpad/) for an overview.

JSON-to-CSharp provided by the [jsonclassgenerator project](http://jsonclassgenerator.codeplex.com/).

![](/content/images/2015/06/cxn_dialog.png)
<center><em><small>Add your search paths and individual file inputs, then.. </em></small></center>

![](/content/images/2015/06/json_context-1.png)
<center><em><small>..queries, easy!</em></small></center>

####Planned Features:

* support for grabbing JSON from the world wide web (GET or POST with parameters and/or request body)
* support for caching of deserialised data, as well as programmatic invalidation of cached data
* support for persisting new data to the context (for example, written to path in the context's search definitions), allowing you to build out your context as you go
* better support for class generation when encountering 'nested listitem' JSON inputs. For example, the current implementation will faithfully generate a set of classes for json like [this](http://www.sitepoint.com/facebook-json-example/) that includes a root object with a single 'data' property under which the list of entities actually exists. This is true to the input but redundant and a PITA to work with.  
* fixes for JSON data that it chokes on (if you have samples, please [add an issue!](https://github.com/rdavisau/jsondatacontext-linqpad/issues)
* better error handling and no modal error dialogs (please forgive me, gods of UX).

####Contributions:

All welcome!! - Fork, branch, PR
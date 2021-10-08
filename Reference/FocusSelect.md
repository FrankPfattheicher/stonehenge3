
## Focus

**HTML ```autofocus``` attribute**

Using autofocus attribute on any input element results in Chrome (starting version 79) throwing this log:

    Autofocus processing was blocked because a document's URL has a fragment

Bad news: This behavior is part of the HTML specification...



**Support using Custom Directive**

To have a kind of autofocus functionality a custom directive is included as described in the Vue.js documentation.   

``` html
    <input v-model="Test" v-focus />
```

This allows automatically setting the input focus on that element if the page shows up.

For more information see [Vue.js Custom Directives...](https://vuejs.org/v2/guide/custom-directive.html)


## Select

**Support using Custom Directive**

To have an autofocus functionality also selecting the entire input a custom directive is included.

``` html
    <input v-model="Test" v-select />
```

This allows automatically setting the input focus on that element and select all text if the page shows up.


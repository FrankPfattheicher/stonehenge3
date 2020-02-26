

updated: function () {

    if(typeof(this.gauge) == "undefined") this.gauge = c3.generate({
        bindto: this.$el,
        data: {
            columns: [
                [this.$props.gaugedata.Name, this.$props.gaugedata.Value]
            ],
            type: 'gauge',
            onclick: function(d, i) { console.log("onclick", d, i); },
            onmouseover: function (d, i) { console.log("onmouseover", d, i); },
            onmouseout: function (d, i) { console.log("onmouseout", d, i); }
        },
        gauge: {
            label: {
                format: function (value, ratio) {
                    return value;
                },
                show: true // to turn off the min/max labels.
            },
            min: 0, // 0 is default, //can handle negative min e.g. vacuum / voltage / current flow / rate of change
            max: this.$props.gaugedata.MaxValue, // 100 is default
            units: this.$props.gaugedata.Units,
            width: 32 // for adjusting arc thickness
        },
        chartColor: '#0000FF',
        //color: {
        //    pattern: ['#008000', '#FFFF00', '#FF0000'], // the color levels for the percentage values.
        //    threshold: { values: [33, 66, 100] }
        //},
        size: {
            height: 200
        }
    });

    this.gauge.load({
        columns: [[this.$props.gaugedata.Name, this.$props.gaugedata.Value]]
    });

}
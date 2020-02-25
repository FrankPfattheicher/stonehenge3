

updated: function () {

    if(typeof(this.chart) == "undefined") this.chart = c3.generate({
        bindto: this.$el,
        data: {
            columns: [['data1'].concat(this.$props.chartdata)],
            axes: {
                'data1': 'y'
            },
        },
        axis: {
            y: {
                show: true,
                min: 0,
                max: 10
            }
        }
    });

    this.chart.load({
        columns: [['data1'].concat(this.$props.chartdata)]
    });

}


updated: function () {

    if(typeof(this.chart) == "undefined") {

        this.chart = c3.generate({
            bindto: this.$el,
            data: this.$props.chartdata.Data,
            axis: this.$props.chartdata.Axis
        });
    }

    this.chart.load({
        columns: this.$props.chartdata.Data.columns
    });

}

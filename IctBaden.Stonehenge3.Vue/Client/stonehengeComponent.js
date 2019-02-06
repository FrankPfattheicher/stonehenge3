

stonehengeViewModelName = function component() {


    let vm = {

        StonehengeCancelRequests: function () {
            try {
                if (app.previousRequest) {
                    app.previousRequest.abort();
                }
            } catch (error) {
                //debugger;
                if (console && console.log) console.log(error);
            }
            this.StonehengePollEventsActive = null;
        },

        StonehengeSetViewModelData: function (vmdata) {
            for (var propertyName in vmdata) {
                if (propertyName === "StonehengeNavigate") {
                    var target = vmdata[propertyName];
                    if (target.startsWith('#')) {
                        try {
                            document.getElementById(target.substring(1))
                                .scrollIntoView({ block: 'end', behaviour: 'smooth' });
                        } catch (error) {
                            // ignore
                            if (console && console.log) {
                                console.log("stonehengeViewModelName error: " + error);
                            }
                        }
                    } else {
                        app.$router.push(target);
                    }
                } else if (propertyName === "StonehengeEval") {
                    try {
                        var script = vmdata[propertyName];
                        eval(script);
                    } catch (error) {
                        // ignore
                        if (console && console.log) {
                            console.log("script: " + script);
                            console.log("stonehengeViewModelName error: " + error);
                        }
                    }
                } else {
                    //debugger;
                    this.model[propertyName] = vmdata[propertyName];
                }
            }
            if (app.stonehengeViewModelName.model.StonehengeInitialLoading) {
                if (typeof (stonehengeViewModelName_InitialLoaded) == 'function') {
                    try {
                        stonehengeViewModelName_InitialLoaded(app.stonehengeViewModelName.model);
                    } catch (e) { }
                }
            } else {
                if (typeof (stonehengeViewModelName_DataLoaded) == 'function') {
                    try {
                        stonehengeViewModelName_DataLoaded(app.stonehengeViewModelName.model);
                    } catch (e) { }
                }
            }
        },

        StonehengePost: function (urlWithParams) {
            this.StonehengeCancelRequests();

            var props = ['propNames'];
            var formData = new Object();
            props.forEach(function (prop) {
                formData[prop] = app.stonehengeViewModelName.model[prop];
            });
            this.StonehengePostActive = true;
            app.$http.post(urlWithParams, JSON.stringify(formData),
                {
                    before(request) {
                        app.previousRequest = request;
                    }
                })
                .then(response => {
                    let data = JSON.parse(response.bodyText);
                    this.StonehengeInitialLoading = false;
                    this.StonehengeIsLoading = false;
                    if (this.StonehengePostActive) {
                        this.StonehengeSetViewModelData(data);
                        this.StonehengePostActive = false;
                    }
                    if (this.StonehengePollEventsActive === null) {
                        setTimeout(function () { app.stonehengeViewModelName.StonehengePollEvents(true); }, this.StonehengePollDelay);
                    }
                })
                .catch(error => {
                    if (error.status >= 400) {
                        this.StonehengeIsDisconnected = true;
                        //debugger;
                        window.location.reload();
                    }
                });
        },

        StonehengePollEvents: function (continuePolling) {
            if (!app.stonehengeViewModelName.model.StonehengeActive || app.stonehengeViewModelName.model.StonehengePostActive
                || app.stonehengeViewModelName.model.StonehengePollEventsActive !== null) return;
            var ts = new Date().getTime();
            app.$http.get('/Events/stonehengeViewModelName?ts=' + ts,
                {
                    before(request) {
                        app.stonehengeViewModelName.model.StonehengePollEventsActive = request;
                        app.previousRequest = request;
                    }
                })
                .then(response => {
                    if (app.stonehengeViewModelName.model.StonehengePostActive) {
                        //debugger;
                        return;
                    }
                    try {
                        let data = JSON.parse(response.bodyText);
                        app.stonehengeViewModelName.model.StonehengePollEventsActive = null;
                        app.stonehengeViewModelName.model.StonehengeIsDisconnected = false;
                        app.stonehengeViewModelName.StonehengeSetViewModelData(data);
                    } catch (error) {
                        setTimeout(function () { window.location.reload(); }, 100);
                    }
                    if (continuePolling || app.stonehengeViewModelName.model.StonehengeContinuePolling) {
                        setTimeout(function () { app.stonehengeViewModelName.StonehengePollEvents(false); }, app.stonehengeViewModelName.model.StonehengePollDelay);
                    }
                })
                .catch(error => {
                    if (app.stonehengeViewModelName.model.StonehengePollEventsActive !== null) {
                        app.stonehengeViewModelName.model.StonehengeIsDisconnected = true;
                    }
                    if (error.status >= 400) {
                        setTimeout(function () { window.location.reload(); }, 1000);
                    } else {
                        app.stonehengeViewModelName.model.StonehengePollEventsActive = null;
                        if (!app.stonehengeViewModelName.model.StonehengePostActive) {
                            setTimeout(function () { app.stonehengeViewModelName.StonehengePollEvents(true); }, app.stonehengeViewModelName.model.StonehengePollDelay);
                        }
                    }
                });
        },

        StonehengeGetViewModel: function () {
            this.StonehengeCancelRequests();
            app.$http.get('ViewModel/stonehengeViewModelName',
                {
                    before(request) {
                        app.previousRequest = request;
                    }
                })
                .then(response => {
                    var cookie = response.headers.get("cookie");
                    var match = (/stonehenge-id=([0-9a-fA-F]+)/).exec(cookie);
                    if (match == null) {
                        app.stonehengeViewModelName.model.StonehengeSession = stonehengeGetCookie("stonehenge-id");
                    }
                    else {
                        app.stonehengeViewModelName.model.StonehengeSession = match[1];
                    }
                    try {
                        let data = JSON.parse(response.bodyText);
                        app.stonehengeViewModelName.StonehengeSetViewModelData(data);
                    } catch (error) {
                        if (console && console.log) console.log(error);
                    }
                    app.stonehengeViewModelName.model.StonehengeInitialLoading = false;
                    app.stonehengeViewModelName.model.StonehengeIsLoading = false;
                    if (app.stonehengeViewModelName.model.StonehengePollEventsActive === null) {
                        setTimeout(function () { app.stonehengeViewModelName.StonehengePollEvents(true); }, app.stonehengeViewModelName.model.StonehengePollDelay);
                    }
                })
                .catch(error => {
                    app.stonehengeViewModelName.model.StonehengeIsDisconnected = true;
                    //debugger;
                    if (console && console.log) console.log(error);
                    setTimeout(function () { window.location.reload(); }, 1000);
                    window.location.reload();
                });

            console.log('stonehengeViewModelName loaded');
        },

        model: {

            StonehengeActive: false,
            StonehengePollEventsActive: null,
            StonehengePollDelay: stonehengePollDelay,
            StonehengeInitialLoading: true,
            StonehengeIsLoading: true,
            StonehengeIsDirty: false,
            StonehengeIsDisconnected: false,
            StonehengePostActive: false,
            StonehengeSession: '<none>'
            //stonehengeProperties

        },

        data: function () {
            console.log('stonehengeViewModelName get data');
            //debugger;
            app.stonehengeViewModelName.StonehengeGetViewModel();
            app.stonehengeViewModelName.model.StonehengeActive = true;

            return app.stonehengeViewModelName.model;
        },
        methods: {
            /*commands*/
        }
    };

    app.stonehengeViewModelName = vm;
    console.log('stonehengeViewModelName created');

    return vm;
};

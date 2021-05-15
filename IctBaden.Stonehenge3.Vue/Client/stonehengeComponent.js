stonehengeViewModelName = function component() {


    let vm = {

        StonehengeCancelVmRequests: function () {
            stonehengeCancelRequests();
            this.model.StonehengePollEventsActive = null;
        },

        StonehengeSetViewModelData: function (vmData) {
            for (let propertyName in vmData) {
                if (propertyName === "StonehengeNavigate") {
                    let target = vmData[propertyName];
                    if (target.startsWith('#')) {
                        try {
                            document.getElementById(target.substring(1))
                                .scrollIntoView({block: 'end', behaviour: 'smooth'});
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
                        let script = vmData[propertyName];
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
                    this.model[propertyName] = vmData[propertyName];
                }
            }
            if (app.stonehengeViewModelName.model.StonehengeInitialLoading) {
                if (typeof (stonehengeViewModelName_InitialLoaded) == 'function') {
                    try {
                        stonehengeViewModelName_InitialLoaded(app.stonehengeViewModelName.model);
                    } catch (e) {
                    }
                }
            } else {
                if (typeof (stonehengeViewModelName_DataLoaded) == 'function') {
                    try {
                        stonehengeViewModelName_DataLoaded(app.stonehengeViewModelName.model);
                    } catch (e) {
                    }
                }
            }
        },

        StonehengePost: function (urlWithParams) {
            this.StonehengeCancelVmRequests();

            let props = ['propNames'];
            let formData = {};
            props.forEach(function (prop) {
                formData[prop] = app.stonehengeViewModelName.model[prop];
            });
            this.StonehengePostActive = true;
            Vue.http.post(urlWithParams, JSON.stringify(formData),
                {
                    before(request) {
                        request.headers.append('Stonehenge-Id', app.stonehengeViewModelName.model.StonehengeSession);
                        app.activeRequests.add(request);
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
                    if (!this.StonehengePollEventsActive) {
                        setTimeout(function () {
                            app.stonehengeViewModelName.StonehengePollEvents(true);
                        }, this.StonehengePollDelay);
                    }
                })
                .catch(error => {
                    if (error.status >= 400) {
                        //debugger;
                        this.StonehengeIsDisconnected = true;
                        app.stonehengeReloadOnError(error);
                    }
                });
        },

        StonehengePollEvents: function (continuePolling) {
            if (!app.stonehengeViewModelName.model.StonehengeActive
                || app.stonehengeViewModelName.model.StonehengePostActive) return;
            if (app.stonehengeViewModelName.model.StonehengePollEventsActive
                || app.activeViewModelName !== 'stonehengeViewModelName') {
                //debugger;
                return;
            }
            let ts = new Date().getTime();
            Vue.http.get('Events/stonehengeViewModelName?ts=' + ts + '&stonehenge-id=' + app.stonehengeViewModelName.model.StonehengeSession,
                {
                    before(request) {
                        app.stonehengeViewModelName.model.StonehengePollEventsActive = request;
                        request.headers.append('Stonehenge-Id', app.stonehengeViewModelName.model.StonehengeSession);
                        app.activeRequests.add(request);
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
                        setTimeout(function () {
                            app.stonehengeReloadOnError(error);
                        }, 100);
                    }
                    if (continuePolling || app.stonehengeViewModelName.model.StonehengeContinuePolling) {
                        setTimeout(function () {
                            app.stonehengeViewModelName.StonehengePollEvents(false);
                        }, app.stonehengeViewModelName.model.StonehengePollDelay);
                    }
                })
                .catch(error => {
                    if (app.stonehengeViewModelName.model.StonehengePollEventsActive
                        && app.activeViewModelName === 'stonehengeViewModelName') {
                        //debugger;
                        app.stonehengeViewModelName.model.StonehengeIsDisconnected = true;
                    }
                    if (error.status >= 400) {
                        setTimeout(function (app) {
                            app.stonehengeReloadOnError(error);
                        }, 1000);
                    } else {
                        app.stonehengeViewModelName.model.StonehengePollEventsActive = null;
                        if (!app.stonehengeViewModelName.model.StonehengePostActive) {
                            setTimeout(function () {
                                app.stonehengeViewModelName.StonehengePollEvents(true);
                            }, app.stonehengeViewModelName.model.StonehengePollDelay);
                        }
                    }
                });
        },

        StonehengeGetViewModel: function () {
            app.activeViewModelName = 'stonehengeViewModelName';
            this.StonehengeCancelVmRequests();
            Vue.http.get('ViewModel/stonehengeViewModelName?stonehenge-id=' + app.stonehengeViewModelName.model.StonehengeSession,
                {
                    before(request) {
                        request.headers.append('Stonehenge-Id', app.stonehengeViewModelName.model.StonehengeSession);
                        app.activeRequests.add(request);
                    }
                })
                .then(response => {
                    let cookie = response.headers.get("cookie");
                    let match = (/stonehenge-id=([0-9a-fA-F]+)/).exec(cookie);
                    if (match == null) {
                        app.stonehengeViewModelName.model.StonehengeSession = stonehengeGetCookie("stonehenge-id");
                    } else {
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
                    if (!app.stonehengeViewModelName.model.StonehengePollEventsActive) {
                        setTimeout(function () {
                            app.stonehengeViewModelName.StonehengePollEvents(true);
                        }, app.stonehengeViewModelName.model.StonehengePollDelay);
                    }
                })
                .catch(error => {
                    if (error.status) {
                        //debugger;
                        app.stonehengeViewModelName.model.StonehengeIsDisconnected = true;
                        app.stonehengeReloadOnError(error);
                    }
                });

            if (stonehengeDebugBuild) console.log('stonehengeViewModelName loaded');
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
            StonehengeSession: ''
            //stonehengeProperties

        },

        data: function () {
            if (stonehengeDebugBuild) console.log('stonehengeViewModelName get data');
            //debugger;
            app.stonehengeViewModelName.StonehengeGetViewModel();
            app.stonehengeViewModelName.model.StonehengeActive = true;

            return app.stonehengeViewModelName.model;
        },
        methods: {
            /*commands*/
        }
    };

    stonehengeCancelRequests();
    app.stonehengeViewModelName = vm;
    if (stonehengeDebugBuild) console.log('stonehengeViewModelName created');

    return vm;
};

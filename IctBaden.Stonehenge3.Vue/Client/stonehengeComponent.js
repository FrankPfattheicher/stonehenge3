

stonehengeViewModelName = function component () {


    let vm = {

    StonehengeCancelRequests : function() {
        //for (var rq = 0; rq < app.$http.pendingRequests.length; rq++) {
            //app.$http.pendingRequests[rq].abort();
        //}
        this.StonehengePollEventsActive = null;
    },

    StonehengeSetViewModelData : function(vmdata) {
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
                            console.log("error: " + error);
                        }
                    }
                } else {
                    //app.stonehengeViewModelName.StonehengeRouter.navigateToRoute(target);
                }
            } else if (propertyName === "StonehengeEval") {
                try {
                    var script = vmdata[propertyName];
                    eval(script);
                } catch (error) {
                    // ignore
                    if (console && console.log) {
                        console.log("script: " + script);
                        console.log("error: " + error);
                    }
                }
            } else {
                //debugger;
                this.model[propertyName] = vmdata[propertyName];
            }
        }
    },

        
    StonehengePollEvents : function(continuePolling) {
        if (!app.stonehengeViewModelName.model.StonehengeActive || app.stonehengeViewModelName.model.StonehengePostActive 
            || app.stonehengeViewModelName.model.StonehengePollEventsActive != null) return;
        var ts = new Date().getTime();
        app.stonehengeViewModelName.model.StonehengePollEventsActive = app.$http.get('/Events/stonehengeViewModelName?ts=' + ts)
            .then(response => {
                let data = JSON.parse(response.bodyText);
                app.stonehengeViewModelName.model.StonehengePollEventsActive = null;
                app.stonehengeViewModelName.model.StonehengeIsDisconnected = false;
                app.stonehengeViewModelName.StonehengeSetViewModelData(data);
                if (continuePolling || app.stonehengeViewModelName.model.StonehengeContinuePolling) {
                    setTimeout(function() { app.stonehengeViewModelName.StonehengePollEvents(false); }, app.stonehengeViewModelName.model.StonehengePollDelay);
            }
        })
        .catch(error => {
            if (app.stonehengeViewModelName.model.StonehengePollEventsActive != null) {
            app.stonehengeViewModelName.model.StonehengeIsDisconnected = true;
        }
        if (error.responseType != "abort") {
            debugger;
            if (status === 200) {
                setTimeout(function() { window.location.reload(); }, 1000);
            }
            app.stonehengeViewModelName.model.StonehengePollEventsActive = null;
            if (!app.stonehengeViewModelName.model.StonehengePostActive) {
                setTimeout(function() { app.stonehengeViewModelName.StonehengePollEvents(true); }, app.stonehengeViewModelName.model.StonehengePollDelay);
            }
        }
    });
    },

    StonehengeGetViewModel : function() {
        this.StonehengeCancelRequests();
        app.$http.get('ViewModel/stonehengeViewModelName')
            .then(response => {
                var cookie = response.headers.get("cookie");
                var match = (/stonehenge-id=([0-9a-fA-F]+)/).exec(cookie);
                if (match == null) {
                    // var tools = new Tools();
                    // app.stonehengeViewModelName.model.StonehengeSession = tools.getCookie("stonehenge-id");
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
                if (app.stonehengeViewModelName.model.StonehengePollEventsActive == null) {
                    setTimeout(function() { app.stonehengeViewModelName.StonehengePollEvents(true); }, app.stonehengeViewModelName.model.StonehengePollDelay);
                }
            })
            .catch(error => {
                app.stonehengeViewModelName.model.StonehengeIsDisconnected = true;
                debugger;
                if (console && console.log) console.log(error);
                setTimeout(function() { window.location.reload(); }, 1000);
                window.location.reload();
            });

        console.log('vm loaded');
    },

        model: {

            StonehengeActive : false,
            StonehengePollEventsActive : null,
            StonehengePollDelay : 10000,
            StonehengeInitialLoading : true,
            StonehengeIsLoading : true,
            StonehengeIsDirty : false,
            StonehengeIsDisconnected : false,
            StonehengePostActive : false,
            StonehengeSession : '<none>'
            //stonehengeProperties
            
    },
    
    data: function ()  {
        console.log('get data');
        //debugger;
        app.stonehengeViewModelName.StonehengeGetViewModel();
        app.stonehengeViewModelName.model.StonehengeActive = true;
        
        return app.stonehengeViewModelName.model;
    },
    methods: {
            
            countDownChanged() {
                this.StonehengeActive = false
            }

        }
};

    app.stonehengeViewModelName = vm;
    console.log('stonehengeViewModelName created');

    return vm;
};

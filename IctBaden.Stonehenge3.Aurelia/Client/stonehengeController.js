
// ReSharper disable Es6Feature
import { inject } from 'aurelia-framework';
import { HttpClient } from 'aurelia-http-client';
import { App, Tools } from 'app';

@inject(App)
export class stonehengeViewModelName {

    constructor(App) {

        this.http = new HttpClient();
        this.StonehengeRouter = App.router;
    
        this.StonehengeActive = false;
        this.StonehengeInitialLoading = true;
        this.StonehengeIsLoading = true;
        this.StonehengeIsDirty = false;
        this.StonehengeIsDisconnected = false;
        this.StonehengePostActive = false;
        this.StonehengePollEventsActive = null;
        this.StonehengePollDelay = stonehengePollDelay;

        this.StonehengeCancelRequests = function(scope) {
            for (var rq = 0; rq < scope.http.pendingRequests.length; rq++) {
                scope.http.pendingRequests[rq].abort();
            }
            scope.StonehengePollEventsActive = null;
        };

        this.StonehengePollEvents = function(scope, continuePolling) {
            if (!scope.StonehengeActive || scope.StonehengePostActive || scope.StonehengePollEventsActive != null) return;
            var ts = new Date().getTime();
            scope.StonehengePollEventsActive = scope.http.get('/Events/stonehengeViewModelName?ts=' + ts)
                .then(response => {
                    let data = JSON.parse(response.response);
                    scope.StonehengePollEventsActive = null;
                    scope.StonehengeIsDisconnected = false;
                    scope.StonehengeSetViewModelData(scope, data);
                    if (continuePolling || scope.StonehengeContinuePolling) {
                        setTimeout(function() { scope.StonehengePollEvents(scope, false); }, scope.StonehengePollDelay);
                    }
                })
                .catch(error => {
                    if (scope.StonehengePollEventsActive != null) {
                        scope.StonehengeIsDisconnected = true;
                    }
                    if (error.responseType != "abort") {
                        //debugger;
                        if (status === 200) {
                            setTimeout(function() { window.location.reload(); }, 1000);
                        }
                        scope.StonehengePollEventsActive = null;
                        if (!scope.StonehengePostActive) {
                            setTimeout(function() { scope.StonehengePollEvents(scope, true); }, scope.StonehengePollDelay);
                        }
                    }
                });
        };

        this.StonehengePost = function(scope, urlWithParams) {
            scope.StonehengeCancelRequests(scope);
      
            var props = ['propNames'];
            var formData = new Object();
            props.forEach(function(prop) {
                formData[prop] = scope[prop];
            });
            scope.StonehengePostActive = true;
            scope.http.post(urlWithParams, JSON.stringify(formData) )
                .then(response => {
                    let data = JSON.parse(response.response);
                    scope.StonehengeInitialLoading = false;
                    scope.StonehengeIsLoading = false;
                    if (scope.StonehengePostActive) {
                        scope.StonehengeSetViewModelData(scope, data);
                        scope.StonehengePostActive = false;
                    }
                    if (scope.StonehengePollEventsActive == null) {
                        setTimeout(function() { scope.StonehengePollEvents(scope, true); }, scope.StonehengePollDelay);
                    }
                })
                .catch(error => {
                    if (error.responseType != "abort") {
                        scope.StonehengeIsDisconnected = true;
                        //debugger;
                        window.location.reload();
                    }
                });
        };

        this.StonehengeGetViewModel = function(scope) {
            scope.StonehengeCancelRequests(scope);
            scope.http.get('ViewModel/stonehengeViewModelName')
                .then(response => {
                    var cookie = response.headers.get("cookie");
                    var match = (/stonehenge-id=([0-9a-fA-F]+)/).exec(cookie);
                    if (match == null) {
                        var tools = new Tools();
                        scope.StonehengeSession = tools.getCookie("stonehenge-id");
                    }
                    else {
                        scope.StonehengeSession = match[1];
                    }
                    try {
                        let data = JSON.parse(response.response);
                        scope.StonehengeSetViewModelData(scope, data);
                    } catch (error) {
                        if (console && console.log) console.log(error);
                    } 
                    if (scope.StonehengeInitialLoading) {
                        if (typeof (user_InitialLoaded) == 'function') {
                            try {
                                user_InitialLoaded(scope);
                            } catch (e) { }
                        }
                    } 
                    scope.StonehengeInitialLoading = false;
                    scope.StonehengeIsLoading = false;
                    if (scope.StonehengePollEventsActive == null) {
                        setTimeout(function() { scope.StonehengePollEvents(scope, true); }, scope.StonehengePollDelay);
                    }
                })
                .catch(error => {
                    scope.StonehengeIsDisconnected = true;
                    //debugger;
                    if (console && console.log) console.log(error);
                    setTimeout(function() { window.location.reload(); }, 1000);
                    window.location.reload();
                });
        };

        this.StonehengeSetViewModelData = function(viewmodel, data) {
            for (var propertyName in data) {

                if (propertyName === "StonehengeNavigate") {
                    var target = data[propertyName];
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
                        viewmodel.StonehengeRouter.navigateToRoute(target);
                    }
                } else if (propertyName === "StonehengeEval") {
                    try {
                        var script = data[propertyName];
                        eval(script);
                    } catch (error) {
                        // ignore
                        if (console && console.log) {
                            console.log("script: " + script);
                            console.log("error: " + error);
                        }
                    } 
                } else {
                    viewmodel[propertyName] = data[propertyName];
                }
            }
            if (!viewmodel.StonehengeInitialLoading) {
                if (typeof (user_DataLoaded) == 'function') {
                    try {
                        user_DataLoaded(viewmodel);
                    } catch (e) { }
                }
            }
        };

        /*commands*/

    }

    activate() {
        this.StonehengeActive = true;
        this.StonehengeGetViewModel(this);
    }

    deactivate() {
        this.StonehengeActive = false;
        this.StonehengeCancelRequests(this);
    }

}
// ReSharper restore Es6Feature

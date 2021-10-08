
// Stonehenge3 application

function stonehengeCancelRequests() {

    try {
        app.activeRequests.forEach(rq => {
            rq.abort();
        });
        app.activeRequests.clear();
    } catch (error) {
        //debugger;
        if (console && console.log) console.log(error);
    }
}

function stonehengeReloadOnError(error) {
    if (console && console.log) console.log(error);
    window.location.reload();
}

function stonehengeMakeRequest(method, url) {
    return new Promise(function (resolve, reject) {

        const xhr = new XMLHttpRequest();
        xhr.open(method, url);
        xhr.onload = function () {
            if (this.status >= 200 && this.status < 300) {
                resolve(xhr.responseText);
            } else {
                reject({
                    status: this.status,
                    statusText: xhr.statusText
                });
            }
        };
        xhr.onerror = function () {
            reject({
                status: this.status,
                statusText: xhr.statusText
            });
        };
        xhr.send();
    });
}

function stonehengeMakeGetRequest(url) {
    return stonehengeMakeRequest('GET', url);
}
function stonehengeMakePostRequest(url) {
    return stonehengeMakeRequest('POST', url);
}

async function stonehengeLoadComponent(name) {

    const srcRequest = stonehengeMakeGetRequest(name + '.js');
    const templateRequest = stonehengeMakeGetRequest(name + '.html');

    let src;
    let srcText;
    let templateText;
    [templateText, srcText] = await Promise.all([templateRequest, srcRequest]);

    src = eval(srcText)();

    return Vue.component('stonehenge_' + name, {
            template: templateText,
            data: src.data,
            methods: src.methods
        }
    );
}
function stonehengeGetCookie(name) {
    let i = 0; //Suchposition im Cookie
    const search = name + "=";
    const maxLen = document.cookie.length;
    while (i < maxLen) {
        if (document.cookie.substring(i, i + search.length) === search) {
            let end = document.cookie.indexOf(";", i + search.length);
            if (end < 0) {
                end = maxLen;
            }
            const cook = document.cookie.substring(i + search.length, end);
            return unescape(cook);
        }
        i++;
    }
    return "";
}

function stonehengeCopyToClipboard(text) {
    const textarea = document.createElement('textarea')
    document.body.appendChild(textarea)
    textarea.value = text
    textarea.select()
    document.execCommand('copy')
    textarea.remove()
}

function stonehengeEnableRoute(route, enabled) {
    // { path: '/rrrr', name: 'rrrr', title: 'rrrr', component: () => Promise.resolve(stonehengeLoadComponent('rrrr')), visible: true }
    let found = routes.filter(function (item) { return item.name === route; })[0] || null;
    if(found) {
        found.visible = enabled;
    }
}

// Router
const routes = [
    //stonehengeAppRoutes
];

const router = new VueRouter({
    routes: routes
});

// Register a global custom directive called `v-focus`
Vue.directive('focus', {
    // When the bound element inserted into the DOM...
    inserted: function (el) {
        // Focus the element
        el.focus();
    }
})

// Register a global custom directive called `v-focus`
Vue.directive('select', {
    // When the bound element inserted into the DOM...
    inserted: function (el) {
        // Focus the element
        el.focus();
        el.select();
    }
})

// Global App Commands
function AppCommand(cmdName) {
    stonehengeMakePostRequest('Command/' + cmdName);
}

// Components

//stonehengeElements

// App
const app = new Vue({
    data: {
        stonehengeReloadOnError: stonehengeReloadOnError,
        stonehengeCancelRequests: stonehengeCancelRequests,
        stonehengeMakeRequest: stonehengeMakeGetRequest,
        routes: routes,
        title: 'stonehengeAppTitle',
        activeViewModelName: '',
        activeRequests: new Set()
    },
    router: router
}).$mount('#app');

router.push('stonehengeRootPage');


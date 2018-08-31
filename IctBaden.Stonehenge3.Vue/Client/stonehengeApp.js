
function stonehengeMakeRequest(url) {
    return new Promise(function (resolve, reject) {

        const xhr = new XMLHttpRequest();
        xhr.open('GET', url);
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

async function stonehengeLoadComponent(name) {

    const srcRequest = stonehengeMakeRequest(name + '.js');
    const templateRequest = stonehengeMakeRequest(name + '.html');

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
    var i = 0; //Suchposition im Cookie
    var suche = name + "=";
    var maxlen = document.cookie.length;
    while (i < maxlen) {
        if (document.cookie.substring(i, i + suche.length) === suche) {
            var ende = document.cookie.indexOf(";", i + suche.length);
            if (ende < 0) {
                ende = maxlen;
            }
            var cook = document.cookie.substring(i + suche.length, ende);
            return unescape(cook);
        }
        i++;
    }
    return "";
}

// Router
const routes = [
    //stonehengeAppRoutes
];

const router = new VueRouter({
    routes: routes
});

// App
const app = new Vue({
    data: {
        stonehengeMakeRequest: stonehengeMakeRequest,
        routes: routes,
        title: 'stonehengeAppTitle'
    },
    router: router
}).$mount('#app');


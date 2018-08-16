
function makeRequest(url) {
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

async function loadComponent(name) {

    const srcRequest = makeRequest(name + '.js');
    const templateRequest = makeRequest(name + '.html');

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
        makeRequest: makeRequest,
        routes: routes,
        title: 'stonehengeAppTitle'
    },
    router: router
}).$mount('#app');



const NotFound = { template: '<p>Page not found</p>' }
const Home = { template: '<div id="app">Home {{message}}</div>' }
const Index = { template: '<div id="app">Index {{message}}<script src="start.js"></script></div>' }

const routes = {
    '/Index.html#test': Home,
    '/Index.html': Index
}

new Vue({
    el: '#app',
    data: {
        currentRoute: window.location.pathname
    },
    computed: {
        ViewComponent() {
            return routes[this.currentRoute] || NotFound
        }
    },
    render(h) { return h(this.ViewComponent) }
})

var app = new Vue({
    el: '#app',
    data: {
        message: 'Hello Vue!'
    }
})


/*!
  * vue-router v4.0.0-beta.10
  * (c) 2020 Eduardo San Martin Morote
  * @license MIT
  */
var VueRouter=function(e,t){"use strict";const n="function"==typeof Symbol&&"symbol"==typeof Symbol.toStringTag,r=e=>n?Symbol(e):"_vr_"+e,o=r("rvlm"),a=r("rvd"),c=r("r"),i=r("rl"),s="undefined"!=typeof window;const l=Object.assign;function u(e,t){const n={};for(const r in t){const o=t[r];n[r]=Array.isArray(o)?o.map(e):e(o)}return n}let f=()=>{};const p=/\/$/;function h(e,t,n="/"){let r,o={},a="",c="";const i=t.indexOf("?"),s=t.indexOf("#",i>-1?i:0);return i>-1&&(r=t.slice(0,i),a=t.slice(i+1,s>-1?s:t.length),o=e(a)),s>-1&&(r=r||t.slice(0,s),c=t.slice(s,t.length)),r=function(e,t){if(e.startsWith("/"))return e;if(!e)return t;const n=t.split("/"),r=e.split("/");let o,a,c=n.length-1;for(o=0;o<r.length;o++)if(a=r[o],1!==c&&"."!==a){if(".."!==a)break;c--}return n.slice(0,c).join("/")+"/"+r.slice(o-(o===r.length?1:0)).join("/")}(null!=r?r:t,n),{fullPath:r+(a&&"?")+a+c,path:r,query:o,hash:c}}function d(e,t){return!t||e.toLowerCase().indexOf(t.toLowerCase())?e:e.slice(t.length)||"/"}function m(e,t){return(e.aliasOf||e)===(t.aliasOf||t)}function g(e,t){if(Object.keys(e).length!==Object.keys(t).length)return!1;for(let n in e)if(!v(e[n],t[n]))return!1;return!0}function v(e,t){return Array.isArray(e)?y(e,t):Array.isArray(t)?y(t,e):e===t}function y(e,t){return Array.isArray(t)?e.length===t.length&&e.every((e,n)=>e===t[n]):1===e.length&&e[0]===t}var b,w;!function(e){e.pop="pop",e.push="push"}(b||(b={})),function(e){e.back="back",e.forward="forward",e.unknown=""}(w||(w={}));function R(e){if(!e)if(s){const t=document.querySelector("base");e=(e=t&&t.getAttribute("href")||"/").replace(/^\w+:\/\/[^\/]+/,"")}else e="/";return"/"!==e[0]&&"#"!==e[0]&&(e="/"+e),e.replace(p,"")}const E=/^[^#]+#/;function O(e,t){return e.replace(E,"#")+t}const A=()=>({left:window.pageXOffset,top:window.pageYOffset});function k(e){let t;if("el"in e){let n=e.el;const r="string"==typeof n&&n.startsWith("#"),o="string"==typeof n?r?document.getElementById(n.slice(1)):document.querySelector(n):n;if(!o)return;t=function(e,t){const n=document.documentElement.getBoundingClientRect(),r=e.getBoundingClientRect();return{behavior:t.behavior,left:r.left-n.left-(t.left||0),top:r.top-n.top-(t.top||0)}}(o,e)}else t=e;"scrollBehavior"in document.documentElement.style?window.scrollTo(t):window.scrollTo(null!=t.left?t.left:window.pageXOffset,null!=t.top?t.top:window.pageYOffset)}function x(e,t){return(history.state?history.state.position-t:-1)+e}const P=new Map;let j=()=>location.protocol+"//"+location.host;function C(e,t){const{pathname:n,search:r,hash:o}=t;if(e.indexOf("#")>-1){let e=o.slice(1);return"/"!==e[0]&&(e="/"+e),d(e,"")}return d(n,e)+r+o}function $(e,t,n,r=!1,o=!1){return{back:e,current:t,forward:n,replaced:r,position:window.history.length,scroll:o?A():null}}function S(e){const{history:t,location:n}=window;let r={value:C(e,n)},o={value:t.state};function a(r,a,c){const i=j()+(e.indexOf("#")>-1&&n.search?n.pathname+n.search+"#":e)+r;try{t[c?"replaceState":"pushState"](a,"",i),o.value=a}catch(e){!function(e){const t=Array.from(arguments).slice(1);console.warn.apply(console,["[Vue Router warn]: "+e].concat(t))}("Error with push/replace State",e),n[c?"replace":"assign"](i)}}return o.value||a(r.value,{back:null,current:r.value,forward:null,position:t.length-1,replaced:!0,scroll:null},!0),{location:r,state:o,push:function(e,n){const c=l({},o.value,t.state,{forward:e,scroll:A()});a(c.current,c,!0),a(e,l({},$(r.value,e,null),{position:c.position+1},n),!1),r.value=e},replace:function(e,n){a(e,l({},t.state,$(o.value.back,e,o.value.forward,!0),n,{position:o.value.position}),!0),r.value=e}}}function L(e){const t=S(e=R(e)),n=function(e,t,n,r){let o=[],a=[],c=null;const i=({state:a})=>{const i=C(e,location),s=n.value,l=t.value;let u=0;if(a){if(n.value=i,t.value=a,c&&c===s)return void(c=null);u=l?a.position-l.position:0}else r(i);o.forEach(e=>{e(n.value,s,{delta:u,type:b.pop,direction:u?u>0?w.forward:w.back:w.unknown})})};function s(){const{history:e}=window;e.state&&e.replaceState(l({},e.state,{scroll:A()}),"")}return window.addEventListener("popstate",i),window.addEventListener("beforeunload",s),{pauseListeners:function(){c=n.value},listen:function(e){o.push(e);const t=()=>{const t=o.indexOf(e);t>-1&&o.splice(t,1)};return a.push(t),t},destroy:function(){for(const e of a)e();a=[],window.removeEventListener("popstate",i),window.removeEventListener("beforeunload",s)}}}(e,t.state,t.location,t.replace);const r=l({location:"",base:e,go:function(e,t=!0){t||n.pauseListeners(),history.go(e)},createHref:O.bind(null,e)},t,n);return Object.defineProperty(r,"location",{get:()=>t.location.value}),Object.defineProperty(r,"state",{get:()=>t.state.value}),r}function q(e){return"string"==typeof e||"symbol"==typeof e}const M={path:"/",name:void 0,params:{},query:{},hash:"",fullPath:"/",matched:[],meta:{},redirectedFrom:void 0},T=r("nf");var _;function B(e,t){return l(new Error,{type:e,[T]:!0},t)}function G(e,t){return e instanceof Error&&T in e&&(null==t||!!(e.type&t))}(_=e.NavigationFailureType||(e.NavigationFailureType={}))[_.aborted=4]="aborted",_[_.cancelled=8]="cancelled",_[_.duplicated=16]="duplicated";const F="[^/]+?",U={sensitive:!1,strict:!1,start:!0,end:!0},V=/[.+*?^${}()[\]/\\]/g;function H(e,t){let n=0;for(;n<e.length&&n<t.length;){const r=t[n]-e[n];if(r)return r;n++}return e.length<t.length?1===e.length&&80===e[0]?-1:1:e.length>t.length?1===t.length&&80===t[0]?1:-1:0}function I(e,t){let n=0;const r=e.score,o=t.score;for(;n<r.length&&n<o.length;){const e=H(r[n],o[n]);if(e)return e;n++}return o.length-r.length}const W={type:0,value:""},D=/[a-zA-Z0-9_]/;function N(e,t,n){const r=function(e,t){const n=l({},U,t);let r=[],o=n.start?"^":"";const a=[];for(const t of e){const e=t.length?[]:[90];n.strict&&!t.length&&(o+="/");for(let r=0;r<t.length;r++){const c=t[r];let i=40+(n.sensitive?.25:0);if(0===c.type)r||(o+="/"),o+=c.value.replace(V,"\\$&"),i+=40;else if(1===c.type){const{value:e,repeatable:t,optional:n,regexp:s}=c;a.push({name:e,repeatable:t,optional:n});const l=s||F;if(l!==F){i+=10;try{new RegExp(`(${l})`)}catch(t){throw new Error(`Invalid custom RegExp for param "${e}" (${l}): `+t.message)}}let u=t?`((?:${l})(?:/(?:${l}))*)`:`(${l})`;r||(u=n?`(?:/${u})`:"/"+u),n&&(u+="?"),o+=u,i+=20,n&&(i+=-8),t&&(i+=-20),".*"===l&&(i+=-50)}e.push(i)}r.push(e)}if(n.strict&&n.end){const e=r.length-1;r[e][r[e].length-1]+=.7000000000000001}n.strict||(o+="/?"),n.end?o+="$":n.strict&&(o+="(?:/|$)");const c=new RegExp(o,n.sensitive?"":"i");return{re:c,score:r,keys:a,parse:function(e){const t=e.match(c),n={};if(!t)return null;for(let e=1;e<t.length;e++){const r=t[e]||"",o=a[e-1];n[o.name]=r&&o.repeatable?r.split("/"):r}return n},stringify:function(t){let n="",r=!1;for(const o of e){r&&n.endsWith("/")||(n+="/"),r=!1;for(const e of o)if(0===e.type)n+=e.value;else if(1===e.type){const{value:o,repeatable:a,optional:c}=e,i=o in t?t[o]:"";if(Array.isArray(i)&&!a)throw new Error(`Provided param "${o}" is an array but it is not repeatable (* or + modifiers)`);const s=Array.isArray(i)?i.join("/"):i;if(!s){if(!c)throw new Error(`Missing required param "${o}"`);n.endsWith("/")?n=n.slice(0,-1):r=!0}n+=s}}return n}}}(function(e){if(!e)return[[]];if("/"===e)return[[W]];if(!e.startsWith("/"))throw new Error(`Route "${e}" should be "/${e}".`);function t(e){throw new Error(`ERR (${n})/"${l}": ${e}`)}let n=0,r=n;const o=[];let a;function c(){a&&o.push(a),a=[]}let i,s=0,l="",u="";function f(){l&&(0===n?a.push({type:0,value:l}):1===n||2===n||3===n?(a.length>1&&("*"===i||"+"===i)&&t(`A repeatable param (${l}) must be alone in its segment. eg: '/:ids+.`),a.push({type:1,value:l,regexp:u,repeatable:"*"===i||"+"===i,optional:"*"===i||"?"===i})):t("Invalid state to consume buffer"),l="")}function p(){l+=i}for(;s<e.length;)if(i=e[s++],"\\"!==i||2===n)switch(n){case 0:"/"===i?(l&&f(),c()):":"===i?(f(),n=1):p();break;case 4:p(),n=r;break;case 1:"("===i?(n=2,u=""):D.test(i)?p():(f(),n=0,"*"!==i&&"?"!==i&&"+"!==i&&s--);break;case 2:")"===i?"\\"==u[u.length-1]?u=u.slice(0,-1)+i:n=3:u+=i;break;case 3:f(),n=0,"*"!==i&&"?"!==i&&"+"!==i&&s--;break;default:t("Unknown state")}else r=n,n=4;return 2===n&&t(`Unfinished custom RegExp for param "${l}"`),f(),c(),o}(e.path),n),o=l(r,{record:e,parent:t,children:[],alias:[]});return t&&!o.record.aliasOf==!t.record.aliasOf&&t.children.push(o),o}function K(e,t){const n=[],r=new Map;function o(e,n,r){let i=!r,s=function(e){return{path:e.path,redirect:e.redirect,name:e.name,meta:e.meta||{},aliasOf:void 0,beforeEnter:e.beforeEnter,props:Q(e),children:e.children||[],instances:{},leaveGuards:[],updateGuards:[],enterCallbacks:{},components:"components"in e?e.components||{}:{default:e.component}}}(e);s.aliasOf=r&&r.record;const u=Y(t,e),p=[s];if("alias"in e){const t="string"==typeof e.alias?[e.alias]:e.alias;for(const e of t)p.push(l({},s,{components:r?r.record.components:s.components,path:e,aliasOf:r?r.record:s}))}let h,d;for(const t of p){let{path:l}=t;if(n&&"/"!==l[0]){let e=n.record.path,r="/"===e[e.length-1]?"":"/";t.path=n.record.path+(l&&r+l)}if(h=N(t,n,u),r?r.alias.push(h):(d=d||h,d!==h&&d.alias.push(h),i&&e.name&&!z(h)&&a(e.name)),"children"in s){let e=s.children;for(let t=0;t<e.length;t++)o(e[t],h,r&&r.children[t])}r=r||h,c(h)}return d?()=>{a(d)}:f}function a(e){if(q(e)){const t=r.get(e);t&&(r.delete(e),n.splice(n.indexOf(t),1),t.children.forEach(a),t.alias.forEach(a))}else{let t=n.indexOf(e);t>-1&&(n.splice(t,1),e.record.name&&r.delete(e.record.name),e.children.forEach(a),e.alias.forEach(a))}}function c(e){let t=0;for(;t<n.length&&I(e,n[t])>=0;)t++;n.splice(t,0,e),e.record.name&&!z(e)&&r.set(e.record.name,e)}return t=Y({strict:!1,end:!0,sensitive:!1},t),e.forEach(e=>o(e)),{addRoute:o,resolve:function(e,t){let o,a,c,i={};if("name"in e&&e.name){if(o=r.get(e.name),!o)throw B(1,{location:e});c=o.record.name,i=l(function(e,t){let n={};for(let r of t)r in e&&(n[r]=e[r]);return n}(t.params,o.keys.filter(e=>!e.optional).map(e=>e.name)),e.params),a=o.stringify(i)}else if("path"in e)a=e.path,o=n.find(e=>e.re.test(a)),o&&(i=o.parse(a),c=o.record.name);else{if(o=t.name?r.get(t.name):n.find(e=>e.re.test(t.path)),!o)throw B(1,{location:e,currentLocation:t});c=o.record.name,i=l({},t.params,e.params),a=o.stringify(i)}const s=[];let u=o;for(;u;)s.unshift(u.record),u=u.parent;return{name:c,path:a,params:i,matched:s,meta:X(s)}},removeRoute:a,getRoutes:function(){return n},getRecordMatcher:function(e){return r.get(e)}}}function Q(e){const t={},n=e.props||!1;if("component"in e)t.default=n;else for(let r in e.components)t[r]="boolean"==typeof n?n:n[r];return t}function z(e){for(;e;){if(e.record.aliasOf)return!0;e=e.parent}return!1}function X(e){return e.reduce((e,t)=>l(e,t.meta),{})}function Y(e,t){let n={};for(let r in e)n[r]=r in t?t[r]:e[r];return n}const Z=/#/g,J=/&/g,ee=/\//g,te=/=/g,ne=/\?/g,re=/%5B/g,oe=/%5D/g,ae=/%5E/g,ce=/%60/g,ie=/%7B/g,se=/%7C/g,le=/%7D/g;function ue(e){return encodeURI(""+e).replace(se,"|").replace(re,"[").replace(oe,"]")}function fe(e){return ue(e).replace(Z,"%23").replace(J,"%26").replace(te,"%3D").replace(ce,"`").replace(ie,"{").replace(le,"}").replace(ae,"^")}function pe(e){return function(e){return ue(e).replace(Z,"%23").replace(ne,"%3F")}(e).replace(ee,"%2F")}function he(e){try{return decodeURIComponent(""+e)}catch(e){}return""+e}function de(e){const t={};if(""===e||"?"===e)return t;const n=("?"===e[0]?e.slice(1):e).split("&");for(let e=0;e<n.length;++e){let[r,o]=n[e].split("=");r=he(r);let a=null==o?null:he(o);if(r in t){let e=t[r];Array.isArray(e)||(e=t[r]=[e]),e.push(a)}else t[r]=a}return t}function me(e){let t="";for(let n in e){t.length&&(t+="&");const r=e[n];if(n=fe(n),null==r){void 0!==r&&(t+=n);continue}let o=Array.isArray(r)?r.map(e=>e&&fe(e)):[r&&fe(r)];for(let e=0;e<o.length;e++)t+=(e?"&":"")+n,null!=o[e]&&(t+="="+o[e])}return t}function ge(e){const t={};for(let n in e){let r=e[n];void 0!==r&&(t[n]=Array.isArray(r)?r.map(e=>null==e?null:""+e):null==r?r:""+r)}return t}function ve(){let e=[];return{add:function(t){return e.push(t),()=>{const n=e.indexOf(t);n>-1&&e.splice(n,1)}},list:()=>e,reset:function(){e=[]}}}function ye(e,n){const r=()=>{const t=e.indexOf(n);t>-1&&e.splice(t,1)};t.onUnmounted(r),t.onDeactivated(r),t.onActivated(()=>{e.indexOf(n)<0&&e.push(n)}),e.push(n)}function be(e,t,n,r,o){const a=r&&(r.enterCallbacks[o]=r.enterCallbacks[o]||[]);return()=>new Promise((c,i)=>{const s=e=>{var s;!1===e?i(B(4,{from:n,to:t})):e instanceof Error?i(e):"string"==typeof(s=e)||s&&"object"==typeof s?i(B(2,{from:t,to:e})):(a&&r.enterCallbacks[o]===a&&"function"==typeof e&&a.push(e),c())},l=e.call(r&&r.instances[o],t,n,s);let u=Promise.resolve(l);e.length<3&&(u=u.then(s)),u.catch(e=>i(e))})}function we(e,t,r,o){const a=[];for(const i of e)for(const e in i.components){let s=i.components[e];if("beforeRouteEnter"===t||i.instances[e])if("object"==typeof(c=s)||"displayName"in c||"props"in c||"__vccOpts"in c){const n=(s.__vccOpts||s)[t];n&&a.push(be(n,r,o,i,e))}else{let c=s();c=c.catch(()=>null),a.push(()=>c.then(a=>{if(!a)return Promise.reject(new Error(`Couldn't resolve component "${e}" for the following record with path "${i.path}"`));const c=(s=a).__esModule||n&&"Module"===s[Symbol.toStringTag]?a.default:a;var s;i.components[e]=c;const l=c[t];return l&&be(l,r,o,i,e)()}))}}var c;return a}function Re(e){const n=t.inject(c),r=t.inject(i),o=t.computed(()=>n.resolve(t.unref(e.to))),a=t.computed(()=>{let{matched:e}=o.value,{length:t}=e;const n=e[t-1];let a=r.matched;if(!n||!a.length)return-1;let c=a.findIndex(m.bind(null,n));if(c>-1)return c;let i=Oe(e[t-2]);return t>1&&Oe(n)===i&&a[a.length-1].path!==i?a.findIndex(m.bind(null,e[t-2])):c}),s=t.computed(()=>a.value>-1&&function(e,t){for(let n in t){let r=t[n],o=e[n];if("string"==typeof r){if(r!==o)return!1}else if(!Array.isArray(o)||o.length!==r.length||r.some((e,t)=>e!==o[t]))return!1}return!0}(r.params,o.value.params)),l=t.computed(()=>a.value>-1&&a.value===r.matched.length-1&&g(r.params,o.value.params));return{route:o,href:t.computed(()=>o.value.href),isActive:s,isExactActive:l,navigate:function(r={}){return function(e){if(e.metaKey||e.altKey||e.ctrlKey||e.shiftKey)return;if(e.defaultPrevented)return;if(void 0!==e.button&&0!==e.button)return;if(e.currentTarget&&e.currentTarget.getAttribute){const t=e.currentTarget.getAttribute("target");if(/\b_blank\b/i.test(t))return}e.preventDefault&&e.preventDefault();return!0}(r)?n[t.unref(e.replace)?"replace":"push"](t.unref(e.to)):Promise.resolve()}}}const Ee=t.defineComponent({name:"RouterLink",props:{to:{type:[String,Object],required:!0},activeClass:String,exactActiveClass:String,custom:Boolean,ariaCurrentValue:{type:String,default:"page"}},setup(e,{slots:n,attrs:r}){const o=t.reactive(Re(e)),{options:a}=t.inject(c),i=t.computed(()=>({[Ae(e.activeClass,a.linkActiveClass,"router-link-active")]:o.isActive,[Ae(e.exactActiveClass,a.linkExactActiveClass,"router-link-exact-active")]:o.isExactActive}));return()=>{const a=n.default&&n.default(o);return e.custom?a:t.h("a",l({"aria-current":o.isExactActive?e.ariaCurrentValue:null,onClick:o.navigate,href:o.href},r,{class:i.value}),a)}}});function Oe(e){return e?e.aliasOf?e.aliasOf.path:e.path:""}let Ae=(e,t,n)=>null!=e?e:null!=t?t:n;const ke=t.defineComponent({name:"RouterView",props:{name:{type:String,default:"default"},route:Object},setup(e,{attrs:n,slots:r}){const c=t.inject(i),s=t.inject(a,0),u=t.computed(()=>(e.route||c).matched[s]);t.provide(a,s+1),t.provide(o,u);const f=t.ref();return t.watch(()=>[f.value,u.value,e.name],([e,t,n],[r,o,a])=>{t&&(t.instances[n]=e,o&&e===r&&(t.leaveGuards=o.leaveGuards,t.updateGuards=o.updateGuards)),!e||!t||o&&m(t,o)&&r||(t.enterCallbacks[n]||[]).forEach(t=>t(e))}),()=>{const o=e.route||c,a=u.value,i=a&&a.components[e.name],s=e.name;if(!i)return r.default?r.default({Component:i,route:o}):null;const p=a.props[e.name],h=p?!0===p?o.params:"function"==typeof p?p(o):p:null,d=t.h(i,l({},h,n,{onVnodeUnmounted:e=>{e.component.isUnmounted&&(a.instances[s]=null)},ref:f}));return r.default?r.default({Component:d,route:o}):d}}});function xe(e){return e.reduce((e,t)=>e.then(()=>t()),Promise.resolve())}return e.RouterLink=Ee,e.RouterView=ke,e.START_LOCATION=M,e.createMemoryHistory=function(e=""){let t=[],n=[""],r=0;function o(e){r++,r===n.length||n.splice(r),n.push(e)}const a={location:"",state:{},base:e,createHref:O.bind(null,e),replace(e){n.splice(r--,1),o(e)},push(e,t){o(e)},listen:e=>(t.push(e),()=>{const n=t.indexOf(e);n>-1&&t.splice(n,1)}),destroy(){t=[]},go(e,o=!0){const a=this.location,c=e<0?w.back:w.forward;r=Math.max(0,Math.min(r+e,n.length-1)),o&&function(e,n,{direction:r,delta:o}){const a={direction:r,delta:o,type:b.pop};for(let r of t)r(e,n,a)}(this.location,a,{direction:c,delta:e})}};return Object.defineProperty(a,"location",{get:()=>n[r]}),a},e.createRouter=function(e){const n=K(e.routes,e);let r=e.parseQuery||de,o=e.stringifyQuery||me,{scrollBehavior:a}=e,p=e.history;const d=ve(),v=ve(),y=ve(),b=t.shallowRef(M);let w=M;s&&a&&"scrollRestoration"in history&&(history.scrollRestoration="manual");const R=u.bind(null,e=>""+e),E=u.bind(null,pe),O=u.bind(null,he);function j(e,t){if(t=l({},t||b.value),"string"==typeof e){let o=h(r,e,t.path),a=n.resolve({path:o.path},t),c=p.createHref(o.fullPath);return l(o,a,{params:O(a.params),redirectedFrom:void 0,href:c})}let a;"path"in e?a=l({},e,{path:h(r,e.path,t.path).path}):(a=l({},e,{params:E(e.params)}),t.params=E(t.params));let c=n.resolve(a,t);const i=ue(e.hash||"").replace(ie,"{").replace(le,"}").replace(ae,"^");c.params=R(O(c.params));const s=function(e,t){let n=t.query?e(t.query):"";return t.path+(n&&"?")+n+(t.hash||"")}(o,l({},e,{hash:i,path:c.path}));let u=p.createHref(s);return l({fullPath:s,hash:i,query:o===me?ge(e.query):e.query},c,{redirectedFrom:void 0,href:u})}function C(e){return"string"==typeof e?{path:e}:l({},e)}function $(e,t){if(w!==e)return B(8,{from:t,to:e})}function S(e){return L(e)}function L(e,t){const n=w=j(e),r=b.value,a=e.state,c=e.force,i=!0===e.replace,s=n.matched[n.matched.length-1];if(s&&s.redirect){const{redirect:e}=s;let r=C("function"==typeof e?e(n):e);return L(l({query:n.query,hash:n.hash,params:n.params},r,{state:a,force:c,replace:i}),t||n)}const u=n;let f;return u.redirectedFrom=t,!c&&function(e,t,n){let r=t.matched.length-1,o=n.matched.length-1;return r>-1&&r===o&&m(t.matched[r],n.matched[o])&&g(t.params,n.params)&&e(t.query)===e(n.query)&&t.hash===n.hash}(o,r,n)&&(f=B(16,{to:u,from:r}),z(r,r,!0,!1)),(f?Promise.resolve(f):_(u,r)).catch(e=>G(e,14)?e:N(e)).then(e=>{if(e){if(G(e,2))return L(l(C(e.to),{state:a,force:c,replace:i}),t||u)}else e=U(u,r,!0,i,a);return F(u,r,e),e})}function T(e,t){const n=$(e,t);return n?Promise.reject(n):Promise.resolve()}function _(e,t){let n;const[r,o,a]=function(e,t){const n=[],r=[],o=[],a=Math.max(t.matched.length,e.matched.length);for(let c=0;c<a;c++){const a=t.matched[c];a&&(e.matched.indexOf(a)<0?n.push(a):r.push(a));const i=e.matched[c];i&&t.matched.indexOf(i)<0&&o.push(i)}return[n,r,o]}(e,t);n=we(r.reverse(),"beforeRouteLeave",e,t);for(const o of r)for(const r of o.leaveGuards)n.push(be(r,e,t));const c=T.bind(null,e,t);return n.push(c),xe(n).then(()=>{n=[];for(const r of d.list())n.push(be(r,e,t));return n.push(c),xe(n)}).then(()=>{n=we(o,"beforeRouteUpdate",e,t);for(const r of o)for(const o of r.updateGuards)n.push(be(o,e,t));return n.push(c),xe(n)}).then(()=>{n=[];for(const r of e.matched)if(r.beforeEnter&&t.matched.indexOf(r)<0)if(Array.isArray(r.beforeEnter))for(const o of r.beforeEnter)n.push(be(o,e,t));else n.push(be(r.beforeEnter,e,t));return n.push(c),xe(n)}).then(()=>(e.matched.forEach(e=>e.enterCallbacks={}),n=we(a,"beforeRouteEnter",e,t),n.push(c),xe(n))).then(()=>{n=[];for(const r of v.list())n.push(be(r,e,t));return n.push(c),xe(n)}).catch(e=>G(e,8)?e:Promise.reject(e))}function F(e,t,n){for(const r of y.list())r(e,t,n)}function U(e,t,n,r,o){const a=$(e,t);if(a)return a;const c=t===M,i=s?history.state:{};n&&(r||c?p.replace(e.fullPath,l({scroll:c&&i&&i.scroll},o)):p.push(e.fullPath,o)),b.value=e,z(e,t,n,c),Q()}let V;function H(){V=p.listen((e,t,n)=>{const r=j(e);w=r;const o=b.value;var a,c;s&&(a=x(o.fullPath,n.delta),c=A(),P.set(a,c)),_(r,o).catch(e=>G(e,12)?e:G(e,2)?(n.delta&&p.go(-n.delta,!1),L(e.to,r).catch(f),Promise.reject()):(n.delta&&p.go(-n.delta,!1),N(e))).then(e=>{(e=e||U(r,o,!1))&&n.delta&&p.go(-n.delta,!1),F(r,o,e)}).catch(f)})}let I,W=ve(),D=ve();function N(e){return Q(e),D.list().forEach(t=>t(e)),Promise.reject(e)}function Q(e){I||(I=!0,H(),W.list().forEach(([t,n])=>e?n(e):t()),W.reset())}function z(e,n,r,o){if(!s||!a)return Promise.resolve();let c=!r&&function(e){const t=P.get(e);return P.delete(e),t}(x(e.fullPath,0))||(o||!r)&&history.state&&history.state.scroll||null;return t.nextTick().then(()=>a(e,n,c)).then(e=>e&&k(e)).catch(N)}const X=e=>p.go(e);let Y;const Z=new Set;return{currentRoute:b,addRoute:function(e,t){let r,o;return q(e)?(r=n.getRecordMatcher(e),o=t):o=e,n.addRoute(o,r)},removeRoute:function(e){let t=n.getRecordMatcher(e);t&&n.removeRoute(t)},hasRoute:function(e){return!!n.getRecordMatcher(e)},getRoutes:function(){return n.getRoutes().map(e=>e.record)},resolve:j,options:e,push:S,replace:function(e){return S(l(C(e),{replace:!0}))},go:X,back:()=>X(-1),forward:()=>X(1),beforeEach:d.add,beforeResolve:v.add,afterEach:y.add,onError:D.add,isReady:function(){return I&&b.value!==M?Promise.resolve():new Promise((e,t)=>{W.add([e,t])})},install(e){e.component("RouterLink",Ee),e.component("RouterView",ke),e.config.globalProperties.$router=this,Object.defineProperty(e.config.globalProperties,"$route",{get:()=>t.unref(b)}),s&&!Y&&b.value===M&&(Y=!0,S(p.location).catch(e=>{}));const n={};for(let e in M)n[e]=t.computed(()=>b.value[e]);e.provide(c,this),e.provide(i,t.reactive(n));let r=e.unmount;Z.add(e),e.unmount=function(){Z.delete(e),Z.size<1&&(V(),b.value=M,Y=!1,I=!1),r.call(this,arguments)}}}},e.createRouterMatcher=K,e.createWebHashHistory=function(e){return(e=location.host&&e||location.pathname).indexOf("#")<0&&(e+="#"),L(e)},e.createWebHistory=L,e.isNavigationFailure=G,e.onBeforeRouteLeave=function(e){const n=t.inject(o,{}).value;n&&ye(n.leaveGuards,e)},e.onBeforeRouteUpdate=function(e){const n=t.inject(o,{}).value;n&&ye(n.updateGuards,e)},e.parseQuery=de,e.stringifyQuery=me,e.useLink=Re,e.useRoute=function(){return t.inject(i)},e.useRouter=function(){return t.inject(c)},e}({},Vue);

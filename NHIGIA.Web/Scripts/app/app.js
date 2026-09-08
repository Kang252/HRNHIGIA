"use strict";

var hrmApp = angular.module('hrmApplication', ['ngRoute', 'ngAnimate', 'ngMessages', 'kendo.directives', 'hrmApplication.directives', 'ngCookies', 'ngSanitize']);

hrmApp.config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
    $httpProvider.defaults.cache = false;
    if (!$httpProvider.defaults.headers.get) {
        $httpProvider.defaults.headers.get = {};
    }
    $httpProvider.defaults.headers.get['If-Modified-Since'] = '0';
}]);

function loading() {
    kendo.ui.progress($(document.body), true);
    $('.k-loading-mask').height($(".k-grid").height());
}

function stopLoading() {
    kendo.ui.progress($(document.body), false);
}

function loadingPopUp() {
    kendo.ui.progress($(".KenWindownPopup"), true);
    $('.k-loading-mask').height($(".KenWindownPopup").height());
}

function stopLoadingPopUp() {
    kendo.ui.progress($(".KenWindownPopup"), false);
}

function loadingImport() {
    kendo.ui.progress($(".KenWindownImportListCategory"), true);
    $('.k-loading-mask').height($(".KenWindownImportListCategory").height());
}

function stopLoadingImport() {
    kendo.ui.progress($(".KenWindownImportListCategory"), false);
}

function loadingProfilePage() {
    kendo.ui.progress($(document.body), true);
    $('.k-loading-mask').height($(".profile_page").height());
}

function stopLoadingProfilePage() {
    kendo.ui.progress($(document.body), false);
}
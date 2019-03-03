﻿'use strict';
app.controller('ArticleController', ['$scope', '$rootScope', '$location','$filter',
    'ngAppSettings', '$routeParams', 'ArticleService',
    function ($scope, $rootScope, $location,$filter, ngAppSettings, $routeParams, service) {
        BaseCtrl.call(this, $scope, $rootScope, $routeParams, ngAppSettings, service);
        $scope.preview = function (item) {
            item.editUrl = '/portal/article/details/' + item.id;
            $rootScope.preview('article', item, item.title, 'modal-lg');
        };
        $scope.referrerUrl = $rootScope.referrerUrl;
        $scope.saveCallback = function () {
            if($rootScope.referrerUrl)
            {
                $location.url($rootScope.referrerUrl);
            }
        }
        $scope.getSingleSuccessCallback = function () {
            var moduleId = $routeParams.module_id;
            var pageId =  $routeParams.page_id;
            if(moduleId){
                var moduleNav = $rootScope.findObjectByKey($scope.activedData.modules, 'moduleId', moduleId);
                if(moduleNav){
                    moduleNav.isActived= true;
                }
            }
            if(pageId){
                var pageNav = $rootScope.findObjectByKey($scope.activedData.categories, 'pageId', pageId);
                if(pageNav){
                    pageNav.isActived= true;
                }
            }
            $scope.activedData.publishedDateTime = $filter('utcToLocalTime')($scope.activedData.publishedDateTime);
        }
        $scope.generateSeo = function () {
            if ($scope.activedData) {
                if ($scope.activedData.seoName === null || $scope.activedData.seoName === '') {
                    $scope.activedData.seoName = $rootScope.generateKeyword($scope.activedData.title, '-');
                }
                if ($scope.activedData.seoTitle === null || $scope.activedData.seoTitle === '') {
                    $scope.activedData.seoTitle = $rootScope.generateKeyword($scope.activedData.title, '-');
                }
                if ($scope.activedData.seoDescription === null || $scope.activedData.seoDescription === '') {
                    $scope.activedData.seoDescription = $rootScope.generateKeyword($scope.activedData.title, '-');
                }
                if ($scope.activedData.seoKeywords === null || $scope.activedData.seoKeywords === '') {
                    $scope.activedData.seoKeywords = $rootScope.generateKeyword($scope.activedData.title, '-');
                }
            }
        }
    }
]);
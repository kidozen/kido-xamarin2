var request = require('request');

var authToken = require('../resources/getAuthToken.js');

var response = require('../resources/responseHandler');



exports.storageTest = function(tenantDomain, tenantPrivateKey, tenantAppDomain, adminUserEmail, callback){

    var token = authToken.getToken(tenantPrivateKey, adminUserEmail);

    var appToken = authToken.getAppToken(tenantPrivateKey, adminUserEmail);

    var addStorageItemOpts = {

                uri: tenantDomain+'/storage/test1/item1',

                method: 'post',

                json: true,

                body: 
                   {"title":"new item1","desc":"item from market url","completed":false},

                headers: { authorization: token}
            };

    var deleteStorageItemOpts = {

                uri: tenantDomain+'/storage/test1/item1',

                method: 'delete',

                json: true,

                body: 
                   {},

                headers: { authorization: token}
            };

    var getStorageMktOpts = {

                uri: tenantDomain+'/storage/test1/item1',

                method: 'get',

                json: true,

                body: 
                   {},

                headers: { authorization: token}
            };


    var getStorageAppOpts = {

                uri: tenantAppDomain+'/storage/local/item1',

                method: 'get',

                json: true,

                body: 
                   {},

                headers: { authorization: appToken}
            };

    request(addStorageItemOpts, function(err, res, body) {
        response.processResponse(err, res, body,'create storage item - '+tenantDomain);

        request(getStorageMktOpts, function(err, res, body) {
            response.processResponse(err, res, body,'get storage item from mkt - '+tenantDomain);

            request(getStorageAppOpts, function(err, res, body) {
                response.processResponse(err, res, body,'get storage item from app - '+tenantDomain);

                request(deleteStorageItemOpts, function(err, res, body) {
                    response.processResponse(err, res, body,'delete storage item - '+tenantDomain);

                    callback();
                });
            });
        });
    });

}





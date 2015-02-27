var request = require('request');
var response = require('../resources/responseHandler.js');

exports.createApp = function (tenantDomain, appName, token, callback){
    var createOpts = {
        uri: tenantDomain + '/api/apps',
        method: 'post',
        json: true,
        body: {"name": appName,"displayName":appName},
        headers: { authorization: token}
    };
    var getSdkKey = {
        uri: tenantDomain + '/api/admin/auth/' + appName + '/keys',
        headers: { authorization: token}
    };
    request(createOpts, function(err, res, body) {
        request(getSdkKey, function(err,res,body) {
            console.log(body);
            callback();
        });
    });
}

exports.deleteApp = function(tenantDomain, application, token, callback) {
    var deleteOpts = {
        uri: tenantDomain + '/api/apps/',
        method: 'delete',
        json: true,
        body: {},
        headers: {
            authorization: token
        }
    };

    var getAppOpts = {
        uri: tenantDomain + '/publicapi/apps?name=' + application,
        method: 'get',
        json: true,
        body: {},
        headers: {
            authorization: token
        }
    };

    request(getAppOpts, function(err, res, body) {
        if (typeof body !== 'undefined') {
            console.log("appid", body[0]['_id']);
            if (body) {
                deleteOpts.uri = deleteOpts.uri + (body[0]['_id']);
            }
            request(deleteOpts, function(err, res, body) {
                console.log("deleteApp", res.statusCode);
                callback();
            });
        } else {
            console.log('\nError getting the API ' + application);
        }
    });
}
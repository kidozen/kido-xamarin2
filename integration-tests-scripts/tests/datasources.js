var request = require('request');
var response = require('../resources/responseHandler');

exports.createDs = function(tenant, service, dsname, token, callback) {
    var reqOpts = {
        uri: tenant + '/api/admin/datasources',
        method: 'post',
        json: true,
        body: {
            "name": dsname,
            "description": "Weather datasource",
            "type": "query",
            "serviceName": service,
            "serviceType": "rest",
            "operationName": "get",
            "timeout": 25,
            "cache": 0,
            "body": "{\"qs\":{\"q\":\"chicago\"}}"
        },
        headers: {
            authorization: token
        }
    };

    request(reqOpts, function(err, res, body) {
        console.log("createDs", res.statusCode);
        callback();
    });
}

exports.deleteDs = function(tenantDomain, dsname, token, callback){
    var deleteDsOpts = {
        uri: tenantDomain + '/api/admin/datasources/' + dsname,
        method: 'Delete',
        json: true,
        body: {},
        headers: { authorization: token}
    };
    request(deleteDsOpts, function(err, res, body) {
        console.log("deleteDs", res.statusCode);
        callback();
    });
}

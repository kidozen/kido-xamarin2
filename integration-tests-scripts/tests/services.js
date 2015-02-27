var request = require('request');
var response = require('../resources/responseHandler');

exports.createServiceOnCloud = function(tenant, name, token, callback) {
    var reqOpts = {
        uri: tenant + '/api/admin/v2/services',
        method: 'post',
        json: true,
        body: {
            "enterpriseApi": "rest",
            "name": name,
            "runOn": {
                "type": "hub"
            },
            "config": {
                "endpoint": "http://api.openweathermap.org/data/2.5/weather",
                "maxRedirects": 10,
                "followRedirect": true,
                "strictSSL": true
            }
        },
        headers: {
            authorization: token
        }
    };
    var enableServiceOpts = {
        uri: tenant + '/api/admin/v2/services/xamarin-tests-weather/enable',
        method: 'post',
        json: true,
        body: {},
        headers: { authorization: token}
    };

    request(reqOpts, function(err, res, body) {
        console.log("createServiceOnCloud", res.statusCode);
        request(enableServiceOpts, function(err, res, body) {
            console.log("enableServiceOpts", res.statusCode);    
            callback();
        });
    });

}

exports.deleteServices = function(tenantDomain, serviceName, token, callback) {
    var disableServiceOpts = {
        uri: tenantDomain + '/api/admin/v2/services/' + serviceName + '/disable',
        method: 'post',
        json: true,
        body: {},
        headers: {
            authorization: token
        }
    };

    var deleteServiceOpts = {
        uri: tenantDomain + '/api/admin/v2/services/' + serviceName,
        method: 'Delete',
        json: true,
        body: {},
        headers: {
            authorization: token
        }
    };

    request(disableServiceOpts, function(err, res, body) {
        console.log("disableService", res.statusCode);
        request(deleteServiceOpts, function(err, res, body) {
            console.log("deleteServices", res.statusCode);
            callback();
        });
    });
}

exports.createServiceOnAgent = function(tenantDomain, tenantPrivateKey, adminUserEmail, callback) {

    var token = authToken.getToken(tenantPrivateKey, adminUserEmail);

    var agentOpts = {

        uri: tenantDomain + '/api/admin/v2/services',

        method: 'post',

        json: true,

        body: {
            "enterpriseApi": "rest",
            "name": "aut-weather-onAgent",
            "runOn": {
                "type": "agent",
                "name": "agent2"
            },
            "config": {
                "endpoint": "http://api.openweathermap.org/data/2.5/weather",
                "maxRedirects": 10,
                "followRedirect": true,
                "strictSSL": true
            }
        },


        headers: {
            authorization: token
        }
    };


    request(agentOpts, function(err, res, body) {

        response.processResponse(err, res, body, 'create service on agent - ' + tenantDomain);

        callback();
    });
}
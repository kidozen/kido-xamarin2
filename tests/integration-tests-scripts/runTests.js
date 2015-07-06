var async = require('async');
var app = require('./tests/application.js');
var service = require('./tests/services.js');
var datasources = require('./tests/datasources.js');

var stdio = require('stdio');
var authToken = require('./resources/getAuthToken.js');

var tenantList = require('./config.json');

var ops = stdio.getopt({
    'tenant': {
        key: 't',
        args: 1,
        mandatory: true,
        description: '\nSTRING\n Use [-t all] in case you want to run tests in all available environments configured in config.json.\n Use [-t tenantName] in case you want to run tests in a specific tenant available in config.json.'
    }
});

tenantList.forEach( function(value) {
    if (ops.tenant == 'all') {
        runTests(value);
    } else if (ops.tenant == value.tenantName) {
        runTests(value)
    }
})



function runTests(config) {
    var token = authToken.getToken(config.tenantPrivateKey, config.adminUserEmail, config.appName);
    async.series(
        [
        function(callback) {
            app.createApp(config.tenantDomain, config.appName, token, callback);
        },
        function(callback) {
            service.createServiceOnCloud(config.tenantDomain, config.serviceName, token, callback);
        },
        function(callback) {
            datasources.createDs(config.tenantDomain, config.serviceName, config.dsName, token, callback);
        },
		function(callback){
			datasources.deleteDs(config.tenantDomain, config.dsName, token, callback);
			},
		function(callback){
			service.deleteServices(config.tenantDomain, config.serviceName , token, callback);
			},
		function(callback){
			app.deleteApp(config.tenantDomain,  config.appName, token, callback);
			} 
        ], 
        function(err) { if (err) { console.log(err);}
    });
}
var swt = require('simplewebtoken');	


exports.getToken = function (tenantPrivateKey, adminUserEmail){

	var options = {

			key: tenantPrivateKey,

			audience: "_marketplace",

			expiresInMinutes: 30,

			issuer: 'http://auth.kidozen.com/'

		};

	var userInfo = {
		'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': adminUserEmail,
		'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'Kidozen Admin',
		'http://schemas.kidozen.com/role': ['Application Center Admin'],
		'http://schemas.kidozen.com/usersource': 'Administrative User',
		'http://schemas.kidozen.com/identityprovider': 'self­signed'

		};

	var token = swt.sign(userInfo, options);
	
	return token;

}

exports.getAppToken = function (tenantPrivateKey, adminUserEmail, audience){

	var optionsApp = {

			key: tenantPrivateKey,

			audience: audience,

			expiresInMinutes: 30,

			issuer: 'http://auth.kidozen.com/'

		};

	var userInfoApp = {
		'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': adminUserEmail,
		'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'Kidozen Admin',
		'http://schemas.kidozen.com/role': ['Application Center Admin'],
		'http://schemas.kidozen.com/usersource': 'Administrative User',
		'http://schemas.kidozen.com/identityprovider': "https://identity.kidozen.com/",
		'http://schemas.kidozen.com/action': 'allow all *'

		};


	var token = swt.sign(userInfoApp, optionsApp);
	
	return token;

}




